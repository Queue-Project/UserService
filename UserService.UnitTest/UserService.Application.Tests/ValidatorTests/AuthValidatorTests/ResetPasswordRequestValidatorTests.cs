using FluentValidation.TestHelper;
using QUserService.Application.UseCases.Auth.Commands.ResetPassword;
using QUserService.Application.Validators.AuthValidators;

namespace UserService.UnitTest.UserService.Application.Tests.ValidatorTests.AuthValidatorTests;

public class ResetPasswordRequestValidatorTests
{
    private readonly ResetPasswordRequestValidator _validator;

    public ResetPasswordRequestValidatorTests()
    {
        _validator = new ResetPasswordRequestValidator();
    }
    
    [Fact]
    public async Task Validator_When_Commands_Valid_Should_Not_HaveValidationError()
    {
        //Arrange
        var command = new ResetPasswordCommand(
            "test@gmail.com", 
            "666666", 
            "Test.1234", 
            "Test.1234");

        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
    
    [Fact]
    public async Task Validator_When_Email_Is_Empty_ShouldHaveValidationError()
    {
        //Arrange
        var command = new ResetPasswordCommand(
            "", 
            "666666", 
            "Test.1234", 
            "Test.1234");
        
        
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
        var command = new ResetPasswordCommand(
            "test.com", 
            "666666", 
            "Test.1234", 
            "Test.1234");
        
        
        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldHaveValidationErrorFor(s => s.EmailAddress)
            .WithErrorMessage("Invalid email address format.");
    }
    
    [Fact]
    public async Task Validator_When_NewPassword_Is_Empty_ShouldHaveValidationError()
    {
        //Arrange
        var command = new ResetPasswordCommand(
            "test@gmail.com", 
            "666666", 
            "", 
            "Test.1234");

        
        
        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldHaveValidationErrorFor(s => s.NewPassword)
            .WithErrorMessage("Password is required");
    }
    
    [Fact]
    public async Task Validator_When_NEwPassword_Is_Shorter_Than_8_ShouldHaveValidationError()
    {
        //Arrange
        var command = new ResetPasswordCommand(
            "test@gmail.com", 
            "666666", 
            "Test", 
            "Test.1234");

        
        
        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldHaveValidationErrorFor(s => s.NewPassword)
            .WithErrorMessage("Password must be at least 8 characters");
    }
    
    [Fact]
    public async Task Validator_When_ConfirmPassword_Is_Empty_ShouldHaveValidationError()
    {
        //Arrange
        var command = new ResetPasswordCommand(
            "test@gmail.com", 
            "666666", 
            "Test.1234", 
            "");

        
        
        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldHaveValidationErrorFor(s => s.ConfirmPassword)
            .WithErrorMessage("Password is required");
    }
    
    [Fact]
    public async Task Validator_When_ConfirmPassword_Is_Shorter_Than_8_ShouldHaveValidationError()
    {
        //Arrange
        var command = new ResetPasswordCommand(
            "test@gmail.com", 
            "666666", 
            "Test.1234", 
            "Test");

        
        
        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldHaveValidationErrorFor(s => s.ConfirmPassword)
            .WithErrorMessage("Password must be at least 8 characters");
    }
    
    [Fact]
    public async Task Validator_When_Code_Is_Empty_ShouldHaveValidationError()
    {
        //Arrange
        var command = new ResetPasswordCommand(
            "test@gmail.com", 
            "", 
            "Test.1234", 
            "Test.1234");

        
        
        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldHaveValidationErrorFor(s => s.Code)
            .WithErrorMessage("Code is required");
    }
    
    [Fact]
    public async Task Validator_When_Code_Is_Shorter_Than_6_ShouldHaveValidationError()
    {
        //Arrange
        var command = new ResetPasswordCommand(
            "test@gmail.com", 
            "6666", 
            "Test.1234", 
            "Test.1234");

        
        
        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldHaveValidationErrorFor(s => s.Code)
            .WithErrorMessage("Code must be 6 digit");
    }
    
    [Fact]
    public async Task Validator_When_Code_Contains_Letters_ShouldHaveValidationError()
    {
        //Arrange
        var command = new ResetPasswordCommand(
            "test@gmail.com", 
            "6666AA", 
            "Test.1234", 
            "Test.1234");

        
        
        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldHaveValidationErrorFor(s => s.Code)
            .WithErrorMessage("Code must contain only digits.");
    }
    
}