using System;

namespace RestApi.Data.Models;

public class IndicatorRuleModel
{
    public virtual string Name { get; set; }
    public virtual string NameEn { get; set; }
    public virtual string Text { get; set; }
    public virtual Guid? ColorId { get; set; }
    public virtual string Expression { get; set; }
    public virtual int Order { get; set; }
    public virtual string LocalId { get; set; }
}
