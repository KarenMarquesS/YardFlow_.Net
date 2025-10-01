#!/bin/bash
 
az group create --name rg-devforge --location brazilsouth
 
docker build -f Dockerfile.bd -t mysql .
 
az acr create \
    --resource-group rg-devforge \
    --name mysqldevforge \
    --sku Standard \
    --location brazilsouth \
    --public-network-enabled true \
    --admin-enabled true
 
MYSQL_LOGIN_SERVER=$(az acr show --name mysqldevforge --resource-group rg-devforge --query loginServer --output tsv)
MYSQL_ADMIN_USERNAME=$(az acr credential show --name mysqldevforge --resource-group rg-devforge --query username --output tsv)
MYSQL_ADMIN_PASSWORD=$(az acr credential show --name mysqldevforge --resource-group rg-devforge --query passwords[0].value --output tsv)
 
echo "MySQL ACR Login Server: $MYSQL_LOGIN_SERVER"
 
az acr login --name mysqldevforge
 
docker tag mysql $MYSQL_LOGIN_SERVER/mysql-devforge:v1
docker push $MYSQL_LOGIN_SERVER/mysql-devforge:v1
 
az container create \
    --resource-group rg-devforge \
    --name mysql-devforge \
    --image $MYSQL_LOGIN_SERVER/mysql-devforge:v1 \
    --cpu 1 \
    --memory 2 \
    --registry-login-server $MYSQL_LOGIN_SERVER \
    --registry-username $MYSQL_ADMIN_USERNAME \
    --registry-password $MYSQL_ADMIN_PASSWORD \
    --ports 3306 \
    --os-type Linux \
    --environment-variables MYSQL_ROOT_PASSWORD=yardflow MYSQL_DATABASE=devforgedb \
    --ip-address Public
 
MYSQL_IP=$(az container show --resource-group rg-devforge --name mysql-devforge --query ipAddress.ip --output tsv)
echo "MySQL IP Público: $MYSQL_IP"
 
 
docker build -f Dockerfile.net -t dotnetapi .


 
az acr create \
    --resource-group rg-devforge \
    --name dotnetdevforge \
    --sku Standard \
    --location brazilsouth \
    --public-network-enabled true \
    --admin-enabled true
 
API_LOGIN_SERVER=$(az acr show --name dotnetdevforge --resource-group rg-devforge --query loginServer --output tsv)
API_ADMIN_USERNAME=$(az acr credential show --name dotnetdevforge --resource-group rg-devforge --query username --output tsv)
API_ADMIN_PASSWORD=$(az acr credential show --name dotnetdevforge --resource-group rg-devforge --query passwords[0].value --output tsv)
 
echo "API ACR Login Server: $API_LOGIN_SERVER"
 
az acr login --name dotnetdevforge
 
docker tag dotnetapi $API_LOGIN_SERVER/dotnet-api:v1
docker push $API_LOGIN_SERVER/dotnet-api:v1
 
 
az container create \
    --resource-group rg-devforge \
    --name dotnetapi \
    --image $API_LOGIN_SERVER/dotnet-api:v1 \
    --cpu 1 \
    --memory 2 \
    --registry-login-server $API_LOGIN_SERVER \
    --registry-username $API_ADMIN_USERNAME \
    --registry-password $API_ADMIN_PASSWORD \
    --os-type Linux \
    --ports 8080 \
    --ip-address Public \
    --environment-variables ConnectionStrings__DefaultConnection="Server=$MYSQL_IP;Port=3306;Database=devforgedb;Uid=root;Pwd=yardflow;"