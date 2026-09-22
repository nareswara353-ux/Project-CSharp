# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Domain-Driven Design foundation with Entities, Value Objects (Email, Address, Money, ProductSku, Quantity, PhoneNumber, Percentage, Discount), and Aggregate Roots (Customer, Product, Order, User).
- CQRS implementation using MediatR for all business operations.
- Repository Pattern with generic `IRepository<T>`, `IOrderRepository`, `IUserRepository`.
- Specification Pattern for reusable query filters.
- Result Pattern for functional error handling.
- FluentValidation for all command/query validation.
- EF Core 8 with SQL Server, configurations split per entity.
- JWT Authentication with Bearer token, role-based authorization.
- BCrypt password hashing with work factor 12.
- Domain Events infrastructure (dispatcher, handlers, notifications).
- Email service abstraction with SMTP implementation and template renderer.
- Caching layer with InMemory, Redis, and NoOp providers, plus `CachingBehavior`.
- Background jobs (OrderCleanup, DailyReport) with hosted scheduler.
- MediatR pipeline behaviors: Validation, Logging, Performance, UnhandledException, Caching, Transaction, Metrics, Tracing.
- OpenTelemetry integration for distributed tracing and metrics (OTLP exporter).
- Global Exception Middleware with structured error responses.
- Correlation ID Middleware for distributed tracing.
- Request Logging Middleware for HTTP observability.
- Serilog structured logging with Console and File sinks.
- Swagger/OpenAPI with Bearer security definition.
- Rate Limiting (Fixed Window, 100 req/min per user/IP).
- Health Checks (Database + Disk Space).
- Docker multi-stage build with non-root user.
- Docker Compose with SQL Server 2022, Redis 7.4, and WebAPI.
- GitHub Actions CI pipeline (restore, build, test, publish).
- Unit tests for Domain, Application, and Integration layers using xUnit, Moq, and FluentAssertions.
- Central Package Management (CPM) with `Directory.Packages.props`.
- EditorConfig with C# code style rules.

### Security

- JWT secret validation (minimum 32 characters).
- Password hashing with BCrypt and Work Factor 12.
- Non-root user in Docker runtime container.
- Environment variables for secrets in production.

## [1.0.0] - Initial Release

First public release of the Portfolio Enterprise API.
