# E-Commerce REST API

A production-ready RESTful API built with **ASP.NET Core 8** for managing an e-commerce platform — featuring JWT authentication, product & category management, and full Swagger documentation.

> Built with GitHub Copilot to accelerate development.

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Framework | ASP.NET Core 8 Web API |
| ORM | Entity Framework Core |
| Database | MySQL (Pomelo Provider) |
| Auth | JWT Bearer Tokens |
| Docs | Swagger / OpenAPI |
| Language | C# |

---

## Features

- User Registration & Login with JWT authentication
- Full CRUD for Products and Categories
- One-to-Many relationship (Category -> Products)
- Data validation using Data Annotations
- Global Error Handling Middleware
- Swagger UI for interactive API testing
- Clean architecture: Controllers -> Services -> Repositories

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- MySQL Server

### Setup

```bash
# 1. Clone the repo
git clone https://github.com/Mohamed-Ezzalregal/dotnet-ecommerce-api-.git
cd dotnet-ecommerce-api-/TestApi

# 2. Update connection string in appsettings.json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=ecommercedb;User=root;Password=YOUR_PASSWORD"
}

# 3. Apply migrations
dotnet ef database update

# 4. Run
dotnet run

# 5. Open Swagger UI
https://localhost:5001/swagger
```

---

## API Endpoints

### Auth

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/register` | Register a new user |
| POST | `/api/auth/login` | Login & receive JWT token |

### Products

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/products` | Get all products |
| GET | `/api/products/{id}` | Get product by ID |
| POST | `/api/products` | Create product (Auth required) |
| PUT | `/api/products/{id}` | Update product (Auth required) |
| DELETE | `/api/products/{id}` | Delete product (Auth required) |

### Categories

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/categories` | Get all categories |
| GET | `/api/categories/{id}` | Get category by ID |
| POST | `/api/categories` | Create category (Auth required) |
| PUT | `/api/categories/{id}` | Update category (Auth required) |
| DELETE | `/api/categories/{id}` | Delete category (Auth required) |

---

## Project Structure

```
TestApi/
├── Controllers/        # API endpoints (Auth, Products, Categories)
├── Models/             # Database entities
├── DTOs/               # Data Transfer Objects with validation
├── Data/               # EF Core DbContext
├── Middlewares/        # Custom middleware (Error Handling)
├── Services/           # Business logic layer
├── Migrations/         # EF Core migrations
└── Program.cs          # App configuration & middleware
```

---

## Author

**Mohamed Ezzalregal**

- [LinkedIn](https://www.linkedin.com/in/mohamedayman-469994243)
- [GitHub](https://github.com/Mohamed-Ezzalregal)
- mohaamed.ezzalregal@gmail.com
