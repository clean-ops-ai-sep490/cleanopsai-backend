# CleanOpsAi Backend

<div align="center">

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-12.0-239120?style=for-the-badge&logo=csharp&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?style=for-the-badge&logo=postgresql&logoColor=white)
![RabbitMQ](https://img.shields.io/badge/RabbitMQ-MassTransit-FF6600?style=for-the-badge&logo=rabbitmq&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?style=for-the-badge&logo=docker&logoColor=white)
![Azure](https://img.shields.io/badge/Azure-Blob_Storage-0078D4?style=for-the-badge&logo=microsoftazure&logoColor=white)
![SignalR](https://img.shields.io/badge/SignalR-Real--time-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![Quartz](https://img.shields.io/badge/Quartz.NET-Scheduler-009688?style=for-the-badge&logoColor=white)
![EF Core](https://img.shields.io/badge/EF_Core-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![Swagger](https://img.shields.io/badge/Swagger-OpenAPI-85EA2D?style=for-the-badge&logo=swagger&logoColor=black)
![JWT](https://img.shields.io/badge/JWT-Auth-000000?style=for-the-badge&logo=jsonwebtokens&logoColor=white)

</div>

> Backend .NET 8 for the **CleanOpsAI** system — a smart cleaning operations management platform. Provides RESTful APIs, asynchronous AI scoring job processing via RabbitMQ, real-time notifications via SignalR, and an automated weekly task generation pipeline.

---

## 📐 Architecture

CleanOpsAi Backend follows a **Modular Monolith** pattern with Clean Architecture principles. Each module is self-contained with its own Domain, Application, and Infrastructure layers, while sharing common building blocks.

```
src/
├── Api/
│   └── CleanOpsAi.Api               # ASP.NET Core Web API (entry point)
├── BuildingBlocks/
│   ├── CleanOpsAi.BuildingBlocks.Domain
│   ├── CleanOpsAi.BuildingBlocks.Application
│   └── CleanOpsAi.BuildingBlocks.Infrastructure  # EF Core, MassTransit, Npgsql
├── Modules/
│   ├── Scoring/                     # AI scoring job submit/poll + retrain pipeline
│   ├── TaskOperations/              # Weekly task generation & assignment
│   ├── QualityControl/              # PPE compliance checks
│   ├── ServicePlanning/             # SOP & schedule management
│   ├── Workforce/                   # Worker & supervisor management
│   ├── UserAccess/                  # Authentication & authorization (JWT)
│   ├── ClientManagement/            # Client & workarea management
│   └── WorkareaCheckin/             # Workarea check-in/check-out
└── Workers/
    └── CleanOpsAi.Scoring.Worker    # Background worker for AI scoring jobs
```

### Key Technology Decisions

| Concern | Technology |
|---|---|
| Framework | ASP.NET Core 8 |
| ORM | Entity Framework Core 8 + Npgsql |
| Database | PostgreSQL (Supabase-hosted) |
| Message Bus | MassTransit 8 over RabbitMQ (CloudAMQP) |
| Scheduling | Quartz.NET 3.16 |
| Real-time | SignalR |
| Object Storage | Azure Blob Storage |
| Auth | JWT Bearer |
| API Docs | Swagger / OpenAPI (Swashbuckle) |
| Containerization | Docker + Docker Compose |

---

## 🧩 Modules Overview

| Module | Responsibility |
|---|---|
| **Scoring** | Submit AI scoring jobs, poll job status, trigger model retrain pipeline |
| **TaskOperations** | Auto-generate weekly task assignments from schedules via Quartz job |
| **QualityControl** | PPE compliance check requests and result processing |
| **ServicePlanning** | SOPs, steps, and schedule management |
| **Workforce** | Workers, supervisors, competencies |
| **UserAccess** | User registration, login, JWT issuance |
| **ClientManagement** | Clients and work area definitions |
| **WorkareaCheckin** | Check-in/check-out tracking for cleaning staff |

---

## 🐳 Docker Strategy

A single `docker-compose.yml` manages the entire stack:

- **`cleanopsai-api`** — Main REST API, runs on port `5000` (HTTP) / `5001` (HTTPS)
- **`cleanopsai-scoring-worker`** — Background worker that consumes scoring jobs from RabbitMQ

External dependencies (database, broker, scoring service) are supplied via environment variables — no local Postgres or RabbitMQ containers needed.

---

## ✅ Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Git](https://git-scm.com/)
- _(Optional)_ [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0) — only required for local development without Docker

---

## 🚀 Quick Start

### 1. Configure environment

Copy `.env.example` to `.env` and fill in the required values:

```powershell
Copy-Item .env.example .env
```

**Required variables:**

```env
BACKEND_DB_CONNECTION=Host=<db-host>;Port=5432;Database=<database>;Username=<username>;Password=<password>
MESSAGE_BROKER_HOST=amqps://<virtual-host-or-uri>
MESSAGE_BROKER_USERNAME=<rabbitmq-username>
MESSAGE_BROKER_PASSWORD=<rabbitmq-password>
SCORING_SERVICE_BASE_URL=http://host.docker.internal:8000
JWT_SECRET=<replace-with-a-strong-secret>
```

### 2. Start the stack

```powershell
docker compose up -d --build
docker compose ps
```

| Service | URL |
|---|---|
| Backend API | http://localhost:5000 |
| Swagger UI | http://localhost:5000/swagger |

### 3. Stop the stack

```powershell
docker compose down
```

---

## 🔬 Test the Scoring Flow

Submit a scoring job via `POST /api/scoring/jobs`:

```json
{
  "requestId": "demo-001",
  "environmentKey": "LOBBY_CORRIDOR",
  "imageUrls": [
    "https://images.unsplash.com/photo-1489515217757-5fd1be406fef?auto=format&fit=crop&w=1200&q=80"
  ]
}
```

Then poll `GET /api/scoring/jobs/{jobId}` until `status` is `SUCCEEDED` or `FAILED`.  
Each result includes a `visualizationBlobUrl` for direct image preview on frontend/mobile.

---

## ⚙️ Environment Variables Reference

### Core (Required)

| Variable | Description |
|---|---|
| `BACKEND_DB_CONNECTION` | PostgreSQL connection string |
| `MESSAGE_BROKER_HOST` | RabbitMQ host URI (`amqp://` or `amqps://`) |
| `MESSAGE_BROKER_USERNAME` | RabbitMQ username |
| `MESSAGE_BROKER_PASSWORD` | RabbitMQ password |
| `SCORING_SERVICE_BASE_URL` | Base URL of the external AI scoring service |
| `JWT_SECRET` | JWT signing secret |
| `JWT_ISSUER` | JWT issuer claim (default: `CleanOpsAi`) |
| `JWT_AUDIENCE` | JWT audience claim (default: `CleanOpsAi.Client`) |

### Job Scheduler (Optional)

| Variable | Default | Description |
|---|---|---|
| `WEEKLY_TASK_JOB_ENABLED` | `false` | Enable weekly task generation Quartz job |
| `WEEKLY_TASK_JOB_LOOKAHEAD_DAYS` | `7` | Days ahead to generate tasks for |
| `SCORING_RETRAIN_WEEKLY_ENABLED_API` | `false` | Enable retrain job in API container |
| `SCORING_RETRAIN_WEEKLY_ENABLED_WORKER` | `false` | Enable retrain job in worker container |

### Remote Retrain (Optional)

Enable the worker to trigger model retraining via the scoring API instead of running a local trainer:

| Variable | Default | Description |
|---|---|---|
| `SCORING_RETRAIN_REMOTE_ENABLED` | `false` | Enable remote retrain mode |
| `SCORING_RETRAIN_REMOTE_BASE_URL` | `http://host.docker.internal:8000` | Scoring API base URL |
| `SCORING_RETRAIN_REMOTE_API_KEY` | _(empty)_ | Optional API key |
| `SCORING_RETRAIN_REMOTE_TIMEOUT_SEC` | `7200` | Retrain job timeout (seconds) |
| `SCORING_RETRAIN_REMOTE_POLL_INTERVAL_SEC` | `5` | Status poll interval (seconds) |
| `SCORING_RETRAIN_USE_EXTERNAL_CANDIDATE` | `false` | Promote from external candidate bucket |
| `SCORING_RETRAIN_EXTERNAL_CANDIDATE_PREFIX` | `scoring/external/latest` | Blob prefix for external model candidate |

---

## 🔒 Security Checklist

- **Never commit real secrets** to `.env`, compose files, or source code.
- Use a local `.env` (git-ignored) or a secret store (Azure Key Vault, CI/CD secrets) to inject environment variables.
- If a secret has been exposed in Git history, **rotate it immediately** and scrub history using a history-rewriting tool.
- Assign separate RabbitMQ virtual hosts and credentials per environment where possible.
- Verify startup logs to ensure connection strings are not printed in full.

---

## 🛠️ Troubleshooting

| Symptom | Check |
|---|---|
| Job stays `QUEUED` indefinitely | Is `cleanopsai-scoring-worker` running? (`docker compose ps`) |
| API / worker can't connect to message broker | Verify `MESSAGE_BROKER_HOST` uses correct scheme (`amqp://` or `amqps://`), and credentials are valid |
| API returns error connecting to scoring service | Verify `SCORING_SERVICE_BASE_URL` and that the scoring service health endpoint (`/`) is reachable |
| Retrain not triggering | Check all `SCORING_RETRAIN_*` variables; if using remote mode, ensure `SCORING_RETRAIN_REMOTE_ENABLED=true` and the scoring API exposes `/retrain/jobs` |

---

## 📁 Project Files

| File / Folder | Purpose |
|---|---|
| `docker-compose.yml` | Defines API + worker services and shared environment |
| `.env.example` | Template for required environment variables |
| `src/Api/` | ASP.NET Core Web API host |
| `src/Modules/` | Feature modules (Domain / Application / Infrastructure) |
| `src/BuildingBlocks/` | Shared abstractions and infrastructure utilities |
| `src/Workers/` | Background scoring worker service |
| `global.json` | Pins .NET SDK version |
