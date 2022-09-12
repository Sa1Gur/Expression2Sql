using System;

namespace Entities;

public sealed class DeletingProjectJob : Job
{
    public Guid? ProjectId { get; set; }

    public DeletingProjectJob() => Type = JobType.DeletingProject;
}
