FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY TestApi/TestApi.csproj TestApi/
COPY TestApi.Domain/TestApi.Domain.csproj TestApi.Domain/
COPY TestApi.Application/TestApi.Application.csproj TestApi.Application/
COPY TestApi.Infrastructure/TestApi.Infrastructure.csproj TestApi.Infrastructure/

RUN dotnet restore TestApi/TestApi.csproj

COPY TestApi/ TestApi/
COPY TestApi.Domain/ TestApi.Domain/
COPY TestApi.Application/ TestApi.Application/
COPY TestApi.Infrastructure/ TestApi.Infrastructure/

WORKDIR /src/TestApi
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TestApi.dll"]
