using Bupa.Appointments.Api.Core.Interfaces;
using Bupa.Appointments.Api.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace Bupa.Appointments.Api.Controllers;

[ApiController]
[Route("api/appointments")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;
    public AppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    //get appointments with filters
    [HttpGet]
    public IActionResult GetAppointments([FromQuery]string? category,[FromQuery]string? status,[FromQuery]string? search,[FromQuery]DateTimeOffset? from,[FromQuery]DateTimeOffset? to,[FromQuery]int page=1,[FromQuery]int pageSize=10)
    {
        return Ok(_appointmentService.GetAppointments(category, status, search, from, to, page, pageSize));
    }

    //get appointment by id
    [HttpGet("{id}")]
    public IActionResult GetAppointmentById(string id)
    {
        var appointment = _appointmentService.GetAppointmentById(id);
        if (appointment == null)
            return NotFound();
        return Ok(appointment);
    }



    //cancel appointment by id
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelAppointment(string id)
    {
        var result = await _appointmentService.CancelAppointment(id);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        return Ok();
    }

    //reschedule appointment by id
    [HttpPut("{id}/reschedule")]
    public async Task<IActionResult> RescheduleAppointment(string id, [FromBody]RescheduleRequest request)
    {
        if (request == null) 
            return BadRequest(new { error = "Request body is required." });
        var result = await _appointmentService.RescheduleAppointment(id, request.NewStartTime, request.NewEndTime);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        return Ok();
    }

    //edit notes for appointment by id
    [HttpPut("{id}/notes")]
    public async Task<IActionResult> EditNotes(string id, [FromBody]EditNotesRequest request)
    {
        if (request == null)
            return BadRequest(new { error = "Request body is required." });
        var result = await _appointmentService.EditNotes(id, request.Notes);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        return Ok();
    }
}



