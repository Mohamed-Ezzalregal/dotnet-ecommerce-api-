# 📄 Pagination, Filtering, Sorting — شرح

---

## إيه اللي اتعمل؟

### المشكلة
`GET /api/products` كان بيرجع **كل المنتجات** مرة واحدة. لو عندك 10,000 منتج — هيرجعهم كلهم! ده بيعمل:
- **بطء** — Response كبير جداً
- **استهلاك Memory** — السيرفر بيحمّل كل الداتا
- **تجربة مستخدم سيئة** — الـ Frontend مش محتاج 10,000 منتج مرة واحدة

### الحل
- **Pagination** — بيرجع صفحة واحدة (مثلاً 10 منتجات)
- **Filtering** — بيفلتر حسب الاسم، السعر، الـ Category
- **Sorting** — بيرتب حسب الاسم أو السعر

---

## الملفات الجديدة

### 1. `DTOs/ProductQueryParameters.cs`
ده الـ DTO اللي بييجي من الـ Query String:
```
GET /api/products?page=1&pageSize=10&search=phone&minPrice=100&sortBy=price&sortDescending=true
```

| الـ Property | الوصف | القيمة الافتراضية |
|-------------|-------|------------------|
| `Page` | رقم الصفحة | `1` |
| `PageSize` | عدد العناصر في الصفحة | `10` (max 50) |
| `Search` | بحث في الاسم أو الوصف | `null` |
| `CategoryId` | فلترة حسب التصنيف | `null` |
| `MinPrice` | أقل سعر | `null` |
| `MaxPrice` | أعلى سعر | `null` |
| `SortBy` | ترتيب حسب (`name`, `price`) | `null` (default: Id) |
| `SortDescending` | ترتيب تنازلي | `false` |

### 2. `DTOs/PagedResult<T>.cs`
ده الـ Response اللي بيرجع:
```json
{
    "items": [...],
    "page": 1,
    "pageSize": 10,
    "totalCount": 150,
    "totalPages": 15,
    "hasPreviousPage": false,
    "hasNextPage": true
}
```

الـ `<T>` ده **Generic** — معناه إنك تقدر تستخدمه مع أي نوع:
- `PagedResult<ProductDto>` — للمنتجات
- `PagedResult<CategoryDto>` — للتصنيفات (لو عايز تعمله بعدين)

---

## الفرق بين `[FromQuery]` و `[FromBody]`

| الطريقة | المكان | المثال |
|---------|--------|--------|
| `[FromQuery]` ✅ | Query String في الـ URL | `GET /api/products?page=1&search=phone` |
| `[FromBody]` | في الـ Request Body (JSON) | `POST /api/products` مع JSON |
| `[FromRoute]` | في الـ URL path | `GET /api/products/{id}` |
| `[FromHeader]` | في الـ Request Headers | Authorization header |

بنستخدم `[FromQuery]` هنا لأن الـ GET requests **مش بيبعتوا Body** — البيانات بتروح في الـ URL.

---

## إزاي الـ Filtering بيشتغل

### في `ProductRepository.GetFilteredAsync()`:
```csharp
var products = _context.Products
    .Include(p => p.Category)
    .AsQueryable();
```
بنبدأ بـ **IQueryable** — ده مش بيجيب داتا من الـ Database لسه! بس بيبني الـ SQL Query.

بعدين بنضيف **شروط** واحد واحد:
```csharp
if (!string.IsNullOrWhiteSpace(query.Search))
    products = products.Where(p => p.Name.ToLower().Contains(search));

if (query.CategoryId.HasValue)
    products = products.Where(p => p.CategoryId == query.CategoryId.Value);
```

كل `.Where()` بيضيف **AND** في الـ SQL Query. الـ Database بتنفذ الكل في query واحدة.

---

## الفرق بين `IQueryable` و `IEnumerable`

| النوع | التنفيذ | الاستخدام |
|-------|---------|----------|
| `IQueryable<T>` ✅ | **على الـ Database** — بيبني SQL Query وبيبعتها | فلترة، ترتيب، pagination قبل ما تجيب الداتا |
| `IEnumerable<T>` | **في الـ Memory** — بيجيب كل الداتا الأول وبعدين يفلتر | بعد ما الداتا جات من الـ Database |

