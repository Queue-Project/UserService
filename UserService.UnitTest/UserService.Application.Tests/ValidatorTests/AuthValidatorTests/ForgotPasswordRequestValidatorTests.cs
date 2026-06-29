using FluentValidation.TestHelper;
using QUserService.Application.Requests;
using QUserService.Application.Validators.AuthValidators;

namespace UserService.UnitTest.UserService.Application.Tests.ValidatorTests.AuthValidatorTests;

public class ForgotPasswordRequestValidatorTests
{
    private readonly ForgotPasswordRequestValidator _validator;

    public ForgotPasswordRequestValidatorTests()
    {
        _validator = new ForgotPasswordRequestValidator();
    }
    
    [Fact]
    public async Task Validator_When_Commands_Valid_Should_Not_HaveValidationError()
    {
        //Arrange
        var command = new ForgotPasswordRequest()
        {
            EmailAddress = "test@gmail.com", 
        };

        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
    
    [Fact]
    public async Task Validator_When_Email_Is_Empty_ShouldHaveValidationError()
    {
        //Arrange
        var command = new ForgotPasswordRequest()
        {
            EmailAddress = "", 
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
        var command = new ForgotPasswordRequest()
        {
            EmailAddress = "testGmail.com", 
        };
        
        
        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldHaveValidationErrorFor(s => s.EmailAddress)
            .WithErrorMessage("Invalid email address format.");
    }
}