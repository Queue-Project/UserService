using MessagePack;

namespace QUserService.Contracts.Responses.EmployeeResponses;

[MessagePackObject]
public class EmployeeScheduleResponse
{
    [Key(0)]
    public int EmployeeId { get; set; }
    
    [Key(1)]
    public DateOnly Date { get; set; }
    
    [Key(2)]
    public List<EmployeeScheduleInfo> Schedules { get; set; } = [];
    
}