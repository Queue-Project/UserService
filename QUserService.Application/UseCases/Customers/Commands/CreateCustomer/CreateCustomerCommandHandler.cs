using System.Net;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QUserService.Application.Exceptions;
using QUserService.Application.Interfaces;
using QUserService.Application.Responses;
using QUserService.Contracts.Events.CustomerEvent;
using QUserService.Domain.Models;

namespace QUserService.Application.UseCases.Customers.Commands.CreateCustomer;

public class CreateCustomerCommandHandler: IRequestHandler<CreateCustomerCommand, CustomerResponseModel>
{
    private readonly ILogger<CreateCustomerCommandHandler> _logger;
    private readonly IUserServiceApplicationDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreateCustomerCommandHandler(ILogger<CreateCustomerCommandHandler> logger, IUserServiceApplicationDbContext dbContext, IPublishEndpoint publishEndpoint)
    {
        _logger = logger;
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<CustomerResponseModel> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Adding new customer with name {request.FirstName}", request.Firstname);

        if (await _dbContext.Customer.FirstOrDefaultAsync(s=>s.PhoneNumber == request.PhoneNumber, cancellationToken) != null)
        {
            _logger.LogWarning("Phone number is already exists.");
            throw new HttpStatusCodeException(HttpStatusCode.BadRequest, "Phone number already exists");
        }
        var customer = new CustomerEntity()
        {
            FirstName = request.Firstname,
            LastName = request.Lastname,
            PhoneNumber = request.PhoneNumber,
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.Customer.AddAsync(customer, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Customer {customer.FirstName} added successfully with Id {customer.Id}",
            customer.FirstName, customer.Id);

        await _publishEndpoint.Publish(new CustomerCreatedEvent
        {
            OccuredAt = DateTimeOffset.UtcNow,
            CustomerId = customer.Id,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            PhoneNumber = customer.PhoneNumber
        }, cancellationToken);
        
        var response = new CustomerResponseModel()
        {
            Id = customer.Id,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            PhoneNumber = customer.PhoneNumber,
        };

        return response;
    }
}