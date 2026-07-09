using FluentValidation.TestHelper;
using QUserService.Application.Requests;
using QUserService.Application.Validators.AuthValidators;

namespace UserService.UnitTest.UserService.Application.Tests.ValidatorTests.AuthValidatorTests;

public class VerifyEmailRequestValidatorTests
{
    private readonly VerifyEmailRequestValidator _validator;

    public VerifyEmailRequestValidatorTests()
    {
        _validator = new VerifyEmailRequestValidator();
    }
    
     [Fact]
    public async Task Validator_When_Commands_Valid_Should_Not_HaveValidationError()
    {
        //Arrange
        var command = new VerifyEmailRequest{
           EmailAddress = "test@gmail.com", 
           Code = "666666"};

        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
    
    [Fact]
    public async Task Validator_When_Email_Is_Empty_ShouldHaveValidationError()
    {
        //Arrange
        var command = new VerifyEmailRequest{
            EmailAddress = "", 
            Code = "666666"};
        
        
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
        var command = new VerifyEmailRequest{
            EmailAddress = "testGmail.com", 
            Code = "666666"};
        
        
        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldHaveValidationErrorFor(s => s.EmailAddress)
            .WithErrorMessage("Invalid email address format.");
    }
    
    
    [Fact]
    public async Task Validator_When_Code_Is_Empty_ShouldHaveValidationError()
    {
        //Arrange
        var command = new VerifyEmailRequest{
            EmailAddress = "test@gmail.com", 
            Code = ""};

        
        
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
        var command = new VerifyEmailRequest{
            EmailAddress = "test@gmail.com", 
            Code = "6666"};

        
        
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
        var command = new VerifyEmailRequest{
            EmailAddress = "test@gmail.com", 
            Code = "6666AA"};

        
        
        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldHaveValidationErrorFor(s => s.Code)
            .WithErrorMessage("Code must contain only digits.");
    }
}