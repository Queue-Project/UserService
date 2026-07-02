using MessagePack;
using QUserService.Contracts.Requests.BlockedCustomersRequests;
using Shouldly;

namespace UserService.UnitTest.UserService.Contracts.Tests.Request.Tests.BlockedCustomerRequestsTests;

public class BlockedCustomerByIdRequestTests
{
    [Fact]
    public void UserRequest_ShouldSerializeAndDeserializeCorrectly()
    {
        var originalRequest = new BlockedCustomerByIdRequest
        {
            RequestId = Guid.NewGuid(),
            BlockedCustomerId = 1
        };

        var bytes = MessagePackSerializer.Serialize(originalRequest);
        var deserializedRequest = MessagePackSerializer.Deserialize<BlockedCustomerByIdRequest>(bytes);


        deserializedRequest.RequestId.ShouldBe(originalRequest.RequestId);
        deserializedRequest.BlockedCustomerId.ShouldBe(originalRequest.BlockedCustomerId);
    }
}