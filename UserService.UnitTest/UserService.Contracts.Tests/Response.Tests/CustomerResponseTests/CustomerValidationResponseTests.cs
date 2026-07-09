using MessagePack;
using QUserService.Contracts.Responses.CustomerResponses;
using Shouldly;

namespace UserService.UnitTest.UserService.Contracts.Tests.Response.Tests.CustomerResponseTests;

public class CustomerValidationResponseTests
{
    [Fact]
    public void UserResponse_ShouldSerializeAndDeserializeCorrectly()
    {
        var originalRequest = new CustomerValidationResponse
        {
            IsValid = true,
            IsBlocked = false,
          BlockReason = null,
          BannedUntil = null,
          ErrorMessage = null
        };

        var bytes = MessagePackSerializer.Serialize(originalRequest);
        var deserializedRequest = MessagePackSerializer.Deserialize<CustomerValidationResponse>(bytes);


        deserializedRequest.IsBlocked.ShouldBe(originalRequest.IsBlocked);
        deserializedRequest.IsValid.ShouldBe(originalRequest.IsValid);
        deserializedRequest.BlockReason.ShouldBe(originalRequest.BlockReason);
        deserializedRequest.BannedUntil.ShouldBe(originalRequest.BannedUntil);
        deserializedRequest.ErrorMessage.ShouldBe(originalRequest.ErrorMessage);
    }
}