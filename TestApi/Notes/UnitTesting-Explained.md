# Unit Testing — شرح كامل

## إيه هو الـ Unit Testing؟

الـ Unit Testing يعني إنك بتختبر **جزء صغير** من الكود (function / method) بشكل منفصل عن باقي النظام.  
الهدف إنك تتأكد إن كل حاجة شغالة صح **قبل** ما تعمل deploy.

---

## الأدوات اللي استخدمناها

### 1. xUnit (Test Framework)

الـ Framework اللي بنكتب بيه الـ Tests. بنستخدم `[Fact]` عشان نعرّف الـ Test Method.

```csharp
[Fact]
public async Task GetById_ReturnsProduct_WhenExists()
{
    // الكود بتاع الاختبار هنا
}
```

### الفرق بين xUnit وبدائله

| Feature | xUnit | NUnit | MSTest |
|---------|-------|-------|--------|
| **الانتشار** | الأكثر استخداماً في .NET الحديث | شائع جداً | الرسمي من مايكروسوفت |
| **Test Attribute** | `[Fact]` / `[Theory]` | `[Test]` / `[TestCase]` | `[TestMethod]` / `[DataTestMethod]` |
| **Setup** | Constructor | `[SetUp]` method | `[TestInitialize]` method |
| **Cleanup** | `IDisposable` | `[TearDown]` method | `[TestCleanup]` method |
| **Parallel** | ✅ افتراضي | ❌ لازم تفعّله | ❌ لازم تفعّله |
| **Class Instance** | Instance جديد لكل test | نفس الـ Instance | نفس الـ Instance |

**ليه اخترنا xUnit؟**
- الأكثر استخداماً في .NET 8+
- أسرع لأنه بيشغّل Tests بالتوازي
- Class instance جديد لكل Test = مفيش shared state بين الـ Tests

---

### 2. Moq (Mocking Library)

بنستخدم Moq عشان نعمل **objects مزيفة** (mocks) بدل ما نستخدم Database حقيقية.

```csharp
// إنشاء mock لـ IUnitOfWork
var _unitOfWorkMock = new Mock<IUnitOfWork>();

// تحديد سلوك وهمي - لما حد يسأل عن Product بـ ID = 1، رجّعله الـ Product ده
_unitOfWorkMock.Setup(u => u.Products.GetByIdWithCategoryAsync(1))
    .ReturnsAsync(product);

// التأكد إن الـ method اتنادت مرة واحدة
_unitOfWorkMock.Verify(u => u.Products.Remove(product), Times.Once);
```

### الفرق بين Moq وبدائله

| Feature | Moq | NSubstitute | FakeItEasy |
|---------|-----|-------------|------------|
| **Syntax** | `Mock<T>()` + `.Setup()` | `Substitute.For<T>()` | `A.Fake<T>()` |
| **سهولة الاستخدام** | متوسط | سهل جداً | سهل |
| **الانتشار** | الأكثر استخداماً | شائع | أقل انتشاراً |
| **Verification** | `.Verify()` | `.Received()` | `A.CallTo().MustHaveHappened()` |
| **Setup Style** | Lambda expressions | Method call syntax | Method call syntax |

**ليه اخترنا Moq؟**
- الأكثر استخداماً = أكتر أمثلة وحلول أونلاين
- مرن جداً ومعمول لـ complex scenarios
- مطلوب في أغلب الشركات

---

### 3. FluentAssertions (Assertion Library)

بنستخدمها عشان نكتب الـ assertions بطريقة أوضح ومقروءة.

```csharp
// بدون FluentAssertions (الـ built-in في xUnit)
Assert.IsType<OkObjectResult>(result.Result);
Assert.Equal(2, pagedResult.Items.Count());

// مع FluentAssertions — أوضح وأسهل في القراءة
result.Result.Should().BeOfType<OkObjectResult>();
pagedResult.Items.Should().HaveCount(2);
pagedResult.TotalCount.Should().Be(2);
productDto.Name.Should().Be("iPhone");
```

### الفرق بين FluentAssertions والطرق التانية

