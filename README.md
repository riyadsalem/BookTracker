# 📚 Book Tracker

A full-stack library management system — **.NET 8 API + React/TypeScript frontend** — built to demonstrate production-grade backend architecture, authentication/authorization design, and modern frontend patterns.

> Started as a simple CRUD API and evolved step by step into a fully authenticated, role-based application with JWT auth, optimistic concurrency, PostgreSQL, containerized deployment, and a React frontend — each capability added deliberately, with tests locking in the behavior.

## ✨ What this project demonstrates

| Area | What's implemented |
|---|---|
| **Clean Architecture** | Domain / Application / Storage / Endpoints, cleanly separated |
| **Domain-Driven Design** | Value Objects (`BookTitle`, `MemberEmail`, `PublicationYear`...) make invalid state unrepresentable |
| **Authentication** | JWT-based login, password hashing (`IPasswordHasher<T>`), no plain-text passwords, ever |
| **Authorization** | Role-based access (`Member` / `Administrator`), enforced in the **domain layer** — not scattered across routes |
| **Concurrency control** | Optimistic locking on book edits — `409 Conflict` instead of silently losing someone's changes |
| **Database migrations** | EF Core migrations against **PostgreSQL** (not `EnsureCreated`) — explicit, reviewable schema changes |
| **Centralized error handling** | One `ExceptionHandlingMiddleware` translates domain exceptions to consistent HTTP responses — no `try/catch` duplicated across endpoints, and unexpected errors never leak internal details to the client |
| **Security-mindedness** | SQL `LIKE`/`ILIKE` wildcard injection prevented, NUL-byte edge cases rejected at the domain boundary, CORS locked to a known origin |
| **Testing** | Unit tests for domain rules, a full integration test suite against a **real PostgreSQL container** (Testcontainers), split into a fast Docker-free suite and a slower infrastructure suite |
| **Modern frontend** | React 19, TypeScript, React Router, TanStack Query — optimistic UI, cache invalidation, route guards |
| **Containerized deployment** | Multi-stage Docker images for API, frontend (Nginx-served) and PostgreSQL, orchestrated with Docker Compose, health checks, and a persistent database volume |
| **CI/CD** | GitHub Actions with separate fast-test and integration-test jobs running on every push |

## 🏗️ Architecture

```
HTTP Request
     │
     ▼
Exception Middleware  →  JWT Authentication  →  ClaimsPrincipal  →  Actor (plain domain object)
                                                                          │
                                                                          ▼
                                                Handler  →  BookPermissions / MemberPermissions
                                                                          │
                                                                          ▼
                                                    Repository  →  EF Core  →  PostgreSQL
```

Authorization rules live as plain, unit-testable domain functions — not `[Authorize]` attributes scattered across controllers. The same rule applies whether the action is triggered by an HTTP request, a background job, or a script. A single middleware sits at the front of the pipeline and turns any domain exception into the right HTTP status code, so endpoints stay focused on their own request/response logic.

## 🧰 Tech Stack

**Backend:** .NET 8 · ASP.NET Core Minimal APIs · Entity Framework Core · PostgreSQL (Npgsql) · JWT Bearer Auth · xUnit · Testcontainers

**Frontend:** React 19 · TypeScript · Vite · React Router · TanStack Query

**Tooling:** GitHub Actions · EF Core Migrations · ESLint · Docker · Docker Compose · Nginx

## 🚀 Quick Start

```bash
# Backend
dotnet restore
dotnet user-secrets set "Jwt:SigningKey" "a-long-random-dev-key" --project BookTracker.Api
dotnet user-secrets set "DevelopmentAdmin:Password" "dev-admin-password" --project BookTracker.Api
dotnet user-secrets set "ConnectionStrings:BookTracker" "Host=localhost;Port=5432;Database=booktracker;Username=booktracker;Password=your-local-password" --project BookTracker.Api
dotnet run --project BookTracker.Api

# Frontend (separate terminal)
cd Frontend && npm install && npm run dev
```

```bash
# Fast tests - no Docker required
dotnet test BookTracker.Api.Tests/BookTracker.Api.Tests.csproj

# Integration tests - spins up a temporary PostgreSQL container, Docker required
dotnet test BookTracker.Api.IntegrationTests/BookTracker.Api.IntegrationTests.csproj
```

### 🐳 Or run everything with Docker Compose

```bash
cp .env.example .env
# fill in JWT_SIGNING_KEY, DEVELOPMENT_ADMIN_PASSWORD and POSTGRES_PASSWORD
docker compose up --build
```
API on `http://localhost:8080`, frontend on `http://localhost:3000`, PostgreSQL on its own service with a health check gating API startup. Data persists in a named Docker volume across container restarts.

## 📡 API at a glance

| | Public | Member | Administrator |
|---|:---:|:---:|:---:|
| Browse books | ✅ | ✅ | ✅ |
| Create / edit / delete books | | | ✅ |
| Register account | ✅ | | |
| View / edit own account | | ✅ | ✅ |
| Manage other members | | | ✅ |

Full schemas via Swagger; sample requests in `BookTracker.Api/BookTracker.Api.http`.

## 🧪 Testing philosophy

Every bug fixed along the way (a SQL wildcard-injection issue, a `NullReferenceException` on `null` input, a lost-update race condition, a PostgreSQL-only NUL-byte rejection) shipped with a **regression test** — so the same class of bug can't silently come back.

Tests are split by infrastructure need rather than by name: fast domain and middleware tests run in milliseconds with zero external dependencies, while endpoint and migration tests run against a real, disposable PostgreSQL container via Testcontainers — giving a short local feedback loop without trading away confidence that the code actually works against the production database engine.

---

*Built as a hands-on exercise in production backend engineering: architecture decisions, security tradeoffs, and testing discipline over raw feature count.*