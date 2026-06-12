# WedNest API

A backend service for a Wedding Registry platform built with **ASP.NET Core (.NET 10)** and **PostgreSQL**.

This API handles registry management, guest interactions, and secure payments via **ZarinPal (Iranian Payment Gateway)**.

---

## Tech Stack

- ASP.NET Core 10 (Web API)
- PostgreSQL 18
- Entity Framework Core
- Keycloak Authentication (JWT Bearer)
- ZarinPal Payments (Iranian Payment Gateway)
- Clean Architecture
- Swagger / OpenAPI
- Serilog Logging
- DotNetEnv (.env configuration)
- Multi-language support (Translation tables)

---

## Database Schema

### ER Diagram

```
Users ──────────┐
  │              │
  ├─── Weddings ─┤────────── WeddingTranslations
  │    (Partner1)│                │
  │    (Partner2)│              Languages
  │              │
  ├── GiftItems ─┘────────── GiftItemTranslations
  │                            │
  ├── CashFunds ─┘────────── CashFundTranslations
  │
  └── Orders ──── OrderItems ──── GiftItems
       │
       ├── CashFunds
       │
       └── Payments
```

---

### Core Tables

#### Users

| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| Id | uuid | NO | Primary key |
| KeycloakId | varchar(128) | NO | Keycloak user identifier (unique) |
| Email | varchar(256) | NO | Unique email address |
| FirstName | varchar(100) | NO | First name |
| LastName | varchar(100) | NO | Last name |
| PhoneNumber | text | YES | Phone number |
| Role | int | NO | 0=Guest, 1=User, 2=Couple, 3=Admin |
| CreatedAt | timestamptz | NO | Record created time |
| UpdatedAt | timestamptz | YES | Last update time |

**Indexes:** `IX_Users_KeycloakId` (unique), `IX_Users_Email` (unique)

---

#### Weddings

| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| Id | uuid | NO | Primary key |
| Title | varchar(200) | NO | Default language title |
| WeddingDate | timestamptz | NO | Date of the wedding |
| Venue | varchar(300) | YES | Default language venue |
| Description | text | YES | Default language description |
| CoverImageUrl | text | YES | Cover photo URL |
| Slug | varchar(200) | NO | URL-friendly identifier (unique) |
| Status | int | NO | 0=Draft, 1=Active, 2=Completed, 3=Cancelled |
| IsPublic | bool | NO | Whether registry is publicly visible |
| Partner1Id | uuid | NO | FK to Users (first partner) |
| Partner2Id | uuid | NO | FK to Users (second partner) |
| CreatedAt | timestamptz | NO | Record created time |
| UpdatedAt | timestamptz | YES | Last update time |

**Indexes:** `IX_Weddings_Slug` (unique), `IX_Weddings_Partner1Id`, `IX_Weddings_Partner2Id`

---

#### GiftItems

| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| Id | uuid | NO | Primary key |
| Name | varchar(200) | NO | Default language name |
| Description | text | YES | Default language description |
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

---

#### CashFunds

| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| Id | uuid | NO | Primary key |
| Name | varchar(200) | NO | Default language fund name |
| Description | text | YES | Default language description |
| TargetAmount | decimal(18,2) | YES | Goal amount |
| CurrentAmount | decimal(18,2) | NO | Amount raised so far |
| ImageUrl | text | YES | Fund image URL |
| DisplayOrder | int | NO | Sort order |
| WeddingId | uuid | NO | FK to Weddings |
| CreatedAt | timestamptz | NO | Record created time |
| UpdatedAt | timestamptz | YES | Last update time |

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

---

#### Payments

| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| Id | uuid | NO | Primary key |
| Authority | varchar(256) | NO | ZarinPal Authority ID (unique) |
| RefId | varchar(256) | YES | ZarinPal Ref ID after verification |
| Amount | decimal(18,2) | NO | Payment amount (IRR) |
| Currency | varchar(3) | NO | ISO currency code (default: IRR) |
| Status | int | NO | 0=Pending, 1=Succeeded, 2=Failed, 3=Refunded |
| PaidAt | timestamptz | YES | When payment was completed |
| FailureReason | text | YES | Error message if failed |
| OrderId | uuid | NO | FK to Orders |
| CreatedAt | timestamptz | NO | Record created time |
| UpdatedAt | timestamptz | YES | Last update time |

---

### Multi-Language Tables

#### Languages

| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| Id | uuid | NO | Primary key |
| Code | varchar(10) | NO | Language code (en, ar, fr) - unique |
| Name | varchar(100) | NO | English name ("English") |
| NativeName | varchar(100) | NO | Native name ("English" / "العربية") |
| IsActive | bool | NO | Is this language available |
| IsDefault | bool | NO | Is this the default fallback language |
| DisplayOrder | int | NO | Sort order in language switcher |
| CreatedAt | timestamptz | NO | Record created time |
| UpdatedAt | timestamptz | YES | Last update time |

**Indexes:** `IX_Languages_Code` (unique)

---

#### WeddingTranslations

| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| Id | uuid | NO | Primary key |
| WeddingId | uuid | NO | FK to Weddings |
| LanguageId | uuid | NO | FK to Languages |
| Title | varchar(200) | NO | Translated title |
| Description | text | YES | Translated description |
| Venue | varchar(300) | YES | Translated venue |
| CreatedAt | timestamptz | NO | Record created time |
| UpdatedAt | timestamptz | YES | Last update time |

**Unique Index:** `(WeddingId, LanguageId)`

---

#### GiftItemTranslations

| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| Id | uuid | NO | Primary key |
| GiftItemId | uuid | NO | FK to GiftItems |
| LanguageId | uuid | NO | FK to Languages |
| Name | varchar(200) | NO | Translated name |
| Description | text | YES | Translated description |
| CreatedAt | timestamptz | NO | Record created time |
| UpdatedAt | timestamptz | YES | Last update time |

**Unique Index:** `(GiftItemId, LanguageId)`

---

#### CashFundTranslations

| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| Id | uuid | NO | Primary key |
| CashFundId | uuid | NO | FK to CashFunds |
| LanguageId | uuid | NO | FK to Languages |
| Name | varchar(200) | NO | Translated name |
| Description | text | YES | Translated description |
| CreatedAt | timestamptz | NO | Record created time |
| UpdatedAt | timestamptz | YES | Last update time |

**Unique Index:** `(CashFundId, LanguageId)`

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

## Multi-Language Queries

### Seed Languages

```sql
INSERT INTO "Languages" ("Id", "Code", "Name", "NativeName", "IsActive", "IsDefault", "DisplayOrder", "CreatedAt")
VALUES
  ('11111111-1111-1111-1111-111111111111', 'en', 'English', 'English', true, true, 1, NOW()),
  ('22222222-2222-2222-2222-222222222222', 'ar', 'Arabic', 'العربية', true, false, 2, NOW()),
  ('33333333-3333-3333-3333-333333333333', 'fr', 'French', 'Francais', true, false, 3, NOW());
```

### Add Translations

```sql
-- English translation (default - matches base entity)
INSERT INTO "WeddingTranslations" ("Id", "WeddingId", "LanguageId", "Title", "Description", "Venue", "CreatedAt")
VALUES ('aaaa1111-0000-0000-0000-000000000001', 'b2c3d4e5-f6a7-8901-bcde-f12345678901', '11111111-1111-1111-1111-111111111111', 'John & Jane Wedding', 'Our special day', 'Grand Hotel', NOW());

-- Arabic translation
INSERT INTO "WeddingTranslations" ("Id", "WeddingId", "LanguageId", "Title", "Description", "Venue", "CreatedAt")
VALUES ('aaaa1111-0000-0000-0000-000000000002', 'b2c3d4e5-f6a7-8901-bcde-f12345678901', '22222222-2222-2222-2222-222222222222', '婚礼 约翰 和 简', '我们特殊的一天', '大酒店', NOW());

-- French translation
INSERT INTO "WeddingTranslations" ("Id", "WeddingId", "LanguageId", "Title", "Description", "Venue", "CreatedAt")
VALUES ('aaaa1111-0000-0000-0000-000000000003', 'b2c3d4e5-f6a7-8901-bcde-f12345678901', '33333333-3333-3333-3333-333333333333', 'Mariage de John et Jane', 'Notre jour special', 'Grand Hotel', NOW());

-- Gift item translations
INSERT INTO "GiftItemTranslations" ("Id", "GiftItemId", "LanguageId", "Name", "Description", "CreatedAt")
VALUES
  ('bbbb1111-0000-0000-0000-000000000001', 'd4e5f6a7-b8c9-0123-defa-234567890123', '11111111-1111-1111-1111-111111111111', 'Dyson V15 Vacuum', 'Cordless vacuum cleaner', NOW()),
  ('bbbb1111-0000-0000-0000-000000000002', 'd4e5f6a7-b8c9-0123-defa-234567890123', '22222222-2222-2222-2222-222222222222', 'مساعد دايسون التنظيف', 'مكنسة كهربائية بدون سلك', NOW());

-- Cash fund translations
INSERT INTO "CashFundTranslations" ("Id", "CashFundId", "LanguageId", "Name", "Description", "CreatedAt")
VALUES
  ('cccc1111-0000-0000-0000-000000000001', 'e5f6a7b8-c9d0-1234-efab-345678901234', '11111111-1111-1111-1111-111111111111', 'Honeymoon Fund', 'Help us travel to Bali!', NOW()),
  ('cccc1111-0000-0000-0000-000000000002', 'e5f6a7b8-c9d0-1234-efab-345678901234', '22222222-2222-2222-2222-222222222222', 'صندوق شهر العسل', 'ساعدونا في السفر إلى بالي!', NOW());
```

