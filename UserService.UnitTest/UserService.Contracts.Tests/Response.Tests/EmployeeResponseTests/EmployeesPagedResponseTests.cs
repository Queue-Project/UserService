using MessagePack;
using QUserService.Contracts.Responses.EmployeeResponses;
using Shouldly;

namespace UserService.UnitTest.UserService.Contracts.Tests.Response.Tests.EmployeeResponseTests;

public class EmployeesPagedResponseTests
{
    [Fact]
    public void UserResponse_ShouldSerializeAndDeserializeCorrectly()
    {
        var originalRequest = new EmployeesPagedResponse()
        {
            Items = new List<EmployeeInfo>(),
            PageNumber = 1,
            PageSize = 15,
            TotalCount = 1
        };

        var bytes = MessagePackSerializer.Serialize(originalRequest);
        var deserializedRequest = MessagePackSerializer.Deserialize<EmployeesPagedResponse>(bytes);


        deserializedRequest.Items.ShouldBe(originalRequest.Items);
        deserializedRequest.PageNumber.ShouldBe(originalRequest.PageNumber);
        deserializedRequest.PageSize.ShouldBe(originalRequest.PageSize);
        deserializedRequest.TotalCount.ShouldBe(originalRequest.TotalCount);
    }
}