# Docker — شرح كامل

## إيه هو Docker؟

Docker هو أداة بتخليك **تحزم** التطبيق بتاعك مع كل اللي محتاجه (runtime, libraries, config) في **container** واحد.  
يعني أي حد يشغّل الـ container ده — هيشتغل عنده زي ما بيشتغل عندك بالظبط.

**المشكلة اللي بيحلها:** "It works on my machine" — الكود شغال عندي بس مش شغال عند حد تاني.

---

## المصطلحات الأساسية

| المصطلح | الوصف |
|---------|-------|
| **Image** | الـ "template" — فيه كل حاجة محتاجها التطبيق (زي ISO file) |
| **Container** | الـ image وهو شغال (زي Virtual Machine بس أخف بكتير) |
| **Dockerfile** | الملف اللي فيه التعليمات عشان تبني الـ Image |
| **docker-compose.yml** | ملف بيديرلك أكتر من container مع بعض |
| **Volume** | مساحة تخزين ثابتة — الداتا مبتتمسحش لما الـ container يقف |
| **Port Mapping** | ربط port من جوه الـ container بـ port على جهازك |

---

## الفرق بين Docker وبدائله

| Feature | Docker | Virtual Machine | Podman |
|---------|--------|----------------|--------|
| **الحجم** | صغير (MBs) | كبير (GBs) | صغير (MBs) |
| **السرعة** | ⚡ بيشتغل في ثواني | 🐢 بياخد دقائق | ⚡ بيشتغل في ثواني |
| **العزل** | Process-level | Full OS-level | Process-level |
| **الموارد** | خفيف | تقيل | خفيف |
| **الانتشار** | الأكثر استخداماً ✅ | قديم بس لسه مستخدم | بديل Docker بدون daemon |
| **Daemon** | محتاج Docker daemon | محتاج Hypervisor | ❌ مش محتاج daemon |

---

## Dockerfile — شرح سطر بسطر

```dockerfile
# ===== Stage 1: Base — الـ Runtime Image =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# ===== Stage 2: Build — بنبني الكود =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# بننسخ ملفات الـ .csproj الأول (عشان الـ caching)
COPY TestApi/TestApi.csproj TestApi/
COPY TestApi.Domain/TestApi.Domain.csproj TestApi.Domain/
COPY TestApi.Application/TestApi.Application.csproj TestApi.Application/
COPY TestApi.Infrastructure/TestApi.Infrastructure.csproj TestApi.Infrastructure/

# بنعمل restore للـ NuGet packages
RUN dotnet restore TestApi/TestApi.csproj

# بننسخ باقي الكود
COPY TestApi/ TestApi/
COPY TestApi.Domain/ TestApi.Domain/
COPY TestApi.Application/ TestApi.Application/
COPY TestApi.Infrastructure/ TestApi.Infrastructure/

# بنعمل publish
WORKDIR /src/TestApi
RUN dotnet publish -c Release -o /app/publish --no-restore

# ===== Stage 3: Final — الـ Image النهائي =====
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TestApi.dll"]
```

### Multi-Stage Build — ليه مهم؟

| Stage | Image | الحجم | الدور |
|-------|-------|-------|-------|
| **build** | `sdk:8.0` (~900MB) | كبير | فيه كل أدوات البناء |
| **final** | `aspnet:8.0` (~220MB) | صغير | Runtime بس — الـ production image |

**الفايدة:** الـ Image النهائي حجمه صغير (220MB بدل 900MB) لأنه مفيهوش الـ SDK — بس الكود المترجم والـ runtime.

### الفرق بين الـ Dockerfile Instructions

| Instruction | الوظيفة | مثال |
|-------------|---------|------|
| `FROM` | الـ base image اللي بنبني عليه | `FROM mcr.microsoft.com/dotnet/aspnet:8.0` |
| `WORKDIR` | بتحدد الـ working directory جوه الـ container | `WORKDIR /app` |
| `COPY` | بتنسخ ملفات من جهازك للـ container | `COPY TestApi/ TestApi/` |
| `RUN` | بتنفذ أمر أثناء البناء | `RUN dotnet restore` |
| `EXPOSE` | بتعلن عن الـ port اللي التطبيق هيستخدمه | `EXPOSE 8080` |
| `ENTRYPOINT` | الأمر اللي بيتنفذ لما الـ container يشتغل | `ENTRYPOINT ["dotnet", "TestApi.dll"]` |
| `CMD` | زي ENTRYPOINT بس ممكن يتغير من بره | `CMD ["--urls", "http://+:8080"]` |
| `ENV` | بتحط environment variable | `ENV ASPNETCORE_ENVIRONMENT=Production` |

### ENTRYPOINT vs CMD

| Feature | ENTRYPOINT | CMD |
|---------|-----------|-----|
| **الدور** | الأمر الأساسي (ثابت) | أمر default (ممكن يتغير) |
| **Override** | صعب يتغير | سهل يتغير من الـ CLI |
| **الاستخدام** | لما عايز الـ container يشتغل كـ executable | لما عايز تديله default arguments |

---

## docker-compose.yml — شرح كامل