### Fetch Localized Content (LEFT JOIN with fallback)

```sql
-- Get wedding with Arabic translation, fallback to default (English)
SELECT
    w."Id",
    w."Slug",
    w."WeddingDate",
    COALESCE(wt."Title", w."Title") AS "Title",
    COALESCE(wt."Description", w."Description") AS "Description",
    COALESCE(wt."Venue", w."Venue") AS "Venue",
    l."Code" AS "Language"
FROM "Weddings" w
LEFT JOIN "WeddingTranslations" wt ON w."Id" = wt."WeddingId"
    AND wt."LanguageId" = (SELECT "Id" FROM "Languages" WHERE "Code" = 'ar')
LEFT JOIN "Languages" l ON wt."LanguageId" = l."Id"
WHERE w."Slug" = 'john-jane-2026';

-- Get all gift items with French translations, fallback to default
SELECT
    gi."Id",
    COALESCE(git."Name", gi."Name") AS "Name",
    COALESCE(git."Description", gi."Description") AS "Description",
    gi."Price",
    gi."Status"
FROM "GiftItems" gi
LEFT JOIN "GiftItemTranslations" git ON gi."Id" = git."GiftItemId"
    AND git."LanguageId" = (SELECT "Id" FROM "Languages" WHERE "Code" = 'fr')
WHERE gi."WeddingId" = 'b2c3d4e5-f6a7-8901-bcde-f12345678901'
ORDER BY gi."DisplayOrder";

-- Get all available translations for a wedding
SELECT
    l."Code" AS "Language",
    l."Name" AS "LanguageName",
    wt."Title",
    wt."Description",
    wt."Venue"
FROM "WeddingTranslations" wt
JOIN "Languages" l ON wt."LanguageId" = l."Id"
WHERE wt."WeddingId" = 'b2c3d4e5-f6a7-8901-bcde-f12345678901'
ORDER BY l."DisplayOrder";
```

---

## Advanced Query Examples

### LEFT JOINs

```sql
-- All weddings with their order count (including weddings with zero orders)
SELECT
    w."Title",
    w."WeddingDate",
    w."Status",
    COUNT(o."Id") AS "TotalOrders",
    COALESCE(SUM(o."TotalAmount"), 0) AS "TotalRevenue"
FROM "Weddings" w
LEFT JOIN "Orders" o ON w."Id" = o."WeddingId" AND o."Status" = 1
GROUP BY w."Id", w."Title", w."WeddingDate", w."Status"
ORDER BY w."WeddingDate";

-- All gift items with their purchase count (including unpurchased)
SELECT
    gi."Name",
    gi."Price",
    gi."Quantity",
    gi."QuantityPurchased",
    COUNT(oi."Id") AS "TimesOrdered",
    COALESCE(SUM(oi."TotalPrice"), 0) AS "TotalRevenue"
FROM "GiftItems" gi
LEFT JOIN "OrderItems" oi ON gi."Id" = oi."GiftItemId"
LEFT JOIN "Orders" o ON oi."OrderId" = o."Id" AND o."Status" = 1
WHERE gi."WeddingId" = 'b2c3d4e5-f6a7-8901-bcde-f12345678901'
GROUP BY gi."Id", gi."Name", gi."Price", gi."Quantity", gi."QuantityPurchased"
ORDER BY gi."DisplayOrder";

-- Users who have NOT placed any orders (potential guests to invite)
SELECT
    u."FirstName",
    u."LastName",
    u."Email",
    u."Role"
FROM "Users" u
LEFT JOIN "Orders" o ON u."Id" = o."UserId"
WHERE o."Id" IS NULL
  AND u."Role" = 1
ORDER BY u."LastName";

-- Weddings with NO cash funds
SELECT w."Title", w."Slug", w."WeddingDate"
FROM "Weddings" w
LEFT JOIN "CashFunds" cf ON w."Id" = cf."WeddingId"
WHERE cf."Id" IS NULL
  AND w."Status" = 1;
```

