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

## Database Schema

### ER Diagram

```
Users ──────────┐
  │              │
  ├─── Weddings ─┤
  │    (Partner1)│
  │    (Partner2)│
  │              │
  ├── GiftItems ─┘
  ├── CashFunds ─┘
  │
  └── Orders ──── OrderItems ──── GiftItems
       │
       ├── CashFunds
       │
       └── Payments
```

### Tables

#### Users

| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| Id | uuid | NO | Primary key |
| Email | varchar(256) | NO | Unique email address |
| PasswordHash | varchar(512) | NO | Bcrypt password hash |
| FirstName | varchar(100) | NO | First name |
| LastName | varchar(100) | NO | Last name |
| PhoneNumber | text | YES | Phone number |
| Role | int | NO | 0=Couple, 1=Guest |
| RefreshToken | text | YES | JWT refresh token |
| RefreshTokenExpiry | timestamptz | YES | Refresh token expiry |
| CreatedAt | timestamptz | NO | Record created time |
| UpdatedAt | timestamptz | YES | Last update time |

**Indexes:** `IX_Users_Email` (unique)

---

#### Weddings

| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| Id | uuid | NO | Primary key |
| Title | varchar(200) | NO | Wedding title |
| WeddingDate | timestamptz | NO | Date of the wedding |
| Venue | varchar(300) | YES | Venue name/address |
| Description | text | YES | Wedding description |
| CoverImageUrl | text | YES | Cover photo URL |
| Slug | varchar(200) | NO | URL-friendly identifier (unique) |
| Status | int | NO | 0=Draft, 1=Active, 2=Completed, 3=Cancelled |
| IsPublic | bool | NO | Whether registry is publicly visible |
| Partner1Id | uuid | NO | FK to Users (first partner) |
| Partner2Id | uuid | NO | FK to Users (second partner) |
| CreatedAt | timestamptz | NO | Record created time |
| UpdatedAt | timestamptz | YES | Last update time |

**Indexes:** `IX_Weddings_Slug` (unique), `IX_Weddings_Partner1Id`, `IX_Weddings_Partner2Id`

**Foreign Keys:** `Partner1Id -> Users(Id)`, `Partner2Id -> Users(Id)` (both RESTRICT on delete)

---

#### GiftItems

| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| Id | uuid | NO | Primary key |
| Name | varchar(200) | NO | Gift name |
| Description | text | YES | Gift description |
| Price | decimal(18,2) | NO | Unit price |
| ImageUrl | text | YES | Product image URL |
| StoreUrl | varchar(500) | YES | Link to buy from store |
| Status | int | NO | 0=Available, 1=Reserved, 2=Purchased |
| Quantity | int | NO | How many needed |
| QuantityPurchased | int | NO | How many bought so far |
| DisplayOrder | int | NO | Sort order on registry page |
| WeddingId | uuid | NO | FK to Weddings |
| CreatedAt | timestamptz | NO | Record created time |
| UpdatedAt | timestamptz | YES | Last update time |

**Indexes:** `IX_GiftItems_WeddingId`

**Foreign Keys:** `WeddingId -> Weddings(Id)` (CASCADE on delete)

---

#### CashFunds

| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| Id | uuid | NO | Primary key |
| Name | varchar(200) | NO | Fund name (e.g. "Honeymoon") |
| Description | text | YES | What the fund is for |
| TargetAmount | decimal(18,2) | YES | Goal amount |
| CurrentAmount | decimal(18,2) | NO | Amount raised so far |
| ImageUrl | text | YES | Fund image URL |
| DisplayOrder | int | NO | Sort order |
| WeddingId | uuid | NO | FK to Weddings |
| CreatedAt | timestamptz | NO | Record created time |
| UpdatedAt | timestamptz | YES | Last update time |

**Indexes:** `IX_CashFunds_WeddingId`

**Foreign Keys:** `WeddingId -> Weddings(Id)` (CASCADE on delete)

---

#### Orders

| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| Id | uuid | NO | Primary key |
| GuestName | varchar(200) | NO | Guest who placed the order |
| GuestEmail | varchar(256) | NO | Guest email |
| GuestMessage | text | YES | Personal message for the couple |
| TotalAmount | decimal(18,2) | NO | Total order amount |
| Status | int | NO | 0=Pending, 1=Completed, 2=Failed, 3=Refunded |
| WeddingId | uuid | NO | FK to Weddings |
| CashFundId | uuid | YES | FK to CashFunds (if cash contribution) |
| UserId | uuid | NO | FK to Users (guest) |
| CreatedAt | timestamptz | NO | Record created time |
| UpdatedAt | timestamptz | YES | Last update time |

