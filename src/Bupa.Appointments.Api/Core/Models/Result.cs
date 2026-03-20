namespace Bupa.Appointments.Api.Core.Models;

public class Result // simple result class to represent success/failure of operations
{
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }
}