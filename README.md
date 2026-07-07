# BookTracker — v1 (API)

BookTracker is a library management system. It is a project designed with a proper backend foundation from the ground up, following clean architecture and solid engineering practices. This first version (v1) focuses on the API layer: a complete, production style backend for managing a book catalog and its members (create, update, delete, list), built with **.NET 8 Minimal APIs**. Future versions may build on top of this foundation with additional capabilities.

## Features

**Clean layered architecture**: clear separation between `Domain` (domain logic), `Application` (Command/Query handlers), `Storage` (EF Core repository), and `Endpoints` (REST API), applied consistently to both `Books` and `Members`.

**Value Objects for validation**: types like `BookTitle`, `AuthorName`, `MemberName` and `MemberEmail` guarantee that incoming data is always valid, with clear error messages via `DomainException`.

**Real persistence**: EF Core with SQLite, instead of in-memory storage.

**Pagination & search**: built-in support for paging through results and searching by title/author (Books) or name/email (Members). Search input is safely escaped, so special SQL `LIKE` characters (`%`, `_`) are always treated as literal text, never as wildcards.

**Automatic seed data**: the database is populated with realistic fake data on startup in the development environment.

**Full test coverage**: unit tests for the Value Objects, and integration tests covering every API endpoint for both Books and Members, including regression tests that lock in bug fixes (such as the search wildcard escaping fix) so they can't silently reappear.

**Continuous Integration**: via GitHub Actions, automatically running `dotnet restore` and `dotnet test` on every push and pull request.

**Interactive API docs**: via Swagger/OpenAPI (Swashbuckle).

## API (v1)

A clean, resource oriented API for the `books` catalog and its `members`, with predictable responses and validated input.

Full request/response details are available through the built-in Swagger/OpenAPI documentation once the project is running, and sample requests for every endpoint (including edge cases) can be found in `BookTracker.Api/BookTracker.Api.http`.

## Tech Stack

.NET 8

ASP.NET Core Minimal APIs

Entity Framework Core + SQLite

Swashbuckle (OpenAPI/Swagger)

xUnit for testing (unit and integration tests)

GitHub Actions for CI

## Getting Started

```bash
git clone <repo-url>
cd BookTracker
dotnet restore
dotnet run --project BookTracker.Api
```

## Running Tests

```bash
dotnet test
```

The test suite includes both unit tests (Value Objects) and integration tests (full HTTP request/response cycle against an in-memory SQLite database) for every endpoint, for Books and Members alike.