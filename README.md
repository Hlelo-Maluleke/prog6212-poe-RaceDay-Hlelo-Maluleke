# RaceDay – South African Event Management Platform

RaceDay is a two-project ASP.NET Core 8 solution for managing road running, walking, and cycling events in South Africa.

| Project | Type | Responsibility |
| :--- | :--- | :--- |
| [`RaceDayManagementAPI`](RaceDayManagementAPI/README.md) | ASP.NET Core Web API | REST endpoints, EF Core + SQL Server, JWT authentication, Azure Blob banner storage, Swagger, xUnit tests. |
| [`RaceDayMVC`](RaceDayMVC/README.md) | ASP.NET Core MVC | Razor front-end, organiser and participant portals, session-based JWT relay, Dockerfile. |

## Documentation

- **[API README](RaceDayManagementAPI/README.md)** – setup, configuration, database schema, endpoints, JWT flow, Azure Blob integration, and tests.
- **[MVC README](RaceDayMVC/README.md)** – MVC structure, request/authentication flow, Azure Blob upload flow from the UI, and Docker setup.

# RaceDay MVC – Web Front-End

## Overview

**RaceDay MVC** is the server-rendered Razor front-end for the RaceDay event management platform. It gives organisers a dashboard for events, categories, enrolments, results, and banner uploads, and gives participants a catalogue of events, enrolment management, results, and a profile page.

The MVC app is a **front-end/BFF layer**. It does **not** use Entity Framework Core and never connects to SQL Server or Azure directly. Every operation goes through the backend API.

## Project Structure

```text
RaceDayMVC/
├── RaceDayMVC.csproj
├── Program.cs                     # DI, session, HttpClient, middleware, default route
├── appsettings.json               # ApiSettings:BaseUrl
├── appsettings.Development.json
├── Dockerfile                     # Multi-stage Linux container build
├── .dockerignore
├── Properties/
│   └── launchSettings.json
├── Controllers/
│   ├── AccountController.cs       # Login, register, logout, error
│   ├── OrganiserController.cs     # Dashboard, events, categories, enrolments, results, banners
│   └── ParticipantController.cs   # Browse, details, enrol, withdraw, results, profile
├── Models/
│   ├── LoginViewModel.cs          # Login/Register view models + AuthResponse
│   ├── EventViewModel.cs          # Event, category, route, enrolment, result, profile DTOs
│   ├── OrganiserDashboardViewModel.cs  # Dashboard row + CreateEventViewModel
│   └── ErrorViewModel.cs
├── Services/
│   └── ApiClient.cs               # Typed HttpClient wrapper with JWT + multipart support
├── Helpers/
│   └── AuthCheck.cs               # Session helper methods (available for reuse)
├── Views/
│   ├── _ViewImports.cshtml
│   ├── _ViewStart.cshtml
│   ├── Account/
│   │   ├── Login.cshtml
│   │   ├── Register.cshtml
│   │   └── Error.cshtml
│   ├── Organiser/
│   │   ├── Index.cshtml           # Dashboard
│   │   ├── CreateEvent.cshtml
│   │   ├── EditEvent.cshtml
│   │   ├── Categories.cshtml
│   │   ├── Enrolments.cshtml
│   │   └── CaptureResults.cshtml
│   ├── Participant/
│   │   ├── Index.cshtml           # Event catalogue + client-side filters
│   │   ├── Details.cshtml
│   │   ├── MyEnrolments.cshtml
│   │   ├── MyResults.cshtml
│   │   └── Profile.cshtml
│   └── Shared/
│       ├── _Layout.cshtml
│       ├── _Layout.cshtml.css
│       ├── _ValidationScriptsPartial.cshtml
│       └── Error.cshtml
└── wwwroot/
    ├── css/site.css
    ├── js/site.js
    ├── favicon.ico
    └── lib/                       # Bootstrap, jQuery, jQuery Validation
```

### End-to-end upload flow

1. An organiser creates or edits an event. Both forms use `enctype="multipart/form-data"`.
2. `OrganiserController` first saves the event through the API:
   - `POST api/events` for a new event
   - `PUT api/events/{id}` for an existing event
3. If a banner file was selected, the controller calls its private `UploadBanner` helper:

   ```csharp
   var response = await _api.PostMultipartAsync(
       $"api/events/{eventId}/banner", file, "file");
   ```

4. `ApiClient.PostMultipartAsync`:
   - Opens the uploaded file stream.
   - Creates a `MultipartFormDataContent` with the field name `file`.
   - Sets the part's content type from `IFormFile.ContentType`.
   - Attaches the JWT from the session.
   - Sends the request to the API.
5. The API validates that the caller owns the event and calls `BlobService.UploadAsync`.
6. `BlobService` uploads to the configured Azure container and returns the blob URL, or falls back to `wwwroot/uploads/banners` and returns a local URL.
7. The API saves the URL in `Events.BannerUrl` and returns `{ "bannerUrl": "..." }`.
8. The MVC app reads `BannerUrl` from the event JSON into `EventViewModel.BannerUrl`.
9. Razor views render the banner with a normal `<img>` tag:

   ```html
   <img src="@ev.BannerUrl" class="card-img-top" alt="@ev.Name banner" />
   ```

   This appears in the participant event catalogue and event details page, and in the organiser edit form as the current banner preview.

## Quick Start

### 1. Start the API

```bash
cd RaceDayManagementAPI
dotnet run --launch-profile https
```

- API: `https://localhost:7065`
- Swagger: `https://localhost:7065/swagger`

### 2. Start the MVC front-end

In a second terminal:

```bash
cd RaceDayMVC/RaceDayMVC
dotnet run --launch-profile https
```

- MVC: `https://localhost:7280`

### 3. Log in

All seeded accounts use the password **`password123`**.

| Role | Email |
| :--- | :--- |
| Organiser | `micheal@runningclub.co.za` |
| Organiser | `susan@capetowncycles.co.za` |
| Participant | `lindiwe@runsa.co.za` |
| Participant | `michael@cyclezone.co.za` |
| Participant | `sipho@fitness.co.za` |

## Docker

The MVC project includes a multi-stage Dockerfile. Build it from the project folder:

```bash
cd RaceDayMVC/RaceDayMVC
docker build -t raceday-mvc .
docker run -d --name raceday-mvc -p 8080:80 -e ApiSettings__BaseUrl=http://host.docker.internal:5120/ raceday-mvc
```

See the [MVC Docker section](RaceDayMVC/README.md#docker-setup) for networking, HTTPS, and container-to-API details. The API is not containerised in this repository.
---

## YouTube Demo
🔗 [(https://youtu.be/stf0DsykFvs) (https://youtu.be/stf0DsykFvs?si=07_xc1kdMxAhtR5d) (https://youtu.be/d3lzgB0bU2k?si=d4nKNp0eMGMnC111) (https://youtu.be/5rSWYvhIh4c?si=zRI2YEJ5V1azQZ0T)]
