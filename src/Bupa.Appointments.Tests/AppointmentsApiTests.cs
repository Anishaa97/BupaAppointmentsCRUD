using System.Net;
using System.Net.Http.Json;
using Bupa.Appointments.Api.Core.Interfaces;
using Bupa.Appointments.Api.Core.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Bupa.Appointments.Tests;

public class AppointmentsApiTests
{
    // spins up the real ASP.NET Core pipeline (routing, middleware, controllers)
    // but swaps the service with a mock so we control responses without needing the JSON file
    private HttpClient CreateClient(Mock<IAppointmentService> mock)
    {
        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // remove the real AppointmentService registration
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IAppointmentService));
                    if (descriptor != null) services.Remove(descriptor);

                    // replace with our mock
                    services.AddSingleton(mock.Object);
                });
            });

        return factory.CreateClient();
    }

    [Fact]
    public async Task GetAppointments_ReturnsOkWithPagedData()
    {
        // arrange
        var mock = new Mock<IAppointmentService>();

        mock.Setup(s => s.GetAppointments(null, null, null, null, null, 1, 10))
            .Returns(new PagedResult<Appointment>
            {
                Data = new List<Appointment>
                {
                    new()
                    {
                        Id = "test-1", Status = "Booked", Category = "GP",
                        StartTime = DateTimeOffset.UtcNow.AddDays(1),
                        EndTime = DateTimeOffset.UtcNow.AddDays(1).AddHours(1),
                        Tags = new List<string>(), Warnings = new List<string>(),
                        Patient = new Patient { FirstName = "Jane", LastName = "Doe", Dob = "1985-06-15" },
                        Provider = new Provider { Id = "p1", Name = "Dr Smith", Role = "GP" },
                        Location = new Location { Type = "InPerson", ClinicName = "City Clinic" },
                        Price = new Price { Amount = 150, Currency = "AUD" },
                        Notes = ""
                    }
                },
                Total = 1,
                Page = 1,
                PageSize = 10
            });

        var client = CreateClient(mock);

        // act
        var response = await client.GetAsync("/api/appointments");

        // assert — correct status and body shape
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<PagedResult<Appointment>>();
        Assert.NotNull(body);
        Assert.Single(body.Data);
        Assert.Equal("test-1", body.Data.First().Id);
    }

    [Fact]
    public async Task CancelAppointment_WhenAlreadyCompleted_ReturnsBadRequest()
    {
        // arrange — service returns failure because appointment is Completed
        var mock = new Mock<IAppointmentService>();

        mock.Setup(s => s.CancelAppointment("appt-999"))
            .ReturnsAsync(new Result
            {
                IsSuccess = false,
                Error = "Only booked appointments can be cancelled."
            });

        var client = CreateClient(mock);

        // act
        var response = await client.PostAsync("/api/appointments/appt-999/cancel", null);

        // assert — controller maps IsSuccess=false to 400 Bad Request
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}