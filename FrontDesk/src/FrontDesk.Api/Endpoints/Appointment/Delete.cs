using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.ApiEndpoints;
using AutoMapper;
using BlazorShared.Models.Appointment;
using BlazorShared.Models.Schedule;
using FrontDesk.Core.ScheduleAggregate.Specifications;
using Microsoft.AspNetCore.Mvc;
using PluralsightDdd.SharedKernel.Interfaces;
using Swashbuckle.AspNetCore.Annotations;
using FrontDesk.Core.ScheduleAggregate;

namespace FrontDesk.Api.AppointmentEndpoints
{
  public class Delete : BaseAsyncEndpoint
    .WithRequest<DeleteAppointmentRequest>
    .WithResponse<DeleteAppointmentResponse>
  {
    private readonly IReadRepository<Schedule> _scheduleReadRepository;
    private readonly IRepository<Schedule> _scheduleRepository;
    private readonly IMapper _mapper;

    public Delete(IRepository<Schedule> scheduleRepository, IReadRepository<Schedule> scheduleReadRepository, IMapper mapper)
    {
      _scheduleRepository = scheduleRepository;
      _scheduleReadRepository = scheduleReadRepository;
      _mapper = mapper;
    }

    [HttpDelete(DeleteAppointmentRequest.Route)]
    [SwaggerOperation(
        Summary = "Deletes an Appointment",
        Description = "Deletes an Appointment",
        OperationId = "appointments.delete",
        Tags = new[] { "AppointmentEndpoints" })
    ]
    public override async Task<ActionResult<DeleteAppointmentResponse>> HandleAsync(
      [FromRoute] DeleteAppointmentRequest request, 
      CancellationToken cancellationToken)
    {
      var response = new DeleteAppointmentResponse(request.CorrelationId());

      var appointmentSpec = new AppointmentById(request.AppointmentId);
      var appointment = await _scheduleReadRepository
        .GetBySpecAsync<Appointment>(appointmentSpec, cancellationToken);

      var scheduleSpec = new ScheduleByIdWithAppointmentsSpecForGivenDate(
        request.ScheduleId,
        appointment.TimeRange.Start); 

      var schedule = await _scheduleReadRepository.GetBySpecAsync(scheduleSpec, cancellationToken);

      if (appointment == null) return NotFound();

      schedule?.DeleteAppointment(appointment);

      await _scheduleRepository.UpdateAsync(schedule, cancellationToken);

      // verify we can still get the schedule
      response.Schedule = _mapper.Map<ScheduleDto>(await _scheduleReadRepository.GetBySpecAsync(scheduleSpec));
 
      return Ok(response);
    }
  }
}
