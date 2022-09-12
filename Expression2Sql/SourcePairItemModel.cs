using System;

namespace RestApi.Data.Models;

public class SourcePairItemModel
{
    public virtual Guid SourceId { get; set; }
    public virtual int Order { get; set; }
    public virtual string CodeSeparator { get; set; }
}
