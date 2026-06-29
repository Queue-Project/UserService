using MessagePack;
using QUserService.Contracts.Responses.BlockedCustomersResponses;
using Shouldly;

namespace UserService.UnitTest.UserService.Contracts.Tests.Response.Tests.BlockedCustomerResponseTests;

public class BlockedCustomersPagedResponseTests
{
    [Fact]
    public void UserResponse_ShouldSerializeAndDeserializeCorrectly()
    {
        var originalRequest = new BlockedCustomersPagedResponse
        {
            Items = new List<BlockedCustomerInfo>(),
            PageNumber = 1,
            PageSize = 15,
            TotalCount = 1
        };

        var bytes = MessagePackSerializer.Serialize(originalRequest);
        var deserializedRequest = MessagePackSerializer.Deserialize<BlockedCustomersPagedResponse>(bytes);


        deserializedRequest.Items.ShouldBe(originalRequest.Items);
        deserializedRequest.PageNumber.ShouldBe(originalRequest.PageNumber);
        deserializedRequest.PageSize.ShouldBe(originalRequest.PageSize);
        deserializedRequest.TotalCount.ShouldBe(originalRequest.TotalCount);
    }
}