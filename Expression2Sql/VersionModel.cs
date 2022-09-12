using System;
using System.Collections.Generic;

namespace RestApi.Data.Models;

public class VersionModel
{
    public virtual int Number { get; set; }
    public virtual string Name { get; set; }
    public virtual string SourceCollisions { get; set; }
    public virtual string SourcePairCollisions { get; set; }
    public virtual string SourcePairConnections { get; set; }
    public virtual DateTime CreatedAt { get; set; }
    public virtual Guid? UserId { get; set; }
    public virtual DateTime? CalculatedAt { get; set; }    
    public virtual VersionCompletionState CompletionState { get; set; }
    public virtual VersionCompletionStatus CompletionStatus { get; set; }
    public virtual Guid? CalculateOperationId { get; set; }
    public virtual IDictionary<Guid, ScheduleModel> Schedules { get; set; }
    public virtual IDictionary<Guid, ReportModel> Reports { get; set; }
    public virtual string LocalId { get; set; }
    public virtual int ScheduleCounter { get; set; }
    public virtual int ReportCounter { get; set; }
}
