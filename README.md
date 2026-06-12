# WedNest API 💍

A backend service for a Wedding Registry platform built with **ASP.NET Core (.NET 10)** and **PostgreSQL**.

This API handles registry management, guest interactions, and secure payments via **Stripe Checkout + Webhooks**.

---

## 🚀 Tech Stack

- ASP.NET Core 10 (Web API)
- PostgreSQL
- Entity Framework Core
- JWT Authentication (Access + Refresh Tokens)
- Stripe Payments
- Clean Architecture
- Swagger / OpenAPI
- Serilog Logging

---

## 🧠 Project Overview

WedNest allows couples to create a wedding registry where guests can:

- Buy gifts from a curated list
- Contribute to cash funds (e.g. honeymoon)
- Track purchased items in real time

Payments are handled securely using **Stripe Checkout Sessions**.

---

## 📁 Architecture
src/
API/
Application/
Domain/
Infrastructure/

---

## 💳 Payment Flow (Stripe)

1. Guest selects a gift or cash fund
2. API creates Stripe Checkout Session
3. Stripe redirects user to secure payment page
4. Stripe sends webhook after payment
5. Backend confirms payment
6. Gift is marked as purchased

---

## 🗄️ Database

PostgreSQL is used.

Run migrations:

```bash
dotnet ef database update
