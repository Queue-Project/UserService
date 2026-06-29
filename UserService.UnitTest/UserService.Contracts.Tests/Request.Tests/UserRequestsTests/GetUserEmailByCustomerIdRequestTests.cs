using MessagePack;
using QUserService.Contracts.Requests.UserRequests;
using Shouldly;

namespace UserService.UnitTest.UserService.Contracts.Tests.Request.Tests.UserRequestsTests;

public class GetUserEmailByCustomerIdRequestTests
{
    [Fact]
    public void UserRequest_ShouldSerializeAndDeserializeCorrectly()
    {
        var originalRequest = new GetUserEmailByCustomerIdRequest
        {
            RequestId = Guid.NewGuid(),
            CustomerId = 1,
        };

        var bytes = MessagePackSerializer.Serialize(originalRequest);
        var deserializedRequest = MessagePackSerializer.Deserialize<GetUserEmailByCustomerIdRequest>(bytes);


        deserializedRequest.RequestId.ShouldBe(originalRequest.RequestId);
        deserializedRequest.CustomerId.ShouldBe(originalRequest.CustomerId);
    }
}