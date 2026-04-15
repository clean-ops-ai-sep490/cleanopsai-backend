# CleanOpsAi Backend

Backend .NET cho he thong CleanOpsAI, cung cap API submit/poll scoring job va worker xu ly bat dong bo qua RabbitMQ.

## Docker Strategy

Repo nay duoc chuan hoa theo mo hinh:

- 1 compose duy nhat: docker-compose.yml

Compose chinh mac dinh chay backend infra + API + worker, va goi scoring service ben ngoai qua SCORING_SERVICE_BASE_URL.

## Prerequisites

- Docker Desktop
- Git
- (Tuy chon) .NET SDK 8.0 neu muon chay local khong dong goi container

## Quick Start (Recommended)

### 1. Chay backend stack mac dinh

```powershell
cd e:\capstone\server-side\cleanops-backend
docker compose up -d --build
docker compose ps
```

Service chinh:

- Backend API: http://localhost:5000
- Swagger backend: http://localhost:5000/swagger
- RabbitMQ UI: http://localhost:15672 (guest/guest)
- PostgreSQL: localhost:5432
- Redis: localhost:6379

Mac dinh backend se goi scoring service tai:

- SCORING_SERVICE_BASE_URL=http://host.docker.internal:8000

### 2. Tat stack

Mac dinh:

```powershell
docker compose down
```

Neu muon xoa volume Postgres local:

```powershell
docker compose down -v
```

## Test Scoring Flow nhanh

Dung endpoint POST /api/scoring/jobs voi body:

```json
{
  "requestId": "demo-swagger-001",
  "environmentKey": "LOBBY_CORRIDOR",
  "imageUrls": [
    "https://images.unsplash.com/photo-1489515217757-5fd1be406fef?auto=format&fit=crop&w=1200&q=80"
  ]
}
```

Sau do lay jobId va poll GET /api/scoring/jobs/{jobId} den khi status la SUCCEEDED hoac FAILED.
Moi result trong response se co them `visualizationBlobUrl` de frontend/mobile mo truc tiep anh detect.

## Environment Notes

Compose su dung fallback defaults, nhung ban nen set env khi chay staging/prod:

- BACKEND_DB_CONNECTION
- MESSAGE_BROKER_HOST, MESSAGE_BROKER_USERNAME, MESSAGE_BROKER_PASSWORD
- REDIS_CONNECTION_STRING
- SCORING_SERVICE_BASE_URL
- SCORING_RETRAIN_STORAGE_CONNECTION_STRING
- JWT_SECRET, JWT_ISSUER, JWT_AUDIENCE

### Remote Retrain (HTTP)

Neu muon worker backend trigger retrain qua scoring API (khong chay trainer local trong backend container), bat cac bien sau:

- SCORING_RETRAIN_REMOTE_ENABLED=true
- SCORING_RETRAIN_REMOTE_BASE_URL=http://host.docker.internal:8000 (hoac URL scoring API thuc te)
- SCORING_RETRAIN_REMOTE_API_KEY=<optional>
- SCORING_RETRAIN_USE_EXTERNAL_CANDIDATE=true
- SCORING_RETRAIN_EXTERNAL_CANDIDATE_PREFIX=scoring/external/latest

Tuy chon timeout/poll:

- SCORING_RETRAIN_REMOTE_TIMEOUT_SEC=7200
- SCORING_RETRAIN_REMOTE_POLL_INTERVAL_SEC=5

## Security Remediation Checklist

- Khong commit secret that su vao file compose, README, hoac code.
- Su dung file .env local (khong commit) hoac secret store (Key Vault/CI secret) de inject bien moi truong.
- Neu secret da lo trong lich su git, can rotate ngay va scrub history bang cong cu rewrite history.
- Doi password mac dinh RabbitMQ/Postgres/Redis khi len environment chia se.
- Kiem tra lai log startup de dam bao khong in connection string day du.

## Troubleshooting nhanh

- Job QUEUED lau: kiem tra cleanopsai-scoring-worker co Up khong.
- API loi ket noi scoring: kiem tra SCORING_SERVICE_BASE_URL va scoring docs health (/).
- Retrain khong chay: kiem tra SCORING_RETRAIN_* env. Neu dung remote trainer, can bat SCORING_RETRAIN_REMOTE_ENABLED=true va dam bao scoring API co endpoint /retrain/jobs.
