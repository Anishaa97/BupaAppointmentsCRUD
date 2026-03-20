using Bupa.Appointments.Api.Core.Models;

namespace Bupa.Appointments.Api.Core.Interfaces
{
    public interface IAppointmentService
    {
        PagedResult<Appointment> GetAppointments(string? category, string? status, string? search, DateTimeOffset? from, DateTimeOffset? to, int page = 1, int pageSize = 10);
        Appointment? GetAppointmentById(string id);
        Task<Result> CancelAppointment(string id);
        Task<Result> RescheduleAppointment(string id, DateTimeOffset newStartTime, DateTimeOffset newEndTime);
        Task<Result> EditNotes(string id, string notes);
    }
}