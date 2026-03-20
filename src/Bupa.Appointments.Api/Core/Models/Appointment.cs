namespace Bupa.Appointments.Api.Core.Models;

public class Appointment
{
    public string Id { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Patient Patient { get; set; } = new();
    public Provider Provider { get; set; } = new();
    public Location Location { get; set; } = new();
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public Price Price { get; set; } = new();
    public int RescheduleCount { get; set; }
    public string Notes { get; set; } = string.Empty;
    public int CancellationPolicyHours { get; set; }
    public List<string> Tags { get; set; } = new();
    public List<string> Warnings { get; set; } = new();  // for data analomies
}

public class Patient
{
    public string Id { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Dob { get; set; } = string.Empty; // kept as string to avoid parse issues
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Insurance Insurance { get; set; } = new();
}

public class Insurance
{
    public string Provider { get; set; } = string.Empty;
    public string MemberId { get; set; } = string.Empty;
}

public class Provider
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class Location
{
    public string Type { get; set; } = string.Empty; // "InPerson" or "Telehealth"
    public string ClinicName { get; set; } = string.Empty;
    public string Room { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Timezone { get; set; } = string.Empty;
}

public class Price
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
}