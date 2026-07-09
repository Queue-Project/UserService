using System.Net;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QUserService.Application.Exceptions;
using QUserService.Application.Interfaces;

namespace QUserService.Application.UseCases.FavoriteEmployees.Commands.DeleteEmployeeFromFavorite;

public class DeleteEmployeeFromFavoriteCommandHandler : IRequestHandler<DeleteEmployeeFromFavoriteCommand, bool>
{
    private readonly ILogger<DeleteEmployeeFromFavoriteCommandHandler> _logger;
    private readonly IUserServiceApplicationDbContext _dbContext;

    public DeleteEmployeeFromFavoriteCommandHandler(ILogger<DeleteEmployeeFromFavoriteCommandHandler> logger,
        IUserServiceApplicationDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(DeleteEmployeeFromFavoriteCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting employee with Id {id} from favorite list", request.EmployeeId);
        
        var favoriteEmployee =
            await _dbContext.FavoriteEmployeeEntities.FirstOrDefaultAsync(s => s.EmployeeId == request.EmployeeId,
                cancellationToken);

        if (favoriteEmployee is null)
        {
            _logger.LogError("Employee with Id {EmployeeId} not found in favorite list", request.EmployeeId);
            throw new HttpStatusCodeException(HttpStatusCode.NotFound,
                $"Employee with Id {request.EmployeeId} not found in favorite list");
        }


        _dbContext.FavoriteEmployeeEntities.Remove(favoriteEmployee);

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Employee with Id {id} deleted successfully from favorite list", request.EmployeeId);
        

        return true;
    }
}