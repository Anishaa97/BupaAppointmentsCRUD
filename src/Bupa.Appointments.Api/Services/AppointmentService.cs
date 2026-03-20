//application logic
using System.Text.Json;
using System.Text.Json.Nodes;
using Bupa.Appointments.Api.Core.Interfaces;
using Bupa.Appointments.Api.Core.Models;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bupa.Appointments.Api.Services
{
    public class AppointmentService: IAppointmentService
    {
        private readonly ILogger<AppointmentService> _logger;
        private readonly string _appointmentsFilePath;
        private List<Appointment> _appointments=new();

        public AppointmentService(ILogger<AppointmentService> logger, IConfiguration configuration)
    {
         _logger = logger;
        // file path is configurable via appsettings.json
        _appointmentsFilePath = configuration["DataFilePath"]
        ?? Path.Combine(AppContext.BaseDirectory, "Data", "appointments.json");
        LoadData();
    }

        // used in tests only to bypass file reading, loads appointments directly
        internal AppointmentService(List<Appointment> appointments)
    {
        _logger = new NullLogger<AppointmentService>();
        _appointments = appointments;
        _appointmentsFilePath = string.Empty;
    }

        //get all the json data and deserialize 
        private void LoadData()
        {
            try
            {
                if (File.Exists(_appointmentsFilePath))
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var jsonData = File.ReadAllText(_appointmentsFilePath);
                    var root = JsonNode.Parse(jsonData);
                    _appointments = root?["appointments"].Deserialize<List<Appointment>>(options) ?? new();
                    _logger.LogInformation("Loaded {Count} appointments.", _appointments.Count);
                }
                else
                {
                    _logger.LogWarning("Appointments file not found at path: {FilePath}", _appointmentsFilePath);
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading appointments data from file: {FilePath}", _appointmentsFilePath);
            }
        }

        public PagedResult<Appointment> GetAppointments(string? category, string? status, string? search, DateTimeOffset? from, DateTimeOffset? to, int page = 1, int pageSize = 10)
        {
                _appointments.ForEach(ApplyWarnings);

                var filtered = _appointments.AsEnumerable();
                if (!string.IsNullOrEmpty(category))
                    filtered = filtered.Where(a => a.Category == category);
                if (!string.IsNullOrEmpty(status))
                    filtered = filtered.Where(a => a.Status == status);
                if (!string.IsNullOrEmpty(search))
                    filtered = filtered.Where(a =>
                        a.Patient.FirstName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        a.Patient.LastName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        a.Provider.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
                if (from.HasValue)
                    filtered = filtered.Where(a => a.StartTime >= from.Value);
                if (to.HasValue)
                    filtered = filtered.Where(a => a.EndTime <= to.Value);
                
                var total = filtered.Count();

                filtered = filtered.Skip((page - 1) * pageSize).Take(pageSize);

                return new PagedResult<Appointment>
                {
                    Data = filtered.ToList(),
                    Total = total,
                    Page = page,
                    PageSize = pageSize
                };
        }

        public Appointment? GetAppointmentById(string id)
        {
            var appointment = _appointments.FirstOrDefault(a => a.Id == id);
            if (appointment is not null) ApplyWarnings(appointment);
            return appointment;
        }
        
        public async Task<Result> CancelAppointment(string id)
        {
            var appointment = GetAppointmentById(id);
            if (appointment == null)
                return new Result { IsSuccess = false, Error = "Appointment not found" };

            if (appointment.Status != "Booked")
                return new Result { IsSuccess = false, Error = "Only booked appointments can be cancelled" };
            
            var allowedHours = appointment.Category =="Dental" && appointment.Tags.Contains("High Complexity") ? 48 : 24; //48H for dental and 24H for others
            var actualHours = (appointment.StartTime - DateTimeOffset.UtcNow).TotalHours;
            if (actualHours < allowedHours)
                return new Result { IsSuccess = false, Error = $"Cancellations must be made at least {allowedHours} hours in advance" };

            appointment.Status = "Cancelled";
            appointment.UpdatedAt = DateTimeOffset.UtcNow;
            await SaveData();
            return new Result { IsSuccess = true };
        }
        
        public async Task<Result> RescheduleAppointment(string id, DateTimeOffset newStartTime, DateTimeOffset newEndTime)
        {
            var appointment = GetAppointmentById(id);
            if (appointment == null)        
                return new Result { IsSuccess = false, Error = "Appointment not found" };

            if(newStartTime < DateTimeOffset.UtcNow || newEndTime < DateTimeOffset.UtcNow)
                return new Result { IsSuccess = false, Error = "New appointment time must be in the future" };

            if(newEndTime <= newStartTime)
                return new Result { IsSuccess = false, Error = "End time must be after start time" };
            
            if (appointment.Status != "Booked")
                return new Result { IsSuccess = false, Error = "Only booked appointments can be rescheduled" };

            if (appointment.RescheduleCount >= 1)
                return new Result { IsSuccess = false, Error = "Appointment has already been rescheduled once" };
            
            var allowedHours = appointment.Category =="Dental" && appointment.Tags.Contains("High Complexity") ? 48 : 24; //48H for dental and 24H for others
            var actualHours = (appointment.StartTime - DateTimeOffset.UtcNow).TotalHours;
            if (actualHours < allowedHours)
                return new Result { IsSuccess = false, Error = $"Rescheduling must be done at least {allowedHours} hours in advance" };

            var overlapCheck = _appointments.Any(a => a.Id != id && 
                                a.Provider.Id == appointment.Provider.Id && 
                                a.Status == "Booked" &&
                                a.StartTime < newEndTime && a.EndTime > newStartTime);

            if (overlapCheck)
                return new Result { IsSuccess = false, Error = "New time overlaps with another appointment for the same provider" };

            appointment.StartTime = newStartTime;
            appointment.EndTime = newEndTime;
            appointment.RescheduleCount++;
            await SaveData();
            return new Result { IsSuccess = true };

        }
        
        public async Task<Result> EditNotes(string id, string notes)
        {
            var appointment = GetAppointmentById(id);
            if (appointment == null)            {
                return new Result { IsSuccess = false, Error = "Appointment not found" };
            }

            appointment.Notes = notes;
            await SaveData();
            return new Result { IsSuccess = true };
        }
        private async Task SaveData()
        {
            if (string.IsNullOrEmpty(_appointmentsFilePath)) return; 
            
            var wrapper = new { appointments = _appointments };
            var json = JsonSerializer.Serialize(wrapper, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_appointmentsFilePath, json);
        }

        //helper class for checking data analomies and adding warnings
        private void ApplyWarnings(Appointment appointment)
        {
            appointment.Warnings.Clear();

            if(appointment.StartTime>DateTimeOffset.UtcNow && appointment.Status=="Completed")
                appointment.Warnings.Add("Appointment is marked as Completed but start time is in the future");

            if(appointment.Location.Type=="Telehealth" && (string.IsNullOrEmpty(appointment.Patient.Email) || string.IsNullOrEmpty(appointment.Patient.Phone)))
                 appointment.Warnings.Add("Telehealth appointments should have patient email and phone specified");

            if(appointment.Category == "Pediatrics" && !string.IsNullOrEmpty(appointment.Patient.Dob))
            {
                if (DateTime.TryParse(appointment.Patient.Dob, out var dob))
                {
                    var age = appointment.StartTime.Year - dob.Year;
                    if (appointment.StartTime.Date < dob.Date.AddYears(age)) age--;
                    
                    if(age >= 16)
                        appointment.Warnings.Add($"Patient is {age} years old. Pediatrics requires under 16.");
                }
            }

            var overlapCheck = _appointments.Any(a => a.Id != appointment.Id &&
                                a.Provider.Id == appointment.Provider.Id &&
                                a.Status == "Booked" &&
                                a.StartTime < appointment.EndTime && a.EndTime > appointment.StartTime);

            if (overlapCheck)
             appointment.Warnings.Add("This appointment overlaps with another appointment for the same provider");
        }
    }
}