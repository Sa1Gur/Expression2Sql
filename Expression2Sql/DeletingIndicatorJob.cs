using System;

namespace Entities;

public sealed class DeletingIndicatorJob : Job
{
    public Guid? IndicatorId { get; set; }

    public DeletingIndicatorJob() => Type = JobType.DeletingIndicator;
}
