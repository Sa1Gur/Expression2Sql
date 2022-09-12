using System;
using System.Collections.Generic;

namespace RestApi.Data.Models;

public class ReportTemplateModel
{
    public virtual string Name { get; set; }
    public virtual string NameEn { get; set; }
    public virtual bool ShowOnlyUnjoined { get; set; }
    public virtual bool UseAggregationMode { get; set; }
    public virtual bool IncludeUnjoinedPrimary { get; set; }
    public virtual bool IncludeUnjoinedSecondary { get; set; }
    public virtual string Comments { get; set; }
    public virtual string UserName { get; set; }
    public virtual DateTime CreatedAt { get; set; }
    public virtual Guid? SourceId { get; set; }
    
    public virtual IDictionary<string, ReportTemplateColumnModel> Columns { get; set; }//Todo: why it's string?
    public virtual IDictionary<Guid, ReportTemplateSourcePairLinkModel> SourcePairLinks { get; set; }
    public virtual string LocalId { get; set; }
}
