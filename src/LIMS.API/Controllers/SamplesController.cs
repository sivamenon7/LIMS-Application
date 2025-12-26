using LIMS.Application.Samples.Commands;
using LIMS.Application.Samples.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LIMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SamplesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<SamplesController> _logger;

    public SamplesController(IMediator mediator, ILogger<SamplesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllSamplesQuery());

        return result.IsSuccess
            ? Ok(result.Data)
            : BadRequest(result.Error);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetSampleByIdQuery(id));

        return result.IsSuccess
            ? Ok(result.Data)
            : NotFound(result.Error);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSampleCommand command)
    {
        var result = await _mediator.Send(command);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Data }, result.Data)
            : BadRequest(result.Error);
    }
}
