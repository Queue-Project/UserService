using MessagePack;
using QUserService.Contracts.Requests.CustomerRequests;
using Shouldly;

namespace UserService.UnitTest.UserService.Contracts.Tests.Request.Tests.CustomerRequestTests;

public class IsCustomerBlockedRequestTests
{
    [Fact]
    public void UserRequest_ShouldSerializeAndDeserializeCorrectly()
    {
        var originalRequest = new IsCustomerBlockedRequest
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            CustomerId = 1,
        };

        var bytes = MessagePackSerializer.Serialize(originalRequest);
        var deserializedRequest = MessagePackSerializer.Deserialize<IsCustomerBlockedRequest>(bytes);


        deserializedRequest.RequestId.ShouldBe(originalRequest.RequestId);
        deserializedRequest.CompanyId.ShouldBe(originalRequest.CompanyId);
        deserializedRequest.CustomerId.ShouldBe(originalRequest.CustomerId);
    }
}