### Subqueries

```sql
-- Gift items priced ABOVE the average gift price for their wedding
SELECT gi."Name", gi."Price", gi."WeddingId"
FROM "GiftItems" gi
WHERE gi."Price" > (
    SELECT AVG(gi2."Price")
    FROM "GiftItems" gi2
    WHERE gi2."WeddingId" = gi."WeddingId"
)
ORDER BY gi."WeddingId", gi."Price" DESC;

-- Top spending guest per wedding
SELECT
    o."WeddingId",
    o."GuestName",
    o."GuestEmail",
    o."TotalAmount"
FROM "Orders" o
INNER JOIN (
    SELECT "WeddingId", MAX("TotalAmount") AS "MaxAmount"
    FROM "Orders"
    WHERE "Status" = 1
    GROUP BY "WeddingId"
) mo ON o."WeddingId" = mo."WeddingId" AND o."TotalAmount" = mo."MaxAmount"
WHERE o."Status" = 1;

-- Weddings where ALL gifts have been purchased
SELECT w."Title", w."Slug"
FROM "Weddings" w
WHERE NOT EXISTS (
    SELECT 1
    FROM "GiftItems" gi
    WHERE gi."WeddingId" = w."Id" AND gi."Status" != 2
)
AND EXISTS (
    SELECT 1
    FROM "GiftItems" gi
    WHERE gi."WeddingId" = w."Id"
);

-- Guests who bought gifts but NOT cash fund contributions
SELECT DISTINCT
    o."GuestName",
    o."GuestEmail"
FROM "Orders" o
WHERE o."WeddingId" = 'b2c3d4e5-f6a7-8901-bcde-f12345678901'
  AND o."CashFundId" IS NULL
  AND o."Id" NOT IN (
      SELECT o2."Id"
      FROM "Orders" o2
      WHERE o2."CashFundId" IS NOT NULL
        AND o2."GuestEmail" = o."GuestEmail"
  );
```

### CTEs (Common Table Expressions)

```sql
-- Recursive CTE: Wedding registry full summary
WITH GiftStats AS (
    SELECT
        gi."WeddingId",
        COUNT(*) AS "TotalGifts",
        COUNT(*) FILTER (WHERE gi."Status" = 2) AS "PurchasedGifts",
        SUM(gi."Price" * gi."Quantity") AS "TotalGiftValue",
        SUM(gi."Price" * gi."QuantityPurchased") AS "PurchasedGiftValue"
    FROM "GiftItems" gi
    GROUP BY gi."WeddingId"
),
CashStats AS (
    SELECT
        cf."WeddingId",
        COUNT(*) AS "TotalFunds",
        COALESCE(SUM(cf."CurrentAmount"), 0) AS "TotalCashRaised",
        COALESCE(SUM(cf."TargetAmount"), 0) AS "TotalCashTarget"
    FROM "CashFunds" cf
    GROUP BY cf."WeddingId"
),
OrderStats AS (
    SELECT
        o."WeddingId",
        COUNT(*) AS "TotalOrders",
        COUNT(*) FILTER (WHERE o."Status" = 1) AS "CompletedOrders",
        COALESCE(SUM(o."TotalAmount") FILTER (WHERE o."Status" = 1), 0) AS "TotalRevenue",
        COUNT(DISTINCT o."UserId") AS "UniqueGuests"
    FROM "Orders" o
    GROUP BY o."WeddingId"
)
SELECT
    w."Title",
    w."WeddingDate",
    w."Status",
    gs."TotalGifts",
    gs."PurchasedGifts",
    gs."TotalGiftValue",
    gs."PurchasedGiftValue",
    cs."TotalFunds",
    cs."TotalCashRaised",
    cs."TotalCashTarget",
    os."TotalOrders",
    os."CompletedOrders",
    os."TotalRevenue",
    os."UniqueGuests",
    ROUND(gs."PurchasedGiftValue" + cs."TotalCashRaised", 2) AS "GrandTotalReceived"
FROM "Weddings" w
LEFT JOIN GiftStats gs ON w."Id" = gs."WeddingId"
LEFT JOIN CashStats cs ON w."Id" = cs."WeddingId"
LEFT JOIN OrderStats os ON w."Id" = os."WeddingId"
WHERE w."Id" = 'b2c3d4e5-f6a7-8901-bcde-f12345678901';

-- CTE: Monthly revenue trend
WITH MonthlyRevenue AS (
    SELECT
        DATE_TRUNC('month', o."CreatedAt") AS "Month",
        o."WeddingId",
        COUNT(*) AS "OrderCount",
        SUM(o."TotalAmount") AS "Revenue"
    FROM "Orders" o
    WHERE o."Status" = 1
    GROUP BY DATE_TRUNC('month', o."CreatedAt"), o."WeddingId"
)
SELECT
    mr."Month",
    w."Title",
    mr."OrderCount",
    mr."Revenue",
    LAG(mr."Revenue") OVER (PARTITION BY mr."WeddingId" ORDER BY mr."Month") AS "PrevMonthRevenue",
    mr."Revenue" - LAG(mr."Revenue") OVER (PARTITION BY mr."WeddingId" ORDER BY mr."Month") AS "MonthOverMonthChange"
FROM MonthlyRevenue mr
JOIN "Weddings" w ON mr."WeddingId" = w."Id"
ORDER BY mr."WeddingId", mr."Month";
```

