using System.Threading;
using System.Threading.Tasks;
using Ardalis.ApiEndpoints;
using AutoMapper;
using BlazorShared.Models.Doctor;
using ClinicManagement.Core.Aggregates;
using Microsoft.AspNetCore.Mvc;
using PluralsightDdd.SharedKernel.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace ClinicManagement.Api.DoctorEndpoints
{
  using System;
  using ApplicationEvents;
  using ClinicManagement.Core.Interfaces;

  public class Update : BaseAsyncEndpoint
    .WithRequest<UpdateDoctorRequest>
    .WithResponse<UpdateDoctorResponse>
  {
    private readonly IRepository<Doctor> _repository;
    private readonly IMapper _mapper;
    private readonly IMessagePublisher _messagePublisher;

    public Update(
      IRepository<Doctor> repository, 
      IMapper mapper, 
      IMessagePublisher messagePublisher)
    {
      _repository = repository;
      _mapper = mapper;
      _messagePublisher = messagePublisher;
    }

    [HttpPut("api/doctors")]
    [SwaggerOperation(
        Summary = "Updates a Doctor",
        Description = "Updates a Doctor",
        OperationId = "doctors.update",
        Tags = new[] { "DoctorEndpoints" })
    ]
    public override async Task<ActionResult<UpdateDoctorResponse>> HandleAsync(
      UpdateDoctorRequest request, 
      CancellationToken cancellationToken)
    {
      var response = new UpdateDoctorResponse(request.CorrelationId);

      var toUpdate = _mapper.Map<Doctor>(request);
      await _repository.UpdateAsync(toUpdate);

      var dto = _mapper.Map<DoctorDto>(toUpdate);
      response.Doctor = dto;

      var appEvent = new NamedEntityUpdatedEvent(
        _mapper.Map<NamedEntity>(toUpdate), 
        "Doctor-Updated");

      _messagePublisher.Publish(appEvent);

      return Ok(response);
    }
  }
}
