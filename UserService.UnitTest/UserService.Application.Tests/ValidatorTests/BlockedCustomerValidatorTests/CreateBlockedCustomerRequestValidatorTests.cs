using FluentValidation.TestHelper;
using QUserService.Application.UseCases.BlockedCustomers.Commands.CreateBlockedCustomer;
using QUserService.Application.Validators.BlockedCustomerValidators;

namespace UserService.UnitTest.UserService.Application.Tests.ValidatorTests.BlockedCustomerValidatorTests;

public class CreateBlockedCustomerRequestValidatorTests
{
    private readonly CreateBlockedCustomerRequestValidator _validator;

    public CreateBlockedCustomerRequestValidatorTests()
    {
        _validator = new CreateBlockedCustomerRequestValidator();
    }

    [Fact]
    public async Task Validator_When_Commands_Valid_Should_Not_HaveValidationError()
    {
        //Arrange
        var command = new CreateBlockedCustomerCommand(1, "Test Reason", DateTime.UtcNow.AddMonths(1), false);

        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldNotHaveAnyValidationErrors();
    }


    [Fact]
    public async Task Validator_When_Reason_Length_Is_Smaller_Than_10_ShouldHaveValidationError()
    {
        //Arrange
        var command = new CreateBlockedCustomerCommand(1, "Reason", DateTime.UtcNow.AddMonths(1), false);

        
        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldHaveValidationErrorFor(s => s.Reason)
            .WithErrorMessage("Blocked reason must be at least 10 characters");
    }
    
    [Fact]
    public async Task Validator_When_BannedUntil_Date_Is_Now_ShouldHaveValidation_Error()
    {
        //Arrange
        var command = new CreateBlockedCustomerCommand(1, "Test Reason", new DateTime(2025,06,20), false);

        
        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldHaveValidationErrorFor(s => s.BannedUntil)
            .WithErrorMessage("Ban and date must be in the future.");
    }
    
    [Fact]
    public async Task Validator_When_BannedUntil_Is_Empty_ShouldHaveValidation_Error()
    {
        //Arrange
        var command = new CreateBlockedCustomerCommand(1, "Test Reason", new DateTime(), false);

        
        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldHaveValidationErrorFor(s => s.BannedUntil)
            .WithErrorMessage("Ban and date is required.");
    }
    
    [Fact]
    public async Task Validator_When_ServiceId_Is_Invalid_Number_ShouldHaveValidationError()
    {
        //Arrange
        var command = new CreateBlockedCustomerCommand(0, "Test Reason", DateTime.UtcNow.AddMonths(1), false);

        
        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldHaveValidationErrorFor(s => s.CustomerId)
            .WithErrorMessage("CustomerId must be greater than 0");
        
    }
    
}