### Window Functions

```sql
-- Rank guests by total spending per wedding
SELECT
    o."GuestName",
    o."GuestEmail",
    o."WeddingId",
    SUM(o."TotalAmount") AS "TotalSpent",
    RANK() OVER (PARTITION BY o."WeddingId" ORDER BY SUM(o."TotalAmount") DESC) AS "SpendingRank",
    ROUND(
        SUM(o."TotalAmount") / SUM(SUM(o."TotalAmount")) OVER (PARTITION BY o."WeddingId") * 100,
        1
    ) AS "PercentOfTotal"
FROM "Orders" o
WHERE o."Status" = 1
GROUP BY o."GuestName", o."GuestEmail", o."WeddingId"
ORDER BY o."WeddingId", "SpendingRank";

-- Running total of payments over time
SELECT
    p."PaidAt",
    p."Amount",
    o."GuestName",
    SUM(p."Amount") OVER (ORDER BY p."PaidAt" ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS "RunningTotal",
    ROUND(
        p."Amount" / SUM(p."Amount") OVER () * 100,
        2
    ) AS "PercentOfAllPayments"
FROM "Payments" p
JOIN "Orders" o ON p."OrderId" = o."Id"
WHERE p."Status" = 1
  AND o."WeddingId" = 'b2c3d4e5-f6a7-8901-bcde-f12345678901'
ORDER BY p."PaidAt";

-- Gift items with percentile ranking by price
SELECT
    gi."Name",
    gi."Price",
    gi."QuantityPurchased",
    NTILE(4) OVER (ORDER BY gi."Price") AS "PriceQuartile",
    PERCENT_RANK() OVER (ORDER BY gi."Price") AS "PricePercentile",
    DENSE_RANK() OVER (ORDER BY gi."Price" DESC) AS "PriceRank"
FROM "GiftItems" gi
WHERE gi."WeddingId" = 'b2c3d4e5-f6a7-8901-bcde-f12345678901'
ORDER BY gi."Price" DESC;

-- Day-over-day order count with moving average
WITH DailyOrders AS (
    SELECT
        DATE(o."CreatedAt") AS "OrderDay",
        COUNT(*) AS "DailyCount"
    FROM "Orders" o
    WHERE o."WeddingId" = 'b2c3d4e5-f6a7-8901-bcde-f12345678901'
    GROUP BY DATE(o."CreatedAt")
)
SELECT
    "OrderDay",
    "DailyCount",
    AVG("DailyCount") OVER (ORDER BY "OrderDay" ROWS BETWEEN 6 PRECEDING AND CURRENT ROW) AS "7DayMovingAvg",
    MAX("DailyCount") OVER (ORDER BY "OrderDay" ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS "AllTimeMax"
FROM DailyOrders
ORDER BY "OrderDay";
```

### Aggregation with GROUP BY and HAVING

```sql
-- Weddings with more than 5 completed orders
SELECT
    w."Title",
    COUNT(o."Id") AS "CompletedOrders",
    SUM(o."TotalAmount") AS "TotalRevenue"
FROM "Weddings" w
JOIN "Orders" o ON w."Id" = o."WeddingId" AND o."Status" = 1
GROUP BY w."Id", w."Title"
HAVING COUNT(o."Id") > 5
ORDER BY "TotalRevenue" DESC;

-- Guests who spent more than $500 total
SELECT
    o."GuestName",
    o."GuestEmail",
    COUNT(DISTINCT o."WeddingId") AS "WeddingsSupported",
    SUM(o."TotalAmount") AS "TotalSpent"
FROM "Orders" o
WHERE o."Status" = 1
GROUP BY o."GuestName", o."GuestEmail"
HAVING SUM(o."TotalAmount") > 500
ORDER BY "TotalSpent" DESC;

-- Gift items where purchased quantity >= 80% of needed
SELECT
    gi."Name",
    gi."Quantity",
    gi."QuantityPurchased",
    ROUND(gi."QuantityPurchased"::decimal / gi."Quantity" * 100, 1) AS "FulfillmentPercent"
FROM "GiftItems" gi
WHERE gi."WeddingId" = 'b2c3d4e5-f6a7-8901-bcde-f12345678901'
GROUP BY gi."Id", gi."Name", gi."Quantity", gi."QuantityPurchased"
HAVING gi."QuantityPurchased"::decimal / gi."Quantity" >= 0.8
ORDER BY "FulfillmentPercent" DESC;
```

