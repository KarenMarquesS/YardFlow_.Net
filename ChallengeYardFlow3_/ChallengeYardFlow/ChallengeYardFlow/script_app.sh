#!/usr/bin/env bash
set -euo pipefail

# === CONFIGURAR antes de rodar ===
RG="myResourceGroup"                
LOCATION="brazilsouth"              
ACR_NAME="meuacr12345"              
APP_IMAGE_NAME="${ACR_NAME}.azurecr.io/yardflow:latest"
ACI_APP_NAME="yardflow-aci"
MYSQL_ACI_NAME="yardflow-mysql-aci" 

# DB variables (ACI will get these to connect)
export MYSQL_HOST="mysql-server"    
export MYSQL_PORT="3306"
export MYSQL_DATABASE="yardflowdb"
export MYSQL_USER="yardflow"
export MYSQL_PASSWORD="StrongP@ssw0rd!"  

# imagem
echo "=== dotnet publish ==="
dotnet publish ChallengeYardFlow/ -c Release -o ./publish

echo "=== Building docker image (Dockerfile.dotnet) ==="
docker build -f Dockerfile.dotnet -t "${APP_IMAGE_NAME}" .

# === Push to ACR ===
echo "=== Login to Azure (make sure CLI is logged in) ==="

# az login # uncomment if needed

az acr create --resource-group "$RG" --name "$ACR_NAME" --sku Basic --location "$LOCATION" --admin-enabled true 


az acr login --name "$ACR_NAME"


docker push "${APP_IMAGE_NAME}"

# === Deploy to ACI ===
echo "=== Get ACR creds ==="
ACR_USERNAME=$(az acr credential show -n "$ACR_NAME" --query "username" -o tsv)
ACR_PASSWORD=$(az acr credential show -n "$ACR_NAME" --query "passwords[0].value" -o tsv)

# Set the connection string env var for dotnet (use __ to nest)
CONN_STR="server=${MYSQL_HOST};port=${MYSQL_PORT};database=${MYSQL_DATABASE};user=${MYSQL_USER};password=${MYSQL_PASSWORD};"

echo "=== Create/replace ACI for the app ==="
az container create \
  --resource-group "$RG" \
  --name "$ACI_APP_NAME" \
  --image "$APP_IMAGE_NAME" \
  --registry-login-server "${ACR_NAME}.azurecr.io" \
  --registry-username "$ACR_USERNAME" \
  --registry-password "$ACR_PASSWORD" \
  --cpu 1 --memory 1.5 \
  --ports 5050 \
  --environment-variables ConnectionStrings__DefaultConnection="$CONN_STR" ASPNETCORE_ENVIRONMENT="Production" \
  --ip-address Public \
  --dns-name-label "${ACI_APP_NAME}-${ACR_NAME}" \
  --restart-policy OnFailure

echo "App deployed. ACI name label: ${ACI_APP_NAME}-${ACR_NAME} (public IP assigned)."
