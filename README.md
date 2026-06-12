# WedNest API

A backend service for a Wedding Registry platform built with **ASP.NET Core (.NET 10)** and **PostgreSQL**.

This API handles registry management, guest interactions, and secure payments via **Stripe Checkout + Webhooks**.

---

## Tech Stack

- ASP.NET Core 10 (Web API)
- PostgreSQL 18
- Entity Framework Core
- JWT Authentication (Access + Refresh Tokens)
- Stripe Payments
- Clean Architecture
- Swagger / OpenAPI
- Serilog Logging
- DotNetEnv (.env configuration)

---

## Prerequisites

1. **.NET 10 SDK** - https://dotnet.microsoft.com/download
2. **PostgreSQL 18** - https://www.postgresql.org/download/
3. **Stripe Account** (for payments) - https://stripe.com

---

## Full Setup Walkthrough

### 1. Clone & Open Project

```bash
git clone <repo-url>
cd wednest-api
```

### 2. PostgreSQL Database Setup

During PostgreSQL installation you set a password. The database `wednest_db` has already been created.

If you need to recreate it:

```bash
# Windows (adjust psql path if needed)
& "C:\Program Files\PostgreSQL\18\bin\psql.exe" -U postgres

# Inside psql:
CREATE DATABASE wednest_db;
\q
```

### 3. Configure Environment Variables

Edit the `.env` file in the project root with your actual values:

```env
# Database
DB_HOST=localhost
DB_PORT=5432
DB_NAME=wednest_db
DB_USER=postgres
DB_PASSWORD=your_postgres_password

# JWT (change the secret to a real strong random string!)
JWT_SECRET=your-super-secret-key-at-least-32-chars!!
JWT_ISSUER=WedNest
JWT_AUDIENCE=WedNest
JWT_ACCESS_EXPIRY_MINUTES=15
JWT_REFRESH_EXPIRY_DAYS=7

# Stripe (get from https://dashboard.stripe.com/apikeys)
STRIPE_SECRET_KEY=sk_test_...
STRIPE_PUBLISHABLE_KEY=pk_test_...
STRIPE_WEBHOOK_SECRET=whsec_...

# App
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=https://localhost:5001;http://localhost:5000
```

### 4. Restore & Build

```bash
dotnet restore
dotnet build
```

### 5. Run Database Migrations

```bash
cd src/API
dotnet ef migrations add InitialCreate --project ../Infrastructure --startup-project .
dotnet ef database update --project ../Infrastructure --startup-project .
```

### 6. Run the API

```bash
cd src/API
dotnet run
```

The API will start at:
- **Swagger UI**: https://localhost:5001/swagger
- **HTTP**: http://localhost:5000

---

## Project Structure

```
wednest-api/
  .env                          # Environment variables (secrets)
  WedNest.sln                   # Solution file
  src/
    API/                        # Web API layer (controllers, Program.cs)
    Application/                # Business logic, DTOs, interfaces
    Domain/                     # Entities, domain models
    Infrastructure/             # EF Core, Stripe, JWT, external services
```

---

## Architecture (Clean Architecture)

```
API --> Application --> Domain
API --> Infrastructure --> Domain
```

- **Domain**: Core entities and business rules (no dependencies)
- **Application**: Service interfaces, DTOs, business logic
- **Infrastructure**: EF Core DbContext, Stripe service, JWT token service
- **API**: Controllers, middleware, program configuration

---

## Payment Flow (Stripe)

1. Guest selects a gift or cash fund
2. API creates Stripe Checkout Session
3. Stripe redirects user to secure payment page
4. Stripe sends webhook after payment
5. Backend confirms payment
6. Gift is marked as purchased

---

## Key Files

| File | Purpose |
|------|---------|
| `src/API/Program.cs` | App entry point, service configuration |
| `src/API/appsettings.json` | Config with env variable placeholders |
| `.env` | All secrets and environment variables |
| `src/Infrastructure/Data/ApplicationDbContext.cs` | EF Core database context |
| `src/Domain/Entities/BaseEntity.cs` | Base entity with Id, CreatedAt, UpdatedAt |

---

## Adding a Migration

```bash
cd src/API
dotnet ef migrations add <MigrationName> --project ../Infrastructure --startup-project .
dotnet ef database update --project ../Infrastructure --startup-project .
```

---

## Environment Variables Reference

| Variable | Description | Example |
|----------|-------------|---------|
| `DB_HOST` | PostgreSQL host | `localhost` |
| `DB_PORT` | PostgreSQL port | `5432` |
| `DB_NAME` | Database name | `wednest_db` |
| `DB_USER` | Database user | `postgres` |
| `DB_PASSWORD` | Database password | `your_password` |
| `JWT_SECRET` | JWT signing key (32+ chars) | `your-secret-key` |
| `JWT_ISSUER` | JWT issuer | `WedNest` |
| `JWT_AUDIENCE` | JWT audience | `WedNest` |
| `JWT_ACCESS_EXPIRY_MINUTES` | Access token lifetime | `15` |
| `JWT_REFRESH_EXPIRY_DAYS` | Refresh token lifetime | `7` |
| `STRIPE_SECRET_KEY` | Stripe secret API key | `sk_test_...` |
| `STRIPE_PUBLISHABLE_KEY` | Stripe publishable key | `pk_test_...` |
| `STRIPE_WEBHOOK_SECRET` | Stripe webhook secret | `whsec_...` |
