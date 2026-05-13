namespace CrmSolid.Models;

/// <summary>Current state of a queued message job.</summary>
public enum JobStatus
{
    Queued,
    Sent,
    Failed,
}
