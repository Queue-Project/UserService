using QUserService.Domain.Enums;
using QUserService.Domain.Models;

namespace UserService.UnitTest.UserService.Application.Tests.Infrastructure;

public class TestDataSeeder
{
    public static CustomerEntity CreateCustomer()
    {
        return new CustomerEntity()
        {
            Id = 1,
            FirstName = "Test Firstname",
            LastName = "Test Lastname",
            PhoneNumber = "+992000000000",
            CreatedAt = DateTime.UtcNow
        };
    }

    public static List<CustomerEntity> CreateCustomers()
    {
        return new List<CustomerEntity>
        {

            new CustomerEntity()
            {
                Id = 1,
                FirstName = "Test Firstname",
                LastName = "Test Lastname",
                PhoneNumber = "+992000000000",
                CreatedAt = DateTime.UtcNow
            },
            new CustomerEntity()
            {
                Id = 2,
                FirstName = "Test Firstname2",
                LastName = "Test Lastname2",
                PhoneNumber = "+992000000002",
                CreatedAt = DateTime.UtcNow
            },
            new CustomerEntity()
            {
                Id = 3,
                FirstName = "Test Firstname3",
                LastName = "Test Lastname3",
                PhoneNumber = "+992000000003",
                CreatedAt = DateTime.UtcNow
            }
        };
    }

    public static CustomerEntity CreateCustomerWithProfileInformation()
    {
        return new CustomerEntity
        {
            Id = 1,
            FirstName = "Test Firstname",
            LastName = "Test Lastname",
            PhoneNumber = "+992000000000",
            DateOfBirth = new DateTime(2006, 06, 02),
            Gender = "Male",
            Country = "Test Country",
            City = "Test City",
            Address = "Test Address",
            PostalCode = "625610",
            UpdatedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
    }


    public static UserEntity CreateUserCustomer()
    {
        return new UserEntity
        {
            Id = 1,
            CustomerId = 1,
            EmailAddress = "test@gmail.com",
            PasswordHash = "Password-hash-1234",
            Roles = UserRoles.Customer,
            IsEmailVerified = true,
            EmailVerificationCode = null,
            EmailVerificationCodeExpires = null,
            VerifiedAt = null,
            ResendCount = 0,
            PasswordResetCode = null,
            PasswordResetExpiry = null,
            EmployeeId = null,
            CreatedAt = DateTime.UtcNow
        };
    }

    
}