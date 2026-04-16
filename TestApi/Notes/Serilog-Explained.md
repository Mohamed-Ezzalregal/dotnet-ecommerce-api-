# Serilog - شرح كامل

## يعني إيه Logging؟

الـ Logging ببساطة هو إنك بتسجل كل حاجة بتحصل في الأبليكيشن بتاعك — مين عمل إيه، إمتى، وإيه اللي حصل.
ده بيساعدك جداً في:
- **Debugging**: لما يكون فيه مشكلة في Production وعايز تعرف إيه اللي حصل.
- **Monitoring**: تتابع الأبليكيشن شغال كويس ولا لأ.
- **Auditing**: تعرف مين عمل إيه، زي مين عمل Login أو مين حذف Product.

---

## الفرق بين Built-in ILogger و Serilog

### Built-in ILogger (اللي جاي مع .NET)

```csharp
var builder = WebApplication.CreateBuilder(args);
// ILogger شغال تلقائي — مش محتاج تعمل حاجة
```

- جاي مع .NET بشكل افتراضي.
- بيكتب في الـ Console بس.
- مفيش تحكم كبير في الـ Output (مش بيعمل ملفات لوج مثلاً).
- مش بيدعم **Structured Logging** بشكل قوي.

### Serilog (اللي إحنا استخدمناه)

```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();
```

- مكتبة خارجية قوية جداً.
- تقدر تكتب في **Console + Files + Database + Cloud** وغيرهم.
- بيدعم **Structured Logging** — يعني بيحفظ الداتا كـ Properties مش مجرد نص.
- بيدعم **Rolling Files** — كل يوم ملف لوج جديد.
- بيدعم **Sinks** متعددة (هنشرحها تحت).

### ليه اخترنا Serilog؟

| الميزة | Built-in ILogger | Serilog |
|--------|-----------------|---------|
| Console Output | ✅ | ✅ |
| File Output | ❌ (محتاج مكتبة) | ✅ |
| Structured Logging | جزئي | ✅ كامل |
| Rolling Files | ❌ | ✅ |
| Database/Cloud Sinks | ❌ | ✅ |
| تخصيص Format | محدود | ✅ كامل |
| سهولة الإعداد | سهل (تلقائي) | سهل (كود قليل) |

---

## يعني إيه Structured Logging؟

### الطريقة العادية (String Interpolation) — مش مستحبة:
```csharp
_logger.LogInformation($"Product created: {product.Id} - {product.Name}");
// الناتج: "Product created: 5 - iPhone 15"
// مجرد نص — مش تقدر تعمل عليه Search بسهولة
```

### Structured Logging (اللي إحنا بنستخدمه) — الأحسن:
```csharp
_logger.LogInformation("Product created: {ProductId} - {ProductName}", product.Id, product.Name);
// الناتج: "Product created: 5 - iPhone 15"
// لكن كمان بيتحفظ كـ Properties: ProductId=5, ProductName="iPhone 15"
```

**الفرق الجوهري**: في الـ Structured Logging، القيم بتتحفظ كـ Properties منفصلة.
يعني تقدر بعدين تعمل:
- Search: "هاتلي كل اللوجات اللي فيها ProductId = 5"
- Filter بالـ Properties بدل ما تعمل string search

---

## Log Levels (مستويات اللوج)

الـ Log Levels بتحدد أهمية الرسالة. مرتبة من الأقل للأكتر أهمية:

| Level | الاستخدام | مثال |
|-------|----------|------|
| `Verbose` | تفاصيل كتير جداً (للـ Debugging العميق) | "Entering method X with param Y" |
| `Debug` | معلومات للمطورين | "Query returned 15 results" |
| `Information` | أحداث عادية ومهمة | "Product created: 5" ✅ (إحنا بنستخدم ده) |
| `Warning` | حاجة غريبة بس مش Error | "Request took 5 seconds" |
| `Error` | حاجة غلط حصلت | "Database connection failed" |
| `Fatal` | الأبليكيشن هيقع | "Unhandled exception — application shutting down" |

### إحنا استخدمنا:
```csharp
// في الـ Controllers — للأحداث العادية
_logger.LogInformation("Product created: {ProductId} - {ProductName}", product.Id, product.Name);

// في الـ ExceptionMiddleware — للأخطاء
_logger.LogError(ex, "Unhandled exception on {Method} {Path}", context.Request.Method, context.Request.Path);

// في Program.cs — للأخطاء القاتلة
Log.Fatal(ex, "Application terminated unexpectedly");
```

### MinimumLevel
```csharp
.MinimumLevel.Information()
```
ده معناه إن أي حاجة أقل من `Information` (زي `Debug` و `Verbose`) مش هتتسجل.
لو عايز تشوف كل حاجة: `.MinimumLevel.Verbose()`

---

## Sinks (أماكن كتابة اللوج)

الـ **Sink** في Serilog يعني "المكان اللي اللوج بيتكتب فيه".

