using MessagePack;

namespace QUserService.Contracts.Responses.EmployeeResponses;

[MessagePackObject]
public class EmployeeScheduleInfo
{
    [Key(0)]public int ScheduleId { get; set; }

    [Key(1)]public string? Description { get; set; }

    [Key(2)]public List<TimeSlot> AvailableSlots { get; set; } = [];
}