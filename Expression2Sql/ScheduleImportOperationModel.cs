using System;

namespace RestApi.Data.Models;

public class ScheduleImportOperationModel
{
    public virtual Guid? ProjectId { get; set; }
    public virtual Guid? VersionId { get; set; }
    public virtual Guid? SourceId { get; set; }
    public virtual Guid? ScheduleId { get; set; }

    public virtual ScheduleImportOperationStatuses Status { get; set; }
    public virtual DateTime BeginAt { get; set; }
    public virtual DateTime? EndAt { get; set; }

    public virtual string Message { get; set; }
    public virtual string Details { get; set; }

    public virtual bool IsCopy { get; set; }
}
