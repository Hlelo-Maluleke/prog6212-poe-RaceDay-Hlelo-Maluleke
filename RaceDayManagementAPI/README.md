# RaceDay API – South African Event Management System

## Brief Description

**RaceDay** is a full-stack event management platform for the **South African road running, walking, and cycling community**. It covers the full event lifecycle: event creation, participant registration, category and route management, race-day entries, result capture, and public results.

This repository contains the **backend REST API**. The Razor MVC front-end is a separate project in [`../RaceDayMVC`](../RaceDayMVC/README.md).

The API replaces paper-based registration, spreadsheets, and disconnected communication channels with a centralised digital solution that connects **Event Organisers** and **Participants**.

### Key Features

- **Event Management** – Organisers create, update, and delete events with multiple categories (distance, entry fee, capacity, start time).
- **Participant Registration** – Participants browse events, enrol in categories, withdraw, and track their history.
- **Route Information** – Route maps, elevation gain, and route descriptions per category.
- **Results Management** – Organisers capture finish times and positions; results are available publicly and to the participant.
- **Role-Based Access** – Separate organiser and participant permissions enforced with JWT bearer tokens.
- **Event Banner Uploads** – Banners are stored in **Azure Blob Storage**, with a local `wwwroot` fallback when Azure is not configured.
- **Pre-Seeded Demo Data** – Realistic South African events load automatically on first startup.

---

## System Roles

RaceDay has two roles. A user's role is determined by which subtype record exists, not by a role column.

### 1. Organiser

Organisers are event managers. Organiser accounts are **pre-created** by an administrator (or by the seeding process) and cannot self-register through the participant registration endpoint.

**Permissions:**

- Create, update, and delete their own events
- Define event categories (distances, entry fees, capacity limits)
- Upload route information (map URL, elevation profile)
- Upload and replace event banners
- View all participant enrolments for their events
- Capture and update participant results
- Publish, complete, or cancel events

**Example:** *Micheal Nkoane (Soweto Running Club) creates the Soweto Marathon, defines 10 km, 21 km, and 42 km categories, and later captures the results.*

### 2. Participant

Participants are event entrants. Participant accounts are **self-registered** through the public registration endpoint.

**Permissions:**

- Create an account (self-registration)
- View published events and their categories
- Enrol in an event category
- View and withdraw their own enrolments
- View their own race results
- View route and elevation information

**Example:** *Lindiwe Nkosi registers, finds the Soweto Marathon, enrols in the 21 km Half Marathon, and later views her finish time and position.*

---

## Technology Stack

| Layer | Technology |
| :--- | :--- |
| **Framework** | ASP.NET Core 8 Web API |
| **ORM** | Entity Framework Core 8 |
| **Database** | SQL Server (LocalDB by default) |
| **Test Database** | EF Core In-Memory provider |
| **Authentication** | JWT (JSON Web Tokens, HMAC-SHA256) |
| **Password Hashing** | SHA-256 |
| **File Storage** | Azure Blob Storage (`Azure.Storage.Blobs`) with local fallback |
| **Documentation** | Swagger / OpenAPI (Swashbuckle) |
| **Testing** | xUnit |
| **IDE** | Visual Studio 2022/2026 or VS Code |

---

## Architecture

```text
Swagger / RaceDayMVC / any HTTP client
                  │
                  │  JWT Bearer token
                  ▼
        RaceDayManagementAPI
          ├── Controllers → EF Core → SQL Server (LocalDB by default)
          └── IBlobService → Azure Blob Storage
                              └── local wwwroot/uploads/banners fallback
```

The MVC front-end does **not** access the database directly. It calls this API and forwards the JWT it receives at login.

---

## Database Schema

The system uses **8 entities** with role separation through subtype tables.

| Entity | Description |
| :--- | :--- |
| `Users` | Base entity – login credentials and personal details for all users. |
| `Organisers` | Subtype of `Users` – event managers. |
| `Participants` | Subtype of `Users` – self-registered event entrants. |
| `Events` | Event details (name, date, location, type, status, banner URL). |
| `Categories` | Event categories (distance, entry fee, capacity, start time). |
| `Routes` | Route information (map URL, elevation gain, description) – 1-to-1 with `Categories`. |
| `Enrolments` | Participant entries into specific categories. |
| `Results` | Race results (finish time in seconds, position, status) – 1-to-1 with `Enrolments`. |

