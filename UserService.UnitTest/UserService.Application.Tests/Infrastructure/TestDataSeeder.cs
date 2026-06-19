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


    public static EmployeeEntity CreateEmployeeCompanyAdmin()
    {
        return new EmployeeEntity
        {
            Id = 1,
            CompanyId = 1,
            BranchId = null,
            ServiceId = null,
            FirstName = "Test Firstname",
            LastName = "Test Lastname",
            PhoneNumber = "+992923324252",
            Position = "CompanyAdmin",
            CreatedAt = DateTime.UtcNow
        };
    }
    
    public static EmployeeEntity CreateEmployee()
    {
        return new EmployeeEntity
        {
            Id = 1,
            CompanyId = 1,
            BranchId = 1,
            ServiceId = 1,
            FirstName = "Test Firstname",
            LastName = "Test Lastname",
            PhoneNumber = "+992923324212",
            Position = "Barber",
            CreatedAt = DateTime.UtcNow
        };
    }

    public static List<EmployeeEntity> CreateEmployees()
    {
        return new List<EmployeeEntity>
        {
            new EmployeeEntity
            {
                Id = 1,
                CompanyId = 1,
                BranchId = 1,
                ServiceId = 1,
                FirstName = "Test Firstname",
                LastName = "Test Lastname",
                PhoneNumber = "+992923324212",
                Position = "Barber",
                CreatedAt = DateTime.UtcNow
            },
            new EmployeeEntity
            {
                Id = 2,
                CompanyId = 1,
                BranchId = 1,
                ServiceId = 1,
                FirstName = "Test Firstname2",
                LastName = "Test Lastname2",
                PhoneNumber = "+992923324233",
                Position = "Barber2",
                CreatedAt = DateTime.UtcNow
            },
            new EmployeeEntity
            {
                Id = 3,
                CompanyId = 1,
                BranchId = 1,
                ServiceId = 1,
                FirstName = "Test Firstname2",
                LastName = "Test Lastname2",
                PhoneNumber = "+992923324244",
                Position = "Barber2",
                CreatedAt = DateTime.UtcNow
            }
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

    public static UserEntity CreateUserCompanyAdmin()
    {
        return new UserEntity
        {
            Id = 1,
            CustomerId = null,
            EmailAddress = "test@gmail.com",
            PasswordHash = "Password-hash-1234",
            Roles = UserRoles.CompanyAdmin,
            IsEmailVerified = true,
            EmailVerificationCode = null,
            EmailVerificationCodeExpires = null,
            VerifiedAt = null,
            ResendCount = 0,
            PasswordResetCode = null,
            PasswordResetExpiry = null,
            EmployeeId = 1,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static UserEntity CreateUserEmployeeRole()
    {
        return new UserEntity
        {
            Id = 1,
            CustomerId = null,
            EmailAddress = "test@gmail.com",
            PasswordHash = "Password-hash-1234",
            Roles = UserRoles.Employee,
            IsEmailVerified = true,
            EmailVerificationCode = null,
            EmailVerificationCodeExpires = null,
            VerifiedAt = null,
            ResendCount = 0,
            PasswordResetCode = null,
            PasswordResetExpiry = null,
            EmployeeId = 1,
            CreatedAt = DateTime.UtcNow
        };
    }


    public static BlockedCustomerEntity CreateBlockedCustomer()
    {
        return new BlockedCustomerEntity
        {
            Id = 1,
            CompanyId = 1,
            CustomerId = 1,
            Reason = "Did not come 3 times",
            BannedUntil = DateTime.UtcNow.AddDays(20),
            DoesBanForever = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static List<BlockedCustomerEntity> CreateBlockedCustomers()
    {
        return new List<BlockedCustomerEntity>
        {
            new BlockedCustomerEntity
            {
                Id = 1,
                CompanyId = 1,
                CustomerId = 1,
                Reason = "Did not come 3 times",
                BannedUntil = DateTime.UtcNow.AddDays(20),
                DoesBanForever = false,
                CreatedAt = DateTime.UtcNow
            },
            new BlockedCustomerEntity
            {
                Id = 2,
                CompanyId = 1,
                CustomerId = 1,
                Reason = "Did not come 3 times",
                BannedUntil = DateTime.UtcNow.AddDays(22),
                DoesBanForever = false,
                CreatedAt = DateTime.UtcNow
            },
            new BlockedCustomerEntity
            {
                Id = 3,
                CompanyId = 1,
                CustomerId = 1,
                Reason = "Did not come 3 times",
                BannedUntil = DateTime.UtcNow.AddDays(23),
                DoesBanForever = false,
                CreatedAt = DateTime.UtcNow
            }
        };
    }

    
}