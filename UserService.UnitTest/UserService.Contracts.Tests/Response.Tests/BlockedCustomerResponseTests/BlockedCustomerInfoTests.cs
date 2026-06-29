using MessagePack;
using QUserService.Contracts.Responses.BlockedCustomersResponses;
using Shouldly;

namespace UserService.UnitTest.UserService.Contracts.Tests.Response.Tests.BlockedCustomerResponseTests;

public class BlockedCustomerInfoTests
{
    [Fact]
    public void UserResponse_ShouldSerializeAndDeserializeCorrectly()
    {
        var originalRequest = new BlockedCustomerInfo
        {
            BlockedId = 1,
            CustomerId = 1,
            CompanyId = 1,
            CustomerName = "Test Name",
            Reason = "Test Reason",
            DoesBanForever = false,
            BannedUntil = DateTime.UtcNow.AddMonths(1),
            CreatedAt = DateTime.UtcNow
        };

        var bytes = MessagePackSerializer.Serialize(originalRequest);
        var deserializedRequest = MessagePackSerializer.Deserialize<BlockedCustomerInfo>(bytes);


        deserializedRequest.BlockedId.ShouldBe(originalRequest.BlockedId);
        deserializedRequest.CompanyId.ShouldBe(originalRequest.CompanyId);
        deserializedRequest.CustomerId.ShouldBe(originalRequest.CustomerId);
        deserializedRequest.CustomerName.ShouldBe(originalRequest.CustomerName);
        deserializedRequest.Reason.ShouldBe(originalRequest.Reason);
        deserializedRequest.DoesBanForever.ShouldBe(originalRequest.DoesBanForever);
        deserializedRequest.BannedUntil.ShouldBe(originalRequest.BannedUntil);
        deserializedRequest.CreatedAt.ShouldBe(originalRequest.CreatedAt);
    }
}