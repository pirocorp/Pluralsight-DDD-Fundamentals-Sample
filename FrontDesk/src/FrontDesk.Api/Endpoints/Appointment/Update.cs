using System.Threading;
using System.Threading.Tasks;

using Ardalis.ApiEndpoints;
using AutoMapper;
using BlazorShared.Models.Appointment;

using FrontDesk.Core.ScheduleAggregate.Specifications;
using FrontDesk.Core.ScheduleAggregate;
using FrontDesk.Core.SyncedAggregates;

using Microsoft.AspNetCore.Mvc;
using PluralsightDdd.SharedKernel.Interfaces;
using Swashbuckle.AspNetCore.Annotations;


namespace FrontDesk.Api.AppointmentEndpoints
{
  public class Update : BaseAsyncEndpoint
    .WithRequest<UpdateAppointmentRequest>
    .WithResponse<UpdateAppointmentResponse>
  {
    private readonly IRepository<Schedule> _scheduleRepository;
    private readonly IReadRepository<Schedule> _scheduleReadRepository;
    private readonly IReadRepository<AppointmentType> _appointmentTypeRepository;
    private readonly IMapper _mapper;

    public Update(IRepository<Schedule> scheduleRepository,
      IReadRepository<Schedule> scheduleReadRepository,
      IReadRepository<AppointmentType> appointmentTypeRepository,
      IMapper mapper)
    {
      _scheduleRepository = scheduleRepository;
      _scheduleReadRepository = scheduleReadRepository;
      _appointmentTypeRepository = appointmentTypeRepository;
      _mapper = mapper;
    }

    [HttpPut(UpdateAppointmentRequest.Route)]
    [SwaggerOperation(
        Summary = "Updates an Appointment",
        Description = "Updates an Appointment",
        OperationId = "appointments.update",
        Tags = new[] { "AppointmentEndpoints" })
    ]
    public override async Task<ActionResult<UpdateAppointmentResponse>> HandleAsync(
      UpdateAppointmentRequest request,
      CancellationToken cancellationToken)
    {
      var response = new UpdateAppointmentResponse(request.CorrelationId());

      var appointmentType = await _appointmentTypeRepository
        .GetByIdAsync(request.AppointmentTypeId, cancellationToken);

      var appointmentSpec = new AppointmentById(request.Id);
      var appointmentToUpdate = await _scheduleReadRepository
        .GetBySpecAsync<Appointment>(appointmentSpec, cancellationToken);

      var spec = new ScheduleByIdWithAppointmentsSpecForGivenDate(
        request.ScheduleId,
        appointmentToUpdate.TimeRange.Start);

      var schedule = await _scheduleReadRepository.GetBySpecAsync(spec, cancellationToken);

      appointmentToUpdate.UpdateAppointmentType(appointmentType, schedule.AppointmentUpdatedHandler);
      appointmentToUpdate.UpdateRoom(request.RoomId);
      appointmentToUpdate.UpdateStartTime(request.Start, schedule.AppointmentUpdatedHandler);
      appointmentToUpdate.UpdateTitle(request.Title);
      appointmentToUpdate.UpdateDoctor(request.DoctorId);

      await _scheduleRepository.UpdateAsync(schedule, cancellationToken);

      var dto = _mapper.Map<AppointmentDto>(appointmentToUpdate);
      response.Appointment = dto;

      return Ok(response);
    }
  }
}
