# Design Decisions

---

## Result pattern instead of exceptions

Business rule failures return `Result { IsSuccess, Error }` rather than throwing. A user trying to cancel a completed appointment is an expected case, not an unexpected error. The controller maps `IsSuccess = false` to a `400 Bad Request` with the error message, giving the frontend something consistent to display.

---

## No repository pattern

Data comes from a JSON file, not a database. Adding a repository interface would be an extra layer with nothing to justify it at this scale. If the data source changed to a real database, that would be the right time to introduce one.

---

## Singleton service registration

`AppointmentService` is registered with `AddSingleton`. This means the appointments list is loaded once from the JSON file and shared in memory across all requests. This is appropriate for a JSON-backed store — there is no connection state to manage. It would be wrong with a database, where a scoped or transient lifetime would be needed instead.

---

## Warnings are computed on read, never stored

`ApplyWarnings` runs on every `GetAppointments` and `GetAppointmentById` call. The warnings list is cleared and recomputed each time from the current data. They are never written back to the JSON file, so there is no risk of stale warning data persisting.

---

## File path in appsettings.json

The JSON file path is read from `appsettings.json` via `IConfiguration` rather than hardcoded. This is standard practice for any path or config value that could differ between environments.

---

## Two constructors for testability

`AppointmentService` has a production constructor (takes `IConfiguration`) and an internal test constructor (takes a `List<Appointment>` directly). The test constructor skips the file entirely, so unit tests work without touching the filesystem.

An alternative was to extract file access behind an `IDataLoader` interface. That was ruled out — it adds a new abstraction just to satisfy a test, when a second constructor does the same job with less code.

`SaveData()` returns early if the file path is empty, which is the case in tests.

---

## Unit tests vs integration tests

Unit tests (`AppointmentServiceTests`) call the service directly. They test business rules in isolation — no HTTP, no file system.

Integration tests (`AppointmentsApiTests`) spin up the full ASP.NET Core pipeline using `WebApplicationFactory`. The service is swapped with a Moq mock at the DI level. These tests check that routing, controller responses, and HTTP status codes are wired correctly.

`public partial class Program {}` is needed at the bottom of `Program.cs` because C# top-level statements generate `Program` as internal. The partial class makes it visible to the test project.

---

## Frontend: two hooks, no state library

The app has two concerns — the list with filters, and actions on a single appointment. These map to two custom hooks: `useAppointments` and `useAppointmentActions`. No external library (Redux, Zustand) was needed for something this focused.

Filter selections are saved to `localStorage` on every change and read back on load, so they survive a page refresh.

---

## Pagination end-to-end

The API accepts `page` and `pageSize` and returns a `PagedResult<T>` with the total count. The frontend calculates page buttons from `total / pageSize`. This avoids loading all records into the browser at once.

