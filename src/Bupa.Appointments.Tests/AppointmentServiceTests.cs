using Bupa.Appointments.Api.Core.Models;
using Bupa.Appointments.Api.Services;

namespace Bupa.Appointments.Tests;

public class AppointmentServiceTests
{
    // helper to create a basic appointment — avoids repeating setup in every test
    private static Appointment MakeAppointment(string id, string status, double hoursFromNow, int rescheduleCount = 0, string category = "GP") =>
        new()
        {
            Id = id,
            Status = status,
            Category = category,
            RescheduleCount = rescheduleCount,
            StartTime = DateTimeOffset.UtcNow.AddHours(hoursFromNow),
            EndTime = DateTimeOffset.UtcNow.AddHours(hoursFromNow + 1),
            Tags = new List<string>(),
            Patient = new Patient { FirstName = "Test", LastName = "User", Dob = "1990-01-01" },
            Provider = new Provider { Id = "p1", Name = "Dr Test", Role = "GP" },
            Location = new Location { Type = "InPerson", ClinicName = "Test Clinic" },
            Price = new Price { Amount = 100, Currency = "AUD" },
            Notes = "",
            Warnings = new List<string>()
        };

    [Fact]
    public async Task CancelAppointment_WhenStatusIsNotBooked_ReturnsFailure()
    {
        // arrange — appointment is already Completed
        var service = new AppointmentService(new List<Appointment>
        {
            MakeAppointment("1", "Completed", hoursFromNow: 48)
        });

        // act
        var result = await service.CancelAppointment("1");

        // assert
        Assert.False(result.IsSuccess);
        Assert.Contains("booked", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CancelAppointment_WhenWithin24Hours_ReturnsFailure()
    {
        // arrange — appointment starts in 2 hours, within the 24h window
        var service = new AppointmentService(new List<Appointment>
        {
            MakeAppointment("1", "Booked", hoursFromNow: 2)
        });

        // act
        var result = await service.CancelAppointment("1");

        // assert
        Assert.False(result.IsSuccess);
        Assert.Contains("24", result.Error);
    }

    [Fact]
    public async Task CancelAppointment_WhenValid_ReturnsSuccess()
    {
        // arrange — appointment starts in 48 hours, well outside the window
        var service = new AppointmentService(new List<Appointment>
        {
            MakeAppointment("1", "Booked", hoursFromNow: 48)
        });

        // act
        var result = await service.CancelAppointment("1");

        // assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task RescheduleAppointment_WhenAlreadyRescheduled_ReturnsFailure()
    {
        // arrange — rescheduleCount is already 1
        var service = new AppointmentService(new List<Appointment>
        {
            MakeAppointment("1", "Booked", hoursFromNow: 48, rescheduleCount: 1)
        });

        // act
        var result = await service.RescheduleAppointment(
            "1",
            DateTimeOffset.UtcNow.AddDays(3),
            DateTimeOffset.UtcNow.AddDays(3).AddHours(1));

        // assert
        Assert.False(result.IsSuccess);
        Assert.Contains("rescheduled", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(15, false)]
    [InlineData(16, true)]  
    [InlineData(17, true)]  
    public void GetAppointments_PediatricsAgeRule_WarnsWhen16OrOver(int age, bool expectWarning)
    {
        // arrange
        var startTime = DateTimeOffset.UtcNow.AddDays(1);
        var dob = startTime.AddYears(-age).ToString("yyyy-MM-dd");
        var appointment = MakeAppointment("1", "Booked", hoursFromNow: 24, category: "Pediatrics");
        appointment.Patient.Dob = dob;

        var service = new AppointmentService(new List<Appointment> { appointment });

        // act
        var result = service.GetAppointments("Pediatrics", null, null, null, null);
        var hasWarning = result.Data.First().Warnings.Any(w => w.Contains("Pediatrics requires under 16"));

        // assert
        Assert.Equal(expectWarning, hasWarning);
    }
}