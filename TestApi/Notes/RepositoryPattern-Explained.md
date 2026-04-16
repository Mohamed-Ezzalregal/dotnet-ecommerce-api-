# Repository Pattern + Unit of Work — شرح كامل

---

## 🤔 إيه هو الـ Repository Pattern؟

الـ **Repository Pattern** هو طبقة (Layer) بتقعد بين الـ **Controller** وبين الـ **Database (DbContext)**.

### المشكلة اللي بيحلّها:
قبل كده، الـ Controller كان بيتكلم مع الـ `AppDbContext` مباشرة:

```
Controller → AppDbContext → Database
```

**ده بيسبب مشاكل:**
1. **الكود متكرر** — لو عندك 5 Controllers كلهم بيعملوا `FindAsync(id)` و `ToListAsync()`
2. **صعب تعمل Unit Testing** — لأنك مش هتقدر تعمل Mock لـ DbContext بسهولة
3. **الـ Controller عارف تفاصيل كتير** — المفروض ما يعرفش إنك بتستخدم MySQL أو SQL Server
4. **لو غيرت الـ Database** — هتحتاج تغير في كل Controller

### الحل:

```
Controller → IUnitOfWork → Repository → AppDbContext → Database
```

الـ Controller بيتكلم مع **Interface** بس، مش عارف حاجة عن الـ Database.

---

## 📁 الملفات اللي عملناها

### الـ Structure:

```
TestApi/
├── Interfaces/
│   ├── IRepository.cs          ← Generic Interface (العمليات المشتركة)
│   ├── ICategoryRepository.cs  ← Interface خاص بالـ Categories
│   ├── IProductRepository.cs   ← Interface خاص بالـ Products
│   └── IUnitOfWork.cs          ← بيجمع كل الـ Repositories مع بعض
├── Repositories/
│   ├── Repository.cs           ← Generic Implementation
│   ├── CategoryRepository.cs   ← Implementation خاص بالـ Categories
│   ├── ProductRepository.cs    ← Implementation خاص بالـ Products
│   └── UnitOfWork.cs           ← بيدير الـ Repositories والـ SaveChanges
```

---

## 🔍 شرح كل ملف بالتفصيل

### 1. `IRepository<T>` — الـ Generic Interface

```csharp
public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task AddAsync(T entity);
    void Update(T entity);
    void Remove(T entity);
}
```

**ليه Generic؟**
- `where T : class` → يعني T لازم يكون class (زي Product, Category, User)
- بدل ما تكتب Interface لكل Model، بتكتب واحد عام يشتغل مع أي حاجة
- الـ `T` بيتبدل بالـ Model لما تعمل implement

**ليه `IEnumerable<T>` مش `List<T>`؟**
| `List<T>` | `IEnumerable<T>` |
|-----------|-------------------|
| نوع محدد (Concrete Type) | Interface (Abstraction) |
| بيحمّل كل الداتا في الـ Memory مرة واحدة | ممكن يكون Lazy (بيحمّل لما تحتاج) |
| فيه Methods زيادة: `Add()`, `Remove()`, `Sort()` | بس `foreach` و LINQ |
| مناسب لما تحتاج تعدّل في الـ Collection | مناسب لما بتقرأ بس (Read-only) |

**في الـ Repository الـ return type هو `IEnumerable<T>`** عشان:
- الـ Controller مش محتاج يعمل `Add()` أو `Remove()` على الـ List — ده شغل الـ Repository
- بتدي مرونة أكتر — ممكن ترجع `List` أو `Array` أو أي حاجة بتـ implement `IEnumerable`

**ليه `AddAsync` بـ `Task` و `Update`/`Remove` من غير `Task`؟**
- `AddAsync` → بتعمل `await _dbSet.AddAsync(entity)` — EF Core ممكن يحتاج يروح يجيب الـ ID من الـ Database
- `Update` و `Remove` → بس بيغيّروا الـ State في الـ Memory (مش بيروحوا الـ Database) — الـ Database بتتحدث لما تعمل `SaveChangesAsync()`

