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
            Schedules = new List<EmployeeScheduleInfo>(),
            Date = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        var bytes = MessagePackSerializer.Serialize(originalRequest);
        var deserializedRequest = MessagePackSerializer.Deserialize<EmployeeScheduleResponse>(bytes);


        deserializedRequest.EmployeeId.ShouldBe(originalRequest.EmployeeId);
        deserializedRequest.Schedules.ShouldBe(originalRequest.Schedules);
        deserializedRequest.Date.ShouldBe(originalRequest.Date);
        
    }
}