### Role Determination

There is **no `role` column** in `Users`. A user is an organiser if a matching `Organisers` row exists, and a participant if a matching `Participants` row exists.

### Schema Creation

The API calls `Database.EnsureCreated()` on startup and then runs a small compatibility statement that adds `Events.BannerUrl` if the column is missing. **There are no EF Core migrations in the project.**

- On a fresh database, `EnsureCreated()` creates the schema and applies the `HasData` seed.
- On an existing database, `EnsureCreated()` does not apply model changes. If you change the schema, either delete the database or introduce EF Core migrations.

---

## Configuration

The main settings live in `RaceDayManagementAPI/appsettings.json`.

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=RaceDayDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },
  "JwtSettings": {
    "Secret": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "ExpiryMinutes": 60,
    "Issuer": "RaceDayAPI",
    "Audience": "RaceDayClient"
  },
  "AzureBlobStorage": {
    "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=racedaystorage;AccountKey=...;EndpointSuffix=core.windows.net",
    "ContainerName": "event-banners"
  },
  "AllowedHosts": "*"
}
```

### Settings explained

| Setting | Purpose |
| :--- | :--- |
| `ConnectionStrings:DefaultConnection` | SQL Server connection. The default uses **SQL Server LocalDB**, which is installed with the Visual Studio ASP.NET workload. |
| `JwtSettings:Secret` | Symmetric signing key. Must be at least 32 characters. Replace it outside development. |
| `JwtSettings:ExpiryMinutes` | Token lifetime in minutes. |
| `JwtSettings:Issuer` / `Audience` | Validated when the token is checked. |
| `AzureBlobStorage:ConnectionString` | Azure Storage connection string. The `AccountKey=...` value is a deliberate placeholder that triggers the local fallback. |
| `AzureBlobStorage:ContainerName` | Blob container name. Defaults to `event-banners` and is created on first upload. |

### Recommended secret handling

Do not commit real secrets. Use **User Secrets** in development or environment variables in deployment:

```powershell
dotnet user-secrets init
dotnet user-secrets set "JwtSettings:Secret" "<a-long-random-secret>"
dotnet user-secrets set "AzureBlobStorage:ConnectionString" "<your-azure-connection-string>"
```

Environment-variable equivalents use double underscores:

```text
ConnectionStrings__DefaultConnection
JwtSettings__Secret
JwtSettings__ExpiryMinutes
JwtSettings__Issuer
JwtSettings__Audience
AzureBlobStorage__ConnectionString
AzureBlobStorage__ContainerName
```

If the Azure connection string is missing or still contains `AccountKey=...`, banner uploads are saved to the API's local `wwwroot/uploads/banners` folder instead. This makes the project runnable without an Azure subscription.

---

## Setup & Run Instructions

### Prerequisites

- **Visual Studio 2022/2026** with the **ASP.NET and web development** workload, or the .NET CLI
- **.NET 8 SDK** – [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
- **SQL Server LocalDB** (included with the Visual Studio ASP.NET workload) or another SQL Server instance
- **Git**
- Optional: an **Azure Storage account** if you want to test real Blob Storage uploads

### Step 1: Open the solution

Open `RaceDayManagementAPI.sln` in Visual Studio, or use the CLI from this folder.

### Step 2: Restore packages

```bash
dotnet restore
```

The API uses these main packages:

- `Microsoft.EntityFrameworkCore.SqlServer`
- `Microsoft.EntityFrameworkCore.InMemory` (tests)
- `Microsoft.AspNetCore.Authentication.JwtBearer`
- `Swashbuckle.AspNetCore`
- `Azure.Storage.Blobs`
- `EFCore.NamingConventions`

### Step 3: Configure the connection and secrets

Confirm `appsettings.json` points at a SQL Server instance you can reach. For LocalDB the default is usually fine. For a full SQL Server instance, replace `DefaultConnection` with your server and credentials.

Optionally configure Azure Blob Storage. If you leave the placeholder, uploads use local storage.

### Step 4: Run the API

In Visual Studio, press **F5** and choose the `https` profile. Swagger opens automatically.

Or use the CLI:

```bash
dotnet run --launch-profile https
```

Default development URLs:

| Profile | URL |
| :--- | :--- |
| `https` | `https://localhost:7065` |
| `http` | `http://localhost:5120` |

Swagger UI:

```text
https://localhost:7065/swagger
```

