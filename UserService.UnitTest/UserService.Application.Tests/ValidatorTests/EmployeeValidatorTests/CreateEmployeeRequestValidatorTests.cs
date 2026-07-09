using FluentValidation.TestHelper;
using QUserService.Application.UseCases.Employees.Commands.CreateEmployee;
using QUserService.Application.Validators.EmployeeValidators;

namespace UserService.UnitTest.UserService.Application.Tests.ValidatorTests.EmployeeValidatorTests;

public class CreateEmployeeRequestValidatorTests
{
    private readonly CreateEmployeeRequestValidator _validator;

    public CreateEmployeeRequestValidatorTests()
    {
        _validator = new CreateEmployeeRequestValidator();
    }

    [Fact]
    public async Task Validator_When_Commands_Valid_Should_Not_HaveValidationError()
    {
        //Arrange
        var command = new CreateEmployeeCommand(1,1,"Test Firstname", "Test Lastname","Developer" ,"+992923324252");

        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldNotHaveAnyValidationErrors();
    }


    [Fact]
    public async Task Validator_When_Firstname_Is_Empty_ShouldHaveValidationError()
    {
        //Arrange
        var command = new CreateEmployeeCommand(1,1,"", "Test Lastname","Developer" ,"+992923324252");
        
        
        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldHaveValidationErrorFor(s => s.Firstname)
            .WithErrorMessage("Firstname is required.");
    }
    
    [Fact]
    public async Task Validator_When_Lastname_Is_Empty_ShouldHaveValidation_Error()
    {
        //Arrange
        var command = new CreateEmployeeCommand(1,1,"Test Firstname", "","Developer" ,"+992923324252");

        
        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldHaveValidationErrorFor(s => s.Lastname)
            .WithErrorMessage("Lastname is required");
    }
    
    [Fact]
    public async Task Validator_When_Position_Is_Empty_ShouldHaveValidation_Error()
    {
        //Arrange
        var command = new CreateEmployeeCommand(1,1,"Test Firstname", "Test Lastname","" ,"+992923324252");

        
        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldHaveValidationErrorFor(s => s.Position)
            .WithErrorMessage("Position is required");
    }
    
    [Fact]
    public async Task Validator_When_PhoneNumber_Is_Empty_Correct_ShouldHaveValidationError()
    {
        //Arrange
        var command = new CreateEmployeeCommand(1,1,"Test Firstname", "Test Lastname","Developer" ,"");


        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldHaveValidationErrorFor(s => s.PhoneNumber)
            .WithErrorMessage("PhoneNumber is required");
    }
    
    [Fact]
    public async Task Validator_When_PhoneNumber_Is_Not_Correct_ShouldHaveValidationError()
    {
        //Arrange
        var command = new CreateEmployeeCommand(1,1,"Test Firstname", "Test Lastname","Developer" ,"A92923324252");


        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldHaveValidationErrorFor(s => s.PhoneNumber)
            .WithErrorMessage("PhoneNumber is invalid");
    }

    [Fact]
    public async Task Validator_When_BranchId_Is_Invalid_Number_ShouldHaveValidationError()
    {
        //Arrange
        var command = new CreateEmployeeCommand(0,1,"Test Firstname", "Test Lastname","Developer" ,"+992923324252");
        
        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldHaveValidationErrorFor(s => s.BranchId)
            .WithErrorMessage("BranchId must be a positive number and greater than 0.");
        
    }
    
    [Fact]
    public async Task Validator_When_ServiceId_Is_Invalid_Number_ShouldHaveValidationError()
    {
        //Arrange
        var command = new CreateEmployeeCommand(1,0,"Test Firstname", "Test Lastname","Developer" ,"+992923324252");
        
        //Act
        var result = _validator.TestValidate(command);
        
        //Assert
        result.ShouldHaveValidationErrorFor(s => s.ServiceId)
            .WithErrorMessage("ServiceId must be a positive number and greater than 0.");
        
    }
    
    
}