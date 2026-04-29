# syntax=docker/dockerfile:1.6

# Multi-stage build: build trên SDK image, runtime chỉ ASP.NET runtime → image nhỏ hơn ~70%

# ─── Stage 1: build ───
FROM mcr.microsoft.com/dotnet/sdk:8.0-jammy AS build
WORKDIR /src

# Copy csproj trước để layer cache `dotnet restore` (khi chỉ source thay đổi, không invalidate cache)
COPY ["BackEndAPI.sln", "./"]
COPY ["BackEndAPI/BackEndAPI.csproj", "BackEndAPI/"]
COPY ["Area/Area.csproj", "Area/"]
COPY ["ClassLibrary/EncryptDecrypt.csproj", "ClassLibrary/"]
COPY ["BackEndAPI.Tests/BackEndAPI.Tests.csproj", "BackEndAPI.Tests/"]
COPY ["Directory.Build.props", "./"]

RUN dotnet restore "BackEndAPI/BackEndAPI.csproj"

# Copy toàn bộ source và build
COPY . .
RUN dotnet publish "BackEndAPI/BackEndAPI.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# ─── Stage 2: runtime ───
FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy AS runtime
WORKDIR /app

# Tạo non-root user cho security (UID 1000 default)
RUN groupadd -r apsp && useradd -r -g apsp -u 1000 apsp \
    && mkdir -p /app/Logs /app/uploads \
    && chown -R apsp:apsp /app

COPY --from=build --chown=apsp:apsp /app/publish .

USER apsp

# Settings runtime
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_USE_POLLING_FILE_WATCHER=false

EXPOSE 8080

# Health check container — dùng /api/health/live (light-weight, không kiểm DB/SAP)
HEALTHCHECK --interval=30s --timeout=5s --start-period=30s --retries=3 \
    CMD curl --fail --silent http://localhost:8080/api/health/live || exit 1

ENTRYPOINT ["dotnet", "BackEndAPI.dll"]
