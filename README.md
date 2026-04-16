# 🛒 E-Commerce REST API

A production-ready RESTful API built with **ASP.NET Core 8** following **Clean Architecture** principles. Features JWT authentication, role-based authorization, pagination, filtering, sorting, structured logging, and full unit test coverage.

---

## Tech Stack

| Technology | Description |
|------------|-------------|
| **Framework** | ASP.NET Core 8 (Web API) |
| **Architecture** | Clean Architecture (4 Layers) |
| **ORM** | Entity Framework Core |
| **Database** | MySQL (Pomelo Provider) |
| **Auth** | JWT Bearer + BCrypt Password Hashing |
| **Authorization** | Role-Based (`Admin`, `User`) |
| **Mapping** | AutoMapper 12.0.1 |
| **Logging** | Serilog (Console + File Sink) |
| **Testing** | xUnit + Moq + FluentAssertions |
| **Docs** | Swagger / OpenAPI (Swashbuckle) |
| **Container** | Docker + Docker Compose |
| **Language** | C# 12 |

---

## Features

- **Authentication & Authorization** — JWT token-based auth with role-based access control (`Admin`/`User`)
- **Full CRUD** — Products and Categories with proper validation
- **Pagination, Filtering & Sorting** — Query parameters for all list endpoints
- **Clean Architecture** — Domain, Application, Infrastructure, API layers
- **Repository Pattern + Unit of Work** — Abstracted data access
- **AutoMapper** — Automatic DTO ↔ Model mapping
- **Global Error Handling** — Custom exceptions (`NotFoundException`, `BadRequestException`) with middleware
- **Structured Logging** — Serilog with console + rolling file output
- **Unit Testing** — 21 tests covering all controller actions (xUnit + Moq + FluentAssertions)
- **Docker Support** — Multi-stage Dockerfile + Docker Compose with MySQL
- **Swagger UI** — Interactive API docs with JWT Bearer support

---

## Architecture

This project follows **Clean Architecture** with 4 layers:

```
┌─────────────────────────────────────────┐
│              TestApi (API)              │  ← Controllers, Middleware, Program.cs
├─────────────────────────────────────────┤
│       TestApi.Application              │  ← DTOs, Interfaces, Mapping, Exceptions
├─────────────────────────────────────────┤
│       TestApi.Infrastructure           │  ← DbContext, Repositories, Migrations
├─────────────────────────────────────────┤
│          TestApi.Domain                │  ← Models (Product, Category, User)
└─────────────────────────────────────────┘
```

**Dependency Rule:** Inner layers have no knowledge of outer layers. Dependencies point inward.

---

## Project Structure

```
├── TestApi/                        # API Layer
│   ├── Controllers/                # Auth, Products, Categories
│   ├── Middlewares/                 # ExceptionMiddleware
│   ├── Services/                   # App services
│   └── Program.cs                  # Configuration & pipeline
│
├── TestApi.Application/            # Application Layer
│   ├── DTOs/                       # CreateProductDto, ProductDto, etc.
│   ├── Interfaces/                 # IRepository, IUnitOfWork, etc.
│   ├── Mapping/                    # AutoMapper profiles
│   └── Exceptions/                 # NotFoundException, BadRequestException
│
├── TestApi.Infrastructure/         # Infrastructure Layer
│   ├── Data/                       # AppDbContext
│   ├── Repositories/               # Repository, UnitOfWork
│   └── Migrations/                 # EF Core migrations
│
├── TestApi.Domain/                 # Domain Layer
│   └── Models/                     # Product, Category, User
│
├── TestApi.Tests/                  # Unit Tests
│   └── Controllers/                # ProductsControllerTests, CategoriesControllerTests
│
├── Dockerfile                      # Multi-stage Docker build
├── docker-compose.yml              # API + MySQL services
└── .dockerignore
```

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- MySQL Server (or Docker)

### Local Setup

```bash
# 1. Clone the repo
git clone https://github.com/Mohamed-Ezzalregal/dotnet-ecommerce-api-.git
cd dotnet-ecommerce-api-

# 2. Update connection string in TestApi/appsettings.json
# "Server=localhost;Port=3306;Database=TestApiDb;User=root;Password=YOUR_PASSWORD"

# 3. Apply migrations
cd TestApi
dotnet ef database update

# 4. Run the API
dotnet run

# 5. Open Swagger UI
# https://localhost:5001/swagger
```

### Docker Setup

```bash
# Run API + MySQL with Docker Compose
docker-compose up --build

# API will be available at:
# http://localhost:5000/swagger
```

The Docker Compose setup includes:
- **API** container on port `5000`
- **MySQL 8.0** container on port `3307`
- Automatic database health check before API starts
- Persistent MySQL volume for data

---

## API Endpoints

### Auth

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/api/auth/register` | Register a new user | ❌ |
| POST | `/api/auth/login` | Login & receive JWT token | ❌ |

### Products

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/products` | Get all products (paginated, filterable, sortable) | ❌ |
| GET | `/api/products/{id}` | Get product by ID | ❌ |
| POST | `/api/products` | Create product | 🔒 Admin |
| PUT | `/api/products/{id}` | Update product | 🔒 Admin |
| DELETE | `/api/products/{id}` | Delete product | 🔒 Admin |

### Categories

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/categories` | Get all categories | ❌ |
| GET | `/api/categories/{id}` | Get category by ID | ❌ |
| POST | `/api/categories` | Create category | 🔒 Admin |
| PUT | `/api/categories/{id}` | Update category | 🔒 Admin |
| DELETE | `/api/categories/{id}` | Delete category (fails if has products) | 🔒 Admin |

### Query Parameters (Products)

| Parameter | Type | Description |
|-----------|------|-------------|
| `page` | int | Page number (default: 1) |
| `pageSize` | int | Items per page (default: 10) |
| `search` | string | Search by product name |
| `categoryId` | int | Filter by category |
| `sortBy` | string | Sort field (name, price) |
| `sortDesc` | bool | Sort descending |

---

## Testing

The project includes **21 unit tests** using xUnit, Moq, and FluentAssertions:

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --verbosity normal
```

### Test Coverage

| Controller | Tests | Scenarios |
|------------|-------|-----------|
| **ProductsController** | 11 | GetAll, GetById, Create, Update, Delete (success + failure cases) |
| **CategoriesController** | 10 | GetAll, GetById, Create, Update, Delete (success + failure + has-products) |

---

## Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `ConnectionStrings__DefaultConnection` | MySQL connection string | `Server=localhost;Port=3306;Database=TestApiDb;User=root;Password=` |
| `Jwt__Key` | JWT signing key (min 32 chars) | Set in appsettings.json |
| `Jwt__Issuer` | JWT token issuer | `TestApi` |
| `Jwt__Audience` | JWT token audience | `TestApiUsers` |
| `ASPNETCORE_ENVIRONMENT` | Runtime environment | `Development` |

---

## Author

**Mohamed Ezzalregal**

- [LinkedIn](https://www.linkedin.com/in/mohamedayman-469994243)
- [GitHub](https://github.com/Mohamed-Ezzalregal)
- mohaamed.ezzalregal@gmail.com
