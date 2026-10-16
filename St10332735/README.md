# RaceDay API (Part 2)

RESTful API built with **ASP.NET Core Web API (C# .NET 8)**, **Entity Framework Core (SQL Server, Code-First)**, session-based authentication with role-based access control (Organiser / Participant), Swagger UI, and unit tests with GitHub Actions CI/CD.

This is the API backbone for the RaceDay event management system. It handles all data operations, enforces role-based access, and is fully tested before the MVC front end (Part 3).

## Project structure

```
RaceDay.Part2.API/          ASP.NET Core Web API project
├── Controllers/            API endpoints (Auth, Users, Events, Categories, Enrolments, Results)
├── Data/                   AppDbContext (EF Core Code-First)
├── DTOs/                   Request/response data transfer objects
├── Filters/                Authenticated, OrganiserOnly, ParticipantOnly authorization filters
├── Middleware/             CurrentUserMiddleware (attaches authenticated user from session)
├── Models/                 Domain entities (User, Event, Category, Enrolment, Result)
├── Services/               AuthService, PasswordHasher, CurrentUser, IPasswordHasher, IAuthService
├── Program.cs              DI, EF Core, Swagger, session, middleware pipeline
├── appsettings.json        Connection string + JWT settings
└── RaceDay.Part2.API.csproj

RaceDay.Part2.Tests/        xUnit unit tests
├── TestInfrastructure.cs   In-memory DbContext + fake session helpers
├── AuthServiceTests.cs
├── AuthControllerTests.cs
├── UsersControllerTests.cs
├── EventsControllerTests.cs
├── EnrolmentsControllerTests.cs
└── ResultsControllerTests.cs

.github/workflows/ci.yml    GitHub Actions CI/CD (build + test on every push)
```

## Roles

| Role | Can do |
|------|--------|
| **Organiser** | Register/login; create, update, delete their own events; define categories for their events; view all enrolments for their events; capture finish times & positions (results); view/update own profile. |
| **Participant** | Register/login; view events & categories; enrol in an event selecting a category; view their own enrolments & results; view/update own profile. |

A Participant cannot access Organiser endpoints (create/edit/delete events/categories, view all enrolments, capture results) and vice versa. Unauthenticated requests to non-public endpoints are rejected (401). Passwords are hashed (never stored in plain text).

## Authentication & session management

- `POST /api/auth/register` — register with first name, last name, email, password, and role (`Organiser` or `Participant`).
- `POST /api/auth/login` — login with email + password; creates a session storing user id, email, name and role.
- `POST /api/auth/logout` — clears the session.
- `GET /api/auth/me` — current user profile (authenticated only).

Session state is maintained via ASP.NET Core session (cookie) and the `CurrentUserMiddleware` attaches the authenticated user to `HttpContext.Items` for every request. Custom authorization filters (`[Authenticated]`, `[OrganiserOnly]`, `[ParticipantOnly]`) enforce access.

## Endpoints (all visible & testable in Swagger UI)

### Authentication — `POST /api/auth/register`, `POST /api/auth/login`, `POST /api/auth/logout`, `GET /api/auth/me`

### User Profile — `GET /api/users/me`, `PUT /api/users/me` (authenticated)

### Events — `GET /api/events`, `GET /api/events/{id}` (public); `POST /api/events`, `PUT /api/events/{id}`, `DELETE /api/events/{id}` (Organiser only)

Each event: name, description, date, location, distance, event type (Run/Walk/Cycle).

### Categories — `GET /api/categories/event/{eventId}` (public); `POST /api/categories`, `DELETE /api/categories/{id}` (Organiser only, must own the event)

### Enrolments — `POST /api/enrolments`, `GET /api/enrolments/mine` (Participant only); `GET /api/enrolments/event/{eventId}` (Organiser only, must own event)

### Results — `POST /api/results` (Organiser only); `GET /api/results/mine` (Participant only)

Swagger UI: `https://localhost:5001` (or `http://localhost:5000`) — opens at the API root.

## Setup & run instructions

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- SQL Server (localdb works with the default connection string)

### 1. Clone the repository
```bash
git clone <your-repo-url>
cd <repo>
```

### 2. Configure the database
Edit `RaceDay.Part2.API/appsettings.json` if needed (default uses SQL Server localdb `RaceDayDb`):
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=RaceDayDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
```

### 3. Run the API
```bash
cd RaceDay.Part2.API
dotnet run
```
The database is created automatically on startup via Code-First (`context.Database.EnsureCreated()`).

### 4. Run the unit tests
```bash
cd RaceDay.Part2.Tests
dotnet test
```

## CI/CD (GitHub Actions)

A workflow at `.github/workflows/ci.yml` runs on every push/pull request:
1. Restores dependencies
2. Builds the API project
3. Builds the test project
4. Runs all unit tests

![CI Build Status](docs/ci-screenshot.png)

> **Note:** Replace `docs/ci-screenshot.png` with a screenshot of your own successful green GitHub Actions build.

## Unit tests

Tests cover both success and failure scenarios:
- User registration & login (duplicate email, short password, wrong password, unknown email, case-insensitive email)
- Event management (Organiser can create/update/delete; Participant and unauthenticated rejected; organisers cannot edit others' events)
- Role-based access enforced (401 unauthenticated, 403 wrong role)
- Event enrolments (Participant can enrol; enrolment recorded; duplicate enrolment rejected; organiser views enrolments for their events)
- Results (Organiser captures results; Participant views own results; duplicate result rejected)

All tests run against an in-memory database.

## Submission requirements

- GitHub repository link submitted on ARC
- README includes setup/run instructions, role descriptions, CI/CD screenshot, and YouTube video link
- Unlisted YouTube video demonstrating the running API, explaining code structure, endpoints, authentication, and unit tests

## License

Educational project — RaceDay system.
