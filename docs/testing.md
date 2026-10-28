
# Testing Guide

Panduan menjalankan test di Portfolio Enterprise.

## Prerequisites

| Tool | Version | Notes |
|------|---------|-------|
| .NET SDK | 8.0.x | Wajib untuk runtime |
| Docker Desktop | Latest | Hanya untuk integration tests |
| `dotnet-ef` | 8.x | Hanya untuk migration |

## Unit Tests

**Tidak butuh Docker.** Jalan dalam ~1 detik. Ini yang dipakai di CI default.

```bash
dotnet test tests/Core.Tests/Core.Tests.csproj --filter 'FullyQualifiedName!~Integration'
Coverage
bash
dotnet test tests/Core.Tests/Core.Tests.csproj \
  --filter 'FullyQualifiedName!~Integration' \
  --collect:"XPlat Code Coverage"
Hasil coverage ada di tests/Core.Tests/TestResults/.

Integration Tests
Butuh Docker running (untuk SQL Server + Redis via TestContainers). Lebih lambat (~4 menit).

bash
dotnet test tests/Core.Tests/Core.Tests.csproj --filter 'FullyQualifiedName~Integration'
Kategori Integration Test
Folder	Yang Diuji
tests/Core.Tests/Integration/*.cs	Endpoint tests via WebApplicationFactory<Program> dengan InMemory DB
tests/Core.Tests/Integration/TestContainers/*.cs	Endpoint tests dengan SQL Server + Redis nyata via TestContainers
Menjalankan di CI
Workflow	Trigger	Test
ci.yml	Setiap push/PR	Unit tests only
integration-tests.yml	Manual + Senin 02:00	Integration tests
Untuk trigger manual integration tests:

Buka GitHub → Actions tab

Pilih "Integration Tests" workflow

Klik "Run workflow"

Migration
Setelah entity/config berubah, generate migration:

bash
dotnet ef migrations add <MigrationName> \
  --project src/Infrastructure \
  --startup-project src/WebAPI
Apply ke database lokal:

bash
dotnet ef database update \
  --project src/Infrastructure \
  --startup-project src/WebAPI
Troubleshooting
FullyQualifiedName!~Integration → zsh: event not found
Ini karena zsh interpret ! sebagai history expansion. Selalu pakai single quotes:

bash
dotnet test ... --filter 'FullyQualifiedName!~Integration'   # ✅
dotnet test ... --filter "FullyQualifiedName!~Integration"   # ❌
dotnet-ef: command not found
bash
dotnet tool install --global dotnet-ef --version "8.*"
echo 'export PATH="$PATH:$HOME/.dotnet/tools"' >> ~/.zshrc
source ~/.zshrc
Integration tests timeout
Pastikan Docker Desktop berjalan dan punya memori minimum 4 GB. TestContainers akan pull image SQL Server 2022 (~1.5 GB) pada run pertama.