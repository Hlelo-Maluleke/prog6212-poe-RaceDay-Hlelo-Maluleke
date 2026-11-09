# RaceDay MVC – Web Front-End

## Overview

**RaceDay MVC** is the server-rendered Razor front-end for the RaceDay event management platform. It gives organisers a dashboard for events, categories, enrolments, results, and banner uploads, and gives participants a catalogue of events, enrolment management, results, and a profile page.

The MVC app is a **front-end/BFF layer**. It does **not** use Entity Framework Core and never connects to SQL Server or Azure directly. Every operation goes through the backend API in [`../RaceDayManagementAPI`](../RaceDayManagementAPI/README.md).

---

## Architecture

```text
┌──────────┐   session cookie   ┌─────────────────────┐   Bearer JWT   ┌──────────────────────┐
│ Browser  │ ─────────────────► │   RaceDayMVC        │ ─────────────► │ RaceDayManagementAPI │
│          │ ◄───────────────── │  Razor + ApiClient  │ ◄───────────── │                      │
└──────────┘                    └─────────────────────┘                └──────────┬───────────┘
                                                                                 │
                                                                    ┌────────────┴────────────┐
                                                                    │                         │
                                                              SQL Server              Azure Blob Storage
                                                                                    (local fallback)
```

### Key design points

- **Server-rendered MVC** – Controllers return Razor views. There is no SPA framework or JavaScript API layer.
- **Session-based token relay** – After login, the JWT returned by the API is stored in the server-side session. The browser only receives a session cookie.
- **Typed `HttpClient`** – `ApiClient` is registered with `AddHttpClient<ApiClient>()` and attaches `Authorization: Bearer <token>` to outgoing requests.
- **Manual role checks** – The app does not use ASP.NET Core cookie authentication or `[Authorize]`. Each controller action checks the session flags (`IsOrganiser` / `IsParticipant`) and redirects to the login page when needed.
- **View models, not entity models** – The `Models` folder contains DTOs/view models that mirror the JSON returned by the API.
- **Bootstrap 5 UI** – Layout, cards, tables, badges, and forms are styled with Bootstrap. A small amount of vanilla JavaScript handles participant event filtering.

> Because the session is backed by `AddDistributedMemoryCache()`, all users are logged out when the MVC process restarts. For multi-instance or production deployments, replace it with a distributed cache such as Redis.

---

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

---

## Request and Authentication Flow

### Login

1. The browser posts the login form to `POST /Account/Login`.
2. `AccountController.Login` calls `POST api/auth/login` through `ApiClient`.
3. On success, the API returns a JWT plus user details.
4. The MVC app stores these session keys:

   | Session key | Value |
   | :--- | :--- |
   | `JwtToken` | The raw JWT |
   | `UserID` | API user ID |
   | `FullName` | Display name |
   | `Email` | Email address |
   | `IsOrganiser` | `"True"` or `"False"` |
   | `IsParticipant` | `"True"` or `"False"` |

5. The user is redirected to `/Organiser` or `/Participant` based on their role.

### Authenticated requests

1. A controller action runs and checks the relevant session flag.
2. The action calls `ApiClient.GetAsync`, `PostAsync`, `PutAsync`, `DeleteAsync`, or `PostMultipartAsync`.
3. `ApiClient` reads `JwtToken` from the session and adds:

   ```http
   Authorization: Bearer <token>
   ```

4. The API validates the token and returns JSON.
5. The controller deserialises the JSON into a view model and returns a Razor view.

### Logout

`GET /Account/Logout` calls `HttpContext.Session.Clear()` and redirects to the login page.

### Role checks

The checks are deliberately simple and live in each controller:

```csharp
private bool IsOrganiser()
    => string.Equals(HttpContext.Session.GetString("IsOrganiser"), "True",
                     StringComparison.OrdinalIgnoreCase);
```

`Helpers/AuthCheck.cs` provides equivalent helpers (`IsLoggedIn`, `IsOrganiser`, `IsParticipant`, `GetUserId`, `GetFullName`, `GetEmail`) if you prefer a shared abstraction. It is not wired into a global filter, so any new controller must perform its own checks.

