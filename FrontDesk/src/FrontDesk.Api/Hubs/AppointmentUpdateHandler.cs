using System.Threading;
using System.Threading.Tasks;
using FrontDesk.Core.Events;
using FrontDesk.Core.ScheduleAggregate;

using MediatR;
using Microsoft.AspNetCore.SignalR;

using PluralsightDdd.SharedKernel.Interfaces;

namespace FrontDesk.Api.Hubs
{
  public class AppointmentUpdateHandler : INotificationHandler<AppointmentUpdatedEvent>
  {
    private readonly IHubContext<ScheduleHub> _hubContext;
    private readonly IRepository<Schedule> _scheduleRepository;

    public AppointmentUpdateHandler(
      IHubContext<ScheduleHub> hubContext,
      IRepository<Schedule> scheduleRepository)
    {
      _hubContext = hubContext;
      _scheduleRepository = scheduleRepository;
    }

    public async Task Handle(AppointmentUpdatedEvent notification, CancellationToken cancellationToken)
    {
      var schedule = await _scheduleRepository.GetByIdAsync(
        notification.AppointmentUpdated.ScheduleId,
        cancellationToken);

      schedule?.AppointmentUpdatedHandler();
      await _scheduleRepository.SaveChangesAsync(cancellationToken);

      await _hubContext.Clients.All
        .SendAsync(
          "ReceiveMessage", 
          notification.AppointmentUpdated.Title + " was updated", 
          cancellationToken);
    }
  }
}
