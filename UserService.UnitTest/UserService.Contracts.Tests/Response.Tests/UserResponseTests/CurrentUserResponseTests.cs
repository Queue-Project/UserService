using MessagePack;
using QUserService.Contracts.Responses.UserResponses;
using Shouldly;

namespace UserService.UnitTest.UserService.Contracts.Tests.Response.Tests.UserResponseTests;

public class CurrentUserResponseTests
{
    [Fact]
    public void UserResponse_ShouldSerializeAndDeserializeCorrectly()
    {
        var originalRequest = new CurrentUserResponse()
        {
            UserId = 1,
            EmployeeId = null,
            CustomerId = 1,
            EmailAddress = "test@gmail.com",
            Role = "Customer",
            ErrorMessage = null,
            IsValid = true
        };

        var bytes = MessagePackSerializer.Serialize(originalRequest);
        var deserializedRequest = MessagePackSerializer.Deserialize<CurrentUserResponse>(bytes);


        deserializedRequest.EmployeeId.ShouldBe(originalRequest.EmployeeId);
        deserializedRequest.UserId.ShouldBe(originalRequest.UserId);
        deserializedRequest.CustomerId.ShouldBe(originalRequest.CustomerId);
        deserializedRequest.EmailAddress.ShouldBe(originalRequest.EmailAddress);
        deserializedRequest.Role.ShouldBe(originalRequest.Role);
        deserializedRequest.ErrorMessage.ShouldBe(originalRequest.ErrorMessage);
        deserializedRequest.IsValid.ShouldBe(originalRequest.IsValid);
    }
}