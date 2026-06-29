using MessagePack;
using QUserService.Contracts.Requests.EmployeeRequests;
using Shouldly;

namespace UserService.UnitTest.UserService.Contracts.Tests.Request.Tests.EmployeeRequestTests;

public class EmployeeByIdRequestTests
{
    [Fact]
    public void UserRequest_ShouldSerializeAndDeserializeCorrectly()
    {
        var originalRequest = new EmployeeByIdRequest
        {
            RequestId = Guid.NewGuid(),
            EmployeeId = 1,
        
        };

        var bytes = MessagePackSerializer.Serialize(originalRequest);
        var deserializedRequest = MessagePackSerializer.Deserialize<EmployeeByIdRequest>(bytes);


        deserializedRequest.RequestId.ShouldBe(originalRequest.RequestId);
        deserializedRequest.EmployeeId.ShouldBe(originalRequest.EmployeeId);
    }
}