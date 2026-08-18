# 🚀 Principal Architect GitHub Showcase Checklist
**Project:** `CloudStorage` (.NET 8 Clean Architecture AWS S3 File Management Platform)  
**Target Profile:** Senior / Staff / Principal Architect (7+ Years Experience Showcase)  

---

## 🔴 Phase 0: Critical Security & Git Hygiene (Immediate)
- [ ] **0.1 Remove Hardcoded Secrets from `appsettings.json`**
  - [ ] Replace PostgreSQL password (`Password=MyGSTcafe@9088`) with placeholder or use User Secrets / Env variables.
  - [ ] Replace JWT secret key (`LE7IhiJ9o1qzZgWDfohZWqAZYg3brcTmR9CJPg2ISpJ`) with placeholder.
  - [ ] Create `appsettings.Example.json` with safe mock values for public showcase.
- [ ] **0.2 Git History & Repository Cleanup**
  - [ ] Untrack and remove committed log files in `CloudStorage.Api/Logs/log-*.txt`.
  - [ ] Remove `Backup/` folder and `zip-code.bat` from repository.
  - [ ] Remove empty unused directory `src/obj/`.
  - [ ] Clean up or implement `CloudStorage.Shared` (currently empty).
  - [ ] Update `.gitignore` to ignore:
    - `Logs/`
    - `Backup/`
    - `*.zip`
    - `appsettings.Development.json` (or any file with personal connection strings)

---

## 🟠 Phase 1: Critical Code & Logic Bug Fixes
- [ ] **1.1 Fix Direct Upload Status Bug**
  - **File:** `CloudStorage.Application/Services/FileService.cs` (`UploadAsync`)
  - [ ] Call `storedFile.MarkAsUploaded()` immediately after S3 upload succeeds and before saving to the database.
  - [ ] Ensure presigned download generation (`GeneratePresignedDownloadAsync`) succeeds for directly uploaded files.
- [ ] **1.2 Fix Failing Unit Test**
  - **File:** `CloudStorage.UnitTests/Utilities/FileNameSanitizerTests.cs` (`Sanitize_Should_Replace_Invalid_Characters`)
  - [ ] Update test input to test invalid filename characters (e.g. `my:file*name?.pdf` -> `myfilename.pdf`) rather than path separators handled by `Path.GetFileName`.
  - [ ] Run `dotnet test` and ensure 100% green test run across all test suites.
- [ ] **1.3 Prevent Unsafe File Extension Mutation on Rename**
  - **Files:** `CloudStorage.Application/Validators/Files/RenameFileRequestValidator.cs` & `StoredFile.cs`
  - [ ] Validate that renamed files preserve their original extension or match the allowed whitelist (`AllowedExtensions`).
  - [ ] Reject renaming that alters MIME type without re-validation.
- [ ] **1.4 Optimize Direct Upload Memory Allocation**
  - **File:** `CloudStorage.Api/Controllers/FilesController.cs` (`Upload`)
  - [ ] Avoid `new MemoryStream()` buffer copy for direct uploads; stream directly using `file.OpenReadStream()` to avoid server RAM spikes under concurrent load.

---

## 🟡 Phase 2: Database Performance & Data Integrity
- [ ] **2.1 Add Essential Entity Framework Composite Indexes**
  - **File:** `CloudStorage.Infrastructure/Persistence/Configurations/StoredFileConfiguration.cs`
  - [ ] Add composite index for user listing & pagination: `builder.HasIndex(x => new { x.UserId, x.CreatedAtUtc })`.
  - [ ] Add status index for pending cleanup & active file filtering: `builder.HasIndex(x => new { x.UserId, x.Status })`.
  - [ ] Add unique index on S3 object key: `builder.HasIndex(x => x.ObjectKey).IsUnique()`.
- [ ] **2.2 Refine Storage Usage Query**
  - **File:** `CloudStorage.Infrastructure/Persistence/Repositories/FileRepository.cs` (`GetStorageUsageAsync`)
  - [ ] Filter by `Status == FileStatus.Uploaded` so unconfirmed pending presigned uploads don't distort user quota.
- [ ] **2.3 Run EF Core Migration**
  - [ ] Generate and apply migration: `dotnet ef migrations add AddIndexesAndPerformanceOptimizations --project CloudStorage.Infrastructure --startup-project CloudStorage.Api`.

---

## 🟢 Phase 3: Infrastructure, Containerization & Local Development (DevX)
- [ ] **3.1 Production Multi-Stage `Dockerfile`**
  - **Location:** `CloudStorage.Api/Dockerfile`
  - [ ] Add multi-stage build (`mcr.microsoft.com/dotnet/sdk:8.0` build -> `mcr.microsoft.com/dotnet/aspnet:8.0` non-root runtime).
- [ ] **3.2 Complete `docker-compose.yml` for Local Development**
  - **Location:** `docker-compose.yml`
  - [ ] Add `api` service (linking to local build).
  - [ ] Add `postgres` service with health checks & persistent volume.
  - [ ] Add `redis` service with health checks & persistent volume.
  - [ ] Add `localstack` service for local S3 simulation (no AWS account required for reviewers).
  - [ ] Add automated S3 bucket creation script/hook on LocalStack startup.