---

### 2. `ICategoryRepository` — الـ Specific Interface

```csharp
public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetByIdWithProductsAsync(int id);
    Task<bool> HasProductsAsync(int id);
}
```

**ليه عملنا Interface تاني مع إن عندنا `IRepository<Category>`؟**
- `IRepository<T>` فيه العمليات الـ **مشتركة** بين كل الـ Models
- `ICategoryRepository` فيه العمليات الـ **خاصة** بالـ Categories بس
- مثال: `HasProductsAsync` → دي عملية مالهاش معنى في Products أو Users
- مثال: `GetByIdWithProductsAsync` → بتجيب Category ومعاها الـ Products بتاعتها (Include)

---

### 3. `Repository<T>` — الـ Generic Implementation

```csharp
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;
    ...
}
```

**ليه `protected` مش `private`؟**
| `private` | `protected` |
|-----------|-------------|
| الـ Class نفسه بس يقدر يوصلّه | الـ Class نفسه + أي Class وارث منه |
| `CategoryRepository` مش هيقدر يستخدم `_context` | `CategoryRepository` يقدر يستخدم `_context` |

احنا محتاجين `protected` عشان `CategoryRepository` و `ProductRepository` وارثين من `Repository<T>` ومحتاجين يوصلوا لـ `_context` عشان يعملوا Queries خاصة (زي `Include`).

**ليه `DbSet<T> _dbSet`؟**
- `context.Set<T>()` → بترجع الـ `DbSet` الصح حسب الـ `T`
- لو `T` = `Product` → `context.Set<Product>()` = نفس `context.Products`
- بدل ما تكتب `_context.Products` في كل method، بتستخدم `_dbSet` العامة

---

### 4. `UnitOfWork` — بيجمع كل حاجة مع بعض

```csharp
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public ICategoryRepository Categories { get; private set; }
    public IProductRepository Products { get; private set; }

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        Categories = new CategoryRepository(context);
        Products = new ProductRepository(context);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
```

**ليه `IDisposable` و `Dispose()`؟**
- الـ `AppDbContext` بيفتح Connection مع الـ Database
- لو ما قفلتهاش، هتفضل مفتوحة وتسبب **Memory Leak**
- `Dispose()` بيقفل الـ Connection لما الـ Request يخلص
- ASP.NET بيعمل `Dispose()` تلقائي لأي حاجة `Scoped`

**ليه `SaveChangesAsync()` في الـ UnitOfWork مش في الـ Repository؟**

ده أهم نقطة! تخيل السيناريو ده:

```
1. أضف Product جديد
2. حدّث Category
3. احفظ كل حاجة مرة واحدة
```

**لو SaveChanges في كل Repository:**
```csharp
await _productRepo.AddAsync(product);
await _productRepo.SaveChangesAsync();  // ← حفظ أول
await _categoryRepo.Update(category);
await _categoryRepo.SaveChangesAsync();  // ← حفظ تاني
// لو الحفظ التاني فشل، الأول اتحفظ خلاص — مفيش Rollback!
```

**لو SaveChanges في UnitOfWork:**
```csharp
await _unitOfWork.Products.AddAsync(product);
_unitOfWork.Categories.Update(category);
await _unitOfWork.SaveChangesAsync();  // ← حفظ كل حاجة مرة واحدة
// لو فشل، مفيش حاجة اتحفظت — Atomic Operation!
```

ده اسمه **Atomic Transaction** — يا كل حاجة تنجح، يا مفيش حاجة تتحفظ.

---

## 🔧 التسجيل في Program.cs

```csharp
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
```

**ليه `AddScoped` مش `AddSingleton` أو `AddTransient`؟**

| الطريقة | الـ Lifetime | الاستخدام |
|---------|------------|----------|
| `AddSingleton` | Instance واحد طول ما الـ App شغال | Config, Logging |
| `AddScoped` | Instance واحد **لكل HTTP Request** | DbContext, UnitOfWork, Repositories |
| `AddTransient` | Instance جديد **كل مرة تطلبه** | Services خفيفة مالهاش State |

