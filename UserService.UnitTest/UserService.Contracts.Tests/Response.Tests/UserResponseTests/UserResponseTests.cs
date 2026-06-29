using MessagePack;
using QUserService.Contracts.Responses.UserResponses;
using Shouldly;

namespace UserService.UnitTest.UserService.Contracts.Tests.Response.Tests.UserResponseTests;

public class UserResponseTests
{
    [Fact]
    public void UserResponse_ShouldSerializeAndDeserializeCorrectly()
    {
        var originalRequest = new UserResponse()
        {
            Id = 1,
            EmployeeId = null,
            CustomerId = 1,
            EmailAddress = "test@gmail.com",
            Roles = "Customer",
            ErrorMessage = null,
            IsValid = true
        };

        var bytes = MessagePackSerializer.Serialize(originalRequest);
        var deserializedRequest = MessagePackSerializer.Deserialize<UserResponse>(bytes);


        deserializedRequest.EmployeeId.ShouldBe(originalRequest.EmployeeId);
        deserializedRequest.Id.ShouldBe(originalRequest.Id);
        deserializedRequest.CustomerId.ShouldBe(originalRequest.CustomerId);
        deserializedRequest.EmailAddress.ShouldBe(originalRequest.EmailAddress);
        deserializedRequest.Roles.ShouldBe(originalRequest.Roles);
        deserializedRequest.ErrorMessage.ShouldBe(originalRequest.ErrorMessage);
        deserializedRequest.IsValid.ShouldBe(originalRequest.IsValid);
    }
}