```yaml
services:
  # ===== الـ API Service =====
  api:
    build:
      context: .              # المجلد اللي فيه الـ Dockerfile
      dockerfile: Dockerfile  # اسم الـ Dockerfile
    ports:
      - "5000:8080"           # port على جهازك : port جوه الـ container
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=db;Port=3306;Database=TestApiDb;User=root;Password=root123;
      - Jwt__Key=YourSuperSecretKeyThatIsAtLeast32Characters!
      - Jwt__Issuer=TestApi
      - Jwt__Audience=TestApiUsers
    depends_on:
      db:
        condition: service_healthy  # استنى لحد ما الـ MySQL يبقى جاهز

  # ===== الـ MySQL Service =====
  db:
    image: mysql:8.0           # بنستخدم MySQL 8 image رسمي
    ports:
      - "3307:3306"            # 3307 على جهازك عشان ميتعارضش مع MySQL المحلي
    environment:
      MYSQL_ROOT_PASSWORD: root123
      MYSQL_DATABASE: TestApiDb
    volumes:
      - mysql_data:/var/lib/mysql   # الداتا بتفضل موجودة حتى لو الـ container اتمسح
    healthcheck:
      test: ["CMD", "mysqladmin", "ping", "-h", "localhost"]
      interval: 10s            # كل 10 ثواني يعمل check
      timeout: 5s              # لو ما ردّش في 5 ثواني = فشل
      retries: 5               # يحاول 5 مرات قبل ما يبلّغ إن الـ service unhealthy

volumes:
  mysql_data:                  # Volume عشان الداتا تفضل ثابتة
```

### depends_on — أنواع الشروط

| Condition | المعنى |
|-----------|--------|
| `service_started` | استنى لحد ما الـ container يبدأ بس (مش بالضرورة جاهز) |
| `service_healthy` ✅ | استنى لحد ما الـ healthcheck يرجع "healthy" |
| `service_completed_successfully` | استنى لحد ما الـ container يخلّص ويقفل بنجاح |

### Environment Variables — `__` بدل `:`

في الـ Docker environment variables:
- `ConnectionStrings__DefaultConnection` = `ConnectionStrings:DefaultConnection` في الـ appsettings.json
- `__` بتترجم لـ `:` في .NET configuration system

---

## .dockerignore

```
**/bin/
**/obj/
**/.vs/
**/Logs/
**/node_modules/
**/.git/
*.user
*.suo
```

**ليه مهم؟** بيمنع Docker من نسخ ملفات مش محتاجها جوه الـ container — ده بيخلي الـ build أسرع والـ image أصغر.

---

## أوامر Docker الأساسية

### Docker Commands

| الأمر | الوظيفة |
|-------|---------|
| `docker build -t myapi .` | بناء Image من الـ Dockerfile |
| `docker run -p 5000:8080 myapi` | تشغيل container |
| `docker ps` | عرض الـ containers الشغالة |
| `docker ps -a` | عرض كل الـ containers (حتى المتوقفة) |
| `docker stop <id>` | إيقاف container |
| `docker rm <id>` | مسح container |
| `docker images` | عرض كل الـ images |
| `docker rmi <image>` | مسح image |
| `docker logs <id>` | عرض logs الـ container |

### Docker Compose Commands

| الأمر | الوظيفة |
|-------|---------|
| `docker-compose up` | تشغيل كل الـ services |
| `docker-compose up --build` | إعادة بناء + تشغيل |
| `docker-compose up -d` | تشغيل في الخلفية (detached) |
| `docker-compose down` | إيقاف ومسح كل الـ containers |
| `docker-compose down -v` | إيقاف + مسح الـ volumes كمان |
| `docker-compose logs` | عرض logs كل الـ services |
| `docker-compose ps` | عرض حالة الـ services |

---

## الفرق بين docker-compose و Kubernetes

| Feature | Docker Compose | Kubernetes (K8s) |
|---------|---------------|-----------------|
| **الاستخدام** | Development + Small Apps | Production + Large Scale |
| **التعقيد** | بسيط | معقد |
| **Scaling** | Manual (`scale: 3`) | Auto-scaling |
| **Load Balancing** | ❌ مفيش | ✅ built-in |
| **Self-Healing** | ❌ محدود | ✅ بيعيد تشغيل الـ pods تلقائي |
| **Config File** | `docker-compose.yml` | `deployment.yaml`, `service.yaml`, etc. |

**ملحوظة:** docker-compose كافي جداً للـ development والمشاريع الصغيرة والمتوسطة. Kubernetes محتاجه لما يكون عندك traffic عالي وعايز auto-scaling.

---

## Port Mapping مهم تفهمه

```
ports:
  - "5000:8080"
     │     │
     │     └── Port جوه الـ Container (الـ API بتسمع عليه)
     └── Port على جهازك (بتفتحه في الـ Browser)
```

يعني لما تفتح `http://localhost:5000` — Docker بيوجّهك لـ port `8080` جوه الـ container.

---

## Volume — ليه مهم؟

```yaml
volumes:
  - mysql_data:/var/lib/mysql
```

**بدون Volume:** لو الـ MySQL container اتمسح أو اتعمله restart — الداتا كلها بتروح.  
**مع Volume:** الداتا بتتحفظ في مكان ثابت على جهازك — حتى لو الـ container اتمسح، الداتا موجودة.
