using MessagePack;
using QUserService.Contracts.Responses.EmployeeResponses;
using Shouldly;

namespace UserService.UnitTest.UserService.Contracts.Tests.Response.Tests.EmployeeResponseTests;

public class EmployeeResponseTests
{
    [Fact]
    public void UserResponse_ShouldSerializeAndDeserializeCorrectly()
    {
        var originalRequest = new EmployeeResponse()
        {
            CompanyId = 1,
            BranchId = 1,
            ServiceId = 1,
            Id = 1,
            FirstName = "Test Firstname",
            LastName = "Test Lastname" ,
            Position = "Test Position",
            PhoneNumber = "+992923324225",
            IsValid = true,
            ErrorMessage = null,
            CreatedAt = DateTime.UtcNow
        };

        var bytes = MessagePackSerializer.Serialize(originalRequest);
        var deserializedRequest = MessagePackSerializer.Deserialize<EmployeeResponse>(bytes);


        deserializedRequest.CompanyId.ShouldBe(originalRequest.CompanyId);
        deserializedRequest.BranchId.ShouldBe(originalRequest.BranchId);
        deserializedRequest.ServiceId.ShouldBe(originalRequest.ServiceId);
        deserializedRequest.Id.ShouldBe(originalRequest.Id);
        deserializedRequest.FirstName.ShouldBe(originalRequest.FirstName);
        deserializedRequest.LastName.ShouldBe(originalRequest.LastName);
        deserializedRequest.Position.ShouldBe(originalRequest.Position);
        deserializedRequest.PhoneNumber.ShouldBe(originalRequest.PhoneNumber);
        deserializedRequest.CreatedAt.ShouldBe(originalRequest.CreatedAt);
        deserializedRequest.IsValid.ShouldBe(originalRequest.IsValid);
        deserializedRequest.ErrorMessage.ShouldBe(originalRequest.ErrorMessage);
    }
}