---

## Features by Area

### Account

| Route | Action | Purpose |
| :--- | :--- | :--- |
| `GET /Account/Login` | `Login` | Show the login form; redirect already-authenticated users to their dashboard. |
| `POST /Account/Login` | `Login` | Authenticate against the API and create the session. |
| `GET /Account/Register` | `Register` | Show the participant registration form. |
| `POST /Account/Register` | `Register` | Create a participant through `POST api/auth/register`. |
| `GET /Account/Logout` | `Logout` | Clear the session. |
| `GET /Account/Error` | `Error` | Production error page. |

### Organiser

| Route | Action | Purpose |
| :--- | :--- | :--- |
| `GET /Organiser` | `Index` | Dashboard with event count, enrolment count, and upcoming events. |
| `GET/POST /Organiser/CreateEvent` | `CreateEvent` | Create an event, then optionally upload its banner. |
| `GET/POST /Organiser/EditEvent/{id}` | `EditEvent` | Load and update an event; optionally replace its banner. |
| `POST /Organiser/DeleteEvent/{id}` | `DeleteEvent` | Delete an owned event. |
| `GET /Organiser/Categories?eventId={id}` | `Categories` | List categories for an event. |
| `POST /Organiser/AddCategory` | `AddCategory` | Add a category to an event. |
| `POST /Organiser/DeleteCategory` | `DeleteCategory` | Delete a category. |
| `GET /Organiser/Enrolments?eventId={id}` | `Enrolments` | View participants enrolled in an event. |
| `GET /Organiser/CaptureResults?eventId={id}` | `CaptureResults` | Show enrolled participants for result capture. |
| `POST /Organiser/CaptureResult` | `CaptureResult` | Upsert a finish time, position, and status. |

### Participant

| Route | Action | Purpose |
| :--- | :--- | :--- |
| `GET /Participant` | `Index` | Event catalogue with search, type, and date filters. |
| `GET /Participant/Details/{id}` | `Details` | Event details, categories, routes, and enrol buttons. |
| `POST /Participant/Enrol` | `Enrol` | Enrol in a category. |
| `POST /Participant/Withdraw` | `Withdraw` | Withdraw an enrolment. |
| `GET /Participant/MyEnrolments` | `MyEnrolments` | List the participant's enrolments and statuses. |
| `GET /Participant/MyResults` | `MyResults` | List the participant's results. |
| `GET/POST /Participant/Profile` | `Profile` | View and update the participant profile. |

The default route is:

```csharp
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");
```

---

## Configuration

`appsettings.json` contains only the API base URL:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ApiSettings": {
    "BaseUrl": "https://localhost:7065/"
  },
  "AllowedHosts": "*"
}
```

`ApiClient` resolves the base URL in this order:

1. `ApiSettings:BaseUrl`
2. `ApiBaseUrl`
3. `https://localhost:7065/`

If the URL does not end with `/`, `ApiClient` appends one.

For containers or other environments, override it with an environment variable:

```text
ApiSettings__BaseUrl=http://host.docker.internal:5120/
```

### Local prerequisites

- .NET 8 SDK
- The RaceDay API running and reachable
- The API's HTTPS development certificate trusted if you use the `https` profile

### Run locally

1. Start the API first (from the API project folder):

   ```bash
   dotnet run --launch-profile https
   ```

   The API listens on `https://localhost:7065` (and `http://localhost:5120`).

2. Start the MVC app (from this folder):

   ```bash
   dotnet run --launch-profile https
   ```

   Default MVC URLs:

   | Profile | URL |
   | :--- | :--- |
   | `https` | `https://localhost:7280` |
   | `http` | `http://localhost:5045` |