### الـ Sinks اللي إحنا استخدمناها:

#### 1. Console Sink
```csharp
.WriteTo.Console()
```
بيكتب اللوج في الـ Terminal — مفيد وأنت بتطور.

#### 2. File Sink
```csharp
.WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
```
- بيكتب اللوج في ملفات في فولدر `Logs/`.
- `rollingInterval: RollingInterval.Day` — كل يوم ملف جديد (مثلاً `log-20250115.txt`).

### Sinks تانية ممكن تستخدمها (مش محتاجها دلوقتي):

| Sink | الوصف | الـ Package |
|------|-------|-------------|
| **Seq** | Dashboard جميل للـ Logs | `Serilog.Sinks.Seq` |
| **Elasticsearch** | للـ Full-text Search على اللوجات | `Serilog.Sinks.Elasticsearch` |
| **SQL Server** | يحفظ اللوجات في Database | `Serilog.Sinks.MSSqlServer` |
| **Email** | يبعتلك Email لو Fatal Error | `Serilog.Sinks.Email` |
| **Slack/Teams** | يبعت Alert على الشات | `Serilog.Sinks.Slack` |

---

## UseSerilogRequestLogging

```csharp
app.UseSerilogRequestLogging();
```

ده Middleware بيسجل كل HTTP Request تلقائي — مش محتاج تعمله يدوي.
بيسجل:
- الـ HTTP Method (GET, POST, PUT, DELETE)
- الـ Path (/api/products)
- الـ Status Code (200, 404, 500)
- الـ Response Time (كام ميلي ثانية)

مثال على الـ Output:
```
HTTP GET /api/products responded 200 in 45.2ms
HTTP POST /api/products responded 201 in 120.5ms
```

### الفرق بينه وبين الـ Logging اليدوي:

| | UseSerilogRequestLogging | الـ Logging اليدوي |
|--|-------------------------|-------------------|
| **إيه بيسجل** | كل Request/Response تلقائي | اللي أنت بتحدده |
| **التفاصيل** | Method, Path, Status, Time | أي حاجة أنت عايزها |
| **الجهد** | سطر واحد | محتاج تكتب في كل Action |
| **الاستخدام** | Monitoring عام | Business Logic Events |

الاتنين **مكملين بعض** — مش بديل لبعض.

---

## الـ Pattern اللي إحنا استخدمناه في Program.cs

```csharp
// 1. إعداد الـ Logger الأول قبل أي حاجة
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Starting web application");
    
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog(); // استبدل الـ Built-in Logger بـ Serilog
    
    // ... باقي الإعدادات
    
    var app = builder.Build();
    app.UseSerilogRequestLogging(); // سجل كل Request تلقائي
    
    // ... باقي الـ Middleware
    
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush(); // تأكد إن كل اللوجات اتكتبت قبل ما الأبليكيشن يقفل
}
```

### ليه try/catch/finally؟
- **try**: الأبليكيشن العادي بيشتغل جوا.
- **catch**: لو حصل أي Exception غير متوقع أثناء بدء التشغيل، بيتسجل كـ `Fatal`.
- **finally**: `Log.CloseAndFlush()` بيتأكد إن كل اللوجات اللي في الـ Buffer اتكتبت فعلاً في الملفات قبل ما الأبليكيشن يقفل.

---

## بدائل Serilog

| المكتبة | الميزة | العيب |
|---------|--------|------|
| **Built-in ILogger** | جاي مع .NET — مش محتاج تثبيت | محدود — مفيش File Sink |
| **Serilog** ✅ | الأشهر والأقوى — Structured + Sinks كتير | محتاج تثبيت Package |
| **NLog** | قديم وقوي — XML Config | أقل شعبية من Serilog |
| **log4net** | من أقدم المكتبات | قديم ومعقد — مش مستحب |
| **Microsoft.Extensions.Logging** | الـ Abstraction Layer | مش مكتبة Logging — ده Interface بس |

### ملحوظة مهمة:
`ILogger<T>` اللي بنستخدمه في الـ Controllers هو **Microsoft.Extensions.Logging.ILogger** — ده الـ Interface.
Serilog بيشتغل **وراه** كـ Implementation. يعني الكود بتاعك مش مربوط بـ Serilog — لو عايز تغير لـ NLog مثلاً، مش هتغير حاجة في الـ Controllers.

---

## الملفات اللي اتعدلت

| الملف | التعديل |
|-------|---------|
| `Program.cs` | إعداد Serilog + try/catch/finally + UseSerilogRequestLogging |
| `ExceptionMiddleware.cs` | Structured Logging بالـ Method + Path |
| `ProductsController.cs` | ILogger + Log في Create/Update/Delete |
| `CategoriesController.cs` | ILogger + Log في Create/Update/Delete |
| `AuthController.cs` | ILogger + Log في Register/Login |
| `.gitignore` | إضافة `TestApi/Logs/` |
