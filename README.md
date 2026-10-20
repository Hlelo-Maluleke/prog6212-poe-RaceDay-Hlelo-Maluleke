# RaceDay – South African Event Management System

## Brief Description

**RaceDay** is a full-stack web-based event management platform designed specifically for the **South African road running, walking, and cycling community**. The system streamlines the entire event lifecycle – from event creation and participant registration to capturing results and publishing race outcomes.

The platform replaces outdated paper-based registration, spreadsheets, and disconnected communication channels by providing a **centralised, digital solution** that connects Event Organisers and Participants in one seamless experience.

### Key Features
- **Event Management** – Organisers can create, update, and delete events with multiple categories (distances, entry fees, capacity limits).
- **Participant Registration** – Participants can browse upcoming events, enrol in categories, and track their personal performance history.
- **Route & Weather Information** – Participants can access route maps and elevation data to prepare for race day.
- **Results Management** – Organisers can capture finish times and positions; participants can view their own results and public race outcomes.
- **Role-Based Access** – Separate login flows and authorisation for Organisers and Participants, enforced by JWT tokens.
- **Pre-Seeded Demo Data** – Realistic South African events (Soweto Marathon, Cape Town Cycle Tour, Durban Parkrun) load automatically on startup.

---

## System Roles

RaceDay supports two distinct user roles, each with specific permissions and responsibilities.

### 1. Organiser

Organisers are **event managers** who create, manage, and oversee events. Organiser accounts are **pre-created** by the system administrator and cannot be self-registered.

**Permissions:**
- Create, update, and delete events
- Define event categories (distances, entry fees, capacity limits)
- Upload route information (map URLs, elevation profiles)
- View all participant enrolments for their events
- Capture and update participant results (finish times and positions)
- Publish or cancel events
- Manage event status (draft, published, completed, cancelled)

**Example Use Case:**
*Thabo Mokoena (Soweto Running Club) creates the Soweto Marathon. He defines three categories: 10km, 21km, and 42km. After the race, he uploads results for all participants.*

---

### 2. Participant

Participants are **event entrants** who browse, register for, and take part in events. Participant accounts are **self-registered** via the public registration endpoint.

**Permissions:**
- Create a user account (self-registration)
- View all published events and their categories
- Enrol in events by selecting a category
- View their own enrolment history and status
- Withdraw from an event
- View their personal race results and performance history
- Access route and elevation information to prepare for race day

**Example Use Case:**
*Lindiwe Nkosi browses the RaceDay platform, finds the Soweto Marathon, enrols in the 21km Half Marathon, and later views her finish time of 1 hour 45 minutes (position 42).*

---

## Technology Stack

| Layer | Technology |
| :--- | :--- |
| **Framework** | ASP.NET Core 8 Web API |
| **Database** | Entity Framework Core (In-Memory Database) |
| **Authentication** | JWT (JSON Web Tokens) |
| **Password Hashing** | SHA-256 |
| **Documentation** | Swagger / OpenAPI |
| **Testing** | xUnit |
| **IDE** | Visual Studio 2022 |

---

## Database Schema

The system uses **8 entities** in a fully normalised structure with role separation:

| Entity | Description |
| :--- | :--- |
| `Users` | Base entity – login credentials and personal details for all users. |
| `Organisers` | Subtype of `Users` – pre-seeded event managers. |
| `Participants` | Subtype of `Users` – self-registered event entrants. |
| `Events` | Event details (name, date, location, type, status). |
| `Categories` | Event categories (distances, entry fees, capacity limits). |
| `Routes` | Route information (map URLs, elevation) – 1-to-1 with `Categories`. |
| `Enrolments` | Participant entries into specific categories. |
| `Results` | Race results (finish times, positions) – 1-to-1 with `Enrolments`. |

### Role Determination
There is **no `role` column** in the `Users` table. A user's role is determined by the existence of a record in either the `Organisers` or `Participants` subtype table.

---

## Setup & Run Instructions

