using System.Net;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using QAuditLogService.Contracts.AuditEvents;
using QUserService.Application.Exceptions;
using QUserService.Application.Helpers;
using QUserService.Application.Interfaces;
using QUserService.Domain.Models;

namespace QUserService.Application.UseCases.Auth.Commands.UpdateUserPassword;

public class UpdateUserPasswordCommandHandler : IRequestHandler<UpdateUserPasswordCommand, bool>
{
    private readonly ILogger<UpdateUserPasswordCommandHandler> _logger;
    private readonly IUserServiceApplicationDbContext _dbContext;
    private readonly ICurrentUserService _contextAccessor;
    private readonly IPasswordHasher<UserEntity> _passwordHasher;
    private readonly IPublishEndpoint _publishEndpoint;

    public UpdateUserPasswordCommandHandler(ILogger<UpdateUserPasswordCommandHandler> logger,
        IUserServiceApplicationDbContext dbContext, ICurrentUserService contextAccessor,
        IPasswordHasher<UserEntity> passwordHasher, IPublishEndpoint publishEndpoint)
    {
        _logger = logger;
        _dbContext = dbContext;
        _contextAccessor = contextAccessor;
        _passwordHasher = passwordHasher;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<bool> Handle(UpdateUserPasswordCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating user password");
        var currentUser = await _contextAccessor.GetCurrentUserAsync(_dbContext, cancellationToken);

        var currentPassword =
            _passwordHasher.VerifyHashedPassword(currentUser, currentUser.PasswordHash, request.OldPassword);
        if (currentPassword == PasswordVerificationResult.Failed)
        {
            _logger.LogWarning("Current Password is incorrect");
            throw new HttpStatusCodeException(HttpStatusCode.BadRequest, "Current Password is incorrect");
        }

        currentUser.PasswordHash = _passwordHasher.HashPassword(currentUser, request.NewPassword);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var entry = _dbContext.Entry(currentUser);
        var changes = AuditHelper.GetChanges(entry);
        
        await _publishEndpoint.Publish(new AuditEvent
        {
            OccuredAt = DateTime.UtcNow,
            UserId = currentUser.Id,
            UserName = "",
            EntityId = currentUser.Id,
            EntityName = nameof(UserEntity),
            ServiceName = "UserService",
            Action = "update.password",
            AuditLogDetails = changes
        }, cancellationToken);
        return true;
    }
}