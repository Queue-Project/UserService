using FluentValidation.TestHelper;
using QUserService.Application.UseCases.Customers.Commands.CreateCustomer;
using QUserService.Application.Validators.CustomerValidators;

namespace UserService.UnitTest.UserService.Application.Tests.ValidatorTests.CustomerValidatorTests;

public class CreateCustomerRequestValidatorTests
{
    private readonly CreateCustomerRequestValidator _validator;

    public CreateCustomerRequestValidatorTests()
    {
        _validator = new CreateCustomerRequestValidator();
    }

    [Fact]
    public async Task Validator_When_Commands_Valid_Should_Not_HaveValidationError()
    {
        //Arrange
        var command = new CreateCustomerCommand("Test Firstname", "Test Lastname", "+992923324252");

        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldNotHaveAnyValidationErrors();
    }


    [Fact]
    public async Task Validator_When_Firstname_Is_Empty_ShouldHaveValidationError()
    {
        //Arrange
        var command = new CreateCustomerCommand("", "Test Lastname", "+992923324252");
        
        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldHaveValidationErrorFor(s => s.Firstname)
            .WithErrorMessage("Firstname is required.");
    }
    
    [Fact]
    public async Task Validator_When_Lastname_Is_Empty_ShouldHaveValidation_Error()
    {
        //Arrange
        var command = new CreateCustomerCommand("Test Firstname", "", "+992923324252");
        
        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldHaveValidationErrorFor(s => s.Lastname)
            .WithErrorMessage("Lastname is required");
    }
    
    [Fact]
    public async Task Validator_When_PhoneNumber_Is_Empty_Correct_ShouldHaveValidationError()
    {
        //Arrange
        var command = new CreateCustomerCommand("Test Firstname", "Test Lastname", "");

        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldHaveValidationErrorFor(s => s.PhoneNumber)
            .WithErrorMessage("PhoneNumber is required");
    }
    
    [Fact]
    public async Task Validator_WhenPhoneNumberIsNotCorrect_ShouldHaveValidationError()
    {
        //Arrange
        var command = new CreateCustomerCommand("Test Firstname", "Test Lastname", "A92923324252");

        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldHaveValidationErrorFor(s => s.PhoneNumber)
            .WithErrorMessage("PhoneNumber is invalid");
    }
    
    
}