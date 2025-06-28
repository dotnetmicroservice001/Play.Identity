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