---

## Data Reports

### Wedding Performance Dashboard

```sql
-- Full wedding overview with all metrics
SELECT
    w."Title" AS "Wedding",
    TO_CHAR(w."WeddingDate", 'YYYY-MM-DD') AS "Date",
    w."Status",

    -- Gift stats
    (SELECT COUNT(*) FROM "GiftItems" WHERE "WeddingId" = w."Id") AS "TotalGifts",
    (SELECT COUNT(*) FROM "GiftItems" WHERE "WeddingId" = w."Id" AND "Status" = 2) AS "GiftsFulfilled",
    (SELECT COALESCE(SUM("Price" * "Quantity"), 0) FROM "GiftItems" WHERE "WeddingId" = w."Id") AS "TotalGiftValue",
    (SELECT COALESCE(SUM("Price" * "QuantityPurchased"), 0) FROM "GiftItems" WHERE "WeddingId" = w."Id") AS "GiftValueReceived",

    -- Cash fund stats
    (SELECT COUNT(*) FROM "CashFunds" WHERE "WeddingId" = w."Id") AS "CashFunds",
    (SELECT COALESCE(SUM("CurrentAmount"), 0) FROM "CashFunds" WHERE "WeddingId" = w."Id") AS "CashRaised",
    (SELECT COALESCE(SUM("TargetAmount"), 0) FROM "CashFunds" WHERE "WeddingId" = w."Id") AS "CashTarget",

    -- Order stats
    (SELECT COUNT(*) FROM "Orders" WHERE "WeddingId" = w."Id" AND "Status" = 1) AS "CompletedOrders",
    (SELECT COALESCE(SUM("TotalAmount"), 0) FROM "Orders" WHERE "WeddingId" = w."Id" AND "Status" = 1) AS "TotalRevenue",
    (SELECT COUNT(DISTINCT "UserId") FROM "Orders" WHERE "WeddingId" = w."Id" AND "Status" = 1) AS "UniqueGuests",

    -- Payment stats
    (SELECT COUNT(*) FROM "Payments" p JOIN "Orders" o ON p."OrderId" = o."Id" WHERE o."WeddingId" = w."Id" AND p."Status" = 1) AS "SuccessfulPayments",
    (SELECT COALESCE(SUM(p."Amount"), 0) FROM "Payments" p JOIN "Orders" o ON p."OrderId" = o."Id" WHERE o."WeddingId" = w."Id" AND p."Status" = 1) AS "TotalPaid"

FROM "Weddings" w
ORDER BY w."WeddingDate";
```

### Revenue by Payment Method / Status

```sql
SELECT
    p."Status" AS "PaymentStatus",
    CASE p."Status"
        WHEN 0 THEN 'Pending'
        WHEN 1 THEN 'Succeeded'
        WHEN 2 THEN 'Failed'
        WHEN 3 THEN 'Refunded'
    END AS "StatusName",
    COUNT(*) AS "PaymentCount",
    SUM(p."Amount") AS "TotalAmount",
    AVG(p."Amount") AS "AvgAmount",
    MIN(p."Amount") AS "MinAmount",
    MAX(p."Amount") AS "MaxAmount"
FROM "Payments" p
GROUP BY p."Status"
ORDER BY p."Status";
```

### Guest Loyalty Report

```sql
-- Guests who have supported multiple weddings
SELECT
    u."FirstName" || ' ' || u."LastName" AS "GuestName",
    u."Email",
    COUNT(DISTINCT o."WeddingId") AS "WeddingsSupported",
    COUNT(o."Id") AS "TotalOrders",
    SUM(o."TotalAmount") AS "TotalSpent",
    MIN(o."CreatedAt") AS "FirstOrder",
    MAX(o."CreatedAt") AS "LastOrder",
    EXTRACT(DAY FROM MAX(o."CreatedAt") - MIN(o."CreatedAt")) AS "DaysBetweenFirstAndLast"
FROM "Users" u
JOIN "Orders" o ON u."Id" = o."UserId" AND o."Status" = 1
GROUP BY u."Id", u."FirstName", u."LastName", u."Email"
HAVING COUNT(DISTINCT o."WeddingId") > 1
ORDER BY "TotalSpent" DESC;
```

