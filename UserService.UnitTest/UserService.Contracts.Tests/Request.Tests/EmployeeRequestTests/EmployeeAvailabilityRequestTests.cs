using MessagePack;
using QUserService.Contracts.Requests.EmployeeRequests;
using Shouldly;

namespace UserService.UnitTest.UserService.Contracts.Tests.Request.Tests.EmployeeRequestTests;

public class EmployeeAvailabilityRequestTests
{
    [Fact]
    public void UserRequest_ShouldSerializeAndDeserializeCorrectly()
    {
        var originalRequest = new EmployeeAvailabilityRequest
        {
            RequestId = Guid.NewGuid(),
            EmployeeId = 1,
            ExistingQueueId = 1,
            StartTime = DateTimeOffset.UtcNow.AddHours(1),
            EndTime = DateTimeOffset.UtcNow.AddHours(2)
        };

        var bytes = MessagePackSerializer.Serialize(originalRequest);
        var deserializedRequest = MessagePackSerializer.Deserialize<EmployeeAvailabilityRequest>(bytes);


        deserializedRequest.RequestId.ShouldBe(originalRequest.RequestId);
        deserializedRequest.EmployeeId.ShouldBe(originalRequest.EmployeeId);
        deserializedRequest.ExistingQueueId.ShouldBe(originalRequest.ExistingQueueId);
        deserializedRequest.StartTime.ShouldBe(originalRequest.StartTime);
        deserializedRequest.EndTime.ShouldBe(originalRequest.EndTime);
    }
}