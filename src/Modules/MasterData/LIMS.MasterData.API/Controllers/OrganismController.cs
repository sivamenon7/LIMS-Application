using LIMS.MasterData.API.Models.Organism;
using LIMS.MasterData.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LIMS.MasterData.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class OrganismController : ControllerBase
{
    private readonly IOrganismService _service;
    private readonly ILogger<OrganismController> _logger;

    public OrganismController(IOrganismService service, ILogger<OrganismController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Create a new organism
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(OrganismModel), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrganismModel>> Create([FromBody] CreateOrganismPayload payload)
    {
        var result = await _service.Create(payload);

        if (result.IsFailed)
        {
            _logger.LogWarning("Failed to create organism: {Error}", result.Errors.FirstOrDefault()?.Message);
            return BadRequest(new { errors = result.Errors.Select(e => e.Message) });
        }

        return Created($"/api/organism/{result.Value.Id}", result.Value);
    }

    /// <summary>
    /// Update an existing organism
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(OrganismModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrganismModel>> Update(Guid id, [FromBody] UpdateOrganismPayload payload)
    {
        if (id != payload.Id)
        {
            return BadRequest("Id mismatch");
        }

        var result = await _service.Update(payload);

        if (result.IsFailed)
        {
            _logger.LogWarning("Failed to update organism {Id}: {Error}", id, result.Errors.FirstOrDefault()?.Message);
            return BadRequest(new { errors = result.Errors.Select(e => e.Message) });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Get organism by id
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OrganismModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrganismModel>> Get(Guid id)
    {
        var result = await _service.Get(id);

        if (result.IsFailed)
        {
            return NotFound(new { error = result.Errors.FirstOrDefault()?.Message });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Get all organisms
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrganismModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<OrganismModel>>> GetAll()
    {
        var result = await _service.GetAll();
        return Ok(result.Value);
    }

    /// <summary>
    /// Get filtered organisms
    /// </summary>
    [HttpGet("filter")]
    [ProducesResponseType(typeof(IEnumerable<OrganismModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<OrganismModel>>> GetFiltered(
        [FromQuery] Guid? typeId = null,
        [FromQuery] Guid? characterizationId = null,
        [FromQuery] bool? active = null)
    {
        var result = await _service.GetFiltered(typeId, characterizationId, active);
        return Ok(result.Value);
    }

    /// <summary>
    /// Activate an organism
    /// </summary>
    [HttpPost("{id}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Activate(Guid id, [FromBody] ActivateRequest request)
    {
        var result = await _service.Activate(id, request.Notes);

        if (result.IsFailed)
        {
            return BadRequest(new { errors = result.Errors.Select(e => e.Message) });
        }

        return NoContent();
    }

    /// <summary>
    /// Deactivate an organism
    /// </summary>
    [HttpPost("{id}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Deactivate(Guid id, [FromBody] DeactivateRequest request)
    {
        var result = await _service.Deactivate(id, request.Notes);

        if (result.IsFailed)
        {
            return BadRequest(new { errors = result.Errors.Select(e => e.Message) });
        }

        return NoContent();
    }

    /// <summary>
    /// Get organism history (temporal data)
    /// </summary>
    [HttpGet("{id}/history")]
    [ProducesResponseType(typeof(IEnumerable<OrganismModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<OrganismModel>>> GetHistory(Guid id)
    {
        var result = await _service.GetHistory(id);
        return Ok(result.Value);
    }
}

public record ActivateRequest(string Notes);
public record DeactivateRequest(string Notes);
