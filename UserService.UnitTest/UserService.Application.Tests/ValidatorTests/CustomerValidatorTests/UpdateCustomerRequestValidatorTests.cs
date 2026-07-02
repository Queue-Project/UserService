using FluentValidation.TestHelper;
using QUserService.Application.Requests.CustomerRequest;
using QUserService.Application.UseCases.Customers.Commands.CreateCustomer;
using QUserService.Application.Validators.CustomerValidators;

namespace UserService.UnitTest.UserService.Application.Tests.ValidatorTests.CustomerValidatorTests;

public class UpdateCustomerRequestValidatorTests
{
    private readonly UpdateCustomerRequestValidator _validator;

    public UpdateCustomerRequestValidatorTests()
    {
        _validator = new UpdateCustomerRequestValidator();
    }

    [Fact]
    public async Task Validator_When_Commands_Valid_Should_Not_HaveValidationError()
    {
        //Arrange
        var command = new UpdateCustomerRequest
        {
            FirstName = "Test Firstname",
            LastName = "Test Lastname",
            PhoneNumber = "+992923324252"
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
        
        var command = new UpdateCustomerRequest
        {
            FirstName = "",
            LastName = "Test Lastname",
            PhoneNumber = "+992923324252"
        };
        
        //Act
        var result = _validator.TestValidate(command);

        //Assert
        result.ShouldHaveValidationErrorFor(s=>s.FirstName)
            .WithErrorMessage("Firstname is required.");
    }

    [Fact]
    public async Task Validator_When_Lastname_Is_Empty_ShouldHaveValidation_Error()
    {
        //Arrange
        var command = new UpdateCustomerRequest
        {
            FirstName = "Test Firstname",
            LastName = "",
            PhoneNumber = "+992923324252"
        };

        //Act
        var result = _validator.TestValidate(command);

        //Assert
        result.ShouldHaveValidationErrorFor(s => s.LastName)
            .WithErrorMessage("Lastname is required");
    }

    [Fact]
    public async Task Validator_When_PhoneNumber_Is_Empty_Correct_ShouldHaveValidationError()
    {
        //Arrange
        var command = new UpdateCustomerRequest
        {
            FirstName = "Test Firstname",
            LastName = "Test Lastname",
            PhoneNumber = ""
        };

        
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
        var command = new UpdateCustomerRequest
        {
            FirstName = "Test Firstname",
            LastName = "Test Lastname",
            PhoneNumber = "A92923324252"
        };


        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldHaveValidationErrorFor(s => s.PhoneNumber)
            .WithErrorMessage("PhoneNumber is invalid");
    }
}