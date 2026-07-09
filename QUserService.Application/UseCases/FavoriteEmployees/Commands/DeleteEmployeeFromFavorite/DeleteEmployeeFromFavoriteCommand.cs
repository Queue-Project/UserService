using MediatR;

namespace QUserService.Application.UseCases.FavoriteEmployees.Commands.DeleteEmployeeFromFavorite;

public record DeleteEmployeeFromFavoriteCommand(int EmployeeId): IRequest<bool>;