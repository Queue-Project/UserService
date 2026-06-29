using MessagePack;
using QUserService.Contracts.Requests.CustomerRequests;
using Shouldly;

namespace UserService.UnitTest.UserService.Contracts.Tests.Request.Tests.CustomerRequestTests;

public class GetAllCustomersRequestTests
{
    [Fact]
    public void UserRequest_ShouldSerializeAndDeserializeCorrectly()
    {
        var originalRequest = new GetAllCustomersRequest
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            PageNumber = 1,
            PageSize = 15
        };

        var bytes = MessagePackSerializer.Serialize(originalRequest);
        var deserializedRequest = MessagePackSerializer.Deserialize<GetAllCustomersRequest>(bytes);


        deserializedRequest.RequestId.ShouldBe(originalRequest.RequestId);
        deserializedRequest.CompanyId.ShouldBe(originalRequest.CompanyId);
    }
}