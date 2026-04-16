# 🔐 Swagger JWT + Role-Based Auth + Custom Exceptions — شرح

---

## 1. Swagger JWT Bearer Support

### إيه اللي اتعمل؟
ضفنا في `Program.cs` جوه `AddSwaggerGen()`:
- **`AddSecurityDefinition("Bearer", ...)`** — بيعرّف Swagger إن فيه نوع Authentication اسمه Bearer (JWT).
- **`AddSecurityRequirement(...)`** — بيقول لـ Swagger إن كل الـ Endpoints ممكن تحتاج Token.

### النتيجة
بقى فيه زر **Authorize** في Swagger UI — بتحط فيه الـ JWT Token وبتقدر تجرب الـ protected endpoints مباشرة.

### الفرق بين الطرق المختلفة

| الطريقة | الوصف |
|---------|-------|
| `SecuritySchemeType.Http` ✅ | النوع اللي استخدمناه — بتحط الـ Token بس من غير كلمة "Bearer" |
| `SecuritySchemeType.ApiKey` | بتحط الـ Token كامل في Header يدوي |
| `SecuritySchemeType.OAuth2` | لو بتستخدم OAuth2 مع Google, Facebook, إلخ |
| `SecuritySchemeType.OpenIdConnect` | لو بتستخدم OpenID Connect |

### الفرق بين `Scheme = "Bearer"` و `Scheme = "Basic"`

| Scheme | الوصف |
|--------|-------|
| `"Bearer"` ✅ | JWT Token — أكتر طريقة مستخدمة في APIs |
| `"Basic"` | Username:Password encoded بـ Base64 — قديمة ومش آمنة |

---

## 2. Role-Based Authorization

### إيه اللي اتعمل؟
غيّرنا `[Authorize]` لـ `[Authorize(Roles = "Admin")]` على الـ Create/Update/Delete endpoints.

### الفرق بين أنواع Authorization

| النوع | المثال | الوصف |
|-------|--------|-------|
| `[Authorize]` | أي حد عامل Login | بيتحقق إن عنده Token صالح بس |
| `[Authorize(Roles = "Admin")]` ✅ | Admin بس | بيتحقق إن الـ Role في الـ Token = "Admin" |
| `[Authorize(Roles = "Admin,User")]` | Admin أو User | أي واحد من الاتنين |
| `[Authorize(Policy = "MinAge18")]` | Policy-Based | صلاحيات معقدة بناءً على Claims |
| `[AllowAnonymous]` | أي حد | بيلغي الـ `[Authorize]` لـ endpoint معين |

### Authentication vs Authorization

| المصطلح | السؤال | المثال |
|---------|--------|--------|
| **Authentication** | مين أنت؟ | Login → JWT Token |
| **Authorization** | إيه المسموحلك؟ | Admin يقدر يمسح، User لأ |

### إزاي الـ Role بيتبعت في الـ Token؟
في `AuthController.GenerateToken()` بنضيف:
```csharp
new Claim(ClaimTypes.Role, user.Role)
```
لما الـ ASP.NET بيقرأ الـ Token وبيلاقي `[Authorize(Roles = "Admin")]`، بيروح يدوّر على `ClaimTypes.Role` في الـ Token ويشوف لو قيمته تساوي `"Admin"`.

---

## 3. Custom Exceptions

### إيه اللي اتعمل؟
- عملنا `NotFoundException` — لما حاجة مش موجودة (404)
- عملنا `BadRequestException` — لما البيانات غلط (400)
- عدّلنا `ExceptionMiddleware` يفرّق بين الأنواع

### الفرق بين الـ Approach القديم والجديد

#### قبل (Manual Returns):
```csharp
var product = await _unitOfWork.Products.GetByIdAsync(id);
if (product is null) return NotFound("Product not found");
```
- كل Controller لازم يعمل الـ check ده يدوي
- لو نسيت في endpoint واحد — هيرجع 500 بدل 404

#### بعد (Custom Exceptions):
```csharp
var product = await _unitOfWork.Products.GetByIdAsync(id);
if (product is null) throw new NotFoundException("Product not found");
```
- الـ `ExceptionMiddleware` بيمسك الـ Exception ويرجع الـ Status Code الصح
- مكان واحد بيتحكم في format الـ Error Response

### الفرق بين الـ Status Codes

| Status Code | الاسم | معناه | الـ Exception |
|-------------|-------|-------|---------------|
| `200` | OK | تمام | — |
| `400` | Bad Request | بيانات غلط | `BadRequestException` |
| `401` | Unauthorized | مش عامل Login | `UnauthorizedAccessException` |
| `403` | Forbidden | عامل Login بس مش Admin | ASP.NET automatic |
| `404` | Not Found | مش موجود | `NotFoundException` |
| `500` | Internal Server Error | مشكلة في السيرفر | أي Exception تاني |

### الفرق بين `switch expression` و `if-else`

#### switch expression ✅ (اللي استخدمناه):
```csharp
context.Response.StatusCode = ex switch
{
    NotFoundException => 404,
    BadRequestException => 400,
    _ => 500
};
```
- أنظف وأقصر
- C# 8+ feature

#### if-else (البديل):
```csharp
if (ex is NotFoundException)
    context.Response.StatusCode = 404;
else if (ex is BadRequestException)
    context.Response.StatusCode = 400;
else
    context.Response.StatusCode = 500;
```
- نفس النتيجة بس كود أطول

### ليه `message` بيفرّق؟
```csharp
message = ex is NotFoundException || ex is BadRequestException || ex is UnauthorizedAccessException
    ? ex.Message           // رسالة واضحة للـ Client
    : "An unexpected error occurred."  // رسالة عامة — مبنكشفش تفاصيل السيرفر
```
- لو الـ Exception **متعمد** (NotFoundException, BadRequestException) → نبعت الرسالة الحقيقية لأنها مفيدة للـ Client
- لو الـ Exception **مش متوقع** (NullReference, Database Error) → نبعت رسالة عامة عشان الأمان — مش عايزين نكشف Stack Trace أو Database Schema

---

## ملخص الملفات اللي اتعدّلت

| الملف | التعديل |
|-------|---------|
| `Program.cs` | Swagger JWT Bearer Support |
| `Middlewares/ExceptionMiddleware.cs` | بيفرّق بين أنواع الـ Exceptions |
| `Exceptions/NotFoundException.cs` | **جديد** — Custom Exception لـ 404 |
| `Exceptions/BadRequestException.cs` | **جديد** — Custom Exception لـ 400 |
| `Controllers/ProductsController.cs` | `[Authorize(Roles = "Admin")]` + `throw` بدل `return` |
| `Controllers/CategoriesController.cs` | `[Authorize(Roles = "Admin")]` + `throw` بدل `return` |
