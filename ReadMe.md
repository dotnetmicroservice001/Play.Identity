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
version="1.0.3"
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
version="1.0.5"
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
az acr login --name $acrname
docker buildx build \
  --platform linux/amd64,linux/arm64 \
  --secret id=GH_OWNER --secret id=GH_PAT \
  -t "$acrname.azurecr.io/play.identity:$version" \
  --push .
```

## Create Kubernetes namespace 
```bash 
export namespace="identity"
kubectl create namespace $namespace 
```

## Create Kubernetes secrets
```bash 
kubectl create secret generic identity-secrets \
--from-literal=cosmosdb-connectionstring="$cosmosDbConnString" \
--from-literal=servicebus-connectionstring="$serviceBusConnString" \
--from-literal=adminpassword="$adminPass" \
-n "$namespace"
```

## Create the Kubernetes Pod
```bash
kubectl apply -f ./kubernetes/identity.yaml -n "$namespace"
```