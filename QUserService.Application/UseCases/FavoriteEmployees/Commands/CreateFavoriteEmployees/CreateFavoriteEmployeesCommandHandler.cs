using System.Net;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QUserService.Application.Exceptions;
using QUserService.Application.Interfaces;
using QUserService.Domain.Models;

namespace QUserService.Application.UseCases.FavoriteEmployees.Commands.CreateFavoriteEmployees;

public class CreateFavoriteEmployeesCommandHandler : IRequestHandler<CreateFavoriteEmployeesCommand, bool>
{
    private readonly ILogger<CreateFavoriteEmployeesCommandHandler> _logger;
    private readonly IUserServiceApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateFavoriteEmployeesCommandHandler(ILogger<CreateFavoriteEmployeesCommandHandler> logger,
        IUserServiceApplicationDbContext dbContext, ICurrentUserService currentUserService)
    {
        _logger = logger;
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }


    public async Task<bool> Handle(CreateFavoriteEmployeesCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Adding Employee into Favorite Employee List");
        var customer = await _currentUserService.GetCurrentCustomerAsync(_dbContext, cancellationToken);

        var employee =
            await _dbContext.Employees.FirstOrDefaultAsync(s => s.Id == request.EmployeeId, cancellationToken);

        if (employee is null)
        {
            _logger.LogError("Employee with Id {EmployeeId} not found", request.EmployeeId);
            throw new HttpStatusCodeException(HttpStatusCode.NotFound,
                $"Employee with Id {request.EmployeeId} not found");
        }

        var favoriteEmployee = new FavoriteEmployeesEntity
        {
            CustomerId = customer.Id,
            EmployeeId = request.EmployeeId,
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.FavoriteEmployeeEntities.AddAsync(favoriteEmployee, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        
        return true;
    }
}