using System;
using System.Linq;
using FrontDesk.Core.ScheduleAggregate;
using PluralsightDdd.SharedKernel;
using Xunit;

namespace UnitTests.Core.AggregatesEntities.ScheduleTests
{
  using System.Threading.Tasks;
  using FrontDesk.Core.Exceptions;

  public class Schedule_AddNewAppointment
  {
    private readonly Guid _scheduleId = Guid.Parse("4a17e702-c20e-4b87-b95b-f915c5a794f7");
    private readonly DateTimeOffsetRange _dateRange = new DateTimeOffsetRange(DateTime.Today, DateTime.Today.AddDays(1));
    private readonly int _clinicId = 1;

    private readonly DateTimeOffset _startTime = new DateTime(2021, 01, 01, 10, 00, 00);
    private readonly DateTimeOffset _endTime;
    private readonly DateTimeOffsetRange _range;

    public Schedule_AddNewAppointment()
    {
      _endTime = _startTime.AddHours(3);
      _range = new DateTimeOffsetRange(_startTime, _endTime);
    }

    [Fact]
    public async Task ThrowsGivenDuplicateAppointmentWithNonEmptyId()
    {
      var schedule = new Schedule(_scheduleId, _dateRange, _clinicId);

      var appointment1 = new Appointment(
        Guid.NewGuid(),
        4,
        _scheduleId,
        1,
        3,
        2,
        5,
        _range,
        "Test Title"
        );

      var appointment2 = new Appointment(
        appointment1.Id,
        1,
        _scheduleId,
        2,
        3,
        4,
        5,
        _range,
        "Test Title"
      );
        
        schedule.AddNewAppointment(appointment1);

        Assert.Throws<DuplicateAppointmentException>(
          () => schedule.AddNewAppointment(appointment2));
    }

    [Fact]
    public async Task MarksConflictingAppointments()
    {
      var schedule = new Schedule(_scheduleId, _dateRange, _clinicId);

      var appointment1 = new Appointment(
        Guid.NewGuid(), 
        5,
        _scheduleId,
        1,
        2,
        3,
        4,
        _range,
        "Test Title"
      );

      var appointment2 = new Appointment(
        Guid.NewGuid(), 
        5,
        _scheduleId,
        1,
        21,
        31,
        41,
        _range,
        "Test Title"
      );

      var appointment3 = new Appointment(
        Guid.NewGuid(), 
        5,
        _scheduleId,
        11,
        2,
        31,
        41,
        _range,
        "Test Title"
      );

      var appointment4 = new Appointment(
        Guid.NewGuid(), 
        5,
        _scheduleId,
        11,
        21,
        3,
        41,
        _range,
        "Test Title"
      );

      var appointment5 = new Appointment(
        Guid.NewGuid(), 
        5,
        _scheduleId,
        11,
        21,
        31,
        4,
        _range,
        "Test Title"
      );

      schedule.AddNewAppointment(appointment1);
      schedule.AddNewAppointment(appointment2);
      schedule.AddNewAppointment(appointment3);
      schedule.AddNewAppointment(appointment4);
      schedule.AddNewAppointment(appointment5);

      Assert.True(appointment1.IsPotentiallyConflicting);
      Assert.True(appointment2.IsPotentiallyConflicting);
      Assert.True(appointment3.IsPotentiallyConflicting);
      Assert.True(appointment4.IsPotentiallyConflicting);
      Assert.True(appointment5.IsPotentiallyConflicting);
    }

    [Fact]
    public void AddsAppointmentScheduledEvent()
    {
      var schedule = new Schedule(_scheduleId, _dateRange, _clinicId);
      var appointmentType = 1;
      var doctorId = 2;
      var patientId = 3;
      var roomId = 4;

      DateTime lisaStartTime = new DateTime(2021, 01, 01, 10, 00, 00);
      DateTime lisaEndTime = new DateTime(2021, 01, 01, 11, 00, 00);
      var lisaDateRange = new DateTimeOffsetRange(lisaStartTime, lisaEndTime);
      var lisaTitle = "Lisa Appointment";
      var lisaAppointment = new Appointment(Guid.NewGuid(), appointmentType, _scheduleId, _clinicId, doctorId, patientId, roomId, lisaDateRange, lisaTitle);
      schedule.AddNewAppointment(lisaAppointment);

      DateTime mimiStartTime = new DateTime(2021, 01, 01, 12, 00, 00);
      DateTime mimiEndTime = new DateTime(2021, 01, 01, 14, 00, 00);
      var mimiDateRange = new DateTimeOffsetRange(mimiStartTime, mimiEndTime);
      var mimiTitle = "Mimi Appointment";
      var mimiAppointment = new Appointment(Guid.NewGuid(), appointmentType, _scheduleId, _clinicId, doctorId, patientId, roomId, mimiDateRange, mimiTitle);
      schedule.AddNewAppointment(mimiAppointment);

      Assert.Equal(2, schedule.Appointments.Count());
      Assert.False(lisaAppointment.IsPotentiallyConflicting);
      Assert.False(mimiAppointment.IsPotentiallyConflicting);
    }
  }
}
