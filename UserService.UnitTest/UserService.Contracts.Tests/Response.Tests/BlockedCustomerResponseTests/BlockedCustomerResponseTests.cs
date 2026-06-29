using MessagePack;
using QUserService.Contracts.Responses.BlockedCustomersResponses;
using Shouldly;

namespace UserService.UnitTest.UserService.Contracts.Tests.Response.Tests.BlockedCustomerResponseTests;

public class BlockedCustomerResponseTests
{
    [Fact]
    public void UserResponse_ShouldSerializeAndDeserializeCorrectly()
    {
        var originalRequest = new BlockedCustomerResponse
        {
            Id = 1,
            CompanyId = 1,
            CustomerId = 1,
            IsValid = true,
            DoesBanForever = false,
            BannedUntil = DateTime.UtcNow.AddMonths(1),
            Reason = "Test Reason",
            ErrorMessage = null,
            CreatedAt = DateTime.UtcNow
        };

        var bytes = MessagePackSerializer.Serialize(originalRequest);
        var deserializedRequest = MessagePackSerializer.Deserialize<BlockedCustomerResponse>(bytes);


        deserializedRequest.Id.ShouldBe(originalRequest.Id);
        deserializedRequest.CompanyId.ShouldBe(originalRequest.CompanyId);
        deserializedRequest.CustomerId.ShouldBe(originalRequest.CustomerId);
        deserializedRequest.ErrorMessage.ShouldBe(originalRequest.ErrorMessage);
        deserializedRequest.IsValid.ShouldBe(originalRequest.IsValid);
        deserializedRequest.DoesBanForever.ShouldBe(originalRequest.DoesBanForever);
        deserializedRequest.BannedUntil.ShouldBe(originalRequest.BannedUntil);
        deserializedRequest.Reason.ShouldBe(originalRequest.Reason);
        deserializedRequest.CreatedAt.ShouldBe(originalRequest.CreatedAt);
    }
}