# 🔍 شرح AuthController.cs — سطر سطر

---

## الـ Usings — ليه كل واحد موجود؟

```csharp
using Microsoft.AspNetCore.Mvc;          // عشان ControllerBase, [ApiController], Ok(), BadRequest()...
using Microsoft.EntityFrameworkCore;      // عشان AnyAsync(), FirstOrDefaultAsync() — دول Extension Methods من EF
using Microsoft.IdentityModel.Tokens;     // عشان SymmetricSecurityKey, SigningCredentials
using System.IdentityModel.Tokens.Jwt;    // عشان JwtSecurityToken, JwtSecurityTokenHandler
using System.Security.Claims;             // عشان Claim, ClaimTypes
using System.Text;                        // عشان Encoding.UTF8
using TestApi.Data;                       // عشان AppDbContext
using TestApi.DTOs;                       // عشان RegisterDto, LoginDto
using TestApi.Models;                     // عشان User
```

---

## الـ Attributes — الفرق بينهم

```csharp
[ApiController]    // بيعمل حاجتين مهمين:
                   // 1. Automatic Model Validation — لو الـ DTO فيه [Required] وانت مبعتش القيمة، بيرجع 400 تلقائي
                   // 2. بيفرض إن الـ Body ييجي من [FromBody] تلقائي
                   
[Route("api/[controller]")]  // [controller] = اسم الـ Class من غير كلمة "Controller"
                              // يعني AuthController → api/Auth
```

---

## الـ Constructor — Dependency Injection

```csharp
private readonly AppDbContext _context;           // readonly = مينفعش تتغير بعد الـ Constructor
private readonly IConfiguration _configuration;   // IConfiguration = بيقرأ من appsettings.json

public AuthController(AppDbContext context, IConfiguration configuration)
{
    _context = context;              // ASP.NET بيعمل Inject تلقائي لأن AppDbContext مسجلة في Program.cs
    _configuration = configuration;  // IConfiguration مسجلة تلقائي من ASP.NET — مش محتاج تسجلها
}
```

**سؤال انترفيو:** ليه `IConfiguration` مش محتاج تسجلها في `Program.cs`؟
**الإجابة:** لأنها من الـ Built-in Services اللي ASP.NET بيسجلها تلقائي.

---

## Register — سطر سطر

```csharp
[HttpPost("register")]    // POST api/Auth/register
public async Task<IActionResult> Register(RegisterDto dto)
//     async  لأن فيه Database calls (بتاخد وقت)
//            IActionResult = ممكن ترجع أي نوع Response (Ok, BadRequest, NotFound...)
//            لو عايز ترجع نوع محدد تستخدم ActionResult<T>
{
    if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
    //                       AnyAsync = بيرجع true لو فيه أي record بيحقق الشرط
    //                       أحسن من FirstOrDefaultAsync لأنك مش محتاج الـ User نفسه
    //                       محتاج تعرف "موجود ولا لأ" بس
    //                       بيتحول لـ SQL: SELECT EXISTS(SELECT 1 FROM Users WHERE Email = @email)
        return BadRequest("Email already exists");
        //     BadRequest = HTTP 400 — يعني الـ Client بعت Request غلط

    var user = new User
    {
        Username = dto.Username,
        Email = dto.Email,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
        //             BCrypt.HashPassword بيعمل Hash + Salt تلقائي
        //             كل مرة بيطلع Hash مختلف لنفس الباسورد (عشان الـ Salt)
        //             مثال: "123456" → "$2a$11$xR3pG..." (مش ممكن ترجعه لـ 123456)
        Role = "User"   // Hardcoded — كل واحد بيعمل Register بيبقى User عادي مش Admin
    };

    _context.Users.Add(user);          // بيحطه في الـ Memory (مراقبة EF) — لسه مراحش Database
    await _context.SaveChangesAsync(); // هنا بيروح Database فعلاً — INSERT INTO Users...

    return Ok("User registered successfully");
    //     Ok = HTTP 200 — نجح
}
```

---

## Login — الفرق بين الـ Methods

