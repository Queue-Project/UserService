using QUserService.Domain.Models;

namespace QUserService.Application.Interfaces;

public interface ICurrentUserService
{
    Task<UserEntity> GetCurrentUserAsync(IUserServiceApplicationDbContext dbContext, CancellationToken cancellationToken);
    Task<CustomerEntity> GetCurrentCustomerAsync(IUserServiceApplicationDbContext dbContext, CancellationToken cancellationToken);
    Task<EmployeeEntity> GetCurrentEmployeeAsync(IUserServiceApplicationDbContext dbContext, CancellationToken cancellationToken);
    Task<bool> IsEmployee(IUserServiceApplicationDbContext dbContext, CancellationToken cancellationToken);
    
}