### ليه `IQueryable` أحسن هنا؟
```csharp
// IQueryable ✅ — الـ SQL بتعمل WHERE و ORDER BY و LIMIT
var products = _context.Products
    .Where(p => p.Price > 100)   // WHERE Price > 100
    .OrderBy(p => p.Name)         // ORDER BY Name
    .Skip(10).Take(10)            // LIMIT 10 OFFSET 10
    .ToListAsync();               // هنا بس بينفذ

// IEnumerable ❌ — بيجيب كل الـ Products الأول!
var all = await _context.Products.ToListAsync();  // SELECT * FROM Products
var filtered = all.Where(p => p.Price > 100);     // فلترة في Memory — بطيء!
```

---

## الفرق بين `Skip/Take` و بدائل Pagination

| الطريقة | الوصف |
|---------|-------|
| `Skip(n).Take(m)` ✅ | **Offset Pagination** — أبسط وأشهر طريقة |
| **Keyset/Cursor Pagination** | `WHERE Id > lastId` — أسرع مع بيانات كتير جداً |

### `Skip/Take` بيعمل إيه في SQL؟
```sql
SELECT * FROM Products
WHERE Price > 100
ORDER BY Name
LIMIT 10 OFFSET 20   -- Skip 20, Take 10
```

---

## الـ Sorting

```csharp
products = query.SortBy?.ToLower() switch
{
    "name" => query.SortDescending
        ? products.OrderByDescending(p => p.Name)
        : products.OrderBy(p => p.Name),
    "price" => query.SortDescending
        ? products.OrderByDescending(p => p.Price)
        : products.OrderBy(p => p.Price),
    _ => products.OrderBy(p => p.Id)   // default
};
```

### الفرق بين `OrderBy` و `OrderByDescending`

| النوع | الوصف | المثال |
|-------|-------|--------|
| `OrderBy` ✅ | تصاعدي (A → Z, 1 → 100) | الأرخص أول |
| `OrderByDescending` | تنازلي (Z → A, 100 → 1) | الأغلى أول |
| `ThenBy` | ترتيب ثانوي | نفس السعر → رتب بالاسم |

---

## الفرق بين `CountAsync` قبل وبعد الـ Pagination

```csharp
// ✅ قبل — الـ TotalCount بيحسب كل النتائج (بعد الـ Filter وقبل الـ Pagination)
var totalCount = await products.CountAsync();

// بعدين — الـ Pagination
var items = await products.Skip(...).Take(...).ToListAsync();
```
لازم نعمل `CountAsync()` **قبل** `Skip/Take` — عشان نعرف كام صفحة موجودة.

---

## `Tuple` Return — إيه `(IEnumerable<Product>, int)`

```csharp
Task<(IEnumerable<Product> Products, int TotalCount)> GetFilteredAsync(...)
```

### الفرق بين طرق رجوع أكتر من قيمة

| الطريقة | الوصف |
|---------|-------|
| **Tuple** ✅ `(List, int)` | أبسط — لما محتاج ترجع 2-3 قيم |
| **Custom Class** `PagedData<T>` | أنظف — لو الـ return معقد |
| **out parameter** `(out int total)` | قديم — مش مستحب مع async |

---

## أمثلة استخدام الـ API

```
# الصفحة الأولى (10 منتجات)
GET /api/products

# الصفحة التانية
GET /api/products?page=2

# 5 منتجات في الصفحة
GET /api/products?pageSize=5

# بحث
GET /api/products?search=phone

# فلترة حسب Category
GET /api/products?categoryId=1

# فلترة حسب السعر
GET /api/products?minPrice=50&maxPrice=500

# ترتيب بالسعر تنازلي
GET /api/products?sortBy=price&sortDescending=true

# كل حاجة مع بعض
GET /api/products?page=1&pageSize=5&search=phone&categoryId=1&minPrice=50&sortBy=price&sortDescending=true
```

---

## ملخص الملفات

| الملف | التعديل |
|-------|---------|
| `DTOs/ProductQueryParameters.cs` | ✨ **جديد** — Query parameters DTO |
| `DTOs/PagedResult.cs` | ✨ **جديد** — Generic paged response |
| `Interfaces/IProductRepository.cs` | ➕ `GetFilteredAsync()` method |
| `Repositories/ProductRepository.cs` | ➕ Implementation مع Filter + Sort + Pagination |
| `Controllers/ProductsController.cs` | 🔄 `GetAll` بقى بياخد `[FromQuery] ProductQueryParameters` |