**`AddScoped` صح هنا عشان:**
- الـ `AppDbContext` أصلاً `Scoped` (EF Core بيسجّله كده)
- الـ `UnitOfWork` لازم يشارك **نفس** الـ `AppDbContext` مع كل الـ Repositories
- لو `Singleton` → كل الـ Users هيشاركوا نفس الـ Context → **كارثة!**
- لو `Transient` → كل Repository ممكن ياخد Context مختلف → الـ SaveChanges مش هيشتغل صح

---

## 📊 التغييرات في الـ Controllers

### قبل (كان بيستخدم DbContext مباشرة):
```csharp
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _db;

    [HttpGet]
    public async Task<ActionResult<List<Category>>> GetAll()
    {
        return Ok(await _db.Categories.ToListAsync());
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category is null) return NotFound("Category not found");
        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
        return Ok("Deleted!");
    }
}
```

### بعد (بيستخدم IUnitOfWork):
```csharp
public class CategoriesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Category>>> GetAll()
    {
        var categories = await _unitOfWork.Categories.GetAllAsync();
        return Ok(categories);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category is null) return NotFound("Category not found");

        if (await _unitOfWork.Categories.HasProductsAsync(id))
            return BadRequest("Cannot delete category that has products.");

        _unitOfWork.Categories.Remove(category);
        await _unitOfWork.SaveChangesAsync();
        return Ok("Deleted!");
    }
}
```

**الفرق:**
1. الـ Controller بقى يعتمد على **Interface** (`IUnitOfWork`) مش **Implementation** (`AppDbContext`)
2. مفيش `using Microsoft.EntityFrameworkCore` في الـ Controller — مش محتاج يعرف عن EF Core
3. أضفنا **CategoryId Validation** في Products (بنتحقق إن الـ Category موجودة)
4. أضفنا **حماية حذف Category** عندها Products

---

## 🆚 بدائل Repository Pattern

### 1. استخدام DbContext مباشرة (اللي كنا بنعمله)
- **مميزاته:** أسرع في الكتابة، أقل كود
- **عيوبه:** تكرار كود، صعب في Testing، الـ Controller عارف تفاصيل الـ Database

### 2. Repository Pattern بدون Unit of Work
- **مميزاته:** أبسط، كل Repository مستقل
- **عيوبه:** كل Repository ليه SaveChanges خاص — مفيش Atomic Transactions

### 3. Repository Pattern + Unit of Work (اللي عملناه)
- **مميزاته:** فصل المسؤوليات، سهل في Testing، Atomic Transactions
- **عيوبه:** كود أكتر، Abstraction زيادة

### 4. CQRS (Command Query Responsibility Segregation)
- مستوى أعلى — بيفصل عمليات القراءة عن الكتابة بالكامل
- هنعمله بعدين في المرحلة 3

### 5. استئجار الـ DbContext كـ Service مباشرة من غير Repository
- بعض المطورين بيقولوا إن EF Core في حد ذاته هو Repository Pattern
- صح تقنياً، لكن في الشغل الشركات بتفضّل الـ Repository Pattern عشان:
  - Testing أسهل
  - الكود أنضف
  - فصل المسؤوليات أوضح

---

## 🎯 الخلاصة

| المفهوم | الوظيفة |
|---------|---------|
| `IRepository<T>` | عقد العمليات المشتركة (CRUD) |
| `ICategoryRepository` | عقد العمليات الخاصة بالـ Categories |
| `Repository<T>` | التنفيذ العام للعمليات المشتركة |
| `CategoryRepository` | التنفيذ الخاص بالـ Categories |
| `IUnitOfWork` | بيجمع الـ Repositories + SaveChanges |
| `UnitOfWork` | التنفيذ — بيدير الـ Context والـ Repositories |

**القاعدة الذهبية:** الـ Controller يعرف بس عن **Interfaces** — مش عارف حاجة عن EF Core أو Database.
