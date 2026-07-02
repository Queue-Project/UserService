using FluentValidation.TestHelper;
using QUserService.Application.UseCases.AvailabilitySchedule.Commands.CreateAvailabilitySchedule;
using QUserService.Application.Validators.AvailabilityScheduleValidators;
using QUserService.Domain.Enums;
using QUserService.Domain.Models;

namespace UserService.UnitTest.UserService.Application.Tests.ValidatorTests.AvailabilityScheduleValidatorTests;

public class CreateAvailabilityScheduleRequestValidatorTests
{
    private readonly CreateAvailabilityScheduleRequestValidator _validator;

    public CreateAvailabilityScheduleRequestValidatorTests()
    {
        _validator = new CreateAvailabilityScheduleRequestValidator();
    }
    
    [Fact]
    public async Task Validator_When_Commands_Valid_Should_Not_HaveValidationError()
    {
        //Arrange
        var command = new CreateAvailabilityScheduleCommand(
            "Test description",
            RepeatSlot.None,
            0){
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
        var command = new CreateAvailabilityScheduleCommand(
            "",
            RepeatSlot.None,
            0){
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
        var command = new CreateAvailabilityScheduleCommand(
            "Test Description",
             RepeatSlot.Daily,
            -1)
        {
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
    public async Task Validator_When_RepeatDuration_Exceed_365_ShouldHaveValidationError()
    {
        //Arrange
        var command = new CreateAvailabilityScheduleCommand(
            "Test Description",
            RepeatSlot.Daily,
            366)
        {
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
            .WithErrorMessage("Repeat duration cannot exceed 365 days.");
    }
    
    [Fact]
    public async Task Validator_When_Slot_Is_Empty_ShouldHaveValidationError()
    {
        //Arrange
        var command = new CreateAvailabilityScheduleCommand(
            "",
            RepeatSlot.None,
            0){
            AvailableSlots = new List<Interval<DateTimeOffset>>()
        };

        
        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldHaveValidationErrorFor(s => s.AvailableSlots)
            .WithErrorMessage("At least one available slot is required.");
    }
    
    [Fact]
    public async Task Validator_When_Slot_Duration_Is_Shorter_Than_15_ShouldHaveValidationError()
    {
        //Arrange
        var command = new CreateAvailabilityScheduleCommand(
            "Test Description",
            RepeatSlot.None,
            0)
        {
            AvailableSlots = new List<Interval<DateTimeOffset>>
            {
                new Interval<DateTimeOffset>(DateTimeOffset.UtcNow.Date.AddMinutes(2),
                    DateTimeOffset.UtcNow.Date.AddMinutes(8))
            }
        };

        
        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldHaveValidationErrorFor(s => s.AvailableSlots)
            .WithErrorMessage("Minimum slot duration is 15 minutes.");
    }
    
}