**Indexes:** `IX_Orders_WeddingId`, `IX_Orders_CashFundId`, `IX_Orders_UserId`

**Foreign Keys:** `WeddingId -> Weddings(Id)` (RESTRICT), `CashFundId -> CashFunds(Id)` (RESTRICT), `UserId -> Users(Id)` (RESTRICT)

---

#### OrderItems

| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| Id | uuid | NO | Primary key |
| Quantity | int | NO | Quantity purchased |
| UnitPrice | decimal(18,2) | NO | Price at time of purchase |
| TotalPrice | decimal(18,2) | NO | Quantity * UnitPrice |
| OrderId | uuid | NO | FK to Orders |
| GiftItemId | uuid | NO | FK to GiftItems |
| CreatedAt | timestamptz | NO | Record created time |
| UpdatedAt | timestamptz | YES | Last update time |

**Indexes:** `IX_OrderItems_OrderId`, `IX_OrderItems_GiftItemId`

**Foreign Keys:** `OrderId -> Orders(Id)` (CASCADE), `GiftItemId -> GiftItems(Id)` (RESTRICT)

---

#### Payments

| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| Id | uuid | NO | Primary key |
| StripePaymentIntentId | varchar(256) | NO | Stripe PaymentIntent ID (unique) |
| StripeSessionId | varchar(256) | NO | Stripe Checkout Session ID |
| Amount | decimal(18,2) | NO | Payment amount |
| Currency | varchar(3) | NO | ISO currency code (default: usd) |
| Status | int | NO | 0=Pending, 1=Succeeded, 2=Failed, 3=Refunded |
| PaidAt | timestamptz | YES | When payment was completed |
| FailureReason | text | YES | Error message if failed |
| OrderId | uuid | NO | FK to Orders |
| CreatedAt | timestamptz | NO | Record created time |
| UpdatedAt | timestamptz | YES | Last update time |

**Indexes:** `IX_Payments_StripePaymentIntentId` (unique), `IX_Payments_StripeSessionId`, `IX_Payments_OrderId`

**Foreign Keys:** `OrderId -> Orders(Id)` (CASCADE)

---

### Enums

| Enum | Values |
|------|--------|
| UserRole | Couple=0, Guest=1 |
| WeddingStatus | Draft=0, Active=1, Completed=2, Cancelled=3 |
| GiftItemStatus | Available=0, Reserved=1, Purchased=2 |
| OrderStatus | Pending=0, Completed=1, Failed=2, Refunded=3 |
| PaymentStatus | Pending=0, Succeeded=1, Failed=2, Refunded=3 |

---

## Query Examples

### Users

```sql
-- Register a new couple
INSERT INTO "Users" ("Id", "Email", "PasswordHash", "FirstName", "LastName", "Role", "CreatedAt")
VALUES ('a1b2c3d4-e5f6-7890-abcd-ef1234567890', 'john@example.com', 'hashed_password', 'John', 'Smith', 0, NOW());

-- Find user by email
SELECT * FROM "Users" WHERE "Email" = 'john@example.com';

-- Update refresh token
UPDATE "Users"
SET "RefreshToken" = 'new_refresh_token', "RefreshTokenExpiry" = NOW() + INTERVAL '7 days'
WHERE "Id" = 'a1b2c3d4-e5f6-7890-abcd-ef1234567890';
```

### Weddings

```sql
-- Create a wedding
INSERT INTO "Weddings" ("Id", "Title", "WeddingDate", "Venue", "Slug", "Partner1Id", "Partner2Id", "Status", "CreatedAt")
VALUES ('b2c3d4e5-f6a7-8901-bcde-f12345678901', 'John & Jane Wedding', '2026-09-15', 'Grand Hotel', 'john-jane-2026', 'a1b2c3d4-e5f6-7890-abcd-ef1234567890', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 1, NOW());

-- Get wedding by slug with partners
SELECT w.*, 
       u1."FirstName" AS "Partner1FirstName", u1."LastName" AS "Partner1LastName",
       u2."FirstName" AS "Partner2FirstName", u2."LastName" AS "Partner2LastName"
FROM "Weddings" w
JOIN "Users" u1 ON w."Partner1Id" = u1."Id"
JOIN "Users" u2 ON w."Partner2Id" = u2."Id"
WHERE w."Slug" = 'john-jane-2026';

-- Get active public weddings
SELECT * FROM "Weddings"
WHERE "Status" = 1 AND "IsPublic" = true
ORDER BY "WeddingDate" ASC;
```

### Gift Items