### Step 5: Confirm the database and seed data

On first run the database is created automatically and the demo data is seeded. The API does **not** print seed counts to the console, so verify by calling the public endpoints or by querying the database.

Expected seed totals:

| Entity | Count |
| :--- | :--- |
| Users | 5 |
| Organisers | 2 |
| Participants | 3 |
| Events | 3 |
| Categories | 7 |
| Routes | 3 |
| Enrolments | 8 |
| Results | 4 |

Quick verification with Swagger or curl:

```bash
curl https://localhost:7065/api/events
```

A successful response contains the three seeded events: **Soweto Marathon**, **Cape Town Cycle Tour**, and **Durban Community Parkrun**.

---

## Default Login Credentials

All seeded accounts use the password **`password123`**.

| Role | Name | Email | Company |
| :--- | :--- | :--- | :--- |
| **Organiser** | Micheal Nkoane | `micheal@runningclub.co.za` | Soweto Running Club |
| **Organiser** | Susan van der Merwe | `susan@capetowncycles.co.za` | Cape Town Cycle Tours |
| **Participant** | Lindiwe Nkosi | `lindiwe@runsa.co.za` | – |
| **Participant** | Michael Jacobs | `michael@cyclezone.co.za` | – |
| **Participant** | Sipho Dlamini | `sipho@fitness.co.za` | – |

> **Note:** Some older notes and the `.http` scratch file refer to an organiser named "Thabo". The actual seeded organiser account is **Micheal Nkoane** (`micheal@runningclub.co.za`).

---

## Authentication Flow

1. **Register** a participant via `POST /api/auth/register` (public), or use a seeded account.
2. **Login** via `POST /api/auth/login` with email and password.
3. The API validates the SHA-256 password hash and returns a **JWT**.
4. The token contains:
   - `NameIdentifier` – user ID
   - `Email`
   - `Name` – full name
   - `isOrganiser` – `"true"` or `"false"`
   - `isParticipant` – `"true"` or `"false"`
5. Send the token on every protected request:

   ```http
   Authorization: Bearer <token>
   ```

6. In Swagger, click **Authorize**, enter `Bearer <token>`, and authorise.

The token is signed with HMAC-SHA256 and validated for issuer, audience, lifetime, and signing key.

---

## API Endpoint Overview

All routes are relative to the API base URL, for example `https://localhost:7065`.

### Authentication

| Method | Endpoint | Description | Access |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/auth/register` | Register a new participant. | Public |
| `POST` | `/api/auth/login` | Login and receive a JWT. | Public |

### Admin / User Management

| Method | Endpoint | Description | Access |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/admin/users` | List all users with role flags. | Any authenticated user |
| `GET` | `/api/admin/users/{id}` | Get a single user. | Any authenticated user |
| `POST` | `/api/admin/organisers` | Create a new organiser. | Public in the current implementation |
| `DELETE` | `/api/admin/users/{id}` | Delete a user and subtype record. | Any authenticated user |

> There is no dedicated administrator role or policy. These endpoints are protected by `[Authorize]` (except organiser creation, which is `[AllowAnonymous]`). Treat organiser creation as a bootstrap/administrative endpoint and restrict it before production use.

### Participant

| Method | Endpoint | Description | Access |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/participants/user` | Get own profile. | Participant |
| `PUT` | `/api/participants/user` | Update own profile. | Participant |
| `GET` | `/api/participants/user/enrolments` | Get own enrolments with event, category, and result data. | Participant |
| `GET` | `/api/participants/user/results` | Get own race results. | Participant |

### Organiser

| Method | Endpoint | Description | Access |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/organisers/user` | Get own profile. | Organiser |
| `PUT` | `/api/organisers/user` | Update own profile and company name. | Organiser |
| `GET` | `/api/organisers/user/events` | List own events with categories. | Organiser |
| `GET` | `/api/organisers/user/events/{eventId}/enrolments` | View enrolments and results for an owned event. | Organiser (owner) |

### Events

