using System;

namespace Entities;

public sealed class CopyingProjectJob : Job
{
    public Guid? ProjectId { get; set; }

    public CopyingProjectJob() => Type = JobType.CopyingProject;
}
