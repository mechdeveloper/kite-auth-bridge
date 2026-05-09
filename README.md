![Dev Container](https://img.shields.io/badge/devcontainer-ready-blue)
![.NET](https://img.shields.io/badge/.NET-10.0-purple)

# Kite Auth Bridge (.NET)

A lightweight .NET Minimal API service that simplifies authentication with the [Zerodha](https://kite.trade) Kite Connect API and exposes a reusable access token for API clients like Bruno, Postman, or custom clients.

---

## 📌 Prerequisites

Before running this project, you must set up Kite Connect API access:

📺 Setup guide:
[Kite Connect Setup Video](https://www.youtube.com/watch?v=r88L9AqnNaE)

📖 Official documentation:
[Kite Connect API Docs](https://kite.trade)

While creating new App you can use following Redirect URL for local development
```
http://localhost:5196/callback
```

You will receive:
* api_key
* api_secret

---

## 🚀 Why this project?

Kite Connect uses a **custom authentication flow** instead of standard OAuth2, which makes API automation slightly complex.

The flow requires:

- Browser-based login
- 2FA authentication
- `request_token` from redirect URL
- SHA-256 checksum generation
- Token exchange via `/session/token`

This project automates the entire flow so developers can focus on consuming APIs instead of handling repetitive authentication steps.

---

## ✨ Key Highlights

- Custom authentication flow integration with Kite Connect
- Secure checksum generation using SHA-256
- Token bridge for API tools (Bruno / Postman)
- Dev Container + Docker support for reproducibility
- Clean Minimal API architecture in .NET

---

## 🧠 How authentication works

1. Redirect user to Kite login page with `api_key`
2. User logs in and completes 2FA
3. Kite redirects to `/callback` with `request_token`
4. Server generates checksum (SHA-256 of `api_key + request_token + api_secret`)
5. Exchange request token for `access_token`
6. Store token for reuse in API clients

---

## 🏗️ System Architecture

```
.NET API root (/)
        ↓
Browser → Kite Login → 2FA
        ↓
   request_token
        ↓
.NET API (/callback)
        ↓
Checksum Generation
        ↓
Kite Session API
        ↓
access_token stored in memory
        ↓
/token endpoint → Bruno / Postman / Clients
```

---

## 🔁 API Flow

### 1. Start authentication 

```
GET http://localhost:5196/
```

Redirects to Kite login page.


App internally handles callback (after login + 2FA)

```
GET http://localhost:5196/callback?request_token=xxx
```

- Validates request_token
- Generates checksum
- Exchanges token with Kite API
- Stores `access_token` in memory

---

### 2. Get access token

```
curl --request GET \
  --url http://localhost:5196/token \
  --header 'x-secret: your-app-secret'
```

Response:
```json
{
  "success": true,
  "data": {
    "api_key": "xxx",
    "access_token": "xxx"
  }
}
```

---

## 🔐 Security

* `/token` endpoint is protected using a custom header (x-secret)
* Access token is stored in memory (for local/dev use only)
* Not production-ready without persistent storage (e.g., Redis, DB, or secure vault)

---

## 🛠 Tech Stack

* .NET Minimal API
* HttpClient
* SHA256 cryptography
* Kite Connect API

---

## ▶️ How to run

```bash
dotnet build
dotnet run
```

Application will start on:
```
http://localhost:5196
```

---

## 🔧 Configuration

Option 1: appsettings.json
```
"Kite": {
  "ApiKey": "your-api-key",
  "ApiSecret": "your-api-secret",
  "BaseUrl": "https://api.kite.trade"
},
"App": {
  "Secret": "your-random-app-secret-string"
}
```

Option 2: Environment Variables (recommended)
```
Kite__ApiKey=your-api-key
Kite__ApiSecret=your-api-secret
Kite__BaseUrl=https://api.kite.trade
App__Secret=your-random-app-secret-string
```

---

## 🔐 App Secret

`App__Secret` is a simple shared key used to secure the `/token` endpoint.

It acts as a lightweight authentication layer between API clients and the service.

Example:

```bash
curl --request GET \
  --url http://localhost:5196/token \
  --header 'x-secret: your-app-secret-string'
```

---

## 🐳 Dev Container Support

This ensures a fully reproducible .NET development environment without local setup dependencies.

It can be used with:

* [Visual Studio Code Dev Containers extension](https://code.visualstudio.com/docs/devcontainers/containers)
* [GitHub Codespaces](https://github.com/features/codespaces)
* [Docker](https://www.docker.com/) based local development environments 

### ⚙️ How to use

#### ▶️ Open in VS Code

1. Install Dev Containers extension
2. Open the repository in VS Code
3. Click: `Reopen in Container`

#### ☁️ Open in GitHub Codespaces

Click: `Code → Codespaces → Create Codespace`

---

## 🐳 Docker Support

This project includes a multi-stage Docker build to ensure consistent deployment across environments.

▶️ Build Docker Image

```bash
cd src/KiteAuthBridge
docker build -t kite-auth-bridge:v0.1 .
```

🚀 Run Container

```bash
docker run -p 8080:8080 \
  -e "Kite__ApiKey=your-api-key" \
  -e "Kite__ApiSecret=your-api-secret" \
  -e "Kite__BaseUrl=https://api.kite.trade" \
  -e "App__Secret=your-random-app-secret-string" \
  kite-auth-bridge:v0.1
```

🌐 Access Application

Once running:
```
http://localhost:8080 
```

## 🌐 Ports

- Local run: `http://localhost:5196`
- Docker run: `http://localhost:8080`

Docker maps to 8080 for container isolation and portability.

---
## 📌 Summary

This project demonstrates how a non-OAuth authentication flow can be abstracted into a reusable API bridge, making external APIs easier to consume in developer tooling environments.

It showcases:
- API integration design
- authentication flow handling
- secure token management
- containerized development setup
