using MessagePack;
using QUserService.Contracts.Responses.UserResponses;
using Shouldly;

namespace UserService.UnitTest.UserService.Contracts.Tests.Response.Tests.UserResponseTests;

public class UserEmailResponseTests
{
    [Fact]
    public void UserResponse_ShouldSerializeAndDeserializeCorrectly()
    {
        var originalRequest = new UserEmailResponse()
        {
            UserId = 1,
            EmployeeId = null,
            CustomerId = 1,
            EmailAddress = "test@gmail.com",
            ErrorMessage = null,
            IsValid = true
        };

        var bytes = MessagePackSerializer.Serialize(originalRequest);
        var deserializedRequest = MessagePackSerializer.Deserialize<UserEmailResponse>(bytes);


        deserializedRequest.EmployeeId.ShouldBe(originalRequest.EmployeeId);
        deserializedRequest.UserId.ShouldBe(originalRequest.UserId);
        deserializedRequest.CustomerId.ShouldBe(originalRequest.CustomerId);
        deserializedRequest.EmailAddress.ShouldBe(originalRequest.EmailAddress);
        deserializedRequest.ErrorMessage.ShouldBe(originalRequest.ErrorMessage);
        deserializedRequest.IsValid.ShouldBe(originalRequest.IsValid);
    }
}