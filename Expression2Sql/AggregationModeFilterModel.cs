namespace RestApi.Data.Models;

public class AggregationModeFilterModel : ILocalIdModel
{
    public virtual string LocalId { get; set; }
    public virtual string Name { get; set; }
    /// <summary>
    /// Данные дизайнера в json
    /// </summary>
    public virtual string FilterJson { get; set; }
    /// <summary>
    /// Продолжить при пустом наборе
    /// </summary>
    public virtual bool ContinueWithZero { get; set; }
    public virtual int Order { get; set; }
}
