using MessagePack;
using QUserService.Contracts.Responses.EmployeeResponses;
using Shouldly;

namespace UserService.UnitTest.UserService.Contracts.Tests.Response.Tests.EmployeeResponseTests;

public class EmployeeScheduleResponseTests
{
    [Fact]
    public void UserResponse_ShouldSerializeAndDeserializeCorrectly()
    {
        var originalRequest = new EmployeeScheduleResponse()
        {
            EmployeeId=1,
            BookedSlots = new List<TimeSlot>(),
            WorkingHours = new List<TimeSlot>(),
            Date = DateTimeOffset.UtcNow.Date
        };

        var bytes = MessagePackSerializer.Serialize(originalRequest);
        var deserializedRequest = MessagePackSerializer.Deserialize<EmployeeScheduleResponse>(bytes);


        deserializedRequest.EmployeeId.ShouldBe(originalRequest.EmployeeId);
        deserializedRequest.BookedSlots.ShouldBe(originalRequest.BookedSlots);
        deserializedRequest.WorkingHours.ShouldBe(originalRequest.WorkingHours);
        deserializedRequest.Date.ShouldBe(originalRequest.Date);
        
    }
}