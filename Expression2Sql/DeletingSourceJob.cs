using System;

namespace Entities;

public sealed class DeletingSourceJob : Job
{
    public Guid? SourceId { get; set; }

    public DeletingSourceJob() => Type = JobType.DeletingSource;
}
