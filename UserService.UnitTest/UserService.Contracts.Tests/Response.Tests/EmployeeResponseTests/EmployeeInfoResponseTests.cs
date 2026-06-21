using MessagePack;
using QUserService.Contracts.Responses.EmployeeResponses;
using Shouldly;

namespace UserService.UnitTest.UserService.Contracts.Tests.Response.Tests.EmployeeResponseTests;

public class EmployeeInfoResponseTests
{
    [Fact]
    public void UserResponse_ShouldSerializeAndDeserializeCorrectly()
    {
        var originalRequest = new EmployeeInfo()
        {
            CompanyId = 1,
            BranchId = 1,
            CompanyServiceId = 1,
            EmployeeId = 1,
            FirstName = "Test Firstname",
            LastName = "Test Lastname" ,
            Position = "Test Position",
            PhoneNumber = "+992923324225",
            CreatedAt = DateTime.UtcNow
        };

        var bytes = MessagePackSerializer.Serialize(originalRequest);
        var deserializedRequest = MessagePackSerializer.Deserialize<EmployeeInfo>(bytes);


        deserializedRequest.CompanyId.ShouldBe(originalRequest.CompanyId);
        deserializedRequest.BranchId.ShouldBe(originalRequest.BranchId);
        deserializedRequest.CompanyServiceId.ShouldBe(originalRequest.CompanyServiceId);
        deserializedRequest.EmployeeId.ShouldBe(originalRequest.EmployeeId);
        deserializedRequest.FirstName.ShouldBe(originalRequest.FirstName);
        deserializedRequest.LastName.ShouldBe(originalRequest.LastName);
        deserializedRequest.Position.ShouldBe(originalRequest.Position);
        deserializedRequest.PhoneNumber.ShouldBe(originalRequest.PhoneNumber);
        deserializedRequest.CreatedAt.ShouldBe(originalRequest.CreatedAt);
    }
}