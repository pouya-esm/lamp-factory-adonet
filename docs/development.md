# Development Setup

Verified, step-by-step guide to set up, run, and test the full stack on a
development machine. Key names, paths, and versions below are taken from the
actual configuration files.

## Prerequisites

| Tool | Version | Source of truth |
|---|---|---|
| .NET SDK | **10.0.401** (rollForward `latestFeature`, prerelease not allowed) | `global.json` |
| SQL Server | any reachable instance (LocalDB/Express/Developer) | default connection string targets `localhost` |
| Node.js | **not pinned by the project** (no `engines` field, no `.nvmrc`). Observed working build: v23.0.0 — treat as informational, not a requirement | absence verified in `frontend/package.json` |
| npm | ships with Node; used via `npm ci` | `frontend/package-lock.json` is committed |

Useful editor configuration (`.editorconfig`) and line-ending normalization
(`.gitattributes`) are already in the repository; no IDE-specific setup is
required.

## 1. Database setup

Run the reviewed schema script with a tool that understands `GO` batches:

```bash
sqlcmd -S localhost -E -i database/lamp-factory.sql
```

(or execute it in SQL Server Management Studio). It creates the `lampdb`
database and its 29 tables. It inserts **no seed data** — bootstrapping the
first administrative user is currently a manual step; see
[database.md](database.md#setup-gaps-verified) for the exact gap.

The default connection string — used by `ApplicationDbContext` via
`ConnectionStrings:DefaultConnection` — is Windows authentication against
localhost:

```
Server=localhost;Database=lampdb;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=30;
```

If your instance differs (different server, SQL auth, non-default database),
override only the connection string locally (see the secrets step below);
do not edit the committed `appsettings.json` values.

## 2. Configure the JWT signing key

The repository contains no secret values: `Jwt:Key` is committed empty. The
application reads the real key from the .NET user-secrets store in
development (the API project has a `UserSecretsId`; `dotnet user-secrets`
works without further initialization).

```bash
cd src/LampFactory.API
dotnet user-secrets set "Jwt:Key" "<your-own-32+-byte-signing-key>"
```

Optionally override the database connection the same way:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=…;Database=…;…"
```

Configuration keys the backend actually binds (verified in code):

| Key | Used by |
|---|---|
| `Jwt:Key` | `JwtTokenService` (signing), `AddJwtAuthentication` (validation) |
| `Jwt:Issuer` | same two places (`ValidateIssuer` is on) |
| `Jwt:Audience` | same two places (`ValidateAudience` is on) |
| `ConnectionStrings:DefaultConnection` | `ApplicationDbContext` (`GetConnectionString`) |

> **Warning verified by live execution:** without `Jwt:Key` the API returns
> the generic 500 envelope for *every* request (including the Scalar UI),
> because `SymmetricSecurityKey` cannot be built from an empty key. If
> everything answers 500 locally, check this first.

For HTTPS endpoints, trust the development certificate once:
`dotnet dev-certs https --trust`.

## 3. Backend: restore, build, run

From the repository root (`Lamp Factory/`):

```bash
dotnet build "Lamp Factory.slnx"
dotnet run --project src/LampFactory.API/LampFactory.API.csproj
```

- HTTP is on `http://localhost:5259`, HTTPS on `https://localhost:7263`
  (`Properties/launchSettings.json`).
- In Development, the OpenAPI document is served at
  `http://localhost:5259/openapi/v1.json` and the Scalar UI at
  `http://localhost:5259/scalar` (both mapped only in Development — verified
  against a running instance).

Observed environment note: builds on the development machine where this
documentation was verified printed these warnings; all are non-blocking
(`Build succeeded`):

| Warning | Meaning | Scope |
|---|---|---|
| `NU1900` | NuGet vulnerability-feed source `mirror-nuget.runflare.com` unreachable — environment-specific | all projects |
| `NU1510` | explicit `Microsoft.Extensions.Configuration` reference is redundant in the integration test project | `LampFactory.Infrastructure.IntegrationTests` |
| `NU1902`/`NU1903` | the transitive dependency `System.Security.Cryptography.Pkcs` `9.0.13` (pulled in by `Microsoft.Data.SqlClient` `7.0.2`) has advisory GHSA-2fmv-rv2c-59x7 (moderate) and GHSA-846f-f764-76mx (high) | API, Infrastructure, all test projects that reference them |

The `NU1902`/`NU1903` pair is not merely cosmetic: it is a transitive-package
advisory. Fixing it means either upgrading `Microsoft.Data.SqlClient` to a
version whose chain resolves a non-vulnerable `Pkcs`, or adding an explicit
`PackageReference` override for `System.Security.Cryptography.Pkcs` — both are
code/config changes deliberately not performed by this documentation task.

## 4. Tests

```bash
dotnet test "Lamp Factory.slnx"
```

or per project, e.g.
`dotnet test tests/LampFactory.Infrastructure.IntegrationTests`. The four test
projects, their responsibilities, the **live SQL Server dependency** of the
integration tests (they skip themselves when no connection is reachable), and
recently observed results are documented in [testing.md](testing.md).

## 5. Frontend: install, configure, run

```bash
cd frontend
npm ci
cp .env.example .env      # Windows PowerShell: Copy-Item .env.example .env
npm run dev
```

- Dev server: **`http://localhost:5173`** (`vite.config.js` pins this port —
  the backend's CORS policy expects exactly this origin).
- The only supported environment variable is **`VITE_API_BASE_URL`**
  (`.env.example`). It must point at the API root **including `/api`**, e.g.
  `https://localhost:7263/api`. If unset, the code falls back to
  `https://localhost:7263/api` — matching the API's HTTPS dev port.
- Other scripts: `npm run build` (production build to `dist/`, observed
  succeeding) and `npm run preview` (serve the built output).

How the client talks to the API — token storage, single-flight refresh,
envelope unwrapping — is described in [frontend.md](frontend.md). How endpoints
behave is in [api.md](api.md).

## 6. Common problems

| Symptom | Verified cause / fix |
|---|---|
| Every request returns 500, even `/scalar` | No `Jwt:Key` configured (see step 2). Set the user-secret and restart. |
| Frontend requests blocked (CORS) | The backend allows only `http://localhost:5173`. Run Vite on that port or extend the policy consciously. |
| 401 loops / immediate logouts | Access tokens expire after 5 minutes by design; if refreshes also fail, the refresh token was rotated or reused — re-login. Check that both processes see the same `Jwt:Issuer`/`Jwt:Audience`/`Jwt:Key`. |
| Integration tests count is lower than expected | They skip silently when no SQL Server connection is reachable (`DatabaseFixture.IsAvailable`). Provide a reachable `DefaultConnection`. |
| `NU1900` warnings during restore/build | NuGet vulnerability-feed mirror unreachable — environment-specific, non-blocking. |
| `NU1902`/`NU1903` warnings about `System.Security.Cryptography.Pkcs` 9.0.13 | Transitive dependency of `Microsoft.Data.SqlClient` 7.0.2 with public advisories (moderate + high). Non-blocking; fix by upgrading the SqlClient chain or overriding the Pkcs version. |
| `NU1510` warning in the integration-test project | Explicit `Microsoft.Extensions.Configuration` reference is redundant (auto-available). Remove the `PackageReference` — a code change, not done here. |
| Data endpoints fail with errors about missing users | The schema script has no seed data; the first user must be created manually (see [database.md](database.md#setup-gaps-verified)). |
