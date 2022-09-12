namespace RestApi.Data.Models;

/// <summary>
/// <see cref="TFlexEntities.Attribute"/>
/// </summary>
public class SourceAttributeModel
{
    public virtual string ColumnName { get; set; }
    public virtual int Order { get; set; }
    public virtual string NameRu { get; set; }
    public virtual string Name { get; set; }
    public virtual string NameEn { get; set; }
    public virtual DataTypes Type { get; set; }
    public virtual string FilterJson { get; set; }
    public virtual string LocalId { get; set; }
    public virtual bool IsRequired { get; set; }
    /// <summary>
    /// Поле по-умолчанию (при первом открытии) видимо в BI,
    /// позже пользователь можент настроить видимость и порядок колонок самостоятельно
    /// </summary>
    public virtual bool BIEnabled { get; set; }
}
