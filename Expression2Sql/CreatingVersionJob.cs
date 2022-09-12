using System;

namespace Entities;

public sealed class CreatingVersionJob : Job
{
    public Guid? ProjectId { get; set; }

    public CreatingVersionJob() => Type = JobType.CreatingVersion;
}
