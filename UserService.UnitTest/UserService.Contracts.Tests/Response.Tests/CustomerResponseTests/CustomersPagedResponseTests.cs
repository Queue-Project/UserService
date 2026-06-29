using MessagePack;
using QUserService.Contracts.Responses.CustomerResponses;
using Shouldly;

namespace UserService.UnitTest.UserService.Contracts.Tests.Response.Tests.CustomerResponseTests;

public class CustomersPagedResponseTests
{
    [Fact]
    public void UserResponse_ShouldSerializeAndDeserializeCorrectly()
    {
        var originalRequest = new CustomersPagedResponse
        {
            Items = new List<CustomerInfo>(),
            PageNumber = 1,
            PageSize = 15,
            TotalCount = 1
        };

        var bytes = MessagePackSerializer.Serialize(originalRequest);
        var deserializedRequest = MessagePackSerializer.Deserialize<CustomersPagedResponse>(bytes);


        deserializedRequest.Items.ShouldBe(originalRequest.Items);
        deserializedRequest.PageNumber.ShouldBe(originalRequest.PageNumber);
        deserializedRequest.PageSize.ShouldBe(originalRequest.PageSize);
        deserializedRequest.TotalCount.ShouldBe(originalRequest.TotalCount);
    }
}