3. Open the MVC URL and log in with one of the seeded accounts. The credentials are listed in the [API README](../RaceDayManagementAPI/README.md#default-login-credentials).

---

## Azure Blob Storage Integration

The MVC app never holds Azure credentials and never uploads directly to Azure. It sends the file to the API, and the API decides whether to store it in Azure Blob Storage or in its local fallback folder.

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

### Key files

| File | Role |
| :--- | :--- |
| `Controllers/OrganiserController.cs` | Calls `UploadBanner` after create/update; logs upload failures without failing the event save. |
| `Services/ApiClient.cs` | `PostMultipartAsync` builds the multipart request and attaches the JWT. |
| `Models/EventViewModel.cs` | `BannerUrl` on `EventViewModel` and `EventDetailViewModel`. |
| `Models/OrganiserDashboardViewModel.cs` | `CreateEventViewModel.BannerImage` (`IFormFile`). |
| `Views/Organiser/CreateEvent.cshtml` | File input for a new banner. |
| `Views/Organiser/EditEvent.cshtml` | File input for replacing a banner. |
| `Views/Participant/Index.cshtml` | Banner card image with emoji fallback. |
| `Views/Participant/Details.cshtml` | Banner hero image with emoji fallback. |

### Important behaviour

- **Banner upload is best-effort.** If the event save succeeds but the banner upload fails, the organiser sees an error message, but the event remains saved.
- **The API owns all Azure configuration.** MVC only needs `ApiSettings:BaseUrl`. See [`Azure Blob Storage Integration`](../RaceDayManagementAPI/README.md#azure-blob-storage-integration) in the API README for connection strings, container setup, and the local fallback.
- **Local fallback URLs and containers.** When Azure is not configured, the API returns an absolute URL built from the request's scheme and host. If the API runs in a container, that host may be `host.docker.internal`, which the browser cannot resolve. Use a real Azure connection string, or make sure the API's returned host is reachable from the browser.
- **Validation.** The create form's helper text suggests JPG/PNG up to 5 MB, but the API currently checks only that a non-empty file was supplied. Add server-side size and MIME validation if the endpoint is exposed beyond a trusted environment.
- **Deleting or replacing banners.** The API does not automatically delete the previous blob when a banner is replaced or an event is deleted. Old blobs may need periodic cleanup.

---

## Docker Setup

The MVC project includes a multi-stage `Dockerfile` for Linux containers.

### Dockerfile stages

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["RaceDayMVC.csproj", "./"]
RUN dotnet restore "RaceDayMVC.csproj"

COPY . .
RUN dotnet publish "RaceDayMVC.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 80
ENTRYPOINT ["dotnet", "RaceDayMVC.dll"]
```

What this does:

- Restores packages in a separate layer so rebuilds are faster when only source files change.
- Publishes the app in Release mode without an app host executable.
- Uses the smaller ASP.NET runtime image for the final image.
- Listens on **port 80** and runs in the **Production** environment.
- In Production, unhandled exceptions are routed to `/Account/Error`, and HSTS is enabled.

### Build the image

Run from the **project folder** (the folder containing the `.csproj` and `Dockerfile`):

```powershell
cd "C:\Users\hmalu\Documents\Assignment Project\RaceDayMVC\RaceDayMVC"
docker build -t raceday-mvc .
```

The build context must be this folder because the Dockerfile copies `RaceDayMVC.csproj` from the context root. The included `.dockerignore` excludes `bin`, `obj`, `.vs`, `.git`, `node_modules`, and other build artefacts.

### Run the container

If the API is running on your host at `http://localhost:5120`:

```powershell
docker run -d --name raceday-mvc -p 8080:80 -e ApiSettings__BaseUrl=http://host.docker.internal:5120/ raceday-mvc
```

Then open:

```text
http://localhost:8080
```

`host.docker.internal` resolves to the host machine on Docker Desktop for Windows and macOS. On Linux, add the host-gateway mapping to the run command:

```powershell
docker run -d --name raceday-mvc -p 8080:80 `
  --add-host=host.docker.internal:host-gateway `
  -e ApiSettings__BaseUrl=http://host.docker.internal:5120/ `
  raceday-mvc
```

### Make the API reachable from the container

The API must listen on an interface the container can reach, not only on `localhost`. From the API project folder, run:

```bash
dotnet run --urls "http://0.0.0.0:5120"
```

The API's `UseHttpsRedirection()` middleware logs a warning when no HTTPS port is configured, but the HTTP endpoint remains usable for local container-to-host testing.

If you prefer to use the API's HTTPS profile, set:

```text
ApiSettings__BaseUrl=https://host.docker.internal:7065/
```

The API must listen on all interfaces and have its development certificate available. The MVC `HttpClient` is configured with `DangerousAcceptAnyServerCertificateValidator` for local development, so it will accept the self-signed certificate.

### If the API is also containerised

There is currently **no Dockerfile for the API** and **no `docker-compose.yml`** in this repository. If you add an API image (for example, `raceday-api`), put both containers on a user-defined network and use the service name as the host:

```powershell
docker network create raceday-net

docker run -d --name raceday-api --network raceday-net -p 5120:80 raceday-api

docker run -d --name raceday-mvc --network raceday-net -p 8080:80 `
  -e ApiSettings__BaseUrl=http://raceday-api:80/ `
  raceday-mvc
```

Inside a user-defined Docker network, containers resolve each other by container name. `host.docker.internal` is only needed when the API runs outside Docker.

### Docker notes and limitations

- The container serves **HTTP only**. Terminate TLS at a reverse proxy (for example, nginx, Caddy, or Azure App Service) in production.
- `ASPNETCORE_ENVIRONMENT=Production` means the developer exception page is not used. Check the container logs for errors.
- Session state is in-memory. Restarting the MVC container logs everyone out and loses all sessions.
- The container needs network access to the API. If login fails immediately, check the API base URL and container networking first.
- The API's local banner fallback writes into the **API** container's filesystem. If the API is containerised and you rely on the fallback, mount a persistent volume or use Azure Blob Storage.

---

## Troubleshooting

| Symptom | Likely cause | Fix |
| :--- | :--- | :--- |
| Login always fails | API not running or `ApiSettings:BaseUrl` wrong | Start the API and confirm the URL, including the trailing slash. |
| `SSL connection could not be established` | API HTTPS certificate not trusted by the MVC process | The dev `HttpClient` accepts any certificate; if the error persists, check that the API is listening on the expected port. |
| Browser shows a certificate warning | API or MVC HTTPS dev certificate is not trusted | Run `dotnet dev-certs https --trust`, or use the HTTP profiles. |
| Redirected to login on every action | Session cookie not being sent or session lost | Check cookies, HTTPS/HTTP mismatch, and whether the app restarted. |
| Banner upload fails but event saves | Azure not configured or upload error | Check API logs; the API falls back to local storage when Azure is unavailable. |
| Banner image broken in Docker | API returned a `host.docker.internal` local fallback URL | Use Azure Blob Storage or expose the API on a browser-reachable host. |
| Container cannot reach the API | API bound to `localhost` only, or wrong host name | Bind the API to `0.0.0.0` and use `host.docker.internal` (or the Docker network service name). |
| `docker build` cannot find the csproj | Build context is not the project folder | Run `docker build` from the folder containing `RaceDayMVC.csproj`. |

---

## Security Notes

- The session cookie is `HttpOnly` and `IsEssential`, with a 60-minute idle timeout.
- The JWT is stored server-side in the session, not in browser-accessible JavaScript storage.
- `DangerousAcceptAnyServerCertificateValidator` is enabled in `Program.cs` for local development and container self-signed certificates. **Remove it or restrict it in production.**
- There is no ASP.NET Core cookie authentication middleware. Authorisation relies on manual session checks in each controller. Review any new controller action carefully.
- The MVC app should not be treated as the security boundary. The API independently validates the JWT, ownership, capacity, duplicate enrolments, and result-capture permissions.
- Do not put Azure connection strings or JWT secrets in the MVC `appsettings.json`. They belong to the API.

---

## Related Documentation

- [RaceDay API README](../RaceDayManagementAPI/README.md) – database schema, API endpoints, JWT authentication, Azure Blob implementation, and backend tests.
- [RaceDay demo video](https://youtu.be/d3lzgB0bU2k?si=d4nKNp0eMGMnC111)
