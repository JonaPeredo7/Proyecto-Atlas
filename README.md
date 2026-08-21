# Atlas

Atlas is a work-in-progress personal health, training, and recovery platform. It combines daily context, exercise planning, symptom tracking, measurable goals, longitudinal analysis, and shareable professional reports in one privacy-conscious application.

> **Project status:** active development. The current repository contains a functional full-stack foundation and automated domain tests, but it is not intended for production medical use.

## Highlights

- Secure personal accounts with ASP.NET Core Identity.
- Daily check-ins for sleep, energy, fatigue, stress, workload, and symptoms.
- Training planning, session execution, RPE, volume, and internal-load tracking.
- Personal goals, measurement protocols, progress, and plan versioning.
- Weekly and longitudinal summaries with transparent data coverage.
- Knee and post-session follow-up without automated diagnosis or injury prediction.
- Evidence references with source, applicability, and limitations.
- Professional reports with frozen snapshots, expiring private links, and feedback review.
- Offline-capable PWA flows with private per-account queues and idempotent retries.
- Portable JSON export, SHA-256 integrity metadata, and safe restoration analysis.
- Responsive Vue interface designed for desktop and mobile use.

## Technology stack

- **Backend:** C# 14, .NET 10, ASP.NET Core Web API
- **Architecture:** Domain, Application, Infrastructure, and API layers
- **Persistence:** Entity Framework Core 10 and SQL Server
- **Authentication:** ASP.NET Core Identity with cookie-based sessions
- **Frontend:** Vue 3, TypeScript, Pinia, Vue Router, and Vite
- **Testing:** xUnit domain and compatibility tests
- **Delivery:** Progressive Web App (PWA)

## Repository structure

```text
src/
  Gimnasio.Api/             HTTP API and authentication endpoints
  Gimnasio.Application/     Use-case contracts
  Gimnasio.Domain/          Entities and domain calculations
  Gimnasio.Infrastructure/  Persistence and service implementations
tests/Gimnasio.Tests/       Automated tests
frontend/gimnasio-web/      Vue and TypeScript client
docs/                       Architecture and evidence documentation
```

The internal `Gimnasio` names are legacy technical identifiers retained temporarily to avoid a risky all-at-once migration. The former gym-management modules have been removed and are not part of Atlas.

## Run locally

### Requirements

- .NET 10 SDK
- SQL Server LocalDB or a compatible SQL Server instance
- Node.js and npm

### Backend

```powershell
dotnet tool restore
dotnet tool run dotnet-ef database update --project src/Gimnasio.Infrastructure --startup-project src/Gimnasio.Api
dotnet run --project src/Gimnasio.Api
```

### Frontend

```powershell
cd frontend/gimnasio-web
npm install
npm run dev
```

On Windows, `IniciarAplicacion.cmd` starts the API and web client for local development.

## Verification

```powershell
dotnet test ProyectoGimnasioV2.sln
cd frontend/gimnasio-web
npm install
npm run build
```

## Privacy and safety

Atlas is designed around personal control and explicit consent. It keeps observations separate from interpretations, exposes missing-data coverage, and avoids automated medical diagnosis or injury prediction. Development and demos use local or synthetic data; private databases, backups, logs, and credentials are excluded from this repository.

## Roadmap

- Complete the internal rename from `Gimnasio` to `Atlas`.
- Expand integration and end-to-end coverage.
- Add production-ready configuration and deployment documentation.
- Improve accessibility and internationalization.
- Continue refining evidence traceability and professional collaboration flows.

## Author

**Jonathan Peredo**  
[GitHub](https://github.com/JonaPeredo7) · [LinkedIn](https://www.linkedin.com/in/jonathan-peredo-desarrollador-net/)
