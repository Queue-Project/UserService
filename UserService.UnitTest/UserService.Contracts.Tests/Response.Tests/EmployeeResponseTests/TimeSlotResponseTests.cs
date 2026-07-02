using MessagePack;
using QUserService.Contracts.Responses.EmployeeResponses;
using Shouldly;

namespace UserService.UnitTest.UserService.Contracts.Tests.Response.Tests.EmployeeResponseTests;

public class TimeSlotResponseTests
{
    [Fact]
    public void UserResponse_ShouldSerializeAndDeserializeCorrectly()
    {
        var originalRequest = new TimeSlot()
        {
            From = DateTimeOffset.UtcNow.Date.AddHours(8),
            To = DateTimeOffset.UtcNow.Date.AddHours(12)
        };

        var bytes = MessagePackSerializer.Serialize(originalRequest);
        var deserializedRequest = MessagePackSerializer.Deserialize<TimeSlot>(bytes);


        deserializedRequest.From.ShouldBe(originalRequest.From);
        deserializedRequest.To.ShouldBe(originalRequest.To);
    }
}