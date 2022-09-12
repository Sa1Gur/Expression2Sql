using System;
using System.Collections.Generic;

namespace Entities;

public partial class JobQueue
{
    public int Id { get; set; }

    public DateTimeOffset? StartedAt { get; set; }

    public DateTimeOffset? EndedAt { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public string? Description { get; set; }

    public Guid? VersionId { get; set; }

    public ICollection<Job> Jobs { get; } = new HashSet<Job>();

    public Guid? UserId { get; set; }

    public Guid ProjectId { get; set; }

    public JobType Type { get; set; }

    public JobStatus Status { get; set; }

    public int FailedAttemptCount { get; set; }

    public string ParamsAsJson { get; set; }
}
