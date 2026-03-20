# Bupa Appointments

A full-stack medical appointments management system. The backend enforces business rules, the frontend provides a filtered, paginated view with the ability to cancel, reschedule, and edit notes.

---

## Tech Stack

| Layer | Technology |
|---|---|
| API | .NET 10, C#, ASP.NET Core Web API |
| Frontend | Next.js 16 (App Router), TypeScript, Tailwind CSS v4 |
| Icons | FontAwesome |
| Data | JSON file (in-memory singleton, written back on change) |
| Tests | xUnit, Moq, WebApplicationFactory |

---

## Prerequisites

**1. .NET 10 SDK**

Check if you have it:
```bash
dotnet --version
```
If the output is not `10.x.x`, download and install it from [https://dotnet.microsoft.com/download](https://dotnet.microsoft.com/download). Select **.NET 10 SDK** for your OS.

**2. Node.js (v18+)**

Check if you have it:
```bash
node --version
```
If it's below v18 or not installed, download it from [https://nodejs.org](https://nodejs.org). The **LTS** version is recommended. npm is included with Node.js — no separate install needed.

---

## Running the Application

Both need to be running at the same time.

**API** (port 5122):
```bash
cd src/Bupa.Appointments.Api
dotnet run
```

**Frontend** (port 3000):
```bash
cd src/bupa-appointments-web
npm install
npm run dev
```

Then open `http://localhost:3000`.

> The frontend calls the API at `http://localhost:5122` (hardcoded in `lib/api.ts`). The API only accepts requests from `http://localhost:3000` via CORS. If you change either port, update both `launchSettings.json` / `Program.cs` and `lib/api.ts`.

**Tests:**
```bash
cd src/Bupa.Appointments.Tests
dotnet test
```

---

## Folder Structure

```
src/
  Bupa.Appointments.Api/
    Controllers/
      AppointmentsController.cs   - 5 endpoints
    Core/
      Interfaces/
        IAppointmentService.cs    - service contract
      Models/
        Appointment.cs            - data models
        Request.cs                - request/response types
        Result.cs                 - IsSuccess/Error wrapper
    Services/
      AppointmentService.cs       - business logic + data access
    Data/
      appointments.json           - source data
    Program.cs                    - DI, CORS config

  Bupa.Appointments.Tests/
    AppointmentServiceTests.cs    - unit tests (business rules)
    AppointmentsApiTests.cs       - integration tests (HTTP layer)

  bupa-appointments-web/
    app/
      page.tsx                    - main dashboard
      layout.tsx                  - root layout
      globals.css                 - base styles
    components/
      Header.tsx                  - nav bar
      FilterBar.tsx               - filters + date range
      AppointmentCard.tsx         - list card
      AppointmentModal.tsx        - detail popup
    hooks/
      useAppointments.ts          - list, filters, pagination
      useAppointmentActions.ts    - cancel/reschedule/notes
    lib/
      api.ts                      - API fetch wrapper
      types.ts                    - TypeScript types
```

---

## Business Rules (enforced in AppointmentService)

- Only `Booked` appointments can be cancelled or rescheduled
- Cancellation: not allowed within 24 hours of the appointment (48 hours for Dental + High Complexity)
- Rescheduling: same time window rules, maximum 1 reschedule, no provider overlap
- Notes: editable on any appointment regardless of status
- Data warnings are computed on each request and never persisted — they flag anomalies such as a future appointment marked Completed, Telehealth with no contact info, Pediatrics patient aged 16 or over, and provider schedule overlaps

---

## Notable Features

- Pagination implemented end-to-end (API returns `PagedResult<T>`, frontend renders page buttons)
- Filter state persisted to `localStorage` — selections survive a page refresh
- Warnings displayed in the modal as amber alerts when present
