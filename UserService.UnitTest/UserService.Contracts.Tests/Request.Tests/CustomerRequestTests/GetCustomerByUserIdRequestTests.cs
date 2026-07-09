using MessagePack;
using QUserService.Contracts.Requests.CustomerRequests;
using Shouldly;

namespace UserService.UnitTest.UserService.Contracts.Tests.Request.Tests.CustomerRequestTests;

public class GetCustomerByUserIdRequestTests
{
    [Fact]
    public void UserRequest_ShouldSerializeAndDeserializeCorrectly()
    {
        var originalRequest = new GetCustomerByUserIdRequest
        {
            RequestId = Guid.NewGuid(),
            UserId = 1,
        };

        var bytes = MessagePackSerializer.Serialize(originalRequest);
        var deserializedRequest = MessagePackSerializer.Deserialize<GetCustomerByUserIdRequest>(bytes);


        deserializedRequest.RequestId.ShouldBe(originalRequest.RequestId);
        deserializedRequest.UserId.ShouldBe(originalRequest.UserId);
    }
}