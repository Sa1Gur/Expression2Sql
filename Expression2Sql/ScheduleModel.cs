using System;

namespace RestApi.Data.Models;

public class ScheduleModel
{
    public virtual int RecordsCount { get; set; }
    public virtual string TableName { get; set; }
    public Guid SourceId { get; set; }
    public virtual DateTime ActualAt { get; set; }
    public virtual string LocalId { get; set; }
}