using FluentValidation.TestHelper;
using QUserService.Application.Requests;
using QUserService.Application.Validators.AuthValidators;

namespace UserService.UnitTest.UserService.Application.Tests.ValidatorTests.AuthValidatorTests;

public class CreateCompanyAdminRequestValidatorTests
{
    private readonly CreateCompanyAdminRequestValidator _validator;

    public CreateCompanyAdminRequestValidatorTests()
    {
        _validator = new CreateCompanyAdminRequestValidator();
    }

    [Fact]
    public async Task Validator_When_Commands_Valid_Should_Not_HaveValidationError()
    {
        //Arrange
        var command = new CreateCompanyAdminRequest
        {
           CompanyId = 1, 
           FirstName = "Test Firstname", 
           LastName = "Test Lastname", 
           Position = "Developer" , 
           PhoneNumber = "+992923324252", 
           EmailAddress = "test@gmail.com", 
           Password = "Test.1234"
        };

        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldNotHaveAnyValidationErrors();
    }


    [Fact]
    public async Task Validator_When_Firstname_Is_Empty_ShouldHaveValidationError()
    {
        //Arrange
        var command = new CreateCompanyAdminRequest
        {
            CompanyId = 1, 
            FirstName = "", 
            LastName = "Test Lastname", 
            Position = "Developer" , 
            PhoneNumber = "+992923324252", 
            EmailAddress = "test@gmail.com", 
            Password = "Test.1234"
        };
        
        
        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldHaveValidationErrorFor(s => s.FirstName)
            .WithErrorMessage("First name is required.");
    }
    
    [Fact]
    public async Task Validator_When_Lastname_Is_Empty_ShouldHaveValidation_Error()
    {
        //Arrange
        var command = new CreateCompanyAdminRequest
        {
            CompanyId = 1, 
            FirstName = "Test Firstname", 
            LastName = "", 
            Position = "Developer" , 
            PhoneNumber = "+992923324252", 
            EmailAddress = "test@gmail.com", 
            Password = "Test.1234"
        };
        
        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldHaveValidationErrorFor(s => s.LastName)
            .WithErrorMessage("Last name is required.");
    }
    
    [Fact]
    public async Task Validator_When_Position_Is_Empty_ShouldHaveValidation_Error()
    {
        //Arrange
        var command = new CreateCompanyAdminRequest
        {
            CompanyId = 1, 
            FirstName = "Test Firstname", 
            LastName = "Test Lastname", 
            Position = "" , 
            PhoneNumber = "+992923324252", 
            EmailAddress = "test@gmail.com", 
            Password = "Test.1234"
        };
        
        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldHaveValidationErrorFor(s => s.Position)
            .WithErrorMessage("Position is required.");
    }
    
    [Fact]
    public async Task Validator_When_PhoneNumber_Is_Empty_ShouldHaveValidationError()
    {
        //Arrange
        var command = new CreateCompanyAdminRequest
        {
            CompanyId = 1, 
            FirstName = "Test Firstname", 
            LastName = "Test Lastname", 
            Position = "Developer" , 
            PhoneNumber = "", 
            EmailAddress = "test@gmail.com", 
            Password = "Test.1234"
        };

        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldHaveValidationErrorFor(s => s.PhoneNumber)
            .WithErrorMessage("Phone number is required.");
    }
    
    [Fact]
    public async Task Validator_When_PhoneNumber_Is_Not_Correct_ShouldHaveValidationError()
    {
        //Arrange
        var command = new CreateCompanyAdminRequest
        {
            CompanyId = 1, 
            FirstName = "Test Firstname", 
            LastName = "Test Lastname", 
            Position = "Developer" , 
            PhoneNumber = "A92923324252", 
            EmailAddress = "test@gmail.com", 
            Password = "Test.1234"
        };

        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldHaveValidationErrorFor(s => s.PhoneNumber)
            .WithErrorMessage("Phone number is invalid. Use formats: +1234567890, (123) 456-7890, or 123-456-7890");
    }
    
    
    [Fact]
    public async Task Validator_When_CompanyId_Is_Invalid_Number_ShouldHaveValidationError()
    {
        //Arrange
        var command = new CreateCompanyAdminRequest
        {
            CompanyId = -1, 
            FirstName = "Test Firstname", 
            LastName = "Test Lastname", 
            Position = "Developer" , 
            PhoneNumber = "+992923324252", 
            EmailAddress = "test@gmail.com", 
            Password = "Test.1234"
        };
        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldHaveValidationErrorFor(s => s.CompanyId)
            .WithErrorMessage("CompanyId must be a positive number and greater than 0.");
        
    }

    [Fact]
    public async Task Validator_When_Email_Is_Empty_ShouldHaveValidationError()
    {
        //Arrange
        var command = new CreateCompanyAdminRequest
        {
            CompanyId = 1, 
            FirstName = "Test Firstname", 
            LastName = "Test Lastname", 
            Position = "Developer" , 
            PhoneNumber = "+992923324252", 
            EmailAddress = "", 
            Password = "Test.1234"
        };
        
        
        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldHaveValidationErrorFor(s => s.EmailAddress)
            .WithErrorMessage("Email address is required.");
    }
    
    [Fact]
    public async Task Validator_When_Email_Is_Invalid_ShouldHaveValidationError()
    {
        //Arrange
        var command = new CreateCompanyAdminRequest
        {
            CompanyId = 1, 
            FirstName = "Test Firstname", 
            LastName = "Test Lastname", 
            Position = "Developer" , 
            PhoneNumber = "+992923324252", 
            EmailAddress = "testGmail.com", 
            Password = "Test.1234"
        };
        
        
        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldHaveValidationErrorFor(s => s.EmailAddress)
            .WithErrorMessage("Invalid email address format.");
    }
    
    [Fact]
    public async Task Validator_When_Password_Is_Empty_ShouldHaveValidationError()
    {
        //Arrange
        var command = new CreateCompanyAdminRequest
        {
            CompanyId = 1, 
            FirstName = "Test Firstname", 
            LastName = "Test Lastname", 
            Position = "Developer" , 
            PhoneNumber = "+992923324252", 
            EmailAddress = "test@mail.com", 
            Password = ""
        };
        
        
        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldHaveValidationErrorFor(s => s.Password)
            .WithErrorMessage("Password is required.");
    }
    
    [Fact]
    public async Task Validator_When_Password_Is_Shorter_Than_8_ShouldHaveValidationError()
    {
        //Arrange
        var command = new CreateCompanyAdminRequest
        {
            CompanyId = 1, 
            FirstName = "Test Firstname", 
            LastName = "Test Lastname", 
            Position = "Developer" , 
            PhoneNumber = "+992923324252", 
            EmailAddress = "test@mail.com", 
            Password = "test"
        };
        
        
        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldHaveValidationErrorFor(s => s.Password)
            .WithErrorMessage("Password must be at least 8 characters.");
    }
}