using MessagePack;
using QUserService.Contracts.Requests.CustomerRequests;
using Shouldly;

namespace UserService.UnitTest.UserService.Contracts.Tests.Request.Tests.CustomerRequestTests;

public class CustomerBlockValidationRequestTests
{
    [Fact]
    public void UserRequest_ShouldSerializeAndDeserializeCorrectly()
    {
        var originalRequest = new CustomerBlockValidationRequest
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            CustomerId = 1
        };

        var bytes = MessagePackSerializer.Serialize(originalRequest);
        var deserializedRequest = MessagePackSerializer.Deserialize<CustomerBlockValidationRequest>(bytes);


        deserializedRequest.RequestId.ShouldBe(originalRequest.RequestId);
        deserializedRequest.CompanyId.ShouldBe(originalRequest.CompanyId);
        deserializedRequest.CustomerId.ShouldBe(originalRequest.CustomerId);
    }
}