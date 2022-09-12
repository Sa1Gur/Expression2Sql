using System;
using System.Collections.Generic;

namespace RestApi.Data.Models;

public class SourcePairModel
{
    public virtual string Name { get; set; }
    public virtual string? NameEn { get; set; }
    public virtual string Expression { get; set; }
    public virtual IDictionary<Guid, SourcePairItemModel> Items { get; set; }
    public virtual string LocalId { get; set; }
}