```csharp
[HttpPost("login")]
public async Task<IActionResult> Login(LoginDto dto)
{
    var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
    //                              FirstOrDefaultAsync
    //                              بيرجع أول User لقاه — أو null لو مفيش

    if (user == null)
        return Unauthorized("Invalid email or password");
        //     Unauthorized = HTTP 401 — مش مسموحلك (مش مسجل دخول)

    if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
    //                     Verify = بيقارن الباسورد العادي بالـ Hash المتخزن
    //                     مش بيعمل Hash تاني ويقارن — عنده Algorithm بيفك الـ Salt ويقارن
        return Unauthorized("Invalid email or password");
        // ليه نفس الرسالة؟ عشان المهاجم ميعرفش الإيميل ده موجود ولا لأ (Security Best Practice)

    var token = GenerateToken(user);

    return Ok(new { Token = token });
    //     Ok() ممكن تاخد:
    //     Ok()              → 200 من غير Body
    //     Ok("text")        → 200 مع String
    //     Ok(new { ... })   → 200 مع JSON object — ده اللي بنستخدمه هنا
}
```

---

## GenerateToken — أهم Method

```csharp
private string GenerateToken(User user)
// private = مش Endpoint — مجرد Helper Method جوه الـ Controller
{
    var claims = new[]
    //  Claims = معلومات عن الـ User بتتخزن جوه الـ Token
    //  زي بطاقة الهوية — فيها اسمك وعنوانك ورقمك
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),  // الـ User Id
        new Claim(ClaimTypes.Email, user.Email),                    // الإيميل
        new Claim(ClaimTypes.Name, user.Username),                  // الاسم
        new Claim(ClaimTypes.Role, user.Role)                       // الصلاحية — ده اللي [Authorize(Roles="Admin")] بيقرأه
    };

    var key = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
    //                         _configuration["Jwt:Key"]
    //                         بيقرأ من appsettings.json:
    //                         "Jwt": { "Key": "your-secret-key-here" }
    //                         ! = null-forgiving — بتقول للـ compiler "أنا متأكد مش null"

    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    //          SigningCredentials = الـ Algorithm اللي بيوقّع بيه الـ Token
    //          HmacSha256 = الـ Standard — لو حد غيّر حرف في الـ Token، الـ Signature هتبقى غلط

    var token = new JwtSecurityToken(
        issuer: _configuration["Jwt:Issuer"],      // مين اللي عمل الـ Token (اسم الـ API بتاعك)
        audience: _configuration["Jwt:Audience"],   // مين المفروض يستخدمه (اسم الـ Client)
        claims: claims,                              // البيانات اللي جوه الـ Token
        expires: DateTime.Now.AddHours(1),           // بيموت بعد ساعة
        signingCredentials: creds                    // التوقيع
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
    //         JwtSecurityTokenHandler   WriteToken
    //         بيحول الـ Token Object    لـ String (eyJhbGci...)
}
```

---

## 📋 جدول الفروقات المهم — Return Types

| Method | Status Code | امتى تستخدمها |
|--------|-------------|---------------|
| `Ok()` | 200 | نجح عادي |
| `CreatedAtAction()` | 201 | لما تعمل Create (بترجع الحاجة الجديدة + الـ URL بتاعها) |
| `BadRequest()` | 400 | الـ Request غلط |
| `Unauthorized()` | 401 | مش مسجل دخول / باسورد غلط |
| `Forbid()` | 403 | مسجل بس مش مسموحلك |
| `NotFound()` | 404 | الحاجة مش موجودة |

---

## 📋 جدول الفروقات المهم — EF Core Methods

| EF Method | بترجع إيه | امتى |
|-----------|----------|------|
| `FindAsync(id)` | Entity أو null | بتدور بالـ Id بس |
| `FirstOrDefaultAsync(condition)` | أول واحد أو null | بتدور بأي شرط |
| `SingleOrDefaultAsync(condition)` | واحد بس أو null | لازم يكون واحد بس — لو أكتر بيطلع Error |
| `AnyAsync(condition)` | true / false | عايز تعرف "موجود ولا لأ" بس |
| `ToListAsync()` | List كاملة | عايز كل النتايج |
