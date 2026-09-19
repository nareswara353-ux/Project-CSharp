using Application.Common;
using Application.Products;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IMediator mediator, ILogger<ProductsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProductDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? searchTerm = null)
    {
        var query = new GetAllProductsQuery
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
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetProductByIdQuery { Id = id };
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
    public async Task<IActionResult> Create([FromBody] CreateProductCommand command)
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
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductCommand command)
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
}
