using Application.Common;
using Application.Orders;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Common;

namespace WebAPI.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route(ApiRoutes.Orders)]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetOrderByIdQuery { Id = id });

        return result.Match(
            onSuccess: () => Ok(result.Value),
            onFailure: () => result.ErrorCode switch
            {
                "NOT_FOUND" => NotFound(new { error = result.Error }),
                _ => BadRequest(new { error = result.Error, code = result.ErrorCode })
            });
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateOrderCommand command)
    {
        var result = await _mediator.Send(command);

        return result.Match(
            onSuccess: () => CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value),
            onFailure: () => result.ErrorCode switch
            {
                "CUSTOMER_NOT_FOUND" => NotFound(new { error = result.Error }),
                _ => BadRequest(new { error = result.Error, code = result.ErrorCode })
            });
    }

    [HttpPost("{id:guid}/lines")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AddLine(Guid id, [FromBody] AddOrderLineCommand command)
    {
        if (id != command.OrderId)
            return BadRequest(new { error = "ID in URL does not match OrderId in body", code = "ID_MISMATCH" });

        var result = await _mediator.Send(command);

        return result.Match(
            onSuccess: () => NoContent(),
            onFailure: () => result.ErrorCode switch
            {
                "NOT_FOUND" => NotFound(new { error = result.Error }),
                _ => BadRequest(new { error = result.Error, code = result.ErrorCode })
            });
    }

    [HttpPost("{id:guid}/confirm")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Confirm(Guid id)
    {
        var result = await _mediator.Send(new ConfirmOrderCommand { OrderId = id });

        return result.Match(
            onSuccess: () => NoContent(),
            onFailure: () => result.ErrorCode switch
            {
                "NOT_FOUND" => NotFound(new { error = result.Error }),
                _ => BadRequest(new { error = result.Error, code = result.ErrorCode })
            });
    }

    [HttpPost("{id:guid}/ship")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Ship(Guid id)
    {
        var result = await _mediator.Send(new ShipOrderCommand { OrderId = id });

        return result.Match(
            onSuccess: () => NoContent(),
            onFailure: () => result.ErrorCode switch
            {
                "NOT_FOUND" => NotFound(new { error = result.Error }),
                _ => BadRequest(new { error = result.Error, code = result.ErrorCode })
            });
    }

    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var result = await _mediator.Send(new CancelOrderCommand { OrderId = id });

        return result.Match(
            onSuccess: () => NoContent(),
            onFailure: () => result.ErrorCode switch
            {
                "NOT_FOUND" => NotFound(new { error = result.Error }),
                _ => BadRequest(new { error = result.Error, code = result.ErrorCode })
            });
    }
}