### Prerequisites
- **Visual Studio 2022** (or later) with the **ASP.NET and web development** workload
- **.NET 8 SDK** – [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Git** (for cloning the repository)

### Step 1: Clone the Repository
```bash
git clone https://github.com/your-username/RaceDayManagementAPI.git
cd RaceDayManagementAPI
```

### Step 2: Open the Solution
1. Launch **Visual Studio 2022**.
2. Click **File → Open → Project/Solution**.
3. Navigate to the cloned folder and open `RaceDayManagementAPI.sln`.

### Step 3: Restore NuGet Packages
Right-click the solution in **Solution Explorer** → **Restore NuGet Packages**.

Alternatively, run:
```bash
dotnet restore
```

**Required packages (already in `.csproj`):**
- `Microsoft.EntityFrameworkCore.InMemory`
- `Microsoft.AspNetCore.Authentication.JwtBearer`
- `Swashbuckle.AspNetCore`
- `System.IdentityModel.Tokens.Jwt`

### Step 4: Verify `appsettings.json`
Confirm the file contains the following:
```json
{
  "JwtSettings": {
    "Secret": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "ExpiryMinutes": 60,
    "Issuer": "RaceDayAPI",
    "Audience": "RaceDayClient"
  },
  "AllowedHosts": "*"
}
```

### Step 5: Run the Application
1. Press **F5** in Visual Studio (or click the green **Run** button).
2. The API will start and Swagger UI will open automatically.
3. If Swagger doesn't open, navigate to:
   ```
   https://localhost:{PORT}/swagger/index.html
   ```
   *(Replace `{PORT}` with the port number shown in the Visual Studio Output window.)*

### Step 6: Verify the Seed Data Loaded
Check the **Output** window in Visual Studio for the following log lines on startup:

```
Users:        5
Organisers:   2
Participants: 3
Events:       3
Categories:   7
Routes:       3
Enrolments:   8
Results:      4
```

If these numbers match, the pre-seeded data is loaded correctly.

---

## Running the Unit Tests

The solution includes a test project (`RaceDayManagementAPI.Tests`) with 5 unit tests covering the **EventsController**.

### Option 1: Run via Test Explorer
1. In Visual Studio, open **Test → Test Explorer**.
2. Click **Run All Tests** (or press `Ctrl + R, A`).
3. All 5 tests should pass (green ticks).

### Option 2: Run via CLI
```bash
dotnet test
```

**Expected output:**
```
Passed!  - Failed: 0, Passed: 5, Skipped: 0, Total: 5
```

### Tests Covered
| # | Test Name | Purpose |
| :--- | :--- | :--- |
| 1 | `GetEvents_ReturnsAllEvents` | Verifies all events are returned. |
| 2 | `GetEvent_WithValidId_ReturnsEvent` | Verifies a single event can be retrieved. |
| 3 | `CreateEvent_AsOrganiser_ReturnsCreatedEvent` | Verifies an organiser can create an event. |
| 4 | `UpdateEvent_AsOwner_ReturnsUpdatedEvent` | Verifies the event owner can update their event. |
| 5 | `DeleteEvent_AsOwner_ReturnsNoContent` | Verifies the event owner can delete their event. |

---

## Default Login Credentials

The database is **pre-seeded** with the following accounts. Use these to log in immediately after starting the API.

| Role | Email | Password |
| :--- | :--- | :--- |
| **Organiser** | `thabo@runningclub.co.za` | `password123` |
| **Organiser** | `susan@capetowncycles.co.za` | `password123` |
| **Participant** | `lindiwe@runsa.co.za` | `password123` |
| **Participant** | `michael@cyclezone.co.za` | `password123` |
| **Participant** | `sipho@fitness.co.za` | `password123` |

---

## API Endpoint Overview

The API exposes the following endpoints, grouped by controller.

### Authentication
| Method | Endpoint | Description | Role |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/auth/register` | Register a new participant. | Public |
| `POST` | `/api/auth/login` | Login and receive a JWT token. | Public |

### Admin
| Method | Endpoint | Description | Role |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/admin/users` | List all users. | Any (logged in) |
| `GET` | `/api/admin/users/{id}` | Get a single user. | Any (logged in) |
| `POST` | `/api/admin/organisers` | Create a new organiser (pre-seed). | Public |
| `DELETE` | `/api/admin/users/{id}` | Delete a user. | Any (logged in) |

### Participant
| Method | Endpoint | Description | Role |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/participants/user` | Get own profile. | Participant |
| `PUT` | `/api/participants/user` | Update own profile. | Participant |
| `GET` | `/api/participants/user/enrolments` | Get own enrolments. | Participant |
| `GET` | `/api/participants/user/results` | Get own race results. | Participant |

### Organiser
| Method | Endpoint | Description | Role |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/organisers/user` | Get own profile. | Organiser |
| `PUT` | `/api/organisers/user` | Update own profile. | Organiser |
| `GET` | `/api/organisers/user/events` | List own events. | Organiser |
| `GET` | `/api/organisers/user/events/{eventId}/enrolments` | View enrolments for own event. | Organiser (owner) |

### Events
| Method | Endpoint | Description | Role |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/events` | List all events. | Public |
| `GET` | `/api/events/{id}` | Get event details. | Public |
| `POST` | `/api/events` | Create event. | Organiser |
| `PUT` | `/api/events/{id}` | Update event. | Organiser (owner) |
| `DELETE` | `/api/events/{id}` | Delete event. | Organiser (owner) |
| `GET` | `/api/events/{eventId}/categories` | List categories for an event. | Public |
| `POST` | `/api/events/{eventId}/categories` | Create a category. | Organiser (owner) |

### Categories
| Method | Endpoint | Description | Role |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/categories/{id}` | Get category details. | Public |
| `PUT` | `/api/categories/{id}` | Update category. | Organiser (owner) |
| `DELETE` | `/api/categories/{id}` | Delete category. | Organiser (owner) |
| `GET` | `/api/categories/{id}/route` | Get route info. | Public |
| `PUT` | `/api/categories/{id}/route` | Create or update route. | Organiser (owner) |

### Enrolments
| Method | Endpoint | Description | Role |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/enrolments` | Enrol in a category. | Participant |
| `GET` | `/api/enrolments/{id}` | Get enrolment details. | Owner or Organiser |
| `DELETE` | `/api/enrolments/{id}` | Withdraw from an event. | Participant |

### Results
| Method | Endpoint | Description | Role |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/enrolments/{enrolmentId}/results` | Capture/update a result (UPSERT). | Organiser (owner) |
| `GET` | `/api/events/{eventId}/results` | View public race results. | Public |
| `GET` | `/api/results/user` | View own results. | Participant |
| `GET` | `/api/results/{id}` | Get a single result. | Public |

---

## Project Structure

```
RaceDayManagementAPI/
├── Controllers/
│   ├── AdminController.cs
│   ├── AuthController.cs
│   ├── CategoriesController.cs
│   ├── EnrolmentsController.cs
│   ├── EventsController.cs
│   ├── OrganiserController.cs
│   ├── ParticipantController.cs
│   └── ResultsController.cs
├── Data/
│   └── RaceDayDbContext.cs
├── Models/
│   ├── User.cs
│   ├── Organiser.cs
│   ├── Participant.cs
│   ├── Event.cs
│   ├── Category.cs
│   ├── Route.cs
│   ├── Enrolment.cs
│   ├── Result.cs
│   ├── JwtSettings.cs
│   ├── LoginRequest.cs
│   ├── LoginResponse.cs
│   └── RegisterRequest.cs
├── Services/
│   └── JwtService.cs
├── Program.cs
├── appsettings.json
└── RaceDayManagementAPI.csproj

RaceDayManagementAPI.Tests/
├── EventsControllerTests.cs
└── RaceDayManagementAPI.Tests.csproj
```

---

## Authentication Flow

1. **Login** via `POST /api/auth/login` with email and password.
2. The API validates credentials and returns a **JWT token** containing the user's `userID` and role flags (`isOrganiser`, `isParticipant`).
3. Include the token in every subsequent request:
   ```
   Authorization: Bearer <token>
   ```
4. In Swagger, click the ** Authorize** button and paste `Bearer <token>`.

---

## Version Control

The project follows a structured Git workflow with a minimum of 20 meaningful commits covering:
- Project setup and dependencies
- Database schema and seed data
- Authentication (register, login, JWT)
- User profile management
- Event and category CRUD
- Enrolments
- Results
- Unit tests
- Documentation

---

## YouTube Demo
🔗 [[https://youtu.be/stf0DsykFvs](https://youtu.be/stf0DsykFvs?si=07_xc1kdMxAhtR5d)](https://youtu.be/d3lzgB0bU2k?si=d4nKNp0eMGMnC111)
