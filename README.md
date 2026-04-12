# CleanOpsAi Backend

Backend .NET cho he thong CleanOpsAI, cung cap API submit/poll scoring job va worker xu ly bat dong bo qua RabbitMQ.

## Prerequisites

- Docker Desktop
- Git
- (Tuy chon) .NET SDK 8.0 neu muon chay local khong dong goi container

## Cau truc repo lien quan

Huong dan Docker mac dinh trong repo nay yeu cau repo AI nam canh nhau:

```
folder/
	cleanopsai-backend/
	cleaning_ai_poc/
```

`docker-compose.yml` cua backend build service scoring tu `../cleaning_ai_poc`.

## Chay nhanh bang Docker (khuyen dung)

### 1. Khoi dong toan bo stack

```powershell
cd e:\capstone\folder\cleanopsai-backend
docker compose up -d --build
```

### 2. Kiem tra service da len

```powershell
docker compose ps
```

Service chinh:

- Backend API: http://localhost:5000
- Swagger backend: http://localhost:5000/swagger
- Scoring API (FastAPI): http://localhost:8000/docs
- RabbitMQ UI: http://localhost:15672 (guest/guest)

### 3. Test nhanh qua Swagger backend

Dung endpoint `POST /api/scoring/jobs` voi request body:

```json
{
	"requestId": "demo-swagger-001",
	"environmentKey": "LOBBY_CORRIDOR",
	"imageUrls": [
		"https://images.unsplash.com/photo-1489515217757-5fd1be406fef?auto=format&fit=crop&w=1200&q=80"
	]
}
```

Sau do lay `jobId` tra ve va goi `GET /api/scoring/jobs/{jobId}` den khi status la `SUCCEEDED` hoac `FAILED`.

### 4. Tat stack

```powershell
docker compose down
```

Neu muon xoa volume Postgres (xoa du lieu local):

```powershell
docker compose down -v
```

## Chay backend rieng (khong build scoring cung compose)

File compose rieng: `docker-compose.backend-only.yml`

Kich ban nay chi chay:

- postgres
- rabbitmq
- redis
- cleanopsai-api
- cleanopsai-scoring-worker

Scoring API duoc goi qua URL ben ngoai stack qua bien moi truong `SCORING_SERVICE_BASE_URL`.

Mac dinh:

- `http://host.docker.internal:8000`

Lenh chay:

```powershell
cd e:\capstone\server-side\cleanops-backend
docker compose -f docker-compose.backend-only.yml up -d --build
docker compose -f docker-compose.backend-only.yml ps
```

Tat stack:

```powershell
docker compose -f docker-compose.backend-only.yml down
```

## Chay local khong dong goi API/Worker (tuy chon)

Ban co the giu infra bang Docker va chay process .NET tren may:

### 1. Bat infra + scoring service

```powershell
cd e:\capstone\folder\cleanopsai-backend
docker compose up -d postgres rabbitmq redis cleaning-ai-scoring
```

### 2. Chay backend API

```powershell
cd e:\capstone\folder\cleanopsai-backend\src\Api\CleanOpsAi.Api
dotnet run
```

### 3. Chay scoring worker

```powershell
cd e:\capstone\folder\cleanopsai-backend\src\Workers\CleanOpsAi.Scoring.Worker
dotnet run
```

## Troubleshooting nhanh

- Neu job bi `QUEUED` lau: kiem tra container `cleanopsai-scoring-worker` co `Up` khong.
- Neu scoring API bao khong tim thay model: kiem tra service `cleaning-ai-scoring` va mount `../cleaning_ai_poc/outputs:/app/outputs:ro`.
- Neu endpoint backend loi ket noi scoring: kiem tra `ScoringService__BaseUrl=http://cleaning-ai-scoring:8000`.