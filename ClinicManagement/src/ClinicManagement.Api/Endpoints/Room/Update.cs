using System.Threading;
using System.Threading.Tasks;
using Ardalis.ApiEndpoints;
using AutoMapper;
using BlazorShared.Models.Room;
using ClinicManagement.Core.Aggregates;
using Microsoft.AspNetCore.Mvc;
using PluralsightDdd.SharedKernel.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace ClinicManagement.Api.RoomEndpoints
{
  using ApplicationEvents;
  using ClinicManagement.Core.Interfaces;

  public class Update : BaseAsyncEndpoint
    .WithRequest<UpdateRoomRequest>
    .WithResponse<UpdateRoomResponse>
  {
    private readonly IRepository<Room> _repository;
    private readonly IMapper _mapper;
    private readonly IMessagePublisher _messagePublisher;

    public Update(
      IRepository<Room> repository, 
      IMapper mapper, 
      IMessagePublisher messagePublisher)
    {
      _repository = repository;
      _mapper = mapper;
      _messagePublisher = messagePublisher;
    }

    [HttpPut("api/rooms")]
    [SwaggerOperation(
        Summary = "Updates a Room",
        Description = "Updates a Room",
        OperationId = "rooms.update",
        Tags = new[] { "RoomEndpoints" })
    ]
    public override async Task<ActionResult<UpdateRoomResponse>> HandleAsync(
      UpdateRoomRequest request, 
      CancellationToken cancellationToken)
    {
      var response = new UpdateRoomResponse(request.CorrelationId);

      var toUpdate = _mapper.Map<Room>(request);
      await _repository.UpdateAsync(toUpdate);

      var dto = _mapper.Map<RoomDto>(toUpdate);
      response.Room = dto;

      var appEvent = new NamedEntityUpdatedEvent(
        _mapper.Map<NamedEntity>(toUpdate), 
        "Room-Updated");

      _messagePublisher.Publish(appEvent);

      return Ok(response);
    }
  }
}
