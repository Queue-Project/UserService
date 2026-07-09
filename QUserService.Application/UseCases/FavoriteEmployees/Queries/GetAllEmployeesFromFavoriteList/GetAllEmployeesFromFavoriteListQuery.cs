using MediatR;
using QUserService.Application.Responses;

namespace QUserService.Application.UseCases.FavoriteEmployees.Queries.GetAllEmployeesFromFavoriteList;

public record GetAllEmployeesFromFavoriteListQuery(int PageNumber): IRequest<PagedResponse<EmployeeResponseModel>>;