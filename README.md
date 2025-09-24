## CarDealership API

A .NET 9 Web API for a car dealership with admin dashboard endpoints, user browsing and purchases, OTP-based auth, JWT tokens, and Redis-backed OTP storage. MySQL is used as the primary database.

### Quick start

#### Option A: Docker (recommended)

```bash
# From repo root
docker compose up --build
# API will be available at http://localhost:8080
```

-   Health check: `GET http://localhost:8080/health` → `OK`
-   Swagger/OpenAPI: `GET http://localhost:8080/openapi/v1.json` (dev only)

Services started by compose:

-   MySQL 8.0 at `localhost:3306` (db `cardealership`, user `root`, pass `rootpassword`)
-   Redis at `localhost:6379`
-   API at `http://localhost:8080`

The API listens on `http://+:8080` inside the container with HTTPS redirection disabled via `EnableHttpsRedirection=false`.

#### Option B: Run locally (without Docker)

Prereqs: .NET SDK 9.0+, MySQL 8.x, Redis 7+

1. Configure `appsettings.json` connection strings to point to your local MySQL and Redis.
2. Apply EF Core migrations (the app seeds on startup, but applying migrations explicitly is fine):

```bash
# From repo root
export ASPNETCORE_ENVIRONMENT=Development
# Optional: dotnet ef database update (if you have dotnet-ef)
# Run the API
dotnet run --project CarDealership.csproj --urls http://localhost:8080
```

-   Health: `GET http://localhost:8080/health`
-   OpenAPI (dev): `GET http://localhost:8080/openapi/v1.json`

### Routing & Versioning

-   Global prefix: `api/v{version}` (e.g., `api/v1`).
-   Admin dashboard endpoints are additionally prefixed with `dashboard` → `api/v{version}/dashboard/...`.
-   Default API version is `1.0`.

### Authentication overview

-   OTP flow (both Admin and User):
    1. `POST .../auth/send` with phone → receive OTP out-of-band (in dev, OTP is accepted as `123456`).
    2. `POST .../auth/verify` with phone + otp → response contains a `loginToken`.
    3. `POST .../auth/authenticate` with `loginToken` + `passcode` → returns `accessToken` and `refreshToken`.
    4. `POST .../auth/refresh` with `refreshToken` → rotates tokens.
-   Use `Authorization: Bearer <accessToken>` for protected routes.

### Endpoints (v1)

Base URL examples assume `http://localhost:8080` and version `v1`.

#### Admin

Auth

-   `POST /api/v1/dashboard/auth/send` — body: `{ "phone": "0555000000" }`
-   `POST /api/v1/dashboard/auth/verify` — body: `{ "phone": "0555000000", "otp": "123456" }` → `loginToken`
-   `POST /api/v1/dashboard/auth/authenticate` — body: `{ "loginToken": "...", "passcode": "123456" }` → `accessToken`, `refreshToken`
-   `POST /api/v1/dashboard/auth/refresh` — body: `{ "refreshToken": "..." }` → rotated tokens

Management

-   `POST /api/v1/dashboard/seed` — seeds demo data (no auth required)
-   `POST /api/v1/dashboard/admin-management/create-admin` — create admin
-   `POST /api/v1/dashboard/admin-management/assign-role` — assign role to admin
-   `GET  /api/v1/dashboard/admin-management/admin-users` — list admins
-   `GET  /api/v1/dashboard/admin-management/admin-users/{id}` — admin by id
-   `GET  /api/v1/dashboard/admin-management/roles` — list roles
-   `GET  /api/v1/dashboard/admin-management/permissions` — list permissions

Inventory

-   `POST /api/v1/dashboard/inventory/vehicles` — add vehicle
-   `PATCH /api/v1/dashboard/inventory/vehicles/{id}` — update partial vehicle

Customers

-   `GET  /api/v1/dashboard/customers` — list customers

Sales

-   `POST /api/v1/dashboard/sales/process` — process a sale for a purchase request

#### User

Auth

-   `POST /api/v1/auth/send` — body: `{ "phone": "0555555555" }`
-   `POST /api/v1/auth/verify` — body: `{ "phone": "0555555555", "otp": "123456" }` → `loginToken`
-   `POST /api/v1/auth/authenticate` — body: `{ "loginToken": "...", "passcode": "123456" }` → `accessToken`, `refreshToken`
-   `POST /api/v1/auth/refresh` — body: `{ "refreshToken": "..." }`

Browse

-   `GET  /api/v1/vehicles` — optional `make`, `model`, `minPrice`, `maxPrice`, etc.
-   `GET  /api/v1/vehicles/{id}` — vehicle details

Purchases (Bearer token required)

-   `POST /api/v1/purchases/request` — body: `{ "vehicleId": 1 }`
-   `GET  /api/v1/purchases/history` — current user purchase history

A complete Postman collection is provided at `postman/CarDealership.postman_collection.json`. It includes scripts that capture tokens automatically between steps.

### Configuration

Key settings (can be set via env vars or `appsettings.json`):

-   `ConnectionStrings:DefaultConnection`
-   `ConnectionStrings:Redis`
-   `Jwt:Key` (min 32 characters), `Jwt:Issuer`, `Jwt:Audience`
-   `EnableHttpsRedirection` (default true; disabled in Docker compose)

### Design notes & assumptions

-   OTP codes are handled by a Redis-backed service; for development, a static OTP like `123456` is accepted for convenience.
-   Admin dashboard endpoints are segregated behind the `dashboard` prefix and use permission attributes internally.
-   Global route prefixing and versioning are enforced via a controller convention.
-   A background startup service seeds data at boot to simplify local/dev onboarding.
-   Minimal happy-path validation is shown; production should harden validation, logging, and error handling.

### Health & troubleshooting

-   Health: `GET /health` returns `OK`.
-   If API fails to start in Docker, ensure MySQL and Redis containers are healthy (`docker ps`, `docker logs`).
-   Verify DB connectivity with the connection string exposed to the API container.

---

License: MIT (or your choice).
