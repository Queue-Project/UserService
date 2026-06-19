using Microsoft.AspNetCore.Http;
using QUserService.Application.Extensions;
using QUserService.Application.Interfaces;
using QUserService.Domain.Models;

namespace QUserService.Application.Services;

public class CurrentUserService: ICurrentUserService
{
    private readonly IHttpContextAccessor _contextAccessor;
    
    public CurrentUserService(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    
    public async Task<UserEntity> GetCurrentUserAsync(IUserServiceApplicationDbContext dbContext, CancellationToken cancellationToken)
    {
        return await _contextAccessor.CurrentUser(dbContext, cancellationToken);
    }

    public async Task<CustomerEntity> GetCurrentCustomerAsync(IUserServiceApplicationDbContext dbContext, CancellationToken cancellationToken)
    {
        return await _contextAccessor.CurrentCustomer(dbContext, cancellationToken);
    }

    public async Task<EmployeeEntity> GetCurrentEmployeeAsync(IUserServiceApplicationDbContext dbContext, CancellationToken cancellationToken)
    {
        return await _contextAccessor.CurrentEmployee(dbContext, cancellationToken);
    }

    public async Task<bool> IsEmployee(IUserServiceApplicationDbContext dbContext, CancellationToken cancellationToken)
    {
        return await _contextAccessor.IsEmployee(dbContext, cancellationToken);
    }
}