using MediatR;
using QUserService.Application.Responses;

namespace QUserService.Application.UseCases.Employees.Commands.CreateEmployee;

public record CreateEmployeeCommand(int BranchId, int ServiceId, string Firstname, string Lastname, string Position, string PhoneNumber)
    : IRequest<EmployeeResponseModel>;