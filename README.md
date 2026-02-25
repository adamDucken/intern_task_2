# intern_task_2

REST API for managing houses, apartments, and residents — built with ASP.NET Core, EF Core, Angular, and PostgreSQL.

## stack

- **backend**: ASP.NET Core 8, EF Core, PostgreSQL
- **frontend**: Angular 19, pnpm
- **auth**: JWT + Google OAuth2
- **tests**: Node.js integration tests

## features

- CRUD for houses, apartments, residents
- JWT authentication with role-based authorization
- Google OAuth2 login
- input validation (email format, required fields, numeric values)
- integration tests covering all endpoints and auth flows
- Swagger UI for interactive API docs

## run with docker
```bash
./start.sh
```

builds and starts db + backend, runs integration tests, then starts frontend if tests pass.

| service  | url                          |
|----------|------------------------------|
| backend  | http://localhost:5291        |
| swagger  | http://localhost:5291/swagger|
| frontend | http://localhost:4200        |

## run locally
```bash
# api
dotnet run --project src/intern_task_2

# tests
cd tests && pnpm test:all 

# frontend
cd client && pnpm install && pnpm start
```

## requirements

- .NET 8.0+
- Node.js 22+, pnpm
- PostgreSQL 16 (or Docker)

## project structure
```
src/intern_task_2/       # api — controllers, services, models, migrations
tests/                   # integration tests (node js)
client/                  # angular frontend
```
