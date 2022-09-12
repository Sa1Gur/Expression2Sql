namespace RestApi.Data.Models;

public class ProjectParameterModel
{
    public virtual string Name { get; set; }
    public virtual string NameEn { get; set; }
    public virtual string Tag { get; set; }
    public virtual DataTypes Type { get; set; }
    public virtual string Value { get; set; }
    public virtual string LocalId { get; set; }
}
