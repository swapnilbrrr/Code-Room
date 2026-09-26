# Code-Room

<p align="center">
  <strong>A practical technology learning platform built with ASP.NET Core MVC.</strong>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white" alt=".NET 8" />
  <img src="https://img.shields.io/badge/ASP.NET%20Core-MVC-512BD4?logo=dotnet&logoColor=white" alt="ASP.NET Core MVC" />
  <img src="https://img.shields.io/badge/Entity%20Framework%20Core-8.0-512BD4?logo=dotnet&logoColor=white" alt="Entity Framework Core" />
  <img src="https://img.shields.io/badge/MySQL-8-4479A1?logo=mysql&logoColor=white" alt="MySQL" />
  <img src="https://img.shields.io/badge/JavaScript-ES6%2B-F7DF1E?logo=javascript&logoColor=black" alt="JavaScript" />
</p>

## Overview

Code-Room is a web-based technology learning platform for students. The project demonstrates ASP.NET Core MVC, MySQL database connectivity, authenticated student activities, administrator content management, form validation and responsive frontend development.

The learning workflow connects courses, lessons, resources, quizzes, enrolment and progress tracking. An administrative area provides controlled CRUD operations for major content modules.

## Features

### Student experience
- Course browsing, search and filtering
- Structured lessons
- Supporting resources and multimedia links
- Course enrolment
- Lesson completion and progress tracking
- Quizzes and scored attempts
- Student dashboard
- Announcements

### Administration
- Course CRUD
- Lesson CRUD
- Quiz and question CRUD
- Resource CRUD
- Announcement CRUD
- User management with role protection
- Dashboard statistics

### Quality and security
- Responsive interface
- Client-side and server-side validation
- PBKDF2 password hashing
- Cookie-based authentication
- Role-based authorization
- Anti-forgery protection on state-changing forms
- Dark/light theme support
- Safe local database configuration without committed credentials

## Project status

> **Academic project:** Code-Room is structured as a complete database-driven learning platform for development, demonstration, and academic evaluation.

## Technology Stack

| Layer | Technology |
| --- | --- |
| Application | ASP.NET Core MVC |
| Language | C# |
| Runtime | .NET 8 |
| Database | MySQL |
| ORM | Entity Framework Core + Pomelo |
| Frontend | Razor, HTML5, CSS3, JavaScript |
| Development | Visual Studio / .NET CLI |
| Optional environment | Dev Container |

## Project Structure

```text
Code-Room/
├── .devcontainer/
├── CodeRoom.Web/
│   ├── Areas/Admin/
│   ├── Controllers/
│   ├── Data/
│   ├── Models/
│   ├── Services/
│   ├── ViewModels/
│   ├── Views/
│   ├── wwwroot/
│   ├── Properties/
│   │   └── launchSettings.json
│   ├── appsettings.json
│   ├── appsettings.Development.json.example
│   ├── CodeRoom.Web.csproj
│   └── Program.cs
├── database/
│   ├── schema/
│   ├── seed/
│   └── setup.sql
├── CodeRoom.sln
├── global.json
└── README.md
```

## Run Locally

### 1. Prerequisites

Install:
- .NET 8 SDK
- MySQL Server 8.x
- Visual Studio with ASP.NET and web development tools (or the .NET CLI)

Verify the SDK:

```powershell
dotnet --version
```

The repository pins the .NET 8 SDK family through `global.json`.

### 2. Create the Code-Room database

Start MySQL, then run:

```powershell
Get-Content -Raw .\database\setup.sql | mysql -u root -p
```

Alternatively, open `database/setup.sql` in MySQL Workbench and execute it.

The setup script creates the empty `coderoom` database. The ASP.NET Core application creates the tables and baseline seed data on first startup.

### 3. Configure the connection string safely

Do **not** put your real MySQL password into a tracked file.

The project includes local user-secrets support. From the repository root:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "server=localhost;port=3306;database=coderoom;user=root;password=YOUR_PASSWORD;TreatTinyAsBoolean=true" --project ./CodeRoom.Web
```

The checked-in `appsettings.Development.json.example` shows the expected connection-string shape.

### 4. Restore and build

```powershell
dotnet restore
dotnet build
```

### 5. Run

```powershell
dotnet run --project ./CodeRoom.Web
```

The included development launch profile uses:

- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`

If HTTPS is not trusted yet:

```powershell
dotnet dev-certs https --trust
```

### 6. Demo accounts

The first successful startup seeds these fictional demonstration accounts:

| Role | Name | Email |
| --- | --- | --- |
| Super Administrator | Swapnil Katuwal | `swapnil.katuwal@coderoom.com` |
| Administrator | Chandra Bhatta | `chandra.bhatta@coderoom.com` |
| Student | Bijay Khadka | `bijay.khadka@coderoom.com` |
| Student | Babin Aryal | `babin.aryal@coderoom.com` |
| Student | Anisha Gurung | `anisha.gurung@coderoom.com` |
| Student | Nischal Bhandari | `nischal.bhandari@coderoom.com` |
| Student | Suman Adhikari | `suman.adhikari@coderoom.com` |
| Student | Prerana Rai | `prerana.rai@coderoom.com` |

The demo accounts are fictional accounts for local evaluation. Their passwords are defined in the seed process and should never be reused for real services.

## Database Setup Model

Code-Room is designed so the submitted project can be recreated on another machine without needing the developer's personal MySQL server:

```text
Submitted ZIP
   |
   +--> ASP.NET Core application
   +--> EF Core model configuration
   +--> database/setup.sql
   +--> seed logic
   +--> README setup instructions
                    |
                    v
             Marker/local MySQL
                    |
                    v
          Code-Room creates schema
          and seeds demo content
```

The application uses EF Core's `EnsureCreated` during startup for this assignment so a fresh database can be prepared without requiring a pre-existing schema or migration history.

## Development & Code Quality

- Keep controllers focused on request handling.
- Keep persistence concerns in the data layer.
- Prefer clear names over clever code.
- Comment intent or non-obvious decisions, not obvious syntax.
- Keep credentials and secrets out of source control.
- Test meaningful behaviour before marking a feature complete.
- Keep generated build output out of the repository.

## Contributors

| Member | Contribution |
| --- | --- |
| Swapnil Katuwal | Programming and web application implementation |
| Member | To be recorded from confirmed group contribution |
| Member | To be recorded from confirmed group contribution |
| Member | To be recorded from confirmed group contribution |

Only confirmed contributions should be recorded.


This project is intended for educational and portfolio use.


## Repository

**GitHub:** https://github.com/swapnilbrrr/Code-Room

---

<p align="center">
  <strong>Code-Room</strong><br>
  Learn. Practise. Track. Progress.
</p>
