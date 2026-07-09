using MessagePack;
using QUserService.Contracts.Requests.EmployeeRequests;
using Shouldly;

namespace UserService.UnitTest.UserService.Contracts.Tests.Request.Tests.EmployeeRequestTests;

public class GetAllCompanyEmployeesRequestTests
{
    [Fact]
    public void UserRequest_ShouldSerializeAndDeserializeCorrectly()
    {
        var originalRequest = new GetAllCompanyEmployeesRequest
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            PageNumber = 1,
            PageSize = 15
        };

        var bytes = MessagePackSerializer.Serialize(originalRequest);
        var deserializedRequest = MessagePackSerializer.Deserialize<GetAllCompanyEmployeesRequest>(bytes);


        deserializedRequest.RequestId.ShouldBe(originalRequest.RequestId);
        deserializedRequest.CompanyId.ShouldBe(originalRequest.CompanyId);
        deserializedRequest.PageNumber.ShouldBe(originalRequest.PageNumber);
        deserializedRequest.PageSize.ShouldBe(originalRequest.PageSize);
    }
}