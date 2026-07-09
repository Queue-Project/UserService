using QAuditLogService.Contracts;

namespace QUserService.Contracts;

public class AuditData
{
    public int PerformedByUserId { get; set; }
    public string PerformedByUserName { get; set; }
    public List<AuditEventLogDetails> Changes { get; set; } = [];

}