# Code-Room

A web-based technology learning platform built with ASP.NET Core MVC and MySQL.

> **Project status:** Active development — frontend application shell is being built first, followed by persistence, authentication, learning workflows, administration and testing.

## Overview

Code-Room is designed to give students a structured place to learn technology through courses, lessons, supporting resources, quizzes and progress tracking.

The project is both a university web-application project and a portfolio project. The codebase therefore follows a clean MVC structure, keeps configuration separate from application logic, uses source control and CI, and aims to remain understandable to another developer reading the repository.

## Core Features

### Student experience
- Course browsing and discovery
- Structured lessons
- Multimedia/resource support
- Course enrollment
- Lesson completion and progress tracking
- Quizzes and scored attempts
- Student dashboard
- Announcements

### Administration
- Course management
- Lesson management
- Quiz/question management
- Resource management
- Announcements
- Dashboard statistics

### Planned quality features
- Search and filtering
- Responsive/mobile interface
- Validation
- Role-based access control
- Secure password handling
- Automated tests and GitHub Actions

## Technology Stack

| Layer | Technology |
| --- | --- |
| Application | ASP.NET Core MVC |
| Language | C# |
| Runtime | .NET 8 |
| Database | MySQL |
| ORM | Entity Framework Core |
| Frontend | Razor, HTML5, CSS3, JavaScript |
| Testing | xUnit |
| CI | GitHub Actions |
| Development | GitHub Codespaces / Dev Container |

## Project Structure

```text
Code-Room/
├── .devcontainer/              # Codespaces development environment
├── .github/workflows/          # CI build and test workflow
├── CodeRoom.Web/
│   ├── Areas/Admin/            # Administrative features
│   ├── Controllers/            # MVC request handling
│   ├── Data/                   # EF Core database context
│   ├── Models/                 # Domain entities
│   ├── Views/                  # Razor UI
│   └── wwwroot/                # CSS and JavaScript
├── tests/CodeRoom.Web.Tests/   # Automated tests
├── PROJECT-CHECKLIST.md        # Development and submission checklist
├── CodeRoom.sln
└── README.md
```

## Run Locally

### 1. Open the repository

Open the repository in GitHub Codespaces or clone it locally with the .NET 8 SDK installed.

### 2. Restore dependencies

```bash
dotnet restore
```

### 3. Build

```bash
dotnet build
```

### 4. Run

```bash
dotnet run --project CodeRoom.Web
```

In Codespaces, open the forwarded application port shown by the terminal/Ports panel.

## Database Setup

The application is being developed in stages. MySQL migrations, seed data and production-safe configuration will be added before the first complete release.

Development database credentials must not be committed to the repository. Use environment variables, user secrets or another local configuration mechanism for real credentials.

## Development Workflow

We build the application in controlled milestones:

1. Frontend and navigation
2. Database and EF Core persistence
3. Authentication and authorization
4. Core learning workflows
5. Administration
6. Selected additional features
7. Testing, security review and UI polish

See [PROJECT-CHECKLIST.md](PROJECT-CHECKLIST.md) for the current definition of done and implementation status.

## Code Quality

The project follows a few simple rules:
- Keep controllers focused on request handling.
- Keep persistence concerns in the data layer.
- Prefer clear names over clever code.
- Add comments when they explain intent or a non-obvious decision; avoid comments that simply restate the code.
- Keep secrets out of source control.
- Test important application behaviour before marking a feature complete.

## Project Status

The repository is intentionally developed incrementally. Features are only marked complete after implementation and verification rather than being listed as complete because they are planned.

## Contributors

Group member responsibilities will be recorded here once the final contribution split is confirmed. Responsibilities should reflect actual work completed in the repository.

## License

This project is currently intended for educational and portfolio use.
