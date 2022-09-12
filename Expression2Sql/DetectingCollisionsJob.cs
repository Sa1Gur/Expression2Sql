using System;

namespace Entities;

public sealed class DetectingCollisionsJob : Job
{
    public Guid? IndicatorId { get; set; }

    public DetectingCollisionsJob() => Type = JobType.DetectingCollision;
}
