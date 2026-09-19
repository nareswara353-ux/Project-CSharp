using Application.Common;
using Application.Customers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(IMediator mediator, ILogger<CustomersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<CustomerDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? searchTerm = null)
    {
        var query = new GetAllCustomersQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            IsActive = isActive,
            SearchTerm = searchTerm
        };
        var result = await _mediator.Send(query);

        return result.Match(
            onSuccess: () => Ok(result.Value),
            onFailure: () => BadRequest(new { error = result.Error, code = result.ErrorCode }));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetCustomerByIdQuery { Id = id };
        var result = await _mediator.Send(query);

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
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCustomerCommand command)
    {
        var result = await _mediator.Send(command);

        return result.Match(
            onSuccess: () => CreatedAtAction(
                nameof(GetById),
                new { id = result.Value },
                result.Value),
            onFailure: () => BadRequest(new { error = result.Error, code = result.ErrorCode }));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerCommand command)
    {
        if (id != command.Id)
            return BadRequest(new { error = "ID in URL does not match ID in request body", code = "ID_MISMATCH" });

        var result = await _mediator.Send(command);

        return result.Match(
            onSuccess: () => NoContent(),
            onFailure: () => result.ErrorCode switch
            {
                "NOT_FOUND" => NotFound(new { error = result.Error }),
                _ => BadRequest(new { error = result.Error, code = result.ErrorCode })
            });
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteCustomerCommand { Id = id };
        var result = await _mediator.Send(command);

        return result.Match(
            onSuccess: () => NoContent(),
            onFailure: () => result.ErrorCode switch
            {
                "NOT_FOUND" => NotFound(new { error = result.Error }),
                _ => BadRequest(new { error = result.Error, code = result.ErrorCode })
            });
    }
}

public static class ResultExtensions
{
    public static IActionResult Match<T>(
        this Result<T> result,
        Func<IActionResult> onSuccess,
        Func<IActionResult> onFailure)
    {
        return result.IsSuccess ? onSuccess() : onFailure();
    }

    public static IActionResult Match(
        this Result result,
        Func<IActionResult> onSuccess,
        Func<IActionResult> onFailure)
    {
        return result.IsSuccess ? onSuccess() : onFailure();
    }
}
