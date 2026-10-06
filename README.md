# 🛒 E-Commerce Web API (.NET 10)

An enterprise-grade RESTful E-Commerce Web API built with **.NET 10** following **Clean / Onion Architecture** principles, designed for scalability, maintainability, and high performance.

---

## 🚀 Features

- **Account Management:** User authentication, authorization, and profile management using ASP.NET Core Identity & JWT Bearer Tokens.
- **Product Catalog:** Product listing with pagination, sorting, filtering by Brand/Type, and search capabilities using the **Specification Pattern**.
- **Basket System:** High-performance shopping basket operations backed by **Redis**.
- **Order Management:** Full order lifecycle processing including item lookup, delivery method assignment, and user history.
- **Payment Processing:** Integrated payment flow with **Stripe** and Webhook handling for real-time order status updates.

---

## 🛠️ Tech Stack & Architecture

- **Framework:** .NET 10 Web API
- **Architecture:** Onion / Clean Architecture
- **Database:** Microsoft SQL Server (EF Core)
- **Caching & State:** Redis
- **Authentication:** JWT Bearer & ASP.NET Core Identity
- **Design Patterns:** Repository Pattern, Unit of Work, Specification Pattern
- **Payment Gateway:** Stripe API

---

## 📮 Postman Collection & API Documentation

This repository includes a pre-configured Postman Collection to easily test all API endpoints.

### How to Import & Use:
1. Download or clone this repository.
2. Open **Postman**.
3. Click **Import** (top left) and select the collection file:
   `Controllers.postman_collection.json` (or inside `docs/` if placed there)
4. For protected endpoints (e.g., Account, Orders, Payments), pass your JWT Bearer token in the `Authorization` header:
   `Bearer YOUR_JWT_TOKEN`

---

## ⚙️ Getting Started

1. **Clone the repository:**
   ```bash
   git clone [https://github.com/A-Hanafy/E-Commerce.git](https://github.com/A-Hanafy/E-Commerce.git)
