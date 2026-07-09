using MessagePack;
using QUserService.Contracts.Responses.BlockedCustomersResponses;
using Shouldly;

namespace UserService.UnitTest.UserService.Contracts.Tests.Response.Tests.BlockedCustomerResponseTests;

public class BlockedCustomerValidationResponseTests
{
    [Fact]
    public void UserResponse_ShouldSerializeAndDeserializeCorrectly()
    {
        var originalRequest = new BlockedCustomerValidationResponse
        {
            IsBlocked = false,
            IsBlockedForever = false,
            BannedUntil = null,
            BlockReason = null,
            ErrorMessage = null
        };

        var bytes = MessagePackSerializer.Serialize(originalRequest);
        var deserializedRequest = MessagePackSerializer.Deserialize<BlockedCustomerValidationResponse>(bytes);


        deserializedRequest.IsBlocked.ShouldBe(originalRequest.IsBlocked);
        deserializedRequest.IsBlockedForever.ShouldBe(originalRequest.IsBlockedForever);
        deserializedRequest.BannedUntil.ShouldBe(originalRequest.BannedUntil);
        deserializedRequest.BlockReason.ShouldBe(originalRequest.BlockReason);
        deserializedRequest.ErrorMessage.ShouldBe(originalRequest.ErrorMessage);
    }
}