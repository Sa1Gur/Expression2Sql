using System;

namespace QueryingCore.Core;

public class PairCollisionRecord
{
    public Guid SourcePairId { get; set; }
    public int Link { get; set; }
    public Guid IndicatorId { get; set; }
    public Guid IndicatorRuleId { get; set; }
}
