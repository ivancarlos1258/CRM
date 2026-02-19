using CRM.Application.Commands.Customers.ActivateCustomer;
using CRM.Application.Commands.Customers.CreateLegalEntity;
using CRM.Application.Commands.Customers.CreateNaturalPerson;
using CRM.Application.Commands.Customers.DeactivateCustomer;
using CRM.Application.Commands.Customers.UpdateCustomer;
using CRM.Application.DTOs;
using CRM.Application.Queries.Customers.GetAllCustomers;
using CRM.Application.Queries.Customers.GetCustomerById;
using CRM.Application.Queries.Customers.GetCustomerEvents;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Server.Controllers;

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
    public async Task<IActionResult> GetAll([FromQuery] bool onlyActive = false)
    {
        var query = new GetAllCustomersQuery(onlyActive);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Data);
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new
        {
            status = "healthy",
            endpoints = new[]
            {
                "GET /api/customers",
                "GET /api/customers/{id}",
                "GET /api/customers/{id}/events",
                "POST /api/customers/natural-person",
                "POST /api/customers/legal-entity",
                "PUT /api/customers/{id}",
                "PUT /api/customers/{id}/deactivate",
                "PUT /api/customers/{id}/activate"
            }
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetCustomerByIdQuery(id);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Data);
    }

    [HttpGet("{id:guid}/events")]
    public async Task<IActionResult> GetEvents(Guid id)
    {
        var query = new GetCustomerEventsQuery(id);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Data);
    }

    [HttpPost("natural-person")]
    public async Task<IActionResult> CreateNaturalPerson([FromBody] CreateNaturalPersonCommand command)
    {
        _logger.LogInformation("Creating natural person customer: {Name}", command.Name);

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to create natural person: {Error}", result.Error);
            return BadRequest(new { error = result.Error, errors = result.Errors });
        }

        _logger.LogInformation("Natural person created successfully: {Id}", result.Data!.Id);
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    [HttpPost("legal-entity")]
    public async Task<IActionResult> CreateLegalEntity([FromBody] CreateLegalEntityCommand command)
    {
        _logger.LogInformation("Creating legal entity customer: {Name}", command.Name);

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to create legal entity: {Error}", result.Error);
            return BadRequest(new { error = result.Error, errors = result.Errors });
        }

        _logger.LogInformation("Legal entity created successfully: {Id}", result.Data!.Id);
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerRequest request)
    {
        _logger.LogInformation("Updating customer: {Id}", id);

        var command = new UpdateCustomerCommand(
            id,
            request.Name,
            request.Phone,
            request.Email,
            request.Address,
            request.StateRegistration,
            request.IsStateRegistrationExempt);

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to update customer {Id}: {Error}", id, result.Error);
            return BadRequest(new { error = result.Error });
        }

        _logger.LogInformation("Customer updated successfully: {Id}", id);
        return Ok(result.Data);
    }

    [HttpPut("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        _logger.LogInformation("Deactivating customer: {Id}", id);

        var command = new DeactivateCustomerCommand(id);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to deactivate customer {Id}: {Error}", id, result.Error);
            return BadRequest(new { error = result.Error });
        }

        _logger.LogInformation("Customer deactivated successfully: {Id}", id);
        return Ok(result.Data);
    }

    [HttpPut("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id)
    {
        _logger.LogInformation("Activating customer: {Id}", id);

        var command = new ActivateCustomerCommand(id);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to activate customer {Id}: {Error}", id, result.Error);
            return BadRequest(new { error = result.Error });
        }

        _logger.LogInformation("Customer activated successfully: {Id}", id);
        return Ok(result.Data);
    }
}

public record UpdateCustomerRequest(
    string Name,
    string Phone,
    string Email,
    CreateAddressDto Address,
    string? StateRegistration,
    bool? IsStateRegistrationExempt
);
