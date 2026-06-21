using MessagePack;
using QUserService.Contracts.Requests.EmployeeRequests;
using Shouldly;

namespace UserService.UnitTest.UserService.Contracts.Tests.Request.Tests.EmployeeRequestTests;

public class EmployeeScheduleRequestTests
{
    [Fact]
    public void UserRequest_ShouldSerializeAndDeserializeCorrectly()
    {
        var originalRequest = new EmployeeScheduleRequest
        {
            RequestId = Guid.NewGuid(),
            EmployeeId = 1,
            Date = DateTimeOffset.UtcNow.Date
        };

        var bytes = MessagePackSerializer.Serialize(originalRequest);
        var deserializedRequest = MessagePackSerializer.Deserialize<EmployeeScheduleRequest>(bytes);


        deserializedRequest.RequestId.ShouldBe(originalRequest.RequestId);
        deserializedRequest.EmployeeId.ShouldBe(originalRequest.EmployeeId);
        deserializedRequest.Date.ShouldBe(originalRequest.Date);
    }
}