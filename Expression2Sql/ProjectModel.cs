using System;
using System.Collections.Generic;

namespace RestApi.Data.Models;

public class ProjectModel
{
    public virtual string Name { get; set; }
    public virtual string NameEn { get; set; }
    public virtual IDictionary<Guid, SourceModel> Sources { get; set; }
    public virtual IDictionary<Guid, SourcePairModel> SourcePairs { get; set; }
    public virtual IDictionary<Guid, IndicatorModel> Indicators { get; set; }
    public virtual IDictionary<Guid, ProjectParameterModel> Parameters { get; set; }
    public virtual IDictionary<Guid, UserFieldModel> UserFields { get; set; }
    public virtual IDictionary<Guid, VersionModel> Versions { get; set; }
    public virtual IDictionary<Guid, ReportTemplateModel> ReportTemplates { get; set; }
    public virtual IDictionary<Guid, AggregationModeModel> AggregationModes { get; set; }

    public virtual int ParameterCounter { get; set; }
    public virtual int SourceCounter { get; set; }
    public virtual int SourcePairCounter { get; set; }
    public virtual int IndicatorCounter { get; set; }
    public virtual int UserFieldCounter { get; set; }
    public virtual int AggregationCounter { get; set; }
    public virtual int ReportTemplateCounter { get; set; }
    public virtual int VersionCounter { get; set; }
}
