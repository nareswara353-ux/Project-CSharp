# Portfolio Enterprise — C# .NET 8

Enterprise-grade API built with **Clean Architecture**, **Domain-Driven Design (DDD)**, **CQRS**, and **modern .NET 8**. Production-ready with Docker, JWT auth, observability, and comprehensive tests.

[![CI](https://github.com/your-org/Project-CSharp/actions/workflows/ci.yml/badge.svg)](https://github.com/your-org/Project-CSharp/actions/workflows/ci.yml)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-blue)](LICENSE)

---

## 🏗️ Architecture
┌─────────────────────────────────────────────────┐
│ WebAPI │
│ Controllers, Middleware, Swagger, Serilog │
├─────────────────────────────────────────────────┤
│ Application Layer │
│ CQRS (MediatR), Behaviors, DTOs, Validators │
├─────────────────────────────────────────────────┤
│ Infrastructure Layer │
│ EF Core, Redis, SMTP, JWT, Jobs, Caching │
├─────────────────────────────────────────────────┤
│ Domain Layer │
│ Entities, Value Objects, Events, Specs │
└─────────────────────────────────────────────────┘

text

**Dependency Rule:** Dependencies always point inward. Domain has zero external dependencies.

---

## 🚀 Tech Stack

| Category | Technology |
|----------|-----------|
| Runtime | .NET 8 (LTS), C# 12 |
| Persistence | EF Core 8, SQL Server 2022 |
| Messaging | MediatR (CQRS) |
| Validation | FluentValidation |
| Auth | JWT Bearer, BCrypt |
| Caching | InMemory, Redis (StackExchange) |
| Logging | Serilog (Console + File) |
| Observability | OpenTelemetry (Traces, Metrics, OTLP) |
| API Docs | Swagger / OpenAPI |
| Testing | xUnit, Moq, FluentAssertions, InMemory DB |
| Container | Docker, Docker Compose |
| CI/CD | GitHub Actions |

---

## 📦 Project Structure
PortfolioEnterprise.sln
├── src/
│ ├── Domain/ # Entities, Value Objects, Events, Specs
│ ├── Application/ # CQRS, Behaviors, DTOs, Validators
│ ├── Infrastructure/ # EF Core, Repositories, Email, Jobs, Cache
│ └── WebAPI/ # Controllers, Middleware, Program.cs
├── tests/
│ └── Core.Tests/ # Unit + Integration tests
├── .github/workflows/ # CI pipeline
├── Dockerfile # Multi-stage, non-root
└── docker-compose.yml # WebAPI + SQL Server + Redis

text

---

## ⚡ Quick Start

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server LocalDB (or SQL Server 2022)
- (Optional) Docker Desktop

### Local Development

```bash
# 1. Clone
git clone https://github.com/your-org/Project-CSharp.git
cd Project-CSharp

# 2. Restore
dotnet restore

# 3. Apply migrations
dotnet ef database update --project src/Infrastructure --startup-project src/WebAPI

# 4. Run
dotnet run --project src/WebAPI
Swagger UI: https://localhost:5001/swagger

Docker Deployment
bash
# 1. Copy env template
cp .env.example .env

# 2. Edit .env (set SQL_SA_PASSWORD and JWT_SECRET)

# 3. Run full stack
docker compose up -d

# 4. Check health
curl http://localhost:8080/health
🧪 Testing
bash
# All tests
dotnet test

# With coverage
dotnet test --collect:"XPlat Code Coverage"

# Specific project
dotnet test tests/Core.Tests
Test coverage includes:

Domain: Value Objects, Entities, Specifications

Application: Command/Query handlers, Pipeline behaviors

Infrastructure: Repositories (InMemory)

Integration: End-to-end HTTP tests with WebApplicationFactory<Program>

🔌 API Endpoints
Authentication
Method	Endpoint	Description	Auth
POST	/api/auth/register	Register new user	No
POST	/api/auth/login	Login and get JWT	No
Customers
Method	Endpoint	Description	Auth
GET	/api/customers	Paged list with filters	Yes
GET	/api/customers/{id}	Get by ID	Yes
POST	/api/customers	Create	Yes
PUT	/api/customers/{id}	Update	Yes
DELETE	/api/customers/{id}	Soft delete	Yes
Products
Method	Endpoint	Description	Auth
GET	/api/products	Paged list	Yes
GET	/api/products/{id}	Get by ID	Yes
POST	/api/products	Create	Yes
PUT	/api/products/{id}	Update	Yes
Orders
Method	Endpoint	Description	Auth
GET	/api/orders/{id}	Get with lines	Yes
POST	/api/orders	Create order	Yes
POST	/api/orders/{id}/lines	Add line	Yes
POST	/api/orders/{id}/confirm	Confirm	Yes
POST	/api/orders/{id}/ship	Ship	Yes
POST	/api/orders/{id}/cancel	Cancel	Yes
Users
Method	Endpoint	Description	Auth
GET	/api/users/me	Current user profile	Yes
GET	/api/users/{id}	Get user (Admin/Manager)	Yes
POST	/api/users/me/change-password	Change password	Yes
PUT	/api/users/{id}/role	Update role (Admin)	Yes
Health
Method	Endpoint	Description
GET	/health	Health check (DB + Disk)
Example: Register
bash
curl -X POST http://localhost:8080/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "johndoe",
    "email": "john@example.com",
    "password": "Password123"
  }'
Example: Authenticated Request
bash
TOKEN="eyJhbGciOi..."

curl http://localhost:8080/api/customers \
  -H "Authorization: Bearer $TOKEN"
📊 Observability
Logging (Serilog)
Console — development

File — logs/app-YYYYMMDD.txt (rolling daily)

Distributed Tracing (OpenTelemetry)
Enable in appsettings.json:

json
{
  "Observability": {
    "Enabled": true,
    "ServiceName": "PortfolioEnterpriseAPI",
    "ServiceVersion": "1.0.0",
    "OtlpEndpoint": "http://localhost:4317",
    "EnableConsoleExporter": false
  }
}
Compatible with Jaeger, Tempo, Honeycomb, Datadog, etc.

Metrics
app.commands.processed — counter

app.queries.processed — counter

app.commands.failed — counter

app.request.duration — histogram (ms)

app.orders.created — counter

app.customers.created — counter

🔐 Security
JWT with HMAC-SHA256 signing (secret ≥ 32 chars enforced)

BCrypt password hashing (work factor 12)

Role-based authorization (Admin, Manager, User)

Rate limiting (100 req/min per user/IP)

Global exception handling with sanitized error responses

Non-root Docker user

Secrets via environment variables

🔧 Configuration
Section	Purpose
ConnectionStrings:DefaultConnection	SQL Server connection
Jwt	Secret, Issuer, Audience, ExpirationMinutes
Email	SMTP settings (Enabled, Host, Port, etc)
Caching	Provider (None / InMemory / Redis)
Observability	OpenTelemetry configuration
Serilog	Logging levels and sinks
🎯 Design Decisions
Pattern	Where	Why
Result Pattern	Application handlers	Explicit error handling, no exceptions for flow
CQRS	Application	Separates read/write models
Specification	Domain	Reusable query logic, testable
Repository	Infrastructure	Abstracts persistence
Unit of Work	Infrastructure	Transaction consistency
Domain Events	Domain → Application	Loose coupling between aggregates
Value Objects	Domain	Encapsulated invariants (Email, Money, SKU)
Options Pattern	Config	Strongly-typed configuration
Pipeline Behaviors	MediatR	Cross-cutting concerns
📈 Roadmap
□ Email templates stored in DB with admin editor
□ Event sourcing for Order aggregate
□ GraphQL API layer
□ Kubernetes deployment manifests
□ Load testing with k6
□ Multi-tenancy support
📄 License
MIT License — see LICENSE file for details.

👤 Author
Built as an enterprise portfolio project demonstrating:

Clean Architecture & SOLID principles

Domain-Driven Design (Entities, Value Objects, Aggregates, Domain Events)

CQRS with MediatR & pipeline behaviors

Modern .NET 8 features (records, primary constructors, file-scoped namespaces)

Testable design (unit + integration tests)

Production-ready deployment (Docker, health checks, observability)