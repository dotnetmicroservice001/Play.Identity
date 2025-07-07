# Play.Identity - Identity Microservice

The Identity microservice is the **authentication and authorization server** for the Play Economy system, built with **Duende IdentityServer**.

It uses **OAuth 2.0**, **OpenID Connect**, and **PKCE** to securely authenticate users and authorize client applications and microservices.

## 🔐 Features

- Acts as the **Identity Provider (IdP)** using Duende IdentityServer
- Supports **Authorization Code Flow with PKCE**
- Issues **JWT tokens** (access & ID) for user and service authentication
- Enables secure, scope-based access control across microservices

## 🧱 Tech Stack

- ASP.NET Core
- Duende IdentityServer
- OAuth 2.0 + OpenID Connect
- PKCE

## Creating and Publishing Package
```bash
version="1.0.7"
owner="dotnetmicroservice001"
gh_pat="[YOUR_PERSONAL_ACCESS_TOKEN]"

dotnet pack src/Play.Identity.Contracts --configuration Release \
  -p:PackageVersion="$version" \
  -p:RepositoryUrl="https://github.com/$owner/Play.Identity" \
  -o ../Packages
  
dotnet nuget push ../Packages/Play.Identity.Contracts.$version.nupkg --api-key $gh_pat \
--source "github"
```

## Build a Docker Image
```bash
export GH_OWNER=dotnetmicroservice001
export GH_PAT="ghp_YourRealPATHere"
export acrname="playeconomy01acr"
docker build --secret id=GH_OWNER --secret id=GH_PAT -t "$acrname.azurecr.io/play.identity:$version" .
```

## Run Docker Image 
```bash 
export adminPass="password here"
export cosmosDbConnString="conn string here"
export serviceBusConnString="conn string here"
docker run -it --rm \
  -p 5002:5002 \
  --name identity \
  -e MongoDbSettings__ConnectionString=$cosmosDbConnString \
  -e ServiceBusSettings__ConnectionString=$serviceBusConnString \
  -e ServiceSettings__MessageBroker="SERVICEBUS" \
  -e IdentitySettings__AdminUserPassword=$adminPass \
  play.identity:$version
```

## 🐳 Build & Push Docker Image (M2 Mac + AKS Compatible)

Build a multi-architecture image (ARM64 for local M2 Mac, AMD64 for AKS) and push to ACR:
```bash
version="1.0.9"
export GH_OWNER=dotnetmicroservice001
export GH_PAT="ghp_YourRealPATHere"
az acr login --name $acrname
docker buildx build \
  --platform linux/amd64 \
  --secret id=GH_OWNER --secret id=GH_PAT \
  -t "$acrname.azurecr.io/play.identity:$version" \
  --push .
```

## Create Kubernetes namespace 
```bash 
export namespace="identity"
kubectl create namespace $namespace 
```

## Create the Kubernetes Pod
```bash
kubectl apply -f ./kubernetes/identity.yaml -n "$namespace"
```

## Creating Azure Managed Identity and granting it access to Key Vault Store 
```bash
export appname=playeconomy-01
export namespace="identity"
az identity create --resource-group $appname --name $namespace 

export IDENTITY_CLIENT_ID=$(az identity show -g "$appname" -n "$namespace" --query clientId -o tsv)
export SUBSCRIPTION_ID=$(az account show --query id -o tsv)

az role assignment create \
  --assignee "$IDENTITY_CLIENT_ID" \
  --role "Key Vault Secrets User" \
  --scope "/subscriptions/$SUBSCRIPTION_ID/resourceGroups/$appname/providers/Microsoft.KeyVault/vaults/$appname"

```

## Establish the related Identity Credential
```bash
export AKS_OIDC_ISSUER="$(az aks show -n $appname -g $appname --query "oidcIssuerProfile.issuerUrl" -otsv)"

az identity federated-credential create --name ${namespace} --identity-name "${namespace}" --resource-group "${appname}" --issuer "${AKS_OIDC_ISSUER}" --subject system:serviceaccount:"${namespace}":"${namespace}-serviceaccount" --audience api://AzureADTokenExchange
```

## Create signing certificate 
```bash
kubectl apply -f ./kubernetes/signing-cert.yaml -n "$namespace"
```