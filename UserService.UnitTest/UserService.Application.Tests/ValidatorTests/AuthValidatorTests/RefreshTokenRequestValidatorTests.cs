using FluentValidation.TestHelper;
using QUserService.Application.Requests;
using QUserService.Application.Validators.AuthValidators;

namespace UserService.UnitTest.UserService.Application.Tests.ValidatorTests.AuthValidatorTests;

public class RefreshTokenRequestValidatorTests
{
    private readonly RefreshTokenRequestValidator _validator;

    public RefreshTokenRequestValidatorTests()
    {
        _validator = new RefreshTokenRequestValidator();
    }
    
    [Fact]
    public async Task Validator_When_Commands_Valid_Should_Not_HaveValidationError()
    {
        //Arrange
        var command = new RefreshTokenRequest
        {
            RefreshToken = "123456789012345678901234567890"
        };

        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
    
    [Fact]
    public async Task Validator_When_Token_Is_Empty_ShouldHaveValidationError()
    {
        //Arrange
        var command = new RefreshTokenRequest
        {
            RefreshToken = ""
        };

        
        
        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldHaveValidationErrorFor(s => s.RefreshToken)
            .WithErrorMessage("Refresh token is required.");
    }
    
    [Fact]
    public async Task Validator_When_Token_Is_Shorter_Than_20_ShouldHaveValidationError()
    {
        //Arrange
        var command = new RefreshTokenRequest
        {
            RefreshToken = "123456789012345678"
        };

        
        
        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldHaveValidationErrorFor(s => s.RefreshToken)
            .WithErrorMessage("Invalid refresh token format.");
    }
}