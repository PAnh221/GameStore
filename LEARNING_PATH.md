# GameStore Learning Path

A step-by-step path for learning ASP.NET Core with this project.
Tick a box `[x]` when you finish a step. Each step has a **Done when** line, a concrete check in the code, so progress can be verified.

Started: 2026-10-19

---

## Stage 1: Housekeeping

- [x] **1.1 Clean up git**
  - Add `*.db`, `*.db-shm`, `*.db-wal` to `.gitignore`.
  - Untrack the database file:
    ```bash
    git rm --cached GameStore.Api/GameStore.db
    ```
  - Commit the `SeedGenres` migration.
  - **Done when:** `git ls-files` shows no `.db` file, and both migrations are committed.

- [x] **1.2 Add the UI to the solution**
  ```bash
  dotnet sln add GameStore.Ui
  ```
  - **Done when:** `GameStore.sln` lists `GameStore.Ui`.

## Stage 2: Correct HTTP behaviour

- [x] **2.1 Validate GenreId**
  - POST and PUT `/games` return `400` when the genre doesn't exist.
  - **Done when:** sending `genreId: 999` returns 400, not a 500 error.

- [x] **2.2 DELETE returns 404 when nothing was deleted**
  - Use the row count returned by `ExecuteDeleteAsync`.
  - **Done when:** deleting a missing id returns 404.

- [x] **2.3 Full create/read/update/delete for genres**
  - Add `GET /genres/{id}`, `POST`, `PUT` and `DELETE`, with DTOs and mapping.
  - **Done when:** `genre.http` has working requests for all five routes.

## Stage 3: Professional API

- [x] **3.1 Swagger / OpenAPI**
  - **Done when:** `/swagger` shows all endpoints in Development.

- [x] **3.2 Pagination and filtering**
  - Support `GET /games?page=1&pageSize=10&genreId=2&search=zelda`.
  - **Done when:** the query parameters work, and the response includes the total count.

- [x] **3.3 Global error handling**
  - Use `UseExceptionHandler` with `ProblemDetails`.
  - **Done when:** an unhandled exception returns `application/problem+json`.

- [x] **3.4 TypedResults**
  - **Done when:** endpoints return `TypedResults.*` with `Results<...>` return types.

- [x] **3.5 Logging**
  - **Done when:** create, update and delete actions log through `ILogger`.

## Stage 4: Data modelling

- [x] **4.1 Entity configuration classes**
  - Add `IEntityTypeConfiguration<Game>` and `IEntityTypeConfiguration<Genre>`.
  - Set price precision and a max length for `Name`.
  - **Done when:** a `Data/Configurations/` folder exists and a migration was added for the changes.

- [x] **4.2 New relationship**
  - Add `Publisher` (one-to-many) or `Platform` (many-to-many).
  - **Done when:** the new entity, its migration and its endpoints exist.

- [ ] **4.3 Service layer**
  - Move database logic into `IGameService` / `GameService`, registered in DI.
  - **Done when:** endpoints no longer use `GameStoreContext` directly.

## Stage 5: Testing

- [ ] **5.1 Unit tests**
  - Add a `GameStore.Api.Tests` xUnit project.
  - **Done when:** mapping and service tests pass with `dotnet test`.

- [ ] **5.2 Integration tests**
  - Use `WebApplicationFactory<Program>` with in-memory SQLite.
  - **Done when:** tests call the real endpoints for games, covering create, read, update and delete.

## Stage 6: Security

- [ ] **6.1 JWT authentication**
  - Reads stay public. Create, update and delete require an authenticated user.
  - **Done when:** POST, PUT and DELETE without a token return 401.

- [ ] **6.2 Roles**
  - **Done when:** only an `Admin` role can delete.

## Stage 7: Frontend

- [ ] **7.1 UI create/edit/delete forms**
  - Add forms in `GameStore.Ui`, with a genre dropdown loaded from `/genres`.
  - **Done when:** you can add, edit and delete a game from the UI.

- [ ] **7.2 Better UI error handling**
  - **Done when:** the UI shows a message when the API is down, instead of an empty list.

## Stage 8: Deployment

- [ ] **8.1 Switch to MySQL or SQL Server in Docker**
  - **Done when:** the app runs against the containerised database, and the connection string is in user secrets.

- [ ] **8.2 Docker Compose**
  - **Done when:** `docker compose up` starts the API, the UI and the database.

- [ ] **8.3 CI with GitHub Actions**
  - **Done when:** a workflow builds and runs the tests on every push.

- [ ] **8.4 Deploy**
  - **Done when:** the API is reachable at a public URL.

---

## Progress log

Add a line whenever you finish something or get stuck.

