namespace RestApi.Data.Models;

public class IndicatorGroupItemModel
{
    public virtual string LocalId { get; set; }
    public virtual string Plan { get; set; }
    public virtual string Fact { get; set; }
    public virtual int Order { get; set; }
}