### Gift Popularity Report

```sql
-- Most popular gifts across all weddings
SELECT
    gi."Name",
    COUNT(DISTINCT gi."WeddingId") AS "WeddingsListed",
    SUM(gi."Quantity") AS "TotalNeeded",
    SUM(gi."QuantityPurchased") AS "TotalPurchased",
    ROUND(SUM(gi."QuantityPurchased")::decimal / NULLIF(SUM(gi."Quantity"), 0) * 100, 1) AS "OverallFulfillment%",
    SUM(gi."Price" * gi."QuantityPurchased") AS "TotalRevenueGenerated"
FROM "GiftItems" gi
GROUP BY gi."Name"
ORDER BY "TotalPurchased" DESC
LIMIT 20;
```

### Cash Fund Performance

```sql
SELECT
    cf."Name" AS "FundName",
    w."Title" AS "Wedding",
    cf."TargetAmount",
    cf."CurrentAmount",
    ROUND(cf."CurrentAmount" / NULLIF(cf."TargetAmount", 0) * 100, 1) AS "ProgressPercent",
    (SELECT COUNT(*) FROM "Orders" o WHERE o."CashFundId" = cf."Id" AND o."Status" = 1) AS "ContributionCount",
    (SELECT AVG(o."TotalAmount") FROM "Orders" o WHERE o."CashFundId" = cf."Id" AND o."Status" = 1) AS "AvgContribution"
FROM "CashFunds" cf
JOIN "Weddings" w ON cf."WeddingId" = w."Id"
ORDER BY "ProgressPercent" DESC NULLS LAST;
```

### Time-Based Analytics

```sql
-- Orders by day of week (to find peak gifting days)
SELECT
    EXTRACT(DOW FROM o."CreatedAt") AS "DayOfWeek",
    CASE EXTRACT(DOW FROM o."CreatedAt")
        WHEN 0 THEN 'Sunday'
        WHEN 1 THEN 'Monday'
        WHEN 2 THEN 'Tuesday'
        WHEN 3 THEN 'Wednesday'
        WHEN 4 THEN 'Thursday'
        WHEN 5 THEN 'Friday'
        WHEN 6 THEN 'Saturday'
    END AS "DayName",
    COUNT(*) AS "OrderCount",
    SUM(o."TotalAmount") AS "TotalRevenue"
FROM "Orders" o
WHERE o."Status" = 1
GROUP BY EXTRACT(DOW FROM o."CreatedAt")
ORDER BY "DayOfWeek";

-- Orders by hour of day
SELECT
    EXTRACT(HOUR FROM o."CreatedAt") AS "Hour",
    COUNT(*) AS "OrderCount",
    SUM(o."TotalAmount") AS "TotalRevenue"
FROM "Orders" o
WHERE o."Status" = 1
GROUP BY EXTRACT(HOUR FROM o."CreatedAt")
ORDER BY "Hour";

-- Weekly growth rate
WITH WeeklyStats AS (
    SELECT
        DATE_TRUNC('week', o."CreatedAt") AS "Week",
        COUNT(*) AS "Orders",
        SUM(o."TotalAmount") AS "Revenue"
    FROM "Orders" o
    WHERE o."Status" = 1
    GROUP BY DATE_TRUNC('week', o."CreatedAt")
)
SELECT
    "Week",
    "Orders",
    "Revenue",
    LAG("Revenue") OVER (ORDER BY "Week") AS "PrevWeekRevenue",
    ROUND(
        ("Revenue" - LAG("Revenue") OVER (ORDER BY "Week"))
        / NULLIF(LAG("Revenue") OVER (ORDER BY "Week"), 0) * 100,
        1
    ) AS "GrowthPercent"
FROM WeeklyStats
ORDER BY "Week";
```

### Multi-Language Content Coverage

