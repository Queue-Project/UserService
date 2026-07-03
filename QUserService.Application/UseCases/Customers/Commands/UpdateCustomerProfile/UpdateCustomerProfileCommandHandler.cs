using System.Net;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QUserService.Application.Exceptions;
using QUserService.Application.Interfaces;
using QUserService.Application.Responses;
using QUserService.Contracts.Events.CustomerEvent;

namespace QUserService.Application.UseCases.Customers.Commands.UpdateCustomerProfile;

public class UpdateCustomerProfileCommandHandler : IRequestHandler<UpdateCustomerProfileCommand, CustomerProfileResponse>
{
    private readonly ILogger<UpdateCustomerProfileCommandHandler> _logger;
    private readonly IUserServiceApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPublishEndpoint _publishEndpoint;

    public UpdateCustomerProfileCommandHandler(ILogger<UpdateCustomerProfileCommandHandler> logger,
        IUserServiceApplicationDbContext dbContext, ICurrentUserService currentUserService, IPublishEndpoint publishEndpoint )
    {
        _logger = logger;
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<CustomerProfileResponse> Handle(UpdateCustomerProfileCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating customer profile");

        if (await _dbContext.Customer.FirstOrDefaultAsync(s=>s.PhoneNumber == request.PhoneNumber, cancellationToken) != null)
        {
            _logger.LogWarning("Phone number is already exists.");
            throw new HttpStatusCodeException(HttpStatusCode.BadRequest, "Phone number already exists");
        }
        var currentCustomer = await _currentUserService.GetCurrentCustomerAsync(_dbContext, cancellationToken);

        var user = await _dbContext.Users.FirstOrDefaultAsync(s => s.CustomerId == currentCustomer.Id,
            cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("User with customer Id: {customerId} not found", currentCustomer.Id);
            throw new HttpStatusCodeException(HttpStatusCode.NotFound,
                $"User with CustomerId {currentCustomer.Id} not found");
        }

        currentCustomer.FirstName = request.FirstName;
        currentCustomer.LastName = request.LastName;
        currentCustomer.PhoneNumber = request.PhoneNumber;
        currentCustomer.DateOfBirth = request.DateOfBirth;
        currentCustomer.Gender = request.Gender;
        currentCustomer.Country = request.Country;
        currentCustomer.City = request.City;
        currentCustomer.Address = request.Address;
        currentCustomer.PostalCode = request.PostalCode;
        currentCustomer.UpdatedAt = DateTimeOffset.UtcNow;


        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Customer Profile with Id {id} updated successfully.", currentCustomer.Id);

        await _publishEndpoint.Publish(new CustomerUpdatedEvent
        {
            OccuredAt = DateTimeOffset.UtcNow,
            CustomerId = currentCustomer.Id,
            FirstName = currentCustomer.FirstName,
            LastName = currentCustomer.LastName,
            PhoneNumber = currentCustomer.PhoneNumber
        }, cancellationToken);


        var response = new CustomerProfileResponse
        {
            Id = currentCustomer.Id,
            EmailAddress = user.EmailAddress,
            FirstName = currentCustomer.FirstName,
            LastName = currentCustomer.LastName,
            PhoneNumber = currentCustomer.PhoneNumber,
            DateOfBirth = currentCustomer.DateOfBirth,
            Gender = currentCustomer.Gender,
            Country = currentCustomer.Country,
            City = currentCustomer.City,
            Address = currentCustomer.Address,
            PostalCode = currentCustomer.PostalCode,
            CreatedAt = currentCustomer.CreatedAt,
            UpdatedAt = currentCustomer.UpdatedAt
        };

        return response;
    }
}