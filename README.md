# RaceDay – South African Event Management System

## Brief Description

**RaceDay** is a full-stack web-based event management platform designed specifically for the **South African road running, walking, and cycling community**. The system streamlines the entire event lifecycle – from event creation and participant registration to capturing results and publishing race outcomes.

### Key Features
- **Event Management** – Organisers can create, update, and delete events with multiple categories (distances, age groups, entry fees).
- **Participant Registration** – Participants can browse upcoming events, enrol in categories, and track their personal performance history.
- **Route & Weather Information** – Participants can access route maps, GPX data, and elevation details to prepare for race day.
- **Results Management** – Organisers can capture finish times and positions; participants can view their own results and public race outcomes.
- **Role-Based Access** – Separate login flows and authorisation for Organisers and Participants.

---

## System Roles

RaceDay supports two distinct user roles, each with specific permissions and responsibilities.

### 1. Organiser

Organisers are **event managers** who create, manage, and oversee events. Organiser accounts are **pre-seeded** by the system administrator and cannot be self-registered.

**Permissions:**
- Create, update, and delete events
- Define event categories (distances, entry fees, age restrictions)
- Upload route information (map URLs, GPX data, elevation profiles)
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
- Withdraw from an event (before cut-off)
- View their personal race results and performance history
- Access route and elevation information to prepare for race day

**Example Use Case:**  
*Lindiwe Nkosi browses the RaceDay platform, finds the Soweto Marathon, enrols in the 21km Half Marathon, and later views her finish time of 1 hour 45 minutes (position 42).*

---

## Technology Stack

| Layer | Technology |
| :--- | :--- |
| **Database** | Microsoft SQL Server (T-SQL) |
| **Backend** | Node.js / Express.js (REST API) |
| **Authentication** | JWT (JSON Web Tokens) |
| **Frontend** | React (or any modern frontend framework) |
| **Version Control** | Git / GitHub |

---

## Database Schema

The database consists of **8 tables** in a fully normalised structure:

| Table | Description |
| :--- | :--- |
| `users` | Base entity – stores login credentials and personal details for all users. |
| `organisers` | Subtype of `users` – pre-seeded event managers. |
| `participants` | Subtype of `users` – self-registered event entrants. |
| `events` | Stores event details (name, date, location, type, status). |
| `categories` | Event categories (distances, entry fees, capacity limits). |
| `routes` | Route information (map URLs, elevation data) – 1-to-1 with `categories`. |
| `enrolments` | Participant entries into specific categories. |
| `results` | Race results (finish times, positions) – 1-to-1 with `enrolments`. |

<img width="647" height="117" alt="image" src="https://github.com/user-attachments/assets/e9341bc4-ce09-4297-a1ee-4481eb9821e5" />
