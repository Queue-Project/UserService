using System.Net;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QAuditLogService.Contracts.AuditEvents;
using QUserService.Application.Exceptions;
using QUserService.Application.Interfaces;
using QUserService.Domain.Models;

namespace QUserService.Application.UseCases.Auth.Commands.VerifyAccount;

public class VerifyEmailCommandHandler: IRequestHandler<VerifyEmailCommand, bool>
{
    private readonly ILogger<VerifyEmailCommandHandler> _logger;
    private readonly IUserServiceApplicationDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public VerifyEmailCommandHandler(ILogger<VerifyEmailCommandHandler> logger, IUserServiceApplicationDbContext dbContext, IPublishEndpoint publishEndpoint)
    {
        _logger = logger;
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<bool> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(s => s.EmailAddress == request.EmailAddress, cancellationToken);

        if (user==null)
        {
            _logger.LogWarning("User with this email {email} not found", request.EmailAddress);
            throw new HttpStatusCodeException(HttpStatusCode.NotFound,
                $"User with this email {request.EmailAddress} not found");
        }

        if (user.EmailVerificationCode != request.Code)
        {
            _logger.LogWarning("Invalid code");
            throw new HttpStatusCodeException(HttpStatusCode.BadRequest, "Invalid Code");
        }

        if (user.EmailVerificationCodeExpires < DateTime.UtcNow)
        {
            _logger.LogWarning("Code expired");
            throw new HttpStatusCodeException(HttpStatusCode.BadRequest, "Code expired");
        }

        user.IsEmailVerified = true;
        user.EmailVerificationCode = null;
        user.VerifiedAt = DateTime.UtcNow;
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        await _publishEndpoint.Publish(new AuditEvent
        {
            OccuredAt = DateTime.UtcNow,
            UserId = user.Id,
            UserName = "",
            EntityId = user.Id,
            EntityName = nameof(UserEntity),
            ServiceName = "UserService",
            Action = "verify.email"
        }, cancellationToken);

        return true;
    }
}