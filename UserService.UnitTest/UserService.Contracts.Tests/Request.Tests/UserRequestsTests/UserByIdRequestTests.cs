using MessagePack;
using QUserService.Contracts.Requests.UserRequests;
using Shouldly;

namespace UserService.UnitTest.UserService.Contracts.Tests.Request.Tests.UserRequestsTests;

public class UserByIdRequestTests
{
    [Fact]
    public void UserRequest_ShouldSerializeAndDeserializeCorrectly()
    {
        var originalRequest = new UserByIdRequest
        {
            RequestId = Guid.NewGuid(),
            UserId = 1,
        };

        var bytes = MessagePackSerializer.Serialize(originalRequest);
        var deserializedRequest = MessagePackSerializer.Deserialize<UserByIdRequest>(bytes);


        deserializedRequest.RequestId.ShouldBe(originalRequest.RequestId);
        deserializedRequest.UserId.ShouldBe(originalRequest.UserId);
    }
}