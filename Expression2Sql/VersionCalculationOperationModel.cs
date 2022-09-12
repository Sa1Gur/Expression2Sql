using System;

namespace RestApi.Data.Models;

public class VersionCalculationOperationModel
{
    public virtual Guid? ProjectId { get; set; }
    public virtual Guid? VersionId { get; set; }

    public virtual VersionCalculationOperationStatuses Status { get; set; }
    public virtual DateTime? BeginAt { get; set; }
    public virtual DateTime? EndAt { get; set; }

    public virtual string Message { get; set; }
    public virtual string Details { get; set; }
    public virtual int JobQueueId { get; set; }
    public virtual Guid? UserId { get; set; }
}
