using MessagePack;
using QUserService.Contracts.Responses.UserResponses;
using Shouldly;

namespace UserService.UnitTest.UserService.Contracts.Tests.Response.Tests.UserResponseTests;

public class CurrentEmployeeResponseTests
{
    [Fact]
    public void UserResponse_ShouldSerializeAndDeserializeCorrectly()
    {
        var originalRequest = new CurrentEmployeeResponse()
        {
            EmployeeId = 1,
            CompanyId = 1,
            BranchId = 1,
            FirstName = "Test Firstname",
            LastName = "Test Lastname",
            PhoneNumber = "+992923324252",
            Position = "Test Position",
            ErrorMessage = null,
            IsValid = true
        };

        var bytes = MessagePackSerializer.Serialize(originalRequest);
        var deserializedRequest = MessagePackSerializer.Deserialize<CurrentEmployeeResponse>(bytes);


        deserializedRequest.EmployeeId.ShouldBe(originalRequest.EmployeeId);
        deserializedRequest.CompanyId.ShouldBe(originalRequest.CompanyId);
        deserializedRequest.BranchId.ShouldBe(originalRequest.BranchId);
        deserializedRequest.FirstName.ShouldBe(originalRequest.FirstName);
        deserializedRequest.LastName.ShouldBe(originalRequest.LastName);
        deserializedRequest.PhoneNumber.ShouldBe(originalRequest.PhoneNumber);
        deserializedRequest.Position.ShouldBe(originalRequest.Position);
        deserializedRequest.ErrorMessage.ShouldBe(originalRequest.ErrorMessage);
        deserializedRequest.IsValid.ShouldBe(originalRequest.IsValid);
    }
}