using System;

namespace QueryingCore.Core;

[AttributeUsage(AttributeTargets.Property)]
public class SourceIndicatorAttribute : Attribute
{
    public SourceIndicatorAttribute(string indicatorId)
    {
        IndicatorId = Guid.Parse(indicatorId);
    }

    public Guid IndicatorId { get; }
}
