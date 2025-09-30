#!/usr/bin/env bash
set -euo pipefail

# === CONFIGURE ===
RG="myResourceGroup"
LOCATION="brazilsouth"
ACR_NAME="meuacr12345"       
MYSQL_IMAGE_NAME="${ACR_NAME}.azurecr.io/yardflow-mysql:latest"  
STORAGE_ACCOUNT_NAME="yardflowstorage$RANDOM"  
FILE_SHARE_NAME="mysql-data"
MYSQL_ROOT_PASSWORD="RootP@ssw0rd!"
MYSQL_DATABASE="yardflowdb"
MYSQL_USER="yardflow"
MYSQL_PASSWORD="StrongP@ssw0rd!"

# Option A: Use official Docker Hub image directly (mais simples)
USE_OFFICIAL=true

# Create resource group if needed
az group create --name "$RG" --location "$LOCATION" || true

# Create a storage account + file share for persistence (for ACI)
echo "=== Creating storage account and file share ==="
az storage account create --name "$STORAGE_ACCOUNT_NAME" --resource-group "$RG" --sku Standard_LRS --location "$LOCATION"
SA_KEY=$(az storage account keys list --resource-group "$RG" --account-name "$STORAGE_ACCOUNT_NAME" --query "[0].value" -o tsv)
az storage share create --name "$FILE_SHARE_NAME" --account-name "$STORAGE_ACCOUNT_NAME" --account-key "$SA_KEY"

if [ "$USE_OFFICIAL" = false ]; then
  # Build custom MySQL image and push to ACR (optional)
  docker build -f Dockerfile.mysql -t "$MYSQL_IMAGE_NAME" .
  az acr login --name "$ACR_NAME"
  docker push "$MYSQL_IMAGE_NAME"
  IMAGE_TO_USE="$MYSQL_IMAGE_NAME"
else
  IMAGE_TO_USE="mysql:8.0"
fi

# Create the ACI for MySQL with Azure File mount
echo "=== Deploying MySQL to ACI with Azure File mount ==="
az container create \
  --resource-group "$RG" \
  --name "$MYSQL_ACI_NAME" \
  --image "$IMAGE_TO_USE" \
  --ports 3306 \
  --cpu 1 --memory 2 \
  --environment-variables MYSQL_ROOT_PASSWORD="$MYSQL_ROOT_PASSWORD" MYSQL_DATABASE="$MYSQL_DATABASE" MYSQL_USER="$MYSQL_USER" MYSQL_PASSWORD="$MYSQL_PASSWORD" \
  --azure-file-volume-share-name "$FILE_SHARE_NAME" \
  --azure-file-volume-account-name "$STORAGE_ACCOUNT_NAME" \
  --azure-file-volume-account-key "$SA_KEY" \
  --azure-file-volume-mount-path "/var/lib/mysql" \
  --ip-address Private \
  --restart-policy OnFailure

echo "MySQL deployed in ACI (container name: $MYSQL_ACI_NAME)."
echo "If MySQL ACI is private, ensure the app ACI can reach it (same vnet or use public IP)."
