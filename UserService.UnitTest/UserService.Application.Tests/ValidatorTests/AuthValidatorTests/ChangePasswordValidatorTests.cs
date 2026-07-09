using FluentValidation.TestHelper;
using QUserService.Application.Requests;
using QUserService.Application.Validators.AuthValidators;

namespace UserService.UnitTest.UserService.Application.Tests.ValidatorTests.AuthValidatorTests;

public class ChangePasswordValidatorTests
{
    private readonly ChangePasswordValidator _validator;

    public ChangePasswordValidatorTests()
    {
        _validator = new ChangePasswordValidator();
    }
    
    [Fact]
    public async Task Validator_When_Commands_Valid_Should_Not_HaveValidationError()
    {
        //Arrange
        var command = new ChangePasswordRequest
        {
            NewPassword = "TestNewPassword.1234",
            OldPassword = "TestOldPassword.1234"
        };

        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
    
    [Fact]
    public async Task Validator_When_NewPassword_Is_Empty_ShouldHaveValidationError()
    {
        //Arrange
        var command = new ChangePasswordRequest
        {
            NewPassword = "",
            OldPassword = "TestOldPassword.1234"
        };
        
        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldHaveValidationErrorFor(s => s.NewPassword)
            .WithErrorMessage("Password is required");
    }
    
    [Fact]
    public async Task Validator_When_OldPassword_Is_Empty_ShouldHaveValidationError()
    {
        //Arrange
        var command = new ChangePasswordRequest
        {
            NewPassword = "TestNewPassword.1234",
            OldPassword = ""
        };
        
        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldHaveValidationErrorFor(s => s.OldPassword)
            .WithErrorMessage("Password is required");
    }
    
    [Fact]
    public async Task Validator_When_NewPassword_Is_Shorter_Than_8_ShouldHaveValidationError()
    {
        //Arrange
        var command = new ChangePasswordRequest
        {
            NewPassword = "Te.1234",
            OldPassword = "TestOldPassword.1234"
        };
        
        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldHaveValidationErrorFor(s => s.NewPassword)
            .WithErrorMessage("Password must be at least 8 characters");
    }
    
    
    [Fact]
    public async Task Validator_When_OldPassword_Is_Shorter_Than_8_ShouldHaveValidationError()
    {
        //Arrange
        var command = new ChangePasswordRequest
        {
            NewPassword = "TestNewPassword.1234",
            OldPassword = "Te.1234"
        };
        
        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldHaveValidationErrorFor(s => s.OldPassword)
            .WithErrorMessage("Password must be at least 8 characters");
    }
}