| Method | Endpoint | Description | Access |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/events` | List events. Optional query parameters: `status`, `type`, `fromDate`. | Public |
| `GET` | `/api/events/{id}` | Get event details, categories, and routes. | Public |
| `POST` | `/api/events` | Create an event. | Organiser |
| `POST` | `/api/events/{id}/banner` | Upload/replace an event banner (`multipart/form-data`, field name `file`). | Organiser (owner) |
| `PUT` | `/api/events/{id}` | Update an event. | Organiser (owner) |
| `DELETE` | `/api/events/{id}` | Delete an event and its dependent data. | Organiser (owner) |
| `GET` | `/api/events/{eventId}/categories` | List categories and routes for an event. | Public |
| `POST` | `/api/events/{eventId}/categories` | Create a category (optionally with route data). | Organiser (owner) |

### Categories

| Method | Endpoint | Description | Access |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/categories/{id}` | Get category details and route. | Public |
| `PUT` | `/api/categories/{id}` | Update a category. | Organiser (owner) |
| `DELETE` | `/api/categories/{id}` | Delete a category. Fails with `409` if active enrolments exist. | Organiser (owner) |
| `GET` | `/api/categories/{id}/route` | Get route information. | Public |
| `PUT` | `/api/categories/{id}/route` | Create or update route information. | Organiser (owner) |

### Enrolments

| Method | Endpoint | Description | Access |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/enrolments` | Enrol in a category. Body: `{ "categoryID": 1 }`. | Participant |
| `GET` | `/api/enrolments/{id}` | Get enrolment details. | Participant owner or event organiser |
| `DELETE` | `/api/enrolments/{id}` | Withdraw from an event (sets status to `withdrawn`). | Participant owner |

Enrolment rules enforced by the API:

- The event must not be `draft`, `cancelled`, or `completed`.
- The category must not be full.
- Duplicate active enrolments are rejected with `409`.
- Re-enrolling after withdrawal reactivates the existing record.

### Results

| Method | Endpoint | Description | Access |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/enrolments/{enrolmentId}/results` | Capture or update a result (UPSERT). | Organiser (owner) |
| `GET` | `/api/events/{eventId}/results` | View public results for an event, ordered by position. | Public |
| `GET` | `/api/results/user` | View own results. | Participant |
| `GET` | `/api/results/{id}` | Get a single result. | Public |

---

## Azure Blob Storage Integration

Event banners are uploaded through the API, not directly from the browser to Azure. This keeps the storage credentials on the server.

### Components

| File | Responsibility |
| :--- | :--- |
| `Services/IBlobService.cs` | Abstraction with `UploadAsync` and `DeleteAsync`. |
| `Services/BlobService.cs` | Azure Blob implementation with local filesystem fallback. |
| `Controllers/EventsController.cs` | `POST /api/events/{id}/banner` endpoint, owner check, and `BannerUrl` persistence. |
| `Program.cs` | Registers `BlobService` and `IBlobService` as singletons. |
| `Models/Event.cs` | `BannerUrl` property stored in SQL Server. |

### Upload flow

1. The organiser submits a banner through the MVC form.
2. The MVC front-end calls `POST /api/events/{id}/banner` with `multipart/form-data` and the field name `file`.
3. The API checks that the caller owns the event.
4. A unique blob name is generated: `event-{id}-{guid}{extension}`.
5. `BlobService.UploadAsync`:
   - Creates a `BlobServiceClient` from `AzureBlobStorage:ConnectionString`.
   - Gets or creates the configured container with **public blob read access**.
   - Uploads the stream with the file's content type.
   - Returns the public blob URL.
6. The API saves the returned URL to `Events.BannerUrl` and responds with `{ "bannerUrl": "..." }`.

### Local fallback

If the connection string is missing or still contains the placeholder `AccountKey=...`, or if the Azure upload throws an exception, the file is written to:

```text
RaceDayManagementAPI/wwwroot/uploads/banners/
```

The service returns a relative URL such as `/uploads/banners/event-1-....jpg`. The controller converts it to an absolute URL using the current request scheme and host before saving it to the database.

This means the project works without Azure, but the API must serve static files (`app.UseStaticFiles()`, already configured) for the fallback URL to resolve.

### Deletion

`IBlobService.DeleteAsync(fileName)` attempts to delete the blob and also removes the matching local fallback file if it exists. The current API does not automatically call `DeleteAsync` when an event is deleted or a banner is replaced, so old blobs may remain until cleaned up separately.

### Security notes

- The container is created with **public blob read access**, so anyone with the blob URL can read the image. Use private containers plus SAS tokens or a CDN/proxy if banners must be private.
- The connection string is a secret. Use User Secrets or environment variables, never a committed real key.
- The current implementation authenticates to Azure with a connection string. For production, consider managed identity and `BlobServiceClient` with `DefaultAzureCredential`.
- The API checks event ownership before uploading, but it does not enforce a file-size or MIME-type allow-list server-side. Add validation if the endpoint is exposed publicly.

---

## Project Structure

```text
RaceDayManagementAPI/
├── RaceDayManagementAPI.sln
├── RaceDayManagementAPI/
│   ├── Controllers/
│   │   ├── AdminController.cs
│   │   ├── AuthController.cs
│   │   ├── CategoriesController.cs
│   │   ├── EnrolmentsController.cs
│   │   ├── EventsController.cs
│   │   ├── OrganiserController.cs
│   │   ├── ParticipantController.cs
│   │   └── ResultsController.cs
│   ├── Data/
│   │   └── RaceDayDbContext.cs        # DbSets, relationships, HasData seed
│   ├── Models/
│   │   ├── User.cs
│   │   ├── Organiser.cs
│   │   ├── Participant.cs
│   │   ├── Event.cs
│   │   ├── Category.cs
│   │   ├── Route.cs
│   │   ├── Enrolment.cs
│   │   ├── Result.cs
│   │   ├── JwtSettings.cs
│   │   ├── LoginRequest.cs
│   │   ├── LoginResponse.cs
│   │   ├── RegisterRequest.cs
│   │   └── RaceDayDB.cs              # Legacy in-memory helpers used by tests
│   ├── Services/
│   │   ├── JwtService.cs
│   │   ├── IBlobService.cs
│   │   └── BlobService.cs
│   ├── Properties/
│   │   └── launchSettings.json
│   ├── Program.cs
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   ├── RaceDayManagementAPI.http
│   └── RaceDayManagementAPI.csproj
└── RaceDayManagementAPI.Tests/
    ├── Controllers/
    │   ├── AuthControllerTests.cs
    │   └── EventsControllerCrudTests.cs
    ├── Fakes/
    │   └── FakeBlobService.cs
    ├── Fixtures/
    │   ├── ControllerContextHelper.cs
    │   ├── JwtServiceFixture.cs
    │   └── RaceDayDBFixture.cs
    ├── TestAssembly.cs
    └── RaceDayManagementAPI.Tests.csproj
```

---

## Running the Unit Tests

The test project uses the **EF Core In-Memory provider**, so the tests do not need SQL Server or Azure. Test parallelisation is disabled in `TestAssembly.cs` because the auth tests share a fixture.

### Via Test Explorer

1. Open **Test → Test Explorer**.
2. Click **Run All Tests** (or press `Ctrl + R, A`).

### Via CLI

From the solution folder:

```bash
dotnet test
```

Expected result:

```text
Passed! - Failed: 0, Passed: 18, Skipped: 0, Total: 18
```

### Test coverage

**EventsController – 5 CRUD tests**

| # | Test | Purpose |
| :--- | :--- | :--- |
| 1 | `GET_ReadEvents_ReturnsAllEvents` | Verifies all events are returned. |
| 2 | `GET_ReadEventById_ReturnsEvent` | Verifies a single event can be retrieved. |
| 3 | `POST_CreateEvent_CreatesEvent` | Verifies an organiser can create an event. |
| 4 | `PUT_UpdateEvent_UpdatesEvent` | Verifies the event owner can update their event. |
| 5 | `DELETE_RemoveEvent_DeletesEvent` | Verifies the event owner can delete their event. |

**AuthController – 13 tests**

| Area | Tests |
| :--- | :--- |
| Registration | Valid registration, empty email, empty password, duplicate email, null fields, password hashing. |
| Login | Valid credentials, invalid email, invalid password, organiser role flag, JWT generation, returned user details, empty email. |

The tests inject `FakeBlobService`, so banner storage is not exercised by the unit tests.

---

## Version Control

The project follows a structured Git workflow with meaningful commits covering:

- Project setup and dependencies
- Database schema and seed data
- Authentication (register, login, JWT)
- User profile management
- Event and category CRUD
- Enrolments
- Results
- Azure Blob banner uploads
- Unit tests
- Documentation

---

## YouTube Demo

🔗 [RaceDay demo video](https://youtu.be/d3lzgB0bU2k?si=d4nKNp0eMGMnC111)

---

## Related Documentation

- [RaceDay MVC front-end README](../RaceDayMVC/README.md) – MVC structure, session/JWT relay, Azure Blob upload flow from the UI, and Docker setup.
