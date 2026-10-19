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

- [ ] **2.3 Full create/read/update/delete for genres**
  - Add `GET /genres/{id}`, `POST`, `PUT` and `DELETE`, with DTOs and mapping.
  - **Done when:** `genre.http` has working requests for all five routes.

## Stage 3: Professional API

- [ ] **3.1 Swagger / OpenAPI**
  - **Done when:** `/swagger` shows all endpoints in Development.

- [ ] **3.2 Pagination and filtering**
  - Support `GET /games?page=1&pageSize=10&genreId=2&search=zelda`.
  - **Done when:** the query parameters work, and the response includes the total count.

- [ ] **3.3 Global error handling**
  - Use `UseExceptionHandler` with `ProblemDetails`.
  - **Done when:** an unhandled exception returns `application/problem+json`.

- [ ] **3.4 TypedResults**
  - **Done when:** endpoints return `TypedResults.*` with `Results<...>` return types.

- [ ] **3.5 Logging**
  - **Done when:** create, update and delete actions log through `ILogger`.

## Stage 4: Data modelling

- [ ] **4.1 Entity configuration classes**
  - Add `IEntityTypeConfiguration<Game>` and `IEntityTypeConfiguration<Genre>`.
  - Set price precision and a max length for `Name`.
  - **Done when:** a `Data/Configurations/` folder exists and a migration was added for the changes.

- [ ] **4.2 New relationship**
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
