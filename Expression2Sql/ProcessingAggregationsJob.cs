using System;

namespace Entities;

public sealed class ProcessingAggregationsJob : Job
{
    public Guid? AggregationId { get; set; }

    public ProcessingAggregationsJob() => Type = JobType.ProcessingAggregations;
}
