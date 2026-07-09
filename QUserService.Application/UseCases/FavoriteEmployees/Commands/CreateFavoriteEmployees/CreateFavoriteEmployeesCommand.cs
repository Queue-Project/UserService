using MediatR;

namespace QUserService.Application.UseCases.FavoriteEmployees.Commands.CreateFavoriteEmployees;

public record CreateFavoriteEmployeesCommand(int EmployeeId ): IRequest<bool>;