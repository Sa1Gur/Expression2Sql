using System;

namespace QueryingCore.Core;

public class IndicatorResultInternal
{
    public int Id { get; set; }
    
    public Guid RuleId { get; set; }

    public DateTime Date { get; set; }
}
