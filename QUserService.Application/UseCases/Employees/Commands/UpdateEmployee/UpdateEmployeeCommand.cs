using MediatR;
using QUserService.Application.Responses;

namespace QUserService.Application.UseCases.Employees.Commands.UpdateEmployee;

public record UpdateEmployeeCommand( int Id, string Firstname, string Lastname, string Position, string PhoneNumber)
    : IRequest<EmployeeResponseModel>;