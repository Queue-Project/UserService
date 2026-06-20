using FluentValidation.TestHelper;
using QUserService.Application.Requests.EmployeeRequest;
using QUserService.Application.Validators.EmployeeValidators;

namespace UserService.UnitTest.UserService.Application.Tests.ValidatorTests.EmployeeValidatorTests;

public class UpdateEmployeeRequestValidatorTests
{
    private readonly UpdateEmployeeRequestValidator _validator;

    public UpdateEmployeeRequestValidatorTests()
    {
        _validator = new UpdateEmployeeRequestValidator();
    }

    [Fact]
    public async Task Validator_When_Commands_Valid_Should_Not_HaveValidationError()
    {
        //Arrange
        var command = new UpdateEmployeeRequest
        {
            FirstName = "Test Firstname", LastName = "Test Lastname", Position = "Developer" , PhoneNumber = "+992923324252"
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
        var command = new UpdateEmployeeRequest
        {
            FirstName = "", LastName = "Test Lastname", Position = "Developer" , PhoneNumber = "+992923324252"
        };
        
        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldHaveValidationErrorFor(s => s.FirstName)
            .WithErrorMessage("Firstname is required.");
    }
    
    [Fact]
    public async Task Validator_When_Lastname_Is_Empty_ShouldHaveValidation_Error()
    {
        //Arrange
        var command = new UpdateEmployeeRequest
        {
            FirstName = "Test Firstname", LastName = "", Position = "Developer" , PhoneNumber = "+992923324252"
        };
        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldHaveValidationErrorFor(s => s.LastName)
            .WithErrorMessage("Lastname is required");
    }
    
    [Fact]
    public async Task Validator_When_Position_Is_Empty_ShouldHaveValidation_Error()
    {
        //Arrange
        var command = new UpdateEmployeeRequest
        {
            FirstName = "Test Firstname", LastName = "Test Lastname", Position = "" , PhoneNumber = "+992923324252"
        };
        
        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldHaveValidationErrorFor(s => s.Position)
            .WithErrorMessage("Position is required");
    }
    
    [Fact]
    public async Task Validator_When_PhoneNumber_Is_Empty_Correct_ShouldHaveValidationError()
    {
        //Arrange
        var command = new UpdateEmployeeRequest
        {
            FirstName = "Test Firstname", LastName = "Test Lastname", Position = "Developer" , PhoneNumber = ""
        };

        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldHaveValidationErrorFor(s => s.PhoneNumber)
            .WithErrorMessage("PhoneNumber is required");
    }
    
    [Fact]
    public async Task Validator_When_PhoneNumber_Is_Not_Correct_ShouldHaveValidationError()
    {
        //Arrange
        var command = new UpdateEmployeeRequest
        {
            FirstName = "Test Firstname", LastName = "Test Lastname", Position = "Developer" , PhoneNumber = "A92923324252"
        };

        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldHaveValidationErrorFor(s => s.PhoneNumber)
            .WithErrorMessage("PhoneNumber is invalid");
    }
    
}