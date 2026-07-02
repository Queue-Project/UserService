using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QUserService.Application.Interfaces;
using QUserService.Application.Responses;

namespace QUserService.Application.UseCases.FavoriteEmployees.Queries.GetAllEmployeesFromFavoriteList;

public class GetAllEmployeesFromFavoriteListQueryHandler : IRequestHandler<GetAllEmployeesFromFavoriteListQuery,
    PagedResponse<EmployeeResponseModel>>
{
    private const int PageSize = 15;
    private readonly ILogger<GetAllEmployeesFromFavoriteListQueryHandler> _logger;
    private readonly IUserServiceApplicationDbContext _dbContext;

    public GetAllEmployeesFromFavoriteListQueryHandler(ILogger<GetAllEmployeesFromFavoriteListQueryHandler> logger,
        IUserServiceApplicationDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }


    public async Task<PagedResponse<EmployeeResponseModel>> Handle(GetAllEmployeesFromFavoriteListQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all employees from favorite list. PageNumber: {pageNumber}, PageSize: {pageSize}",
            request.PageNumber,
            PageSize);

        var totalCount = await _dbContext.FavoriteEmployeeEntities.CountAsync(cancellationToken);

        var dbEmployees = await _dbContext.FavoriteEmployeeEntities
            .Include(s => s.Employee)
            .OrderBy(s => s.Id)
            .Skip((request.PageNumber - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync(cancellationToken);
        

        var response = dbEmployees.Select(employee => new EmployeeResponseModel
        {
            Id = employee.Id,
            CompanyId = employee.Employee.CompanyId,
            BranchId = employee.Employee.BranchId,
            ServiceId = employee.Employee.ServiceId ?? 0,
            FirstName = employee.Employee.FirstName,
            LastName = employee.Employee.LastName,
            Position = employee.Employee.Position,
            PhoneNumber = employee.Employee.PhoneNumber
        }).ToList();
        
        _logger.LogInformation("Fetched {employeeCount} employees.", response.Count);

        return new PagedResponse<EmployeeResponseModel>
        {
            Items = response,
            PageNumber = request.PageNumber,
            PageSize = PageSize,
            TotalCount = totalCount
        };
        
    }
}