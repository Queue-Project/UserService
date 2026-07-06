using System.Net;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QAuditLogService.Contracts.AuditEvents;
using QUserService.Application.Exceptions;
using QUserService.Application.Helpers;
using QUserService.Application.Interfaces;
using QUserService.Domain.Models;

namespace QUserService.Application.UseCases.Auth.Commands.ResetPassword;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, bool>
{
    private readonly ILogger<ResetPasswordCommandHandler> _logger;
    private readonly IUserServiceApplicationDbContext _dbContext;
    private readonly IPasswordHasher<UserEntity> _hasher;
    private readonly IPublishEndpoint _publishEndpoint;

    public ResetPasswordCommandHandler(ILogger<ResetPasswordCommandHandler> logger,
        IUserServiceApplicationDbContext dbContext, IPasswordHasher<UserEntity> hasher, IPublishEndpoint publishEndpoint)
    {
        _logger = logger;
        _dbContext = dbContext;
        _hasher = hasher;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<bool> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Resetting password for email {Email}",
            request.EmailAddress);

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(x => x.EmailAddress == request.EmailAddress, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning(
                "Password reset failed. User not found");

            throw new HttpStatusCodeException(
                HttpStatusCode.NotFound,
                $"User with {request.EmailAddress} not found");
        }


        if (user.PasswordResetCode == null ||
            user.PasswordResetExpiry == null)
        {
            _logger.LogWarning(
                "Reset code does not exist for user {Email}",
                request.EmailAddress);

            throw new HttpStatusCodeException(
                HttpStatusCode.BadRequest,
                "Reset code not found");
        }


        if (user.PasswordResetCode != request.Code)
        {
            _logger.LogWarning(
                "Invalid reset code for user {Email}",
                request.EmailAddress);

            throw new HttpStatusCodeException(
                HttpStatusCode.BadRequest,
                "Invalid reset code");
        }

        if (user.PasswordResetExpiry < DateTime.UtcNow)
        {
            _logger.LogWarning(
                "Reset code expired for user {Email}",
                request.EmailAddress);

            throw new HttpStatusCodeException(
                HttpStatusCode.BadRequest,
                "Reset code expired");
        }


        if (request.NewPassword != request.ConfirmPassword)
        {
            _logger.LogWarning(
                "Password confirmation failed for user {Email}",
                request.EmailAddress);

            throw new HttpStatusCodeException(
                HttpStatusCode.BadRequest,
                "Passwords do not match");
        }


        user.PasswordHash = _hasher.HashPassword(user, request.NewPassword);

        user.PasswordResetCode = null;
        user.PasswordResetExpiry = null;

        var refreshTokens = await _dbContext.RefreshTokens
            .Where(x => x.UserId == user.Id &&
                        x.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var token in refreshTokens)
        {
            token.RevokedAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var entry = _dbContext.Entry(user);
        var changes = AuditHelper.GetChanges(entry);
        
        await _publishEndpoint.Publish(new AuditEvent
        {
            OccuredAt = DateTime.UtcNow,
            UserId = user.Id,
            UserName = "",
            EntityId = user.Id,
            EntityName = nameof(UserEntity),
            ServiceName = "UserService",
            Action = "reset.password",
            AuditLogDetails = changes
        }, cancellationToken);

        _logger.LogInformation(
            "Password reset successfully for user {Email}",
            request.EmailAddress);
        return true;
    }
}