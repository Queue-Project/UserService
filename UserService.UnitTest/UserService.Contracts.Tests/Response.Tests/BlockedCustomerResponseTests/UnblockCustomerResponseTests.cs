using MessagePack;
using QUserService.Contracts.Responses.BlockedCustomersResponses;
using Shouldly;

namespace UserService.UnitTest.UserService.Contracts.Tests.Response.Tests.BlockedCustomerResponseTests;

public class UnblockCustomerResponseTests
{
    [Fact]
    public void UserResponse_ShouldSerializeAndDeserializeCorrectly()
    {
        var originalRequest = new UnblockCustomerResponse
        {
            Success = true,
            ErrorMessage = null
        };

        var bytes = MessagePackSerializer.Serialize(originalRequest);
        var deserializedRequest = MessagePackSerializer.Deserialize<UnblockCustomerResponse>(bytes);


        deserializedRequest.Success.ShouldBe(originalRequest.Success);
        deserializedRequest.ErrorMessage.ShouldBe(originalRequest.ErrorMessage);
    }
}