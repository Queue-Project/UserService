using MessagePack;
using QUserService.Contracts.Responses.BlockedCustomersResponses;
using Shouldly;

namespace UserService.UnitTest.UserService.Contracts.Tests.Response.Tests.BlockedCustomerResponseTests;

public class BlockCustomerResponseTests
{
    [Fact]
    public void UserResponse_ShouldSerializeAndDeserializeCorrectly()
    {
        var originalRequest = new BlockCustomerResponse
        {
            BlockedCustomerId = 1,
            Success = true,
            ErrorMessage = null
        };

        var bytes = MessagePackSerializer.Serialize(originalRequest);
        var deserializedRequest = MessagePackSerializer.Deserialize<BlockCustomerResponse>(bytes);


        deserializedRequest.BlockedCustomerId.ShouldBe(originalRequest.BlockedCustomerId);
        deserializedRequest.Success.ShouldBe(originalRequest.Success);
        deserializedRequest.ErrorMessage.ShouldBe(originalRequest.ErrorMessage);
    }
}