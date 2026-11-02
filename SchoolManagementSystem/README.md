# School Management System API

A beginner-friendly ASP.NET Core Web API starter project using C#, .NET 10, Entity Framework Core 10, SQL Server, JWT authentication, role-based authorization, and Docker.

## Requirements

- .NET 10 SDK
- SQL Server, or Docker Desktop
- VS Code (C# Dev Kit extension recommended)
- EF Core CLI: `dotnet tool install --global dotnet-ef --version 10.0.0`

## 1. Open the project

Open this folder in VS Code. Open a terminal in the folder containing `SchoolManagementSystem.csproj`.

## 2. Configure the database

For a local SQL Server installation, update `ConnectionStrings:DefaultConnection` in `appsettings.json`.

For Docker, the provided Compose file starts SQL Server and the API together.

## 3. Create the database schema

Install EF CLI if needed:

```powershell
dotnet tool install --global dotnet-ef --version 10.0.0
```

Restore packages:

```powershell
dotnet restore
```

Create and apply the first migration:

```powershell
dotnet ef migrations add InitialCreate
dotnet ef database update
```

The application also calls `Database.Migrate()` at startup for this learning project. For production, run migrations as a controlled deployment step.

## 4. Run locally

Make sure SQL Server is running, then:

```powershell
dotnet run
```

Open the URL printed in the terminal to use the web dashboard. Swagger is available at `/swagger` in Development.

The dashboard supports sign-in and registration, viewing students, courses, and enrollments, and (for Admin users) creating, editing, and deleting records. It uses the same-origin API, so no separate frontend server or CORS configuration is needed.

The expanded portal also includes:

- Teacher accounts (created by an Admin with a temporary password), class sections assigned to teachers, and class rosters.
- Per-class, per-date attendance with Present, Absent, Late, and Excused statuses. Teachers can manage attendance for their assigned classes.
- Class assessments with points possible, student scores, percentages, and letter grades (A ≥ 90%, B ≥ 80%, C ≥ 70%, D ≥ 60%, F below 60%). Teachers can grade their assigned classes.
- Filterable roster, attendance, and grade reports with CSV downloads.
- Account profile/password settings, plus Admin-managed school name and time zone.

The new database schema is applied by the existing startup migration. If you have an earlier database, back it up before upgrading. Existing course enrollments are preserved; enrollments added to a class section appear in that class roster.

## 5. Default seeded administrator

- Email: `admin@school.local`
- Password: `ChangeMe123!`

Change this immediately before using the project beyond local learning. The seed credentials are for local development only.

New registrations receive the `Teacher` role. Only an Admin can create, update, or delete students, courses, and enrollments. Both Admin and Teacher can view students and courses.

## 6. API endpoints

| Method | Endpoint | Access |
|---|---|---|
| POST | `/api/auth/register` | Public |
| POST | `/api/auth/login` | Public |
| GET | `/api/students` | Admin, Teacher |
| GET | `/api/students/{id}` | Admin, Teacher |
| POST | `/api/students` | Admin |
| PUT | `/api/students/{id}` | Admin |
| DELETE | `/api/students/{id}` | Admin |
| GET | `/api/courses` | Admin, Teacher |
| GET | `/api/courses/{id}` | Admin, Teacher |
| POST | `/api/courses` | Admin |
| PUT | `/api/courses/{id}` | Admin |
| DELETE | `/api/courses/{id}` | Admin |
| GET | `/api/enrollments` | Admin, Teacher |
| POST | `/api/enrollments` | Admin |
| DELETE | `/api/enrollments/{id}` | Admin |
| GET, POST | `/api/teachers` | Admin |
| GET, POST | `/api/classes` | Admin; teachers see assigned classes |
| PUT, DELETE | `/api/classes/{id}` | Admin |
| GET, POST, DELETE | `/api/classes/{id}/roster` | Admin; teachers can view |
| GET, PUT | `/api/classes/{id}/attendance?date=YYYY-MM-DD` | Admin, assigned teacher |
| GET, POST | `/api/classes/{id}/assessments` | Admin, assigned teacher |
| PUT, DELETE | `/api/assessments/{id}/grades` or `/api/assessments/{id}` | Admin; assigned teacher can save grades |
| GET | `/api/reports/roster`, `/api/reports/attendance`, `/api/reports/grades` | Admin, assigned teacher |
| GET, PUT | `/api/settings` | Admin can update; teachers can view |
| PUT | `/api/auth/profile`, `/api/auth/password` | Signed-in user |

For protected endpoints, first log in and copy the JWT. In Swagger, click **Authorize** and enter `Bearer YOUR_TOKEN`.

## 7. Run with Docker Compose

First generate the EF Core migration files so the API can create the database schema on startup:

```powershell
dotnet tool install --global dotnet-ef --version 10.0.0
dotnet restore
dotnet ef migrations add InitialCreate
```

You do not need to run `dotnet ef database update` separately when using the provided Compose setup; the API applies the migration when it starts.

Set a strong unique SQL Server password and JWT signing key before using this outside a local learning environment. Keep real secrets out of source control.

```powershell
docker compose up --build
```

API: `http://localhost:8080`

Stop:

```powershell
docker compose down
```

Stop and delete the database volume (this permanently deletes local database data):

```powershell
docker compose down -v
```

## Learning sequence

1. C# fundamentals: types, variables, conditions, loops, methods.
2. OOP: classes, objects, encapsulation, inheritance, interfaces.
3. ASP.NET Core: controllers, routing, HTTP methods, dependency injection.
4. EF Core: DbContext, entities, relationships, migrations, LINQ.
5. SQL Server: tables, keys, relationships, queries.
6. Authentication: password hashing, JWT, claims, roles.
7. Docker: image, container, ports, environment variables, volumes, Compose.

## Important learning and production notes

- This is a starter project, not a complete production school information system.
- The sample JWT key and SQL password are development placeholders. Replace them with secrets before running in any shared environment.
- Use HTTPS in production.
- Add rate limiting, refresh-token strategy, audit logging, pagination, and stronger account-management flows before production.
- Review privacy and data-retention requirements before storing real student data.
- The sample uses a simple custom user table and role claim to teach authentication concepts; larger systems may use ASP.NET Core Identity.
