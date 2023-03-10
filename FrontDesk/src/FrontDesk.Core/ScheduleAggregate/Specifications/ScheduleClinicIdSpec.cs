using Ardalis.Specification;

namespace FrontDesk.Core.ScheduleAggregate.Specifications
{
  public class ScheduleClinicIdSpec : Specification<Schedule>
  {
    public ScheduleClinicIdSpec(int clinicId)
    {
      Query
          .Where(schedule =>
              schedule.ClinicId == clinicId);
    }
  }
}
