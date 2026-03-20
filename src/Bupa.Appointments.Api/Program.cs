using Bupa.Appointments.Api.Core.Interfaces;
using Bupa.Appointments.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

//single instance shared across all requests
builder.Services.AddSingleton<IAppointmentService, AppointmentService>();

//allow the Next.js frontend to call this API
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();
app.UseHttpsRedirection();
app.MapControllers();

app.Run();


// exposed so WebApplicationFactory<Program> can reference this class in integration tests
public partial class Program { }