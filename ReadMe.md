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
version="1.0.2"
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
docker build --secret id=GH_OWNER --secret id=GH_PAT -t play.identity:$version .
```

## Run Docker Image 
```bash 
export adminPass="password here"
docker run -it --rm -p 5002:5002 --name identity -e MongoDbSettings__Host=mongo -e RabbitMQSettings__Host=rabbitmq -e IdentityServerSettings__AdminUserPassword=$adminPass --network playinfra_default play.identity:$version
```