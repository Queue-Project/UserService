using MessagePack;
using QUserService.Contracts.Requests.BlockedCustomersRequests;
using Shouldly;

namespace UserService.UnitTest.UserService.Contracts.Tests.Request.Tests.BlockedCustomerRequestsTests;

public class GetAllCompanyBlockedCustomersRequestTests
{
    [Fact]
    public void UserRequest_ShouldSerializeAndDeserializeCorrectly()
    {
        var originalRequest = new GetAllCompanyBlockedCustomersRequest
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            PageNumber = 1,
            PageSize = 15
        };

        var bytes = MessagePackSerializer.Serialize(originalRequest);
        var deserializedRequest = MessagePackSerializer.Deserialize<GetAllCompanyBlockedCustomersRequest>(bytes);


        deserializedRequest.RequestId.ShouldBe(originalRequest.RequestId);
        deserializedRequest.CompanyId.ShouldBe(originalRequest.CompanyId);
        deserializedRequest.PageNumber.ShouldBe(originalRequest.PageNumber);
        deserializedRequest.PageSize.ShouldBe(originalRequest.PageSize);
    }
}