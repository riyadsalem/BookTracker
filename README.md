# 📚 Book Tracker

A full-stack library management system — **.NET 8 API + React/TypeScript frontend** — built to demonstrate production-grade backend architecture, authentication/authorization design, and modern frontend patterns.

> Started as a simple CRUD API and evolved step by step into a fully authenticated, role-based application with JWT auth, optimistic concurrency, EF Core migrations, and a React frontend — each capability added deliberately, with tests locking in the behavior.

## ✨ What this project demonstrates

| Area | What's implemented |
|---|---|
| **Clean Architecture** | Domain / Application / Storage / Endpoints, cleanly separated |
| **Domain-Driven Design** | Value Objects (`BookTitle`, `MemberEmail`, `PublicationYear`...) make invalid state unrepresentable |
| **Authentication** | JWT-based login, password hashing (`IPasswordHasher<T>`), no plain-text passwords, ever |
| **Authorization** | Role-based access (`Member` / `Administrator`), enforced in the **domain layer** — not scattered across routes |
| **Concurrency control** | Optimistic locking on book edits — `409 Conflict` instead of silently losing someone's changes |
| **Database migrations** | EF Core migrations (not `EnsureCreated`) — explicit, reviewable schema changes |
| **Security-mindedness** | SQL `LIKE` wildcard injection prevented, NUL-byte edge cases handled, CORS locked to a known origin |
| **Testing** | Unit tests for domain rules + full integration test suite (success, validation, `401`/`403`/`404`/`409` paths) |
| **Modern frontend** | React 19, TypeScript, React Router, TanStack Query — optimistic UI, cache invalidation, route guards |
| **CI/CD** | GitHub Actions running the full test suite on every push |

## 🏗️ Architecture

```
HTTP Request
     │
     ▼
JWT Authentication  →  ClaimsPrincipal  →  Actor (plain domain object)
                                                │
                                                ▼
                              Handler  →  BookPermissions / MemberPermissions
                                                │
                                                ▼
                                    Repository  →  EF Core  →  SQLite
```

Authorization rules live as plain, unit-testable domain functions — not `[Authorize]` attributes scattered across controllers. The same rule applies whether the action is triggered by an HTTP request, a background job, or a script.

## 🧰 Tech Stack

**Backend:** .NET 8 · ASP.NET Core Minimal APIs · Entity Framework Core · SQLite · JWT Bearer Auth · xUnit

**Frontend:** React 19 · TypeScript · Vite · React Router · TanStack Query

**Tooling:** GitHub Actions · EF Core Migrations · ESLint

## 🚀 Quick Start

```bash
# Backend
dotnet restore
dotnet user-secrets set "Jwt:SigningKey" "a-long-random-dev-key" --project BookTracker.Api
dotnet user-secrets set "DevelopmentAdmin:Password" "dev-admin-password" --project BookTracker.Api
dotnet run --project BookTracker.Api

# Frontend (separate terminal)
cd Frontend && npm install && npm run dev
```

```bash
# Run the tests
dotnet test
```

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

Every bug fixed along the way (a SQL wildcard-injection issue, a `NullReferenceException` on `null` input, a lost-update race condition) shipped with a **regression test** — so the same class of bug can't silently come back.

---

*Built as a hands-on exercise in production backend engineering: architecture decisions, security tradeoffs, and testing discipline over raw feature count.*