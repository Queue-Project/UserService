using FluentValidation.TestHelper;
using QUserService.Application.Requests.AvailabilityScheduleRequest;
using QUserService.Application.Validators.AvailabilityScheduleValidators;
using QUserService.Domain.Enums;
using QUserService.Domain.Models;

namespace UserService.UnitTest.UserService.Application.Tests.ValidatorTests.AvailabilityScheduleValidatorTests;

public class UpdateAvailabilityScheduleRequestValidatorTests
{
    private readonly UpdateAvailabilityScheduleRequestValidator _validator;

    public UpdateAvailabilityScheduleRequestValidatorTests()
    {
        _validator = new UpdateAvailabilityScheduleRequestValidator();
    }
    
    [Fact]
    public async Task Validator_When_Commands_Valid_Should_Not_HaveValidationError()
    {
        //Arrange
        var command = new UpdateAvailabilityScheduleRequest{
           Description = "Test description",
           RepeatSlot = RepeatSlot.None,
            RepeatDuration = 0,
            AvailableSlots = new List<Interval<DateTimeOffset>>
            {
                new Interval<DateTimeOffset>(DateTimeOffset.UtcNow.Date.AddHours(8),
                    DateTimeOffset.UtcNow.Date.AddHours(12))
            }
        };

        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldNotHaveAnyValidationErrors();
    }


    [Fact]
    public async Task Validator_When_Description_Is_Empty_ShouldHaveValidationError()
    {
        //Arrange
        var command = new UpdateAvailabilityScheduleRequest{
            Description = "",
            RepeatSlot = RepeatSlot.None,
            RepeatDuration = 0,
            AvailableSlots = new List<Interval<DateTimeOffset>>
            {
                new Interval<DateTimeOffset>(DateTimeOffset.UtcNow.Date.AddHours(8),
                    DateTimeOffset.UtcNow.Date.AddHours(12))
            }
        };

        
        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldHaveValidationErrorFor(s => s.Description)
            .WithErrorMessage("Description is required");
    }
    
    [Fact]
    public async Task Validator_When_Repeat_Duration_Is_Negative_ShouldHaveValidationError()
    {
        //Arrange
        var command = new UpdateAvailabilityScheduleRequest{
            Description = "Test description",
            RepeatSlot = RepeatSlot.Daily,
            RepeatDuration = -1,
            AvailableSlots = new List<Interval<DateTimeOffset>>
            {
                new Interval<DateTimeOffset>(DateTimeOffset.UtcNow.Date.AddHours(8),
                    DateTimeOffset.UtcNow.Date.AddHours(12))
            }
        };

        
        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldHaveValidationErrorFor(s => s.RepeatDuration)
            .WithErrorMessage("Repeat duration must be positive.");
    }
    
    
    [Fact]
    public async Task Validator_When_Slot_Is_Empty_ShouldHaveValidationError()
    {
        //Arrange
        var command = new UpdateAvailabilityScheduleRequest{
            Description = "Test description",
            RepeatSlot = RepeatSlot.None,
            RepeatDuration = 0,
            AvailableSlots = new List<Interval<DateTimeOffset>>()
        };

        
        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldHaveValidationErrorFor(s => s.AvailableSlots)
            .WithErrorMessage("At least one available slot is required.");
    }
    
}