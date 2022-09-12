using System;

namespace Entities;

public sealed class CalculatingUserFieldJob : Job
{
    public Guid? ProjectId { get; set; }

    public Guid? UserFieldId { get; set; }

    public CalculatingUserFieldJob() => Type = JobType.CalculatingUserField;
}