- [ ] **3.3 Verify One-Command Onboarding**
  - [ ] Test that running `docker compose up -d` boots all dependencies and the API is fully functional with Swagger on `http://localhost:5000/swagger`.

---

## 🔵 Phase 4: Automated Testing & Test Coverage
- [ ] **4.1 Comprehensive Unit Testing**
  - **Project:** `CloudStorage.UnitTests`
  - [ ] Unit tests for `FileService` (Upload, Download, Delete, Rename, Presigned URL generation, S3 compensation on DB error).
  - [ ] Unit tests for `AuthService` (Registration, Duplicate Email conflict, Login success/failure, Password hashing).
  - [ ] Unit tests for `RedisCacheService` (Get, Set, Distributed Lock acquisition, retry loop, Lua release fallback).
  - [ ] Unit tests for `FileSignatureValidator` (Valid magic bytes, spoofed extensions, unsupported files).
- [ ] **4.2 End-to-End Integration Testing with Testcontainers**
  - **Project:** `CloudStorage.IntegrationTests`
  - [ ] Setup `CustomWebApplicationFactory` using `Testcontainers.PostgreSql`, `Testcontainers.Redis`, and `Testcontainers.LocalStack`.
  - [ ] Test complete Auth Flow: Register -> Login -> Receive JWT.
  - [ ] Test direct upload flow: Upload -> Download -> Check Storage Details -> Delete.
  - [ ] Test presigned upload flow: Request Upload URL -> Simulate S3 PUT -> Call Complete -> Generate Presigned Download URL.

---

## 🟣 Phase 5: Production Hardening & Architectural Patterns
- [ ] **5.1 Rate Limiting & Abuse Prevention**
  - **File:** `CloudStorage.Api/Program.cs`
  - [ ] Configure ASP.NET Core `AddRateLimiter` with fixed/sliding window algorithms on `/api/auth/*` and `/api/files/presigned-upload`.
- [ ] **5.2 Refresh Token Support**
  - **Projects:** `CloudStorage.Domain` & `CloudStorage.Application`
  - [ ] Add `RefreshToken` entity (token string, expiry, revoked status, user ID).
  - [ ] Implement `POST /api/auth/refresh` and `POST /api/auth/revoke` endpoints.
- [ ] **5.3 Orphaned Pending Upload Background Cleaner**
  - **Project:** `CloudStorage.Infrastructure`
  - [ ] Implement a `BackgroundService` (`PendingUploadCleanupService`) that periodically queries `stored_files` where `Status == FileStatus.Pending` and `CreatedAtUtc < (UtcNow - 1 hour)`, verifies against S3, and cleans up abandoned database records.
- [ ] **5.4 Resilience & Retry Policies**
  - [ ] Configure `Microsoft.Extensions.Http.Resilience` or `Polly` for AWS S3 and external calls with exponential backoff and jitter.

---

## 🌟 Phase 6: GitHub Showcase Presentation & CI/CD
- [ ] **6.1 GitHub Actions CI Pipeline**
  - **Location:** `.github/workflows/ci.yml`
  - [ ] Build solution on `ubuntu-latest`.
  - [ ] Run unit and integration test suites with code coverage reporting.
  - [ ] Code formatting & lint checking (`dotnet format --verify-no-changes`).
- [ ] **6.2 Production-Grade `README.md`**
  - [ ] **Project Badges:** .NET 8, C#, AWS S3, PostgreSQL, Redis, Docker, CI Status, License.
  - [ ] **Architecture Overview:** Clean Architecture layer diagram + Mermaid request sequence diagrams (Presigned S3 flow & Distributed Lock flow).
  - [ ] **Engineering Highlights Section:**
    - *Why Presigned S3 URLs instead of server streaming? (Bandwidth offloading & scalability).*
    - *Distributed lock-based Single-Flight mechanism in Redis (Cache stampede / thundering herd mitigation).*
    - *Zero-trust binary signature validation (Magic byte verification).*
  - [ ] **Quick Start Guide:** One-click `docker compose up -d` instructions.
  - [ ] **Interactive API Documentation:** Key endpoints, request/response samples, cURL commands.
  - [ ] **Architecture Decision Records (ADRs):** Summary of trade-offs (e.g. EF Core vs Dapper, Clean Architecture vs Vertical Slices, S3 partitioning strategy).

---

### Progress Tracking
| Phase | Focus Area | Status |
| :--- | :--- | :---: |
| **Phase 0** | Security & Git Hygiene | ⏳ Pending |
| **Phase 1** | Critical Code & Logic Bug Fixes | ⏳ Pending |
| **Phase 2** | Database Performance & Indexing | ⏳ Pending |
| **Phase 3** | Docker & LocalStack (DevX) | ⏳ Pending |
| **Phase 4** | Unit & Testcontainers Integration Tests | ⏳ Pending |
| **Phase 5** | Production Hardening & Background Workers | ⏳ Pending |
| **Phase 6** | README & GitHub Actions CI | ⏳ Pending |
