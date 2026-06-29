using FluentValidation.TestHelper;
using QUserService.Application.UseCases.Auth.Commands.RegisterCustomer;
using QUserService.Application.Validators.AuthValidators;

namespace UserService.UnitTest.UserService.Application.Tests.ValidatorTests.AuthValidatorTests;

public class RegisterCustomerRequestValidatorTests
{
    private readonly RegisterCustomerRequestValidator _validatorTests;

    public RegisterCustomerRequestValidatorTests()
    {
        _validatorTests = new RegisterCustomerRequestValidator();
    }

    [Fact]
    public async Task Validator_When_Commands_Valid_Should_Not_HaveValidationError()
    {
        //Arrange
        var command = new RegisterCustomerCommand
        (
            "test@gmail.com",
            "Test.1234",
            "Test Firstname",
            "Test Lastname",
            "+992923324252"
        );

        //Act
        var result = _validatorTests.TestValidate(command);

        //Assert
        result.ShouldNotHaveAnyValidationErrors();
    }


    [Fact]
    public async Task Validator_When_Firstname_Is_Empty_ShouldHaveValidationError()
    {
        //Arrange
        var command = new RegisterCustomerCommand
        (
            "test@gmail.com",
            "Test.1234",
            "",
            "Test Lastname",
            "+992923324252"
        );


        //Act
        var result = _validatorTests.TestValidate(command);

        //Assert
        result.ShouldHaveValidationErrorFor(s => s.FirstName)
            .WithErrorMessage("Firstname is required.");
    }

    [Fact]
    public async Task Validator_When_Lastname_Is_Empty_ShouldHaveValidation_Error()
    {
        //Arrange
        var command = new RegisterCustomerCommand
        (
            "test@gmail.com",
            "Test.1234",
            "Test Firstname",
            "",
            "+992923324252"
        );

        //Act
        var result = _validatorTests.TestValidate(command);

        //Assert
        result.ShouldHaveValidationErrorFor(s => s.LastName)
            .WithErrorMessage("Lastname is required");
    }
    

    [Fact]
    public async Task Validator_When_PhoneNumber_Is_Empty_ShouldHaveValidationError()
    {
        //Arrange
        var command = new RegisterCustomerCommand
        (
            "test@gmail.com",
            "Test.1234",
            "Test Firstname",
            "Test Lastname",
            ""
        );

        //Act
        var result = _validatorTests.TestValidate(command);

        //Assert
        result.ShouldHaveValidationErrorFor(s => s.PhoneNumber)
            .WithErrorMessage("PhoneNumber is required");
    }

    [Fact]
    public async Task Validator_When_PhoneNumber_Is_Not_Correct_ShouldHaveValidationError()
    {
        //Arrange
        var command = new RegisterCustomerCommand
        (
            "test@gmail.com",
            "Test.1234",
            "Test Firstname",
            "Test Lastname",
            "A92923324252"
        );
        //Act
        var result = _validatorTests.TestValidate(command);

        //Assert
        result.ShouldHaveValidationErrorFor(s => s.PhoneNumber)
            .WithErrorMessage("PhoneNumber is invalid");
    }
    

    [Fact]
    public async Task Validator_When_Email_Is_Empty_ShouldHaveValidationError()
    {
        //Arrange
        var command = new RegisterCustomerCommand
        (
            "",
            "Test.1234",
            "Test Firstname",
            "Test Lastname",
            "+992923324252"
        );


        //Act
        var result = _validatorTests.TestValidate(command);

        //Assert
        result.ShouldHaveValidationErrorFor(s => s.EmailAddress)
            .WithErrorMessage("EmailAddress is required");
    }

    [Fact]
    public async Task Validator_When_Email_Is_Invalid_ShouldHaveValidationError()
    {
        //Arrange
        var command = new RegisterCustomerCommand
        (
            "testGmail.com",
            "Test.1234",
            "Test Firstname",
            "Test Lastname",
            "+992923324252"
        );


        //Act
        var result = _validatorTests.TestValidate(command);

        //Assert
        result.ShouldHaveValidationErrorFor(s => s.EmailAddress)
            .WithErrorMessage("EmailAddress is not valid");
    }

    [Fact]
    public async Task Validator_When_Password_Is_Empty_ShouldHaveValidationError()
    {
        //Arrange
        var command = new RegisterCustomerCommand
        (
            "test@gmail.com",
            "",
            "Test Firstname",
            "Test Lastname",
            "+992923324252"
        );


        //Act
        var result = _validatorTests.TestValidate(command);

        //Assert
        result.ShouldHaveValidationErrorFor(s => s.Password)
            .WithErrorMessage("Password is required");
    }

    [Fact]
    public async Task Validator_When_Password_Is_Shorter_Than_8_ShouldHaveValidationError()
    {
        //Arrange
        var command = new RegisterCustomerCommand
        (
            "test@gmail.com",
            "Test",
            "Test Firstname",
            "Test Lastname",
            "+992923324252"
        );


        //Act
        var result = _validatorTests.TestValidate(command);

        //Assert
        result.ShouldHaveValidationErrorFor(s => s.Password)
            .WithErrorMessage("Password must be at least 8 characters");
    }
}