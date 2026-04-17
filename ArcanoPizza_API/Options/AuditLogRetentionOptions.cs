namespace ArcanoPizza_API.Options;

public sealed class AuditLogRetentionOptions
{
    public const string SectionName = "AuditLogs:Retention";

    public int Days { get; set; } = 30;
    public int RunHourUtc { get; set; } = 3;
}

