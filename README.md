# E-Commerce API

A RESTful API built with ASP.NET Core 8 for managing products and categories, with JWT authentication.

## Tech Stack

- ASP.NET Core 8 Web API
- Entity Framework Core
- MySQL (Pomelo Provider)
- JWT Authentication
- Swagger / OpenAPI

## Features

- User Registration & Login with JWT tokens
- Full CRUD for Products and Categories
- One-to-Many relationship (Category → Products)
- Data validation using Data Annotations
- Swagger UI for API testing

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- MySQL Server

### Setup

1. Clone the repository
   ```bash
   git clone https://github.com/YOUR_USERNAME/ecommerce-api.git
   cd ecommerce-api/TestApi
   ```

2. Update the connection string in `appsettings.json`
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost;Database=YourDbName;User=root;Password=YOUR_PASSWORD"
   }
   ```

3. Apply migrations
   ```bash
   dotnet ef database update
   ```

4. Run the project
   ```bash
   dotnet run
   ```

5. Open Swagger UI at `https://localhost:5001/swagger`

## API Endpoints

### Auth

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/register` | Register a new user |
| POST | `/api/auth/login` | Login and get JWT token |

### Products

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/products` | Get all products |
| GET | `/api/products/{id}` | Get product by ID |
| POST | `/api/products` | Create a new product |
| PUT | `/api/products/{id}` | Update a product |
| DELETE | `/api/products/{id}` | Delete a product |

### Categories

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/categories` | Get all categories |
| GET | `/api/categories/{id}` | Get category by ID |
| POST | `/api/categories` | Create a new category |
| PUT | `/api/categories/{id}` | Update a category |
| DELETE | `/api/categories/{id}` | Delete a category |

## Project Structure

```
TestApi/
├── Controllers/        # API endpoints
├── Models/             # Database entities
├── DTOs/               # Data Transfer Objects
├── Data/               # DbContext
├── Services/           # Business logic
├── Migrations/         # EF Core migrations
└── Program.cs          # App configuration
```