| Date | Step | Notes |
|---|---|---|
| 2026-10-19 | — | Path created. Baseline: basic create/read/update/delete for games, GET for genres, SQLite, EF migrations, read-only Razor UI. |
| 2026-10-19 | 1.1 | Done (commit 57bde8b). `.db` ignored and untracked; both migrations committed. |
| 2026-10-19 | 1.2 | Done. `GameStore.sln` includes the UI; `dotnet build` succeeds for both projects. Solution change not committed yet. |
| 2026-10-19 | 2.1 | In progress. POST checks the genre with `AnyAsync` (returns a plain-string 400); PUT not done yet. Test request added to `game.http`. |
| 2026-10-19 | 2.1 | Done (commit d70f0ee). Tested: POST and PUT with genreId 999 both return 400. Optional polish: switch to `ValidationProblem`, and check the game exists before the genre in PUT. |
| 2026-10-19 | 2.1 | PUT check order fixed: a missing game now returns 404 even when the genre is also bad. Tested. Still a plain-string 400. Not committed yet. |
| 2026-10-19 | 2.2 | Done. DELETE uses the row count: an existing game returns 204, a missing one 404. Tested. Committed in 4014ed9. |
| 2026-10-19 | 2.3 | In progress. POST and PUT done, with duplicate-name checks (PUT correctly excludes itself). Missing: GET /genres/{id}, DELETE, and `.WithParameterValidation()` on the genres group (an empty name isn't rejected). Not committed yet. |
| 2026-10-19 | 2.3 | Nearly done. Validation, GET by id and DELETE added; all routes tested. Remaining: deleting a genre cascade-deletes its games (confirmed on a throwaway DB), POST still uses a hard-coded URL instead of CreatedAtRoute, `genre.http` lacks GET-by-id/DELETE requests. Not committed yet. |
| 2026-10-19 | 2.3 | CreatedAtRoute done ✅. A `hasGames` check was added, but it runs *after* `ExecuteDeleteAsync`, so the delete and cascade already happened. Tested: deleting a genre with games still returns 204 and wipes its games. Not committed yet. |
| 2026-10-19 | 2.3 | Done (commit fc03fe7). The games check now runs before the delete. Tested: a genre with games returns 409 and its games survive; no games returns 204; a missing genre returns 404. Also added `.editorconfig` and formatted the solution. **Stage 2 complete.** |
| 2026-10-19 | 3.1 | In progress. Swashbuckle 6.5.0 added; `/swagger` lists all 10 endpoints. Bug: `app.Environment.IsDevelopment();` is a statement on its own that doesn't guard anything, so Swagger is also served in Production (verified). Not committed yet. |
| 2026-10-19 | 3.1 | Done. Claude wrapped `UseSwagger`/`UseSwaggerUI` in `if (app.Environment.IsDevelopment())`. Tested: `/swagger` returns 200 in Development and 404 in Production. Committed in 8932429. |
| 2026-10-23 | 3.2 | Done. `GET /games` takes `genreId`, `search`, `page` and `pageSize` and returns a `PagedResult` with `totalCount` and `totalPages`. Out-of-range `page`/`pageSize` are clamped (pageSize 1–100). Sorted by Id before Skip/Take. UI updated to read `.Items` (it only shows page 1). Paging/filter requests added to `game.http`, and 7 sample games added. Tested. Note: search is case-sensitive in SQLite. |
| 2026-10-23 | 3.3 | Done. `AddProblemDetails`, `UseExceptionHandler` and `UseStatusCodePages` added. Tested with a temporary `/boom` route (now removed): in both Development and Production an unhandled exception returns a 500 `application/problem+json` without the exception details, and is still logged on the server. Empty 404s also come back as ProblemDetails. The games invalid-genre errors now use `Results.Problem(..., statusCode: 400)`. Gotcha: without `statusCode`, `Results.Problem` defaults to 500. Genre endpoint errors are still plain strings. |
| 2026-10-26 | 3.4 | Done (commit 5ffdb19). All 10 endpoints return `TypedResults.*` with `Results<...>` return types (plain `Ok<T>` for the list endpoints). Genre errors converted to ProblemDetails, including the 409 on delete, so every error now uses one format. Tested: status codes unchanged. Swagger lists every response code per endpoint. Gotcha: `ProblemHttpResult` doesn't tell Swagger its status, so the 409 needed `.ProducesProblem(409)`. Also switched the solution to tab indentation (commit 9c5d6b6). |
| 2026-10-26 | 3.5 | Done. Claude implemented it. Each endpoints file creates one named logger at startup (`CreateLogger("GamesEndpoints")` / `"GenresEndpoints"`), shared by its handlers; a static class can't be used for `ILogger<T>`. Successful create, update and delete log at Information; rejections (bad genre, duplicate name, genre with games) at Warning. Message templates, not `$"..."`. 404s and reads aren't logged. Tested: 11 requests gave 11 correct log lines. Loggers are singletons, so they're safe to capture; DbContext is scoped, so it stays a handler parameter. **Stage 3 complete.** |
| 2026-10-28 | 4.1 | Done. `GameConfiguration` and `GenreConfiguration` in `Data/Configurations/`: `Name` max length 50 on both, `Price` precision (5, 2). Genre seed data moved into `GenreConfiguration`; `OnModelCreating` now just calls `ApplyConfigurationsFromAssembly`. Installed `dotnet-ef` 8 (wasn't on this machine). `ConfigureEntities` migration is empty because SQLite has no column lengths or decimal precision; the rules are recorded in the model snapshot and will apply on SQL Server or MySQL (8.1). Tested on a copy of the DB: migration applied, data intact. |
| 2026-10-30 | 4.2 | Done. Chose `Platform` (many-to-many). `Platform` entity and configuration with 5 seeded platforms; `AddPlatforms` migration creates the `Platforms` table and the `GamePlatform` join table automatically (commit f1d2c22). Full create/read/update/delete for `/platforms`, with a 409 when deleting a platform that has games (commit 8d4c40b). Games: `PlatformIds` on the input DTOs and `GameDetailsDto`, platform names on `GameSummaryDto`; reads use `.Include(game => game.Platforms)`; POST and PUT load the platforms from the DB, return 400 if any ID is unknown, and assign the list (PUT replaces it, and EF adds and removes the join rows). Tested: create, replace, empty, unknown IDs, duplicates and a missing list all behave correctly. Lessons: `ToEntity` can't set platforms, because new `Platform` objects would be INSERTed; `SetValues` doesn't copy collections. Known: malformed JSON returns 500 in Development (400 in Production) because `UseExceptionHandler` catches the thrown `BadHttpRequestException`. |
