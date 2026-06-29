using MessagePack;
using QUserService.Contracts.Responses.EmployeeResponses;
using Shouldly;

namespace UserService.UnitTest.UserService.Contracts.Tests.Response.Tests.EmployeeResponseTests;

public class EmployeeAvailabilityResponseTests
{
    [Fact]
    public void UserResponse_ShouldSerializeAndDeserializeCorrectly()
    {
        var originalRequest = new EmployeeAvailabilityResponse
        {
            IsAvailable = true,
            AvailableSlots = new List<TimeSlot>(),
            ErrorMessage = null
        };

        var bytes = MessagePackSerializer.Serialize(originalRequest);
        var deserializedRequest = MessagePackSerializer.Deserialize<EmployeeAvailabilityResponse>(bytes);


        deserializedRequest.IsAvailable.ShouldBe(originalRequest.IsAvailable);
        deserializedRequest.AvailableSlots.ShouldBe(originalRequest.AvailableSlots);
        deserializedRequest.ErrorMessage.ShouldBe(originalRequest.ErrorMessage);
    }
}