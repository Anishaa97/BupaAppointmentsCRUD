namespace Bupa.Appointments.Api.Core.Models;

public record RescheduleRequest(DateTimeOffset NewStartTime, DateTimeOffset NewEndTime);
public record EditNotesRequest(string Notes);
public class PagedResult<T>
{
    public List<T> Data { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}