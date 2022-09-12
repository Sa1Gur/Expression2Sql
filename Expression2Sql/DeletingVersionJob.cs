using System;

namespace Entities;

public sealed class DeletingVersionJob : Job
{
    public Guid? VersionId { get; set; }

    public DeletingVersionJob() => Type = JobType.DeletingVersion;
}
