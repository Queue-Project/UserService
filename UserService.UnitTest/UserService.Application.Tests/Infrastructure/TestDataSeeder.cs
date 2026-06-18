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

    public static EmployeeEntity CreateEmployee()
    {
        return new EmployeeEntity
        {
            CompanyId = 1,
            BranchId = 1,
            FirstName = "Test Firstname",
            LastName = "Test Lastname",
            Position = "Test Position",
            PhoneNumber = "+992923324252",
            CreatedAt = DateTime.UtcNow
        };
    }
}