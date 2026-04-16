# Clean Architecture - شرح كامل

## يعني إيه Clean Architecture؟

الـ Clean Architecture هو **طريقة لتنظيم المشروع** بحيث كل جزء يكون مسؤول عن حاجة واحدة بس، ومفيش تداخل بين الأجزاء.

الفكرة الأساسية: **الـ Business Logic (الكود المهم)** مش بيعتمد على حاجة خارجية زي الـ Database أو الـ Framework.

---

## ليه محتاجين Clean Architecture؟

### قبل (كل حاجة في مشروع واحد):
```
TestApi/
├── Controllers/
├── Models/
├── DTOs/
├── Interfaces/
├── Repositories/
├── Data/
├── Exceptions/
├── Mapping/
├── Middlewares/
├── Services/
└── Program.cs
```

**المشاكل:**
- كل حاجة مربوطة ببعض — لو غيرت الـ Database لازم تغير في كل مكان.
- مفيش فصل واضح بين الـ Business Logic والـ Infrastructure.
- صعب تعمل Unit Testing لأن الكود مربوط بالـ Database مباشرة.
- لما المشروع يكبر، بيبقى صعب تلاقي حاجة.

### بعد (Clean Architecture — 4 مشاريع):
```
TestApi.sln
├── TestApi.Domain/          ← الطبقة الداخلية (Pure Models)
├── TestApi.Application/     ← الـ Business Logic (DTOs, Interfaces, Mapping)
├── TestApi.Infrastructure/  ← التنفيذ (Database, Repositories)
└── TestApi/                 ← الـ API (Controllers, Program.cs)
```

**المميزات:**
- كل Layer مسؤول عن حاجة واحدة.
- تقدر تغير الـ Database من MySQL لـ SQL Server من غير ما تمس الـ Controllers.
- الـ Unit Testing أسهل بكتير.
- الكود منظم وسهل تلاقي أي حاجة.

---

## الـ 4 Layers بالتفصيل

### 1. TestApi.Domain (الطبقة الداخلية)

```
TestApi.Domain/
└── Models/
    ├── Category.cs
    ├── Product.cs
    └── User.cs
```

**المسؤوليات:**
- الـ Models (Entities) بس — تمثيل الداتا.
- **مفيش أي dependency** على أي مشروع تاني.
- أنقى Layer — مفيش using لحاجة خارجية.

**مثال:**
```csharp
namespace TestApi.Domain.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
}
```

**ليه Models في Domain؟**
لأن الـ Models هي أساس كل حاجة — كل Layer تاني بيستخدمها. ولو حطيناها في مكان تاني، هنعمل Circular Dependency.

---

### 2. TestApi.Application (طبقة الـ Business Logic)

```
TestApi.Application/
├── DTOs/
│   ├── CreateProductDto.cs
│   ├── CreateCategoryDto.cs
│   ├── ProductDto.cs
│   ├── CategoryDto.cs
│   ├── LoginDto.cs
│   ├── RegisterDto.cs
│   ├── ProductQueryParameters.cs
│   └── PagedResult.cs
├── Interfaces/
│   ├── IRepository.cs
│   ├── ICategoryRepository.cs
│   ├── IProductRepository.cs
│   └── IUnitOfWork.cs
├── Mapping/
│   └── MappingProfile.cs
└── Exceptions/
    ├── NotFoundException.cs
    └── BadRequestException.cs
```

**المسؤوليات:**
- **DTOs** — الكلاسات اللي بتتبعت وبتترجع من الـ API.
- **Interfaces** — العقود (Contracts) اللي الـ Repositories لازم تنفذها.
- **Mapping** — تحويل بين Models و DTOs.
- **Exceptions** — Custom Exceptions بتاعة الأبليكيشن.
- **يعتمد على:** `TestApi.Domain` بس.

**ليه Interfaces في Application مش Domain؟**
لأن الـ Interfaces بتستخدم DTOs (زي `ProductQueryParameters`). والـ Domain Layer لازم يفضل نقي بدون أي dependency.
كمان في Clean Architecture، الـ Application Layer هي اللي بتحدد "الأبليكيشن محتاج إيه" والـ Infrastructure بتقول "أنا هنفذ".

---

### 3. TestApi.Infrastructure (طبقة التنفيذ)

```
TestApi.Infrastructure/
├── Data/
│   └── AppDbContext.cs
├── Repositories/
│   ├── Repository.cs
│   ├── CategoryRepository.cs
│   ├── ProductRepository.cs
│   └── UnitOfWork.cs
├── Migrations/
│   ├── 20260319150544_InitialCreate.cs
│   ├── 20260327201814_AddUserTable.cs
│   └── AppDbContextModelSnapshot.cs
└── DependencyInjection.cs
```

**المسؤوليات:**
- **AppDbContext** — التعامل مع الـ Database.
- **Repositories** — التنفيذ الفعلي للـ Interfaces.
- **Migrations** — تغييرات الـ Database.
- **DependencyInjection.cs** — تسجيل الـ Services بتاعة Infrastructure.
- **يعتمد على:** `TestApi.Application` (وعن طريقه `TestApi.Domain`).

**DependencyInjection.cs — Extension Method:**
```csharp
namespace TestApi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<AppDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
```

**ده بيخلي Program.cs نضيف — سطر واحد بدل 5:**
```csharp
// قبل (كل حاجة في Program.cs):
builder.Services.AddDbContext<AppDbContext>(...);
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// بعد (Clean Architecture):
builder.Services.AddInfrastructure(builder.Configuration);
```

---

### 4. TestApi (طبقة الـ API / Presentation)

```
TestApi/
├── Controllers/
│   ├── AuthController.cs
│   ├── CategoriesController.cs
│   └── ProductsController.cs
├── Middlewares/
│   └── ExceptionMiddleware.cs
├── Services/
│   └── MyService.cs
├── Program.cs
├── appsettings.json
└── appsettings.Development.json
```

**المسؤوليات:**
- **Controllers** — الـ Endpoints اللي المستخدم بيكلمها.
- **Middlewares** — كود بيشتغل على كل Request.
- **Program.cs** — إعداد الأبليكيشن.
- **يعتمد على:** `TestApi.Application` + `TestApi.Infrastructure`.

---

## Dependency (مين بيعتمد على مين)

```
TestApi.Domain          → لا يعتمد على حد (pure)
TestApi.Application     → يعتمد على Domain
TestApi.Infrastructure  → يعتمد على Application (+ Domain بالتبعية)
TestApi (API)          → يعتمد على Application + Infrastructure
```

**القاعدة الذهبية:**
- الـ Dependencies دايماً بتمشي **للداخل** (من API → Domain).
- **أبداً** مش بتمشي للبرا (Domain مبيعرفش عن API أو Database).

```
┌─────────────────────────────────┐
│           TestApi (API)          │  ← الطبقة الخارجية
│  ┌───────────────────────────┐  │
│  │    Infrastructure         │  │
│  │  ┌─────────────────────┐  │  │
│  │  │    Application      │  │  │
│  │  │  ┌───────────────┐  │  │  │
│  │  │  │    Domain     │  │  │  │  ← النواة
│  │  │  └───────────────┘  │  │  │
│  │  └─────────────────────┘  │  │
│  └───────────────────────────┘  │
└─────────────────────────────────┘
```

---

## بدائل Clean Architecture

| النمط | الوصف | الإيجابيات | السلبيات |
|-------|-------|------------|----------|
| **Monolithic (اللي كنا فيه)** | كل حاجة في مشروع واحد | سهل وسريع في البداية | بيبقى فوضى لما يكبر |
| **Clean Architecture** ✅ | 4 Layers مفصولين | منظم + Testable + Maintainable | محتاج شغل أكتر في البداية |
| **Onion Architecture** | شبه Clean Architecture تقريباً | نفس المميزات | الفرق بسيط — بيحط الـ Interfaces في Domain |
| **Hexagonal (Ports & Adapters)** | فصل كامل بالـ Ports | مرن جداً | أعقد |
| **N-Tier / Layered** | طبقات (Presentation → Business → Data) | بسيط ومفهوم | الـ Dependencies بتمشي في اتجاه واحد بس |
| **Vertical Slice** | كل Feature لوحده كامل | مفيش طبقات معقدة | ممكن يبقى فيه تكرار |

### الفرق بين Clean Architecture و N-Tier:

| | Clean Architecture | N-Tier |
|--|-------------------|--------|
| **الاتجاه** | الـ Dependencies بتمشي للداخل (Dependency Inversion) | الـ Dependencies بتمشي من فوق لتحت |
| **الـ Domain** | مش بيعتمد على حاجة | بيعتمد على الـ Data Layer |
| **الـ Testability** | سهل جداً (Mock الـ Interfaces) | أصعب (مربوط بالـ Database) |
| **المرونة** | تقدر تغير الـ Database بسهولة | صعب |

---

## الـ NuGet Packages — مين فين؟

| Package | المشروع | السبب |
|---------|---------|-------|
| `Pomelo.EntityFrameworkCore.MySql` | Infrastructure | لأن الـ Database Access في Infrastructure |
| `Microsoft.EntityFrameworkCore.Design` | Infrastructure | للـ Migrations |
| `AutoMapper.Extensions.Microsoft.DependencyInjection` | Application | لأن الـ MappingProfile في Application |
| `Serilog.AspNetCore` | TestApi (API) | لأن Serilog بيتعد في Program.cs |
| `BCrypt.Net-Next` | TestApi (API) | مستخدم في AuthController |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | TestApi (API) | الـ JWT Auth في API layer |

---

## الملفات اللي اتعدلت

| الملف | التعديل |
|-------|---------|
| `Program.cs` | اتغيرت الـ using statements + استخدام `AddInfrastructure()` + `typeof(MappingProfile)` |
| `ProductsController.cs` | `TestApi.DTOs` → `TestApi.Application.DTOs` وهكذا |
| `CategoriesController.cs` | نفس التغييرات |
| `AuthController.cs` | `TestApi.Data` → `TestApi.Infrastructure.Data` |
| `ExceptionMiddleware.cs` | `TestApi.Exceptions` → `TestApi.Application.Exceptions` |
| `TestApi.csproj` | اتشال منه Pomelo + EF Design + AutoMapper (لأنهم في المشاريع التانية) |

## المشاريع الجديدة

| المشروع | النوع | Dependencies |
|---------|-------|-------------|
| `TestApi.Domain` | Class Library | لا يوجد |
| `TestApi.Application` | Class Library | TestApi.Domain |
| `TestApi.Infrastructure` | Class Library | TestApi.Application |
