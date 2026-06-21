using MessagePack;
using QUserService.Contracts.Responses.UserResponses;
using Shouldly;

namespace UserService.UnitTest.UserService.Contracts.Tests.Response.Tests.UserResponseTests;

public class IsEmployeeResponseTests
{
    [Fact]
    public void UserResponse_ShouldSerializeAndDeserializeCorrectly()
    {
        var originalRequest = new IsEmployeeResponse()
        {
            IsEmployee = true
        };

        var bytes = MessagePackSerializer.Serialize(originalRequest);
        var deserializedRequest = MessagePackSerializer.Deserialize<IsEmployeeResponse>(bytes);


        deserializedRequest.IsEmployee.ShouldBe(originalRequest.IsEmployee);

    }
}