using System;

namespace Entities;

public sealed class DeletingSourcePairJob : Job
{
    public Guid? SourcePairId { get; set; }

    public DeletingSourcePairJob() => Type = JobType.DeletingSourcePair;
}
