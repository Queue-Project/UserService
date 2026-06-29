using FluentValidation.TestHelper;
using QUserService.Application.UseCases.Auth.Queries.Login;
using QUserService.Application.Validators.AuthValidators;

namespace UserService.UnitTest.UserService.Application.Tests.ValidatorTests.AuthValidatorTests;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator;

    public LoginRequestValidatorTests()
    {
        _validator = new LoginRequestValidator();
    }
    
    [Fact]
    public async Task Validator_When_Commands_Valid_Should_Not_HaveValidationError()
    {
        //Arrange
        var command = new LoginQuery("test@gmail.com", "Test.1234");

        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
    
    [Fact]
    public async Task Validator_When_Email_Is_Empty_ShouldHaveValidationError()
    {
        //Arrange
        var command = new LoginQuery("", "Test.1234");

        
        
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
        var command = new LoginQuery("testGmail.com", "Test.1234");

        
        
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
        var command = new LoginQuery("test@gmail.com", "");

        
        
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
        var command = new LoginQuery("test@gmail.com", "Te1234");

        
        
        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldHaveValidationErrorFor(s => s.Password)
            .WithErrorMessage("Password must be at least 8 characters.");
    }
}