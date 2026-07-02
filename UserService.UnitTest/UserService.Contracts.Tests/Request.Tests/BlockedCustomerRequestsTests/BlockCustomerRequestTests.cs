using BranchService.Contracts.Requests;
using MessagePack;
using QUserService.Contracts.Requests.BlockedCustomersRequests;
using Shouldly;

namespace UserService.UnitTest.UserService.Contracts.Tests.Request.Tests.BlockedCustomerRequestsTests;

public class BlockCustomerRequestTests
{
    [Fact]
    public void UserRequest_ShouldSerializeAndDeserializeCorrectly()
    {
        var originalRequest = new BlockCustomerRequest()
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            CustomerId = 1,
            BlockedByUserId = 1,
            DoesBanForever = false,
            Reason = "Test Reason",
            BannedUntil = DateTime.UtcNow.AddMonths(1)
        };

        var bytes = MessagePackSerializer.Serialize(originalRequest);
        var deserializedRequest = MessagePackSerializer.Deserialize<BlockCustomerRequest>(bytes);


        deserializedRequest.RequestId.ShouldBe(originalRequest.RequestId);
        deserializedRequest.CompanyId.ShouldBe(originalRequest.CompanyId);
        deserializedRequest.CustomerId.ShouldBe(originalRequest.CustomerId);
        deserializedRequest.BlockedByUserId.ShouldBe(originalRequest.BlockedByUserId);
        deserializedRequest.DoesBanForever.ShouldBe(originalRequest.DoesBanForever);
        deserializedRequest.Reason.ShouldBe(originalRequest.Reason);
        deserializedRequest.BannedUntil.ShouldBe(originalRequest.BannedUntil);
    }
}