using MessagePack;
using QUserService.Contracts.Requests.CustomerRequests;
using Shouldly;

namespace UserService.UnitTest.UserService.Contracts.Tests.Request.Tests.CustomerRequestTests;

public class CustomerByIdRequestTests
{
    [Fact]
    public void UserRequest_ShouldSerializeAndDeserializeCorrectly()
    {
        var originalRequest = new CustomerByIdRequest
        {
            RequestId = Guid.NewGuid(),
            CustomerId = 1
        };

        var bytes = MessagePackSerializer.Serialize(originalRequest);
        var deserializedRequest = MessagePackSerializer.Deserialize<CustomerByIdRequest>(bytes);


        deserializedRequest.RequestId.ShouldBe(originalRequest.RequestId);
        deserializedRequest.CustomerId.ShouldBe(originalRequest.CustomerId);
    }
}