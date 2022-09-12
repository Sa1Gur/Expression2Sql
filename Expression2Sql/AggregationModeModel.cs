using System;
using System.Collections.Generic;

namespace RestApi.Data.Models;

public class AggregationModeModel
{
    public virtual string Name { get; set; }
    public virtual string NameEn { get; set; }
    public virtual  Guid SourceId { get; set; }
    public virtual Guid SourcePairId { get; set; }
    public virtual IDictionary<Guid, AggregationModeFilterModel> Filters { get; set; }
    public virtual string LocalId { get; set; }
    public virtual int FilterCounter { get; set; }
}
