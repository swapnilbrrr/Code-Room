# Code-Room

<p align="center">
  <strong>A practical technology learning platform built with ASP.NET Core MVC.</strong>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white" alt=".NET 8" />
  <img src="https://img.shields.io/badge/ASP.NET%20Core-MVC-512BD4?logo=dotnet&logoColor=white" alt="ASP.NET Core MVC" />
  <img src="https://img.shields.io/badge/Entity%20Framework%20Core-8.0-512BD4?logo=dotnet&logoColor=white" alt="Entity Framework Core" />
  <img src="https://img.shields.io/badge/MySQL-8-4479A1?logo=mysql&logoColor=white" alt="MySQL" />
  <img src="https://img.shields.io/badge/Razor-Views-512BD4" alt="Razor Views" />
  <img src="https://img.shields.io/badge/JavaScript-ES6%2B-F7DF1E?logo=javascript&logoColor=black" alt="JavaScript" />
  <img src="https://img.shields.io/badge/Tests-xUnit-512BD4" alt="xUnit" />
  <img src="https://img.shields.io/github/actions/workflow/status/swapnilbrrr/Code-Room/dotnet.yml?branch=main&label=CI&logo=github" alt="CI status" />
</p>

<p align="center">
  <a href="#features">Features</a> •
  <a href="#technology-stack">Technology</a> •
  <a href="#project-structure">Structure</a> •
  <a href="#run-locally">Run locally</a> •
  <a href="#development-workflow">Workflow</a>
</p>

## Overview

Code-Room is a web-based technology learning platform for students. It is being developed as a university web-application project and as a portfolio project, with an emphasis on clear MVC structure, maintainable code, database-backed learning workflows and practical usability.

The planned learning experience connects courses, lessons, resources, quizzes and progress tracking. An administrative area will provide controlled content management.

## Features

### Student experience
- Course browsing and discovery
- Structured lessons
- Supporting resources and multimedia
- Course enrollment
- Lesson completion and progress tracking
- Quizzes and scored attempts
- Student dashboard
- Announcements

### Administration
- Course management
- Lesson management
- Quiz and question management
- Resource management
- Announcements
- Dashboard statistics

### Quality and usability
- Responsive interface
- Client-side and server-side validation
- Secure password handling
- Role-based authorization
- Automated tests
- GitHub Actions CI
- Search and course filtering
- Progress indicators and quiz history
- Dark/light theme support planned within the controlled feature scope

> **Status:** Active development. Items are treated as complete only after implementation and verification. See [PROJECT-CHECKLIST.md](PROJECT-CHECKLIST.md).

## Technology Stack

| Layer | Technology |
| --- | --- |
| Application | ASP.NET Core MVC |
| Language | C# |
| Runtime | .NET 8 |
| Database | MySQL |
| ORM | Entity Framework Core + Pomelo |
| Frontend | Razor, HTML5, CSS3, JavaScript |
| Testing | xUnit |
| CI | GitHub Actions |
| Development | GitHub Codespaces / Dev Container |

## Project Structure

```text
Code-Room/
├── .devcontainer/              # Reproducible Codespaces environment
├── .github/workflows/          # CI build and test workflow
├── CodeRoom.Web/
│   ├── Areas/Admin/            # Administrative area
│   ├── Controllers/            # MVC request handling
│   ├── Data/                   # EF Core database context
│   ├── Models/                 # Domain entities
│   ├── Views/                  # Razor UI
│   └── wwwroot/                # CSS and JavaScript
├── tests/CodeRoom.Web.Tests/   # Automated tests
├── PROJECT-CHECKLIST.md        # Definition of done and project control
├── CodeRoom.sln
├── global.json                 # .NET SDK version policy
└── README.md
```

## Run Locally

### 1. Open the repository

Open the repository in GitHub Codespaces or clone it locally with the .NET 8 SDK installed.

The included Dev Container config provides the expected development environment. The repository currently targets the .NET 8 SDK installed by the container image.

### 2. Verify the SDK

```bash
dotnet --version
```

Expected major version:

```text
8.0.x
```

### 3. Restore dependencies

```bash
dotnet restore
```

### 4. Build

```bash
dotnet build
```

### 5. Run

```bash
dotnet run --project CodeRoom.Web
```

In Codespaces, open the forwarded application port shown in the Ports panel.

### Codespaces troubleshooting

The Dev Container explicitly configures `TMPDIR`, `TMP` and `TEMP` to use `/tmp`. If the container was created before this configuration was added, use **Codespaces → Rebuild Container** after pulling the latest changes.

## Database Configuration

Database persistence is being implemented incrementally.

Do not commit database credentials. The tracked `appsettings.json` intentionally contains an empty connection string. During database development, provide `DefaultConnection` through user secrets or environment-specific configuration.

Example connection string shape:

```text
server=localhost;port=3306;database=coderoom;user=coderoom;password=YOUR_PASSWORD
```

Migrations, seed data and clean database setup are tracked in [PROJECT-CHECKLIST.md](PROJECT-CHECKLIST.md).

## Development Workflow

Implementation follows controlled milestones:

1. Foundation and development environment
2. Database and EF Core persistence
3. Authentication and authorization
4. Core learning workflows
5. Administration
6. Selected additional features
7. Testing, security review and UI polish

The repository is reviewed before moving between milestones so incomplete demo UI is not mistaken for finished functionality.

## Code Quality

- Keep controllers focused on request handling.
- Keep persistence concerns in the data layer.
- Prefer clear names over clever code.
- Comment intent or non-obvious decisions, not obvious syntax.
- Keep credentials and secrets out of source control.
- Test meaningful behaviour before marking a feature complete.
- Keep the repository free from generated build output and unused template files.

## Contributors

| Member | Contribution |
| --- | --- |
| Swapnil Katuwal | Programming and web application implementation |
| Om Kyapchhaki Magar | To be recorded from confirmed group contribution |
| Bihason Ben Luitel | To be recorded from confirmed group contribution |
| Ganesh Chaudhary | To be recorded from confirmed group contribution |

Only confirmed contributions should be added here; the table is intentionally not inventing responsibilities.

## Project Control

[PROJECT-CHECKLIST.md](PROJECT-CHECKLIST.md) contains the definition of done, implementation phases and final submission checks.

## Repository Topics / Tags

`aspnet-core` · `csharp` · `dotnet` · `mvc` · `entity-framework-core` · `mysql` · `razor` · `web-application` · `e-learning` · `education` · `xunit` · `github-actions` · `github-codespaces`

## License

This project is intended for educational and portfolio use.