```sql
-- Add a gift item to a wedding
INSERT INTO "GiftItems" ("Id", "Name", "Description", "Price", "Status", "Quantity", "WeddingId", "DisplayOrder", "CreatedAt")
VALUES ('d4e5f6a7-b8c9-0123-defa-234567890123', 'Dyson V15 Vacuum', 'Cordless vacuum cleaner', 749.99, 0, 1, 'b2c3d4e5-f6a7-8901-bcde-f12345678901', 1, NOW());

-- Get all available gifts for a wedding
SELECT * FROM "GiftItems"
WHERE "WeddingId" = 'b2c3d4e5-f6a7-8901-bcde-f12345678901' AND "Status" = 0
ORDER BY "DisplayOrder" ASC;

-- Get gift progress (how many bought vs needed)
SELECT "Name", "Price", "Quantity", "QuantityPurchased",
       "Quantity" - "QuantityPurchased" AS "Remaining"
FROM "GiftItems"
WHERE "WeddingId" = 'b2c3d4e5-f6a7-8901-bcde-f12345678901';

-- Mark gift as purchased after order
UPDATE "GiftItems"
SET "Status" = 2, "QuantityPurchased" = "QuantityPurchased" + 1, "UpdatedAt" = NOW()
WHERE "Id" = 'd4e5f6a7-b8c9-0123-defa-234567890123';
```

### Cash Funds

```sql
-- Create a cash fund
INSERT INTO "CashFunds" ("Id", "Name", "Description", "TargetAmount", "CurrentAmount", "WeddingId", "DisplayOrder", "CreatedAt")
VALUES ('e5f6a7b8-c9d0-1234-efab-345678901234', 'Honeymoon Fund', 'Help us travel to Bali!', 5000.00, 0, 'b2c3d4e5-f6a7-8901-bcde-f12345678901', 1, NOW());

-- Update fund amount after a contribution
UPDATE "CashFunds"
SET "CurrentAmount" = "CurrentAmount" + 100.00, "UpdatedAt" = NOW()
WHERE "Id" = 'e5f6a7b8-c9d0-1234-efab-345678901234';

-- Get all cash funds with progress percentage
SELECT "Name", "TargetAmount", "CurrentAmount",
       ROUND(("CurrentAmount" / NULLIF("TargetAmount", 0)) * 100, 1) AS "ProgressPercent"
FROM "CashFunds"
WHERE "WeddingId" = 'b2c3d4e5-f6a7-8901-bcde-f12345678901';
```

### Orders

```sql
-- Create an order (gift purchase)
INSERT INTO "Orders" ("Id", "GuestName", "GuestEmail", "GuestMessage", "TotalAmount", "Status", "WeddingId", "UserId", "CreatedAt")
VALUES ('f6a7b8c9-d0e1-2345-fabc-456789012345', 'Alice Guest', 'alice@example.com', 'Congrats!', 749.99, 0, 'b2c3d4e5-f6a7-8901-bcde-f12345678901', 'c3d4e5f6-a7b8-9012-cdef-123456789012', NOW());

-- Add order items
INSERT INTO "OrderItems" ("Id", "Quantity", "UnitPrice", "TotalPrice", "OrderId", "GiftItemId", "CreatedAt")
VALUES ('a7b8c9d0-e1f2-3456-abcd-567890123456', 1, 749.99, 749.99, 'f6a7b8c9-d0e1-2345-fabc-456789012345', 'd4e5f6a7-b8c9-0123-defa-234567890123', NOW());

-- Get all orders for a wedding with details
SELECT o."Id", o."GuestName", o."GuestEmail", o."TotalAmount", o."Status", o."CreatedAt",
       oi."Quantity", oi."UnitPrice", gi."Name" AS "GiftName"
FROM "Orders" o
JOIN "OrderItems" oi ON o."Id" = oi."OrderId"
JOIN "GiftItems" gi ON oi."GiftItemId" = gi."Id"
WHERE o."WeddingId" = 'b2c3d4e5-f6a7-8901-bcde-f12345678901'
ORDER BY o."CreatedAt" DESC;

-- Get order total by wedding
SELECT COUNT(*) AS "TotalOrders", SUM("TotalAmount") AS "TotalRevenue"
FROM "Orders"
WHERE "WeddingId" = 'b2c3d4e5-f6a7-8901-bcde-f12345678901' AND "Status" = 1;

-- Create a cash fund order
INSERT INTO "Orders" ("Id", "GuestName", "GuestEmail", "GuestMessage", "TotalAmount", "Status", "WeddingId", "CashFundId", "UserId", "CreatedAt")
VALUES ('b8c9d0e1-f2a3-4567-bcde-678901234567', 'Bob Donor', 'bob@example.com', 'Enjoy your honeymoon!', 100.00, 1, 'b2c3d4e5-f6a7-8901-bcde-f12345678901', 'e5f6a7b8-c9d0-1234-efab-345678901234', 'c3d4e5f6-a7b8-9012-cdef-123456789012', NOW());
```

