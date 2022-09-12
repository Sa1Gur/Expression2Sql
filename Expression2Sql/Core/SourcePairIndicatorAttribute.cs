using System;

namespace QueryingCore.Core;

[AttributeUsage(AttributeTargets.Property)]
public class SourcePairIndicatorAttribute : Attribute
{
    public SourcePairIndicatorAttribute(string sourcePairId, string indicatorId)
    {
        SourcePairId = Guid.Parse(sourcePairId);
        IndicatorId = Guid.Parse(indicatorId);
    }

    public Guid SourcePairId { get; }
    public Guid IndicatorId { get; }
}
