using MessagePack;
using QUserService.Contracts.Requests.BlockedCustomersRequests;
using Shouldly;

namespace UserService.UnitTest.UserService.Contracts.Tests.Request.Tests.BlockedCustomerRequestsTests;

public class UnblockCustomerRequestTests
{
    [Fact]
    public void UserRequest_ShouldSerializeAndDeserializeCorrectly()
    {
        var originalRequest = new UnblockCustomerRequest
        {
            RequestId = Guid.NewGuid(),
            BlockedCustomerId = 1,
            UnblockedByUserId = 1
        };

        var bytes = MessagePackSerializer.Serialize(originalRequest);
        var deserializedRequest = MessagePackSerializer.Deserialize<UnblockCustomerRequest>(bytes);


        deserializedRequest.RequestId.ShouldBe(originalRequest.RequestId);
        deserializedRequest.BlockedCustomerId.ShouldBe(originalRequest.BlockedCustomerId);
        deserializedRequest.UnblockedByUserId.ShouldBe(originalRequest.UnblockedByUserId);
    }
}