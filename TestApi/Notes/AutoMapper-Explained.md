# 🔄 AutoMapper — شرح

---

## إيه هو AutoMapper؟
مكتبة بتعمل **تحويل تلقائي** بين Objects — بدل ما تنسخ كل property يدوي، بتقولها "حوّل من النوع ده للنوع ده" وهي بتعمل الباقي.

---

## إيه اللي اتعمل؟

### 1. Response DTOs (`ProductDto`, `CategoryDto`)
بنرجعهم للـ Client **بدل الـ Models مباشرة**.

#### ليه مش بنرجع الـ Model؟
| المشكلة | التفصيل |
|---------|---------|
| **Circular Reference** | `Product` فيه `Category` اللي فيها `List<Product>` — لما الـ JSON Serializer يحاول يحوّلهم بيدخل في لوب لانهائي |
| **أمان** | الـ `User` Model فيه `PasswordHash` — مش عايز ترجعه للـ Client |
| **تحكم** | بتتحكم إيه اللي الـ Client يشوفه — ممكن `ProductDto` يبقى فيه `CategoryName` بس من غير كل الـ Category object |

### 2. MappingProfile
ملف واحد فيه كل الـ Mappings:
```csharp
CreateMap<Product, ProductDto>()
    .ForMember(dest => dest.CategoryName,
               opt => opt.MapFrom(src => src.Category.Name));
```
ده بيقول: لما تحوّل `Product` لـ `ProductDto`، خد `CategoryName` من `src.Category.Name`.

### 3. تسجيل AutoMapper في Program.cs
```csharp
builder.Services.AddAutoMapper(typeof(Program));
```
ده بيدوّر على كل الـ classes اللي بترث من `Profile` (زي `MappingProfile`) ويسجلهم تلقائي.

---

## الفرق بين الـ Approach القديم والجديد

### Create — قبل (Manual):
```csharp
var product = new Product
{
    Name = dto.Name,
    Price = dto.Price,
    Description = dto.Description,
    CategoryId = dto.CategoryId
};
```
لو الـ Model فيه 20 property هتكتب 20 سطر.

### Create — بعد (AutoMapper):
```csharp
var product = _mapper.Map<Product>(dto);
```
**سطر واحد** — AutoMapper بيشوف إن `dto.Name` = `product.Name` (نفس الاسم) فبينسخ تلقائي.

### Update — قبل:
```csharp
product.Name = dto.Name;
product.Price = dto.Price;
product.Description = dto.Description;
product.CategoryId = dto.CategoryId;
```

### Update — بعد:
```csharp
_mapper.Map(dto, product);
```
بيعمل update للـ **existing object** مش بيعمل واحد جديد.

### Response — قبل:
```csharp
return Ok(product);  // بيرجع الـ Model — ممكن circular reference
```

### Response — بعد:
```csharp
var productDto = _mapper.Map<ProductDto>(product);
return Ok(productDto);  // DTO نظيف — فيه CategoryName بس
```

---

## الفرق بين `Map<T>(source)` و `Map(source, destination)`

| الطريقة | الاستخدام | الوصف |
|---------|----------|-------|
| `_mapper.Map<Product>(dto)` ✅ | **Create** | بيعمل **object جديد** من نوع `Product` |
| `_mapper.Map(dto, product)` ✅ | **Update** | بيعمل **update** للـ existing `product` object |
| `_mapper.Map<ProductDto>(product)` ✅ | **Response** | بيعمل **object جديد** من نوع `ProductDto` |
| `_mapper.Map<IEnumerable<ProductDto>>(products)` ✅ | **List Response** | بيحوّل **List كاملة** |

---

## الفرق بين AutoMapper وبدائله

| الطريقة | الوصف |
|---------|-------|
| **AutoMapper** ✅ | مكتبة — Convention-based (بيماتش Properties بالاسم تلقائي) |
| **Mapster** | بديل أسرع — نفس الفكرة بس أخف |
| **Manual Mapping** | بتنسخ كل property يدوي — أبسط بس كود أكتر |
| **Implicit/Explicit Operators** | C# built-in — محدود ومش عملي لـ complex mappings |

### إمتى تستخدم AutoMapper؟
- لما عندك **models كتير** وكل واحد ليه DTO
- لما الـ **properties أسماءها متشابهة** بين الـ Source والـ Destination
- لما عايز **كود أنظف وأقل تكرار**

### إمتى مش لازم AutoMapper؟
- لو عندك **1-2 models** بس — Manual mapping أبسط
- لو الـ **mapping معقد جداً** — ساعات Manual أوضح
- لو محتاج **أقصى performance** — Mapster أسرع

---

## ForMember — لما الأسماء مختلفة

```csharp
CreateMap<Product, ProductDto>()
    .ForMember(dest => dest.CategoryName,
               opt => opt.MapFrom(src => src.Category.Name));
```

### ليه محتاجين `ForMember` هنا؟
- `ProductDto.CategoryName` مفيش حاجة اسمها `CategoryName` في `Product` مباشرة
- لازم نقوله: "خد `CategoryName` من `src.Category.Name`"
- لو الأسماء **متطابقة** (زي `Name`, `Price`, `Description`) — AutoMapper بيعرف لوحده

### أنواع Configuration في ForMember

| الطريقة | الوصف |
|---------|-------|
| `opt.MapFrom(src => src.X)` ✅ | حدد مصدر القيمة |
| `opt.Ignore()` | تجاهل الـ property ده — متحطش فيه حاجة |
| `opt.Condition(src => src.X != null)` | حوّل بس لو الشرط ده true |
| `opt.NullSubstitute("N/A")` | لو القيمة null، حط "N/A" بدلها |

---

## الفرق بين `AddAutoMapper(typeof(Program))` والبدائل

| الطريقة | الوصف |
|---------|-------|
| `AddAutoMapper(typeof(Program))` ✅ | بيدوّر على كل الـ Profiles في نفس الـ Assembly |
| `AddAutoMapper(typeof(MappingProfile))` | نفس النتيجة — بس بتحدد class معين |
| `AddAutoMapper(cfg => { ... })` | بتعمل Configuration inline — مش مستحب |

---

## الفرق بين `IMapper` و `Profile`

| النوع | الدور |
|-------|-------|
| `Profile` | **التعريف** — بتقول فيه "حوّل من A لـ B إزاي" |
| `IMapper` | **التنفيذ** — بتستخدمه في الـ Controller عشان تعمل الـ mapping فعلاً |

---

## ملخص الملفات

| الملف | التعديل |
|-------|---------|
| `TestApi.csproj` | ✨ package: `AutoMapper.Extensions.Microsoft.DependencyInjection` |
| `DTOs/ProductDto.cs` | ✨ **جديد** — Response DTO فيه `CategoryName` |
| `DTOs/CategoryDto.cs` | ✨ **جديد** — Response DTO نظيف |
| `Mapping/MappingProfile.cs` | ✨ **جديد** — كل الـ mappings في مكان واحد |
| `Program.cs` | ➕ `AddAutoMapper(typeof(Program))` |
| `Controllers/ProductsController.cs` | 🔄 `_mapper.Map<>()` بدل manual mapping |
| `Controllers/CategoriesController.cs` | 🔄 نفس الكلام |
| `Controllers/AuthController.cs` | 🔄 `_mapper.Map<User>(dto)` في Register |
