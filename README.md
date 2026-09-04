# 🏢 Portfolio Enterprise - C# .NET 8

Proyek portofolio enterprise yang mendemonstrasikan implementasi **Clean Architecture** dengan pendekatan **Domain-Driven Design (DDD)**, **CQRS**, dan **Repository Pattern**. Dibangun dengan ekosistem .NET 8 modern dan siap untuk production.

---

## 📑 Daftar Isi

- [Arsitektur](#-arsitektur)
- [Teknologi](#-teknologi)
- [Struktur Proyek](#-struktur-proyek)
- [Setup & Menjalankan Aplikasi](#-setup--menjalankan-aplikasi)
- [API Endpoints](#-api-endpoints)
- [Testing](#-testing)
- [Logging](#-logging)
- [Keamanan & Best Practices](#-keamanan--best-practices)
- [Coverage & Testing](#-coverage--testing)
- [Fitur Utama](#-fitur-utama)
- [Catatan Developer](#-catatan-developer)

---

## 🏗️ Arsitektur

Proyek mengikuti Clean Architecture dengan pemisahan layer yang jelas:

```
┌─────────────────────────────────────────────────────────┐
│                      WebAPI Layer                        │
│  (ASP.NET Core Controllers, Swagger, Serilog)           │
├─────────────────────────────────────────────────────────┤
│                  Application Layer                       │
│  (CQRS: Commands, Queries, Handlers, Validators)       │
│  (MediatR, FluentValidation, Result Pattern)           │
├─────────────────────────────────────────────────────────┤
│                Infrastructure Layer                      │
│  (EF Core, SQL Server, Repository Implementation)       │
├─────────────────────────────────────────────────────────┤
│                    Domain Layer                          │
│  (Entities, Value Objects, Repository Interfaces)       │
│  (Pure Business Logic, No External Dependencies)        │
└─────────────────────────────────────────────────────────┘
```

### Penjelasan Layer:

- **Domain Layer**: Inti bisnis aplikasi tanpa ketergantungan eksternal
- **Application Layer**: Orchestration, CQRS handlers, validasi
- **Infrastructure Layer**: Implementasi konkret (database, repository)
- **WebAPI Layer**: Presentation layer (controllers, endpoints)

---

## 🚀 Teknologi

| Komponen | Versi | Kegunaan |
|----------|-------|----------|
| **.NET** | 8 (LTS) | Runtime & Framework |
| **C#** | 12 | Language |
| **Entity Framework Core** | 8 | ORM & Database Access |
| **SQL Server** | Latest | Database |
| **MediatR** | Latest | CQRS Implementation |
| **FluentValidation** | Latest | Input Validation |
| **Serilog** | Latest | Structured Logging |
| **Swagger/OpenAPI** | Built-in | API Documentation |
| **xUnit** | Latest | Unit Testing Framework |
| **Moq** | Latest | Mocking Library |
| **FluentAssertions** | Latest | Assertion Library |

---

## 📦 Struktur Proyek

```
PortfolioEnterprise.sln
│
├── src/
│   ├── Domain/                          # Domain Layer
│   │   ├── Common/                      # Base classes (Entity, ValueObject)
│   │   ├── Entities/                    # Aggregate Roots (Customer)
│   │   ├── ValueObjects/                # Value Objects (Email, Address, Money)
│   │   └── Repositories/                # Repository Contracts
│   │
│   ├── Application/                     # Application Layer
│   │   ├── Common/                      # Shared utilities (Result Pattern)
│   │   ├── Customers/                   # Customer Commands/Queries
│   │   │   ├── Commands/
│   │   │   ├── Queries/
│   │   │   └── Handlers/
│   │   └── DependencyInjection.cs      # DI Registration
│   │
│   ├── Infrastructure/                  # Infrastructure Layer
│   │   ├── Data/                        # DbContext, Migrations, Repositories
│   │   │   ├── ApplicationDbContext.cs
│   │   │   ├── Migrations/
│   │   │   └── Repositories/
│   │   └── DependencyInjection.cs      # DI Registration
│   │
│   └── WebAPI/                          # Presentation Layer
│       ├── Controllers/                 # API Controllers
│       ├── Middleware/                  # Exception Handling Middleware
│       ├── Program.cs                   # Application Setup
│       ├── appsettings.json            # Configuration
│       └── appsettings.Development.json
│
└── tests/
    └── Core.Tests/                      # Unit Tests
        ├── Domain/                      # Tests for ValueObjects & Entities
        ├── Application/                 # Tests for Commands/Queries
        └── Infrastructure/              # Tests for Repository

```

---

## 🛠️ Setup & Menjalankan Aplikasi

### Prasyarat

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) atau SQL Server LocalDB
- [Git](https://git-scm.com/) (opsional)

### Langkah-langkah Instalasi

#### 1. Clone Repository

```bash
git clone https://github.com/yourusername/PortfolioEnterprise.git
cd PortfolioEnterprise
```

#### 2. Restore NuGet Packages

```bash
dotnet restore
```

#### 3. Konfigurasi Connection String

Edit file `src/WebAPI/appsettings.json` dan sesuaikan connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=PortfolioDb;Trusted_Connection=true;Encrypt=false;"
  }
}
```

Untuk **SQL Server LocalDB**:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=PortfolioDb;Trusted_Connection=true;"
  }
}
```

#### 4. Apply Database Migrations

```bash
dotnet ef database update --project src/Infrastructure --startup-project src/WebAPI
```

#### 5. Jalankan Aplikasi

```bash
dotnet run --project src/WebAPI
```

Aplikasi akan berjalan di `https://localhost:5001` (atau port yang dikonfigurasi).

#### 6. Akses Swagger UI

Buka browser dan navigasi ke: **https://localhost:5001/swagger**

---

## 🧪 API Endpoints

### Customer Management

| Method | Endpoint | Deskripsi |
|--------|----------|-----------|
| **POST** | `/api/customers` | Buat customer baru |
| **GET** | `/api/customers/{id}` | Ambil customer berdasarkan ID |
| **PUT** | `/api/customers/{id}` | Update data customer |
| **GET** | `/api/health` | Health check aplikasi |

### Contoh Request

#### POST /api/customers - Create Customer

```bash
curl -X POST "https://localhost:5001/api/customers" \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@example.com",
    "street": "123 Main St",
    "city": "New York",
    "state": "NY",
    "postalCode": "10001",
    "country": "USA"
  }'
```

#### Response Success (200 OK)

```json
{
  "isSuccess": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@example.com",
    "address": {
      "street": "123 Main St",
      "city": "New York",
      "state": "NY",
      "postalCode": "10001",
      "country": "USA"
    }
  }
}
```

#### Response Error (400 Bad Request)

```json
{
  "isSuccess": false,
  "errors": [
    "Email format is invalid",
    "First name is required"
  ]
}
```

---

## 🧪 Testing

### Menjalankan Unit Tests

```bash
dotnet test tests/Core.Tests
```

### Menjalankan Tests dengan Coverage Report

```bash
dotnet test tests/Core.Tests /p:CollectCoverage=true /p:CoverageFormat=opencover
```

### Struktur Test

Tests diorganisir mengikuti struktur domain:

- **Domain Tests**: Value Objects, Entities, Business Rules
- **Application Tests**: Commands, Queries, Handlers
- **Infrastructure Tests**: Repository, DbContext (dengan InMemory Database)

### Contoh Unit Test

```csharp
public class EmailValueObjectTests
{
    [Fact]
    public void Create_WithValidEmail_ShouldSucceed()
    {
        // Arrange & Act
        var email = Email.Create("test@example.com");

        // Assert
        email.Should().NotBeNull();
        email.Value.Should().Be("test@example.com");
    }

    [Fact]
    public void Create_WithInvalidEmail_ShouldFail()
    {
        // Arrange, Act & Assert
        Action act = () => Email.Create("invalid-email");
        act.Should().Throw<DomainException>();
    }
}
```

---

## 📊 Logging

Aplikasi menggunakan **Serilog** untuk structured logging:

### Konfigurasi Logging

**Development Environment**:
- Output ke Console dengan format yang readable
- Minimal level: Information

**Production Environment**:
- Output ke rolling file: `logs/app-YYYYMMDD.txt`
- Minimal level: Warning
- Automatic file rotation daily

### Contoh Log Output

```
2024-01-15 10:30:45.123 [INF] Application started successfully
2024-01-15 10:30:46.456 [INF] HTTP GET /api/customers/123 completed in 45ms
2024-01-15 10:30:47.789 [WRN] Validation failed for CreateCustomer command
2024-01-15 10:30:48.012 [ERR] Database connection failed
```

---

## 🔒 Keamanan & Best Practices

### 1. **Result Pattern**
Menghindari exception untuk kontrol flow normal, menggunakan Result object:

```csharp
public class Result
{
    public bool IsSuccess { get; set; }
    public List<string> Errors { get; set; }
    public object Data { get; set; }
}
```

### 2. **Global Exception Middleware**
Menangani semua exception yang tidak tertangani di satu tempat:

```csharp
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
```

### 3. **FluentValidation**
Validasi input ketat di Application Layer:

```csharp
public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(x => x.Email).EmailAddress().NotEmpty();
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
    }
}
```

### 4. **Value Objects**
Enkapsulasi aturan bisnis dan validasi:

```csharp
public class Email : ValueObject
{
    public string Value { get; private set; }

    public static Email Create(string value)
    {
        if (!IsValidEmail(value))
            throw new DomainException("Invalid email format");
        
        return new Email { Value = value };
    }
}
```

### 5. **Repository Pattern**
Abstraksi persistence layer:

```csharp
public interface ICustomerRepository
{
    Task<Customer> GetByIdAsync(Guid id);
    Task AddAsync(Customer customer);
    Task UpdateAsync(Customer customer);
    Task DeleteAsync(Guid id);
}
```

### 6. **Dependency Injection**
Decoupling dan improved testability:

```csharp
services.AddScoped<ICustomerRepository, CustomerRepository>();
services.AddMediatR(typeof(CreateCustomerCommand));
```

---

## 📈 Coverage & Testing

### Target Coverage

- **Value Objects**: 95%+
- **Entities**: 90%+
- **Commands/Queries**: 85%+
- **Repositories**: 80%+

### Tools

- **xUnit**: Testing Framework
- **Moq**: Object Mocking
- **FluentAssertions**: Readable Assertions
- **Coverlet**: Coverage Analysis

### Menjalankan Coverage Analysis

```bash
dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover /p:Exclude="[*.Tests]*"
```

---

## ✨ Fitur Utama

### Domain-Driven Design (DDD)
- Entities dengan business logic
- Value Objects dengan validasi
- Repository Interfaces di Domain Layer
- Aggregate Roots untuk consistency

### CQRS (Command Query Responsibility Segregation)
- Pemisahan Command (write) dan Query (read)
- MediatR untuk orchestration
- Focused, single-responsibility handlers

### Result Pattern
- Functional error handling
- Tidak menggunakan exception untuk flow kontrol
- Explicit error messages untuk client

### Repository Pattern
- Abstraksi data access
- Easy to mock untuk testing
- Flexible database implementation

### Structured Logging
- Serilog untuk structured logging
- Contextual information
- Production-ready log rotation

### Global Exception Handling
- Middleware terpusat
- Konsisten error response
- Security: tidak expose sensitive info

---

## 🧑‍💻 Catatan Developer

### Designing untuk Test

Proyek ini didesain dengan testability sebagai prinsip utama:

- Dependency Injection digunakan di semua layer
- Value Objects immutable dan mudah ditest
- Repository pattern memudahkan mocking
- CQRS handlers fokus pada single responsibility

### Menambah Fitur Baru

1. **Domain Layer**: Definisikan Entity/Value Object
2. **Application Layer**: Buat Command/Query dan Handler
3. **Infrastructure Layer**: Implementasikan Repository
4. **WebAPI Layer**: Buat Controller endpoint
5. **Tests**: Tulis unit tests untuk setiap layer

### Konvensi Coding

- PascalCase untuk class dan method names
- camelCase untuk variable names
- Implicit usings (C# 12)
- Primary constructors untuk dependency injection
- Records untuk immutable DTOs

---

## 📝 Lisensi

Proyek ini dibuat sebagai portfolio untuk mendemonstrasikan:

✅ Clean Code & SOLID Principles  
✅ Domain-Driven Design Implementation  
✅ CQRS & MediatR Pattern  
✅ Entity Framework Core & SQL Server  
✅ Unit Testing & Testable Design  
✅ Modern .NET Ecosystem Best Practices  

---

## 👨‍💼 Author

Dibangun oleh **Principal Engineer & .NET Solutions Architect** sebagai portofolio enterprise-grade.

---

## 📧 Support

Untuk pertanyaan atau issues, silakan buat issue di repository atau hubungi developer.

**Last Updated**: January 2024  
**.NET Version**: 8 (LTS)