### Payments

```sql
-- Record a successful payment
INSERT INTO "Payments" ("Id", "StripePaymentIntentId", "StripeSessionId", "Amount", "Currency", "Status", "PaidAt", "OrderId", "CreatedAt")
VALUES ('c9d0e1f2-a3b4-5678-cdef-789012345678', 'pi_abc123xyz', 'cs_def456uvw', 749.99, 'usd', 1, NOW(), 'f6a7b8c9-d0e1-2345-fabc-456789012345', NOW());

-- Update order status after payment
UPDATE "Orders" SET "Status" = 1, "UpdatedAt" = NOW() WHERE "Id" = 'f6a7b8c9-d0e1-2345-fabc-456789012345';

-- Get payment history for a wedding
SELECT p."StripePaymentIntentId", p."Amount", p."Currency", p."Status", p."PaidAt",
       o."GuestName", o."GuestEmail"
FROM "Payments" p
JOIN "Orders" o ON p."OrderId" = o."Id"
WHERE o."WeddingId" = 'b2c3d4e5-f6a7-8901-bcde-f12345678901'
ORDER BY p."PaidAt" DESC;

-- Find payment by Stripe session (for webhook handling)
SELECT p.*, o."WeddingId"
FROM "Payments" p
JOIN "Orders" o ON p."OrderId" = o."Id"
WHERE p."StripeSessionId" = 'cs_def456uvw';
```

### Dashboard / Stats

```sql
-- Wedding registry summary
SELECT 
    w."Title",
    w."WeddingDate",
    (SELECT COUNT(*) FROM "GiftItems" WHERE "WeddingId" = w."Id" AND "Status" = 2) AS "GiftsPurchased",
    (SELECT COUNT(*) FROM "GiftItems" WHERE "WeddingId" = w."Id") AS "TotalGifts",
    (SELECT COALESCE(SUM("TotalAmount"), 0) FROM "Orders" WHERE "WeddingId" = w."Id" AND "Status" = 1) AS "TotalSpent",
    (SELECT COALESCE(SUM("CurrentAmount"), 0) FROM "CashFunds" WHERE "WeddingId" = w."Id") AS "CashFundRaised"
FROM "Weddings" w
WHERE w."Id" = 'b2c3d4e5-f6a7-8901-bcde-f12345678901';

-- Recent activity
SELECT 'gift' AS "Type", gi."Name" AS "Item", o."GuestName", o."TotalAmount", o."CreatedAt"
FROM "Orders" o
JOIN "OrderItems" oi ON o."Id" = oi."OrderId"
JOIN "GiftItems" gi ON oi."GiftItemId" = gi."Id"
WHERE o."WeddingId" = 'b2c3d4e5-f6a7-8901-bcde-f12345678901'

UNION ALL

SELECT 'cash' AS "Type", cf."Name" AS "Item", o."GuestName", o."TotalAmount", o."CreatedAt"
FROM "Orders" o
JOIN "CashFunds" cf ON o."CashFundId" = cf."Id"
WHERE o."WeddingId" = 'b2c3d4e5-f6a7-8901-bcde-f12345678901'

ORDER BY "CreatedAt" DESC
LIMIT 20;
```

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
  WedNest.slnx                  # Solution file
  src/
    API/                        # Web API layer (controllers, Program.cs)
    Application/                # Business logic, DTOs, interfaces
    Domain/                     # Entities, domain models
      Entities/
        BaseEntity.cs           # Id, CreatedAt, UpdatedAt
        User.cs                 # Users table
        Wedding.cs              # Weddings table
        GiftItem.cs             # GiftItems table
        CashFund.cs             # CashFunds table
        Order.cs                # Orders table
        OrderItem.cs            # OrderItems table
        Payment.cs              # Payments table
    Infrastructure/             # EF Core, Stripe, JWT, external services
      Data/
        ApplicationDbContext.cs # DbContext with all DbSets
        DesignTimeDbContextFactory.cs # For EF migrations
      Migrations/               # EF Core migrations
```

---

## Adding a Migration

```bash
cd src/API
dotnet ef migrations add <MigrationName> --project ../Infrastructure --startup-project .
dotnet ef database update --project ../Infrastructure --startup-project .
```
