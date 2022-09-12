using System;

namespace QueryingCore.Core;

public class SingleCollisionRecord
{
    public Guid SourceId { get; set; }
    public int RowId { get; set; }
    public Guid IndicatorId { get; set; }
    public Guid IndicatorRuleId { get; set; }
}
