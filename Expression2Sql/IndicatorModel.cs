using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RestApi.Data.Models;

public class IndicatorModel
{
    [JsonConverter(typeof(StringEnumConverter))]
    public virtual IndicatorTypes Type { get; set; }
    public virtual string Shape { get; set; }
    public virtual string Name { get; set; }
    public virtual string NameEn { get; set; }
    public virtual string Suffix { get; set; }
    public virtual Guid? SourceId { get; set; }
    public virtual Guid? SourcePairId { get; set; }
    public virtual Guid? GroupId { get; set; }
    public virtual IDictionary<Guid, IndicatorRuleModel> Rules { get; set; }
    public virtual IDictionary<Guid, IndicatorGroupItemModel> GroupItems { get; set; }
    public virtual bool BIEnabled { get; set; }
    public virtual string LocalId { get; set; }
    public virtual int RuleCounter { get; set; }
    public virtual int GroupItemCounter { get; set; }

    /// <summary>
    /// Ссылки на сущности упоминаемые в выражениях правил
    /// <see cref="IndicatorRuleModel.Expression"/> в правилах <see cref="Rules"/>
    /// </summary>
    public virtual IDictionary<string, DependencyModel> Dependencies { get; set; }
}
