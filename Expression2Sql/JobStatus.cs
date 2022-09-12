namespace Entities;

public enum JobStatus
{
    Created = 0,
    Running = 1,
    Succeeded = 2,
    Failed = 3,
    Cancelled = 4,
    Pending = 5,
    PendingCancellation = 6,
    Broken = 7
}