| Feature | FluentAssertions | xUnit Assert | Shouldly |
|---------|-----------------|--------------|----------|
| **Syntax** | `.Should().Be()` | `Assert.Equal()` | `.ShouldBe()` |
| **قراءة الكود** | ممتازة (طبيعية) | عادية | ممتازة |
| **Error Messages** | مفصلة جداً | أساسية | مفصلة |
| **Chain Assertions** | ✅ `.Should().BeOfType<T>().Subject` | ❌ | ❌ |
| **Collection Support** | `.Should().HaveCount()`, `.Contain()` | `Assert.Collection()` | `.ShouldContain()` |

**ليه اخترنا FluentAssertions؟**
- الكود بيبان **زي جملة إنجليزية** = أسهل في الفهم
- لو الـ Test فشل، الـ error message مفصلة ومفيدة
- بتدعم chaining (تربط أكتر من assertion ورا بعض)

---

## هيكل الـ Test (AAA Pattern)

كل Test بيتبع نمط **AAA**: Arrange → Act → Assert

```csharp
[Fact]
public async Task GetById_ReturnsProduct_WhenExists()
{
    // Arrange — تجهيز الـ Data والـ Mocks
    var product = new Product
    {
        Id = 1,
        Name = "iPhone",
        Price = 999,
        CategoryId = 1,
        Category = new Category { Id = 1, Name = "Phones" }
    };

    _unitOfWorkMock.Setup(u => u.Products.GetByIdWithCategoryAsync(1))
        .ReturnsAsync(product);

    // Act — تنفيذ الـ Method اللي بنختبرها
    var result = await _controller.GetById(1);

    // Assert — التأكد من النتيجة
    var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
    var productDto = okResult.Value.Should().BeOfType<ProductDto>().Subject;
    productDto.Id.Should().Be(1);
    productDto.Name.Should().Be("iPhone");
    productDto.CategoryName.Should().Be("Phones");
}
```

### الفرق بين AAA وبدائله

| Pattern | الوصف | متى تستخدمه |
|---------|-------|-------------|
| **AAA (Arrange-Act-Assert)** | تجهيز → تنفيذ → تحقق | ✅ الأكثر استخداماً — في أغلب الحالات |
| **GWT (Given-When-Then)** | BDD style — نفس الفكرة بس Naming مختلف | BDD frameworks زي SpecFlow |
| **Four-Phase** | Setup → Exercise → Verify → Teardown | لما تحتاج cleanup بعد الـ test |

---

## Constructor Setup vs [SetUp]

في xUnit، بنستخدم الـ **Constructor** عشان نجهز الـ shared setup:

```csharp
public class ProductsControllerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<ProductsController>> _loggerMock;
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<ProductsController>>();

        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();

        _controller = new ProductsController(_unitOfWorkMock.Object, _mapper, _loggerMock.Object);
    }
}
```

**مهم:** xUnit بيعمل **instance جديد** من الـ class لكل test method. يعني الـ Constructor بيتنادى من الأول لكل test = isolation كامل.

---

## [Fact] vs [Theory]

| Attribute | الوصف | متى تستخدمه |
|-----------|-------|-------------|
| `[Fact]` | Test بدون parameters — بيتنفذ مرة واحدة | لما الـ scenario واحد ومحدد |
| `[Theory]` + `[InlineData]` | Test بيتنفذ أكتر من مرة بـ data مختلفة | لما عايز تختبر نفس المنطق بقيم مختلفة |

```csharp
// Fact — scenario واحد
[Fact]
public async Task GetById_ThrowsNotFoundException_WhenNotExists()
{
    _unitOfWorkMock.Setup(u => u.Products.GetByIdWithCategoryAsync(999))
        .ReturnsAsync((Product?)null);
    var act = () => _controller.GetById(999);
    await act.Should().ThrowAsync<NotFoundException>();
}

// Theory — نفس المنطق بأكتر من ID
[Theory]
[InlineData(999)]
[InlineData(0)]
[InlineData(-1)]
public async Task GetById_ThrowsNotFoundException_ForInvalidIds(int id)
{
    _unitOfWorkMock.Setup(u => u.Products.GetByIdWithCategoryAsync(id))
        .ReturnsAsync((Product?)null);
    var act = () => _controller.GetById(id);
    await act.Should().ThrowAsync<NotFoundException>();
}
```

---

## Verify — التأكد إن Method اتنادت

```csharp
[Fact]
public async Task Delete_ReturnsOk_WhenProductExists()
{
    var product = new Product { Id = 1, Name = "iPhone", Price = 999 };
    _unitOfWorkMock.Setup(u => u.Products.GetByIdAsync(1)).ReturnsAsync(product);
    _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

    var result = await _controller.Delete(1);

    // بنتأكد إن Remove و SaveChanges اتنادوا
    _unitOfWorkMock.Verify(u => u.Products.Remove(product), Times.Once);
    _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
}
```

### Times Options

| Option | المعنى |
|--------|--------|
| `Times.Once` | اتنادت مرة واحدة بالظبط |
| `Times.Never` | ما اتنادتش خالص |
| `Times.Exactly(n)` | اتنادت بالظبط n مرات |
| `Times.AtLeastOnce` | اتنادت مرة واحدة على الأقل |
| `Times.AtMost(n)` | اتنادت n مرات على الأكتر |

---

## أنواع الـ Testing

| النوع | الوصف | السرعة | مثال |
|-------|-------|--------|------|
| **Unit Test** ✅ | اختبار method واحدة منعزلة | ⚡ سريع جداً | اختبار Controller مع Mock |
| **Integration Test** | اختبار أكتر من component مع بعض | 🐢 متوسط | Controller + Database حقيقية |
| **End-to-End (E2E)** | اختبار النظام كله من البداية للنهاية | 🐌 بطيء | HTTP Request → API → DB → Response |

**إحنا استخدمنا Unit Tests** — بنختبر الـ Controller بشكل منفصل باستخدام Mocks بدل Database حقيقية.

---

## الأوامر

```bash
# تشغيل كل الـ Tests
dotnet test

# تشغيل مع تفاصيل
dotnet test --verbosity normal

# تشغيل ملف test واحد بس
dotnet test --filter "FullyQualifiedName~ProductsControllerTests"
```

---

## ملخص الـ Tests اللي كتبناها

### ProductsControllerTests (11 test)

| Test | السيناريو |
|------|-----------|
| `GetAll_ReturnsPagedResult_WithProducts` | ✅ رجّع products مع pagination |
| `GetAll_ReturnsEmptyResult_WhenNoProducts` | ✅ رجّع نتيجة فاضية |
| `GetById_ReturnsProduct_WhenExists` | ✅ رجّع product موجود |
| `GetById_ThrowsNotFoundException_WhenNotExists` | ❌ رمى NotFoundException |
| `Create_ReturnsCreatedProduct_WhenValid` | ✅ عمل product جديد |
| `Create_ThrowsBadRequest_WhenCategoryNotFound` | ❌ رمى BadRequestException |
| `Update_ReturnsUpdatedProduct_WhenValid` | ✅ عدّل الـ product |
| `Update_ThrowsNotFoundException_WhenProductNotExists` | ❌ الـ product مش موجود |
| `Update_ThrowsBadRequest_WhenCategoryNotFound` | ❌ الـ category مش موجودة |
| `Delete_ReturnsOk_WhenProductExists` | ✅ مسح الـ product |
| `Delete_ThrowsNotFoundException_WhenProductNotExists` | ❌ الـ product مش موجود |

### CategoriesControllerTests (10 test)

| Test | السيناريو |
|------|-----------|
| `GetAll_ReturnsAllCategories` | ✅ رجّع كل الـ categories |
| `GetAll_ReturnsEmpty_WhenNoCategories` | ✅ رجّع نتيجة فاضية |
| `GetById_ReturnsCategory_WhenExists` | ✅ رجّع category موجودة |
| `GetById_ThrowsNotFoundException_WhenNotExists` | ❌ رمى NotFoundException |
| `Create_ReturnsCreatedCategory_WhenValid` | ✅ عملت category جديدة |
| `Update_ReturnsUpdatedCategory_WhenValid` | ✅ عدّلت الـ category |
| `Update_ThrowsNotFoundException_WhenCategoryNotExists` | ❌ الـ category مش موجودة |
| `Delete_ReturnsOk_WhenCategoryExists_AndNoProducts` | ✅ مسحت الـ category |
| `Delete_ThrowsBadRequest_WhenCategoryHasProducts` | ❌ الـ category عندها products |
| `Delete_ThrowsNotFoundException_WhenCategoryNotExists` | ❌ الـ category مش موجودة |
