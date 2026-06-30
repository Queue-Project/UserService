using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QUserService.Application.Responses;
using QUserService.Application.UseCases.FavoriteEmployees.Commands.CreateFavoriteEmployees;
using QUserService.Application.UseCases.FavoriteEmployees.Commands.DeleteEmployeeFromFavorite;
using QUserService.Application.UseCases.FavoriteEmployees.Queries.GetAllEmployeesFromFavoriteList;
using QUserService.Domain.Enums;

namespace QUserService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FavoriteEmployeesController : ControllerBase
{
    private readonly ILogger<FavoriteEmployeesController> _logger;
    private readonly IMediator _mediator;

    public FavoriteEmployeesController(ILogger<FavoriteEmployeesController> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }


    [Authorize(Roles = nameof(UserRoles.Customer))]
    [HttpGet("get-all-employees-from-favorite-list")]
    public async Task<ActionResult<PagedResponse<EmployeeResponseModel>>> GetAllEmployeesFromFavoriteListAsync(
        [FromRoute] int pageNumber = 1)
    {
        _logger.LogInformation(
            "Received request to get all employees from favorite list. PageNumber: {PageNUmber}, PageSize: 15",
            pageNumber);
        var query = new GetAllEmployeesFromFavoriteListQuery(pageNumber);
        var employees = await _mediator.Send(query);

        return Ok(employees);
    }


    [Authorize(Roles = nameof(UserRoles.Customer))]
    [HttpPost("add-employee-to-favorite-list-{employeeId}")]
    public async Task<IActionResult> AddEmployeeToFavoriteListAsync([FromRoute] int employeeId)
    {
        _logger.LogInformation("Received request to add employee to favorite list with Id: {EmployeeId}", employeeId);
        var command = new CreateFavoriteEmployeesCommand(employeeId);
        await _mediator.Send(command);
        _logger.LogInformation("Successfully added employee to favorite list with Id: {employeeId}", employeeId);
        return Ok("Successfully added employee into favorite list");
    }

    
    [Authorize(Roles = nameof(UserRoles.Customer))]
    [HttpDelete("remove-employee-from-favorite-list-{employeeId}")]
    public async Task<IActionResult> DeleteEmployeeFromFavoriteList([FromRoute] int employeeId)
    {
        _logger.LogInformation("Received request to delete employee with Id from favorite list: {employeeId}",
            employeeId);
        var command = new DeleteEmployeeFromFavoriteCommand(employeeId);
        await _mediator.Send(command);
        _logger.LogInformation("Successfully deleted employee with Id from favorite list: {employeeId}", employeeId);
        return NoContent();
    }
}