```sql
-- Translation completeness report
SELECT
    l."Code" AS "Language",
    l."Name" AS "LanguageName",
    (SELECT COUNT(*) FROM "Weddings" WHERE "Status" IN (1, 2)) AS "TotalActiveWeddings",
    (SELECT COUNT(*) FROM "WeddingTranslations" WHERE "LanguageId" = l."Id") AS "WeddingTranslations",
    (SELECT COUNT(*) FROM "GiftItems") AS "TotalGiftItems",
    (SELECT COUNT(*) FROM "GiftItemTranslations" WHERE "LanguageId" = l."Id") AS "GiftTranslations",
    (SELECT COUNT(*) FROM "CashFunds") AS "TotalCashFunds",
    (SELECT COUNT(*) FROM "CashFundTranslations" WHERE "LanguageId" = l."Id") AS "FundTranslations"
FROM "Languages" l
WHERE l."IsActive" = true
ORDER BY l."DisplayOrder";

-- Content missing translations
SELECT
    'Wedding' AS "EntityType",
    w."Id",
    w."Title" AS "DefaultContent",
    l."Code" AS "MissingLanguage"
FROM "Weddings" w
CROSS JOIN "Languages" l
LEFT JOIN "WeddingTranslations" wt ON w."Id" = wt."WeddingId" AND l."Id" = wt."LanguageId"
WHERE l."IsDefault" = false
  AND wt."Id" IS NULL
  AND w."Status" IN (1, 2)

UNION ALL

SELECT
    'GiftItem' AS "EntityType",
    gi."Id",
    gi."Name" AS "DefaultContent",
    l."Code" AS "MissingLanguage"
FROM "GiftItems" gi
CROSS JOIN "Languages" l
LEFT JOIN "GiftItemTranslations" git ON gi."Id" = git."GiftItemId" AND l."Id" = git."LanguageId"
WHERE l."IsDefault" = false
  AND git."Id" IS NULL

UNION ALL

SELECT
    'CashFund' AS "EntityType",
    cf."Id",
    cf."Name" AS "DefaultContent",
    l."Code" AS "MissingLanguage"
FROM "CashFunds" cf
CROSS JOIN "Languages" l
LEFT JOIN "CashFundTranslations" cft ON cf."Id" = cft."CashFundId" AND l."Id" = cft."LanguageId"
WHERE l."IsDefault" = false
  AND cft."Id" IS NULL

ORDER BY "EntityType", "MissingLanguage";
```

---

## Full Setup Walkthrough

### 1. Clone & Open Project

```bash
git clone <repo-url>
cd wednest-api
```

### 2. PostgreSQL Database Setup

```bash
& "C:\Program Files\PostgreSQL\18\bin\psql.exe" -U postgres
# Inside psql:
CREATE DATABASE wednest_db;
\q
```

### 3. Configure Environment Variables

Edit the `.env` file in the project root:

```env
# Database
DB_HOST=localhost
DB_PORT=5432
DB_NAME=wednest_db
DB_USER=postgres
DB_PASSWORD=your_postgres_password

# JWT
JWT_SECRET=your-super-secret-key-at-least-32-chars!!
JWT_ISSUER=WedNest
JWT_AUDIENCE=WedNest
JWT_ACCESS_EXPIRY_MINUTES=15
JWT_REFRESH_EXPIRY_DAYS=7

# ZarinPal
ZARINPAL_MERCHANT_ID=your-merchant-id
ZARINPAL_SANDBOX=true
ZARINPAL_CALLBACK_URL=http://localhost:5000/api/payments/callback

# App
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=https://localhost:5001;http://localhost:5000
```

### 4. Restore, Build & Migrate

```bash
dotnet restore
dotnet build
cd src/API
dotnet ef database update --project ../Infrastructure --startup-project .
```

### 5. Run the API

From the project root:

```bash
dotnet run --project src/API
```

Or with a specific URL:

```bash
dotnet run --project src/API --urls http://localhost:5000
```

Or from the API directory:

```bash
cd src/API
dotnet run
```

- **Swagger UI**: https://localhost:5001/swagger
- **HTTP**: http://localhost:5000

---

## Project Structure

```
wednest-api/
  .env                          # Environment variables (secrets)
  WedNest.slnx                  # Solution file
  src/
    API/                        # Web API layer
    Application/                # Business logic, DTOs, interfaces
    Domain/                     # Entities
      Entities/
        BaseEntity.cs
        User.cs
        Wedding.cs
        GiftItem.cs
        CashFund.cs
        Order.cs
        OrderItem.cs
        Payment.cs
        Language.cs             # Supported languages
        WeddingTranslation.cs   # Localized wedding content
        GiftItemTranslation.cs  # Localized gift content
        CashFundTranslation.cs  # Localized fund content
    Infrastructure/             # EF Core, ZarinPal, JWT
      Data/
        ApplicationDbContext.cs
        DesignTimeDbContextFactory.cs
      Migrations/
```
