# ComHub

ComHub is a .NET 9 ASP.NET Core minimal API for a community marketplace. It handles user accounts, profiles, items, categories, and item images, plus a basic SignalR chat hub. The service uses PostgreSQL for persistence, Redis for OTP caching, AWS S3 for file storage, and SMTP for email delivery.

**What It Does**
- User registration, login, and password reset with JWT authentication and OTPs.
- Profile read/update for authenticated users.
- Item and category management, including image uploads to S3.
- Real-time hub endpoints for basic chat/testing via SignalR.
- Consistent API responses through a global response/exception middleware.

**Tech Stack**
- .NET 9, ASP.NET Core Minimal APIs
- Entity Framework Core + Npgsql (PostgreSQL)
- Redis (distributed cache for OTPs)
- MassTransit (in-memory transport for email events)
- AWS S3 (file storage)
- MailKit (SMTP email)
- SignalR (real-time hub)
- Swagger/OpenAPI

**Project Layout**
- `ComHub/Program.cs`: App startup, DI, middleware, routing
- `ComHub/Features`: Endpoint modules and handlers
- `ComHub/Infrastructure`: Database entities/config and cloud storage
- `ComHub/Shared`: Cross-cutting concerns (auth, middleware, responses, utilities)

**Local Development**
1. Prerequisites
- .NET 9 SDK
- PostgreSQL
- Redis
- SMTP credentials
- AWS S3 bucket and credentials

2. Configure `AppConfig`
The app reads configuration from environment variables (or user secrets) under `AppConfig`. Use `__` for nesting:

```bash
AppConfig__ConnectionStrings__DefaultConnection=Host=localhost;Database=comhub;Username=postgres;Password=postgres
AppConfig__ConnectionStrings__RedisConnection=localhost:6379

AppConfig__JwtSettings__Secret=your_jwt_secret
AppConfig__JwtSettings__Issuer=ComHub
AppConfig__JwtSettings__Audience=ComHub
AppConfig__JwtSettings__ExpiryMinutes=60

AppConfig__Jwt__Secret=your_jwt_secret
AppConfig__Jwt__Issuer=ComHub
AppConfig__Jwt__Audience=ComHub
AppConfig__Jwt__ExpiryMinutes=60

AppConfig__Mail__Host=smtp.example.com
AppConfig__Mail__Port=587
AppConfig__Mail__Username=your_smtp_user
AppConfig__Mail__Password=your_smtp_pass
AppConfig__Mail__FromEmail=no-reply@example.com
AppConfig__Mail__FromName=ComHub

AppConfig__CloudConfig__BucketName=your-bucket
AppConfig__CloudConfig__AccessKey=your_access_key
AppConfig__CloudConfig__SecretKey=your_secret_key
AppConfig__CloudConfig__Region=us-east-1
AppConfig__CloudConfig__GeneralFolder=uploads
```

3. Run database migrations
```bash
dotnet tool install --global dotnet-ef
dotnet ef database update --project ComHub/ComHub.csproj
```

4. Run the API
```bash
ASPNETCORE_ENVIRONMENT=Development dotnet run --project ComHub/ComHub.csproj
```

Swagger UI is available in Development at `/swagger`.

**API Surface (Base Path: `/api`)**
- `POST /auth/register`
- `POST /auth/login`
- `POST /auth/forgot-password`
- `POST /auth/reset-password`
- `GET /profile`
- `PUT /profile`
- `GET /items`
- `GET /items/{id}`
- `POST /items`
- `PUT /items/{id}`
- `PATCH /items/{id}/images`
- `DELETE /items/{id}/images`
- `GET /items/categories`
- `POST /items/categories`
- `DELETE /items/categories`

**SignalR Hub**
- `/hub/test`

