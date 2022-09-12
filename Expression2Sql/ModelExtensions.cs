using System;
using System.Globalization;
using System.Linq;
using RestApi.Data.Models;

namespace QueryingCore;

public static class ModelExtensions
{
    public static ProjectModel? GetProjectOrDefault(this RootModel root, Guid projectId)
    {
        if (root == null) throw new ArgumentNullException(nameof(root));
        return root.Projects.ContainsKey(projectId) ? root.Projects[projectId] : default;
    }

    public static SourceModel? GetSourceOrDefault(this ProjectModel project, Guid sourceId)
    {
        if (project == null) throw new ArgumentNullException(nameof(project));
        return project.Sources.ContainsKey(sourceId) ? project.Sources[sourceId] : null;
    }

    public static SourceAttributeModel? GetSourceAttributeOrDefault(this SourceModel source, Guid attributeId)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        return source.Attributes.ContainsKey(attributeId) ? source.Attributes[attributeId] : null;
    }

    public static SourceAttributeModel? GetSourceAttributeOrDefault(this ProjectModel project, Guid sourceId, Guid attributeId)
    {
        if (project == null) throw new ArgumentNullException(nameof(project));
        return project.GetSourceOrDefault(sourceId)?.GetSourceAttributeOrDefault(attributeId);
    }

    public static IndicatorModel? GetIndicatorOrDefault(this ProjectModel project, Guid indicatorId)
    {
        if (project == null) throw new ArgumentNullException(nameof(project));
        return project.Indicators.ContainsKey(indicatorId) ? project.Indicators[indicatorId] : null;
    }

    public static IndicatorRuleModel? GetIndicatorRuleOrDefault(this IndicatorModel indicator, Guid indicatorRuleId)
    {
        if (indicator == null) throw new ArgumentNullException(nameof(indicator));
        return indicator.Rules.ContainsKey(indicatorRuleId) ? indicator.Rules[indicatorRuleId] : null;
    }

    public static IndicatorRuleModel? GetIndicatorRuleOrDefault(this ProjectModel project, Guid indicatorId, Guid indicatorRuleId)
    {
        if (project == null) throw new ArgumentNullException(nameof(project));
        return project.GetIndicatorOrDefault(indicatorId)?.GetIndicatorRuleOrDefault(indicatorRuleId);
    }

    public static ProjectParameterModel? GetParameterOrDefault(this ProjectModel project, Guid parameterId)
    {
        if (project == null) throw new ArgumentNullException(nameof(project));
        return project.Parameters.ContainsKey(parameterId) ? project.Parameters[parameterId] : null;
    }

    public static UserFieldModel? GetUserFieldOrDefault(this ProjectModel project, Guid userFieldId)
    {
        if (project == null) throw new ArgumentNullException(nameof(project));
        return project.UserFields.ContainsKey(userFieldId) ? project.UserFields[userFieldId] : null;
    }

    public static ReportTemplateModel? GetReportTemplateOrDefault(this ProjectModel project, Guid reportTemplateId)
    {
        if (project == null) throw new ArgumentNullException(nameof(project));
        return project.ReportTemplates.ContainsKey(reportTemplateId) ? project.ReportTemplates[reportTemplateId] : null;
    }

    public static SourcePairModel? GetSourcePairOrDefault(this ProjectModel project, Guid sourcePairId)
    {
        if (project == null) throw new ArgumentNullException(nameof(project));
        return project.SourcePairs.ContainsKey(sourcePairId) ? project.SourcePairs[sourcePairId] : null;
    }

    public static Guid GetAnotherSourceId(this ProjectModel project, Guid sourceId, Guid? sourcePairId)
    {
        if (!sourcePairId.HasValue || sourcePairId == Guid.Empty)
            return sourceId;
        if (project == null) throw new ArgumentNullException(nameof(project));
        var source = project.GetSourceOrDefault(sourceId);
        if (source == null) throw new ArgumentOutOfRangeException(nameof(sourceId));
        var sourcePair = project.GetSourcePairOrDefault(sourcePairId.Value);
        if (sourcePair == null) throw new ArgumentOutOfRangeException(nameof(sourcePairId));

        var currentSourceCount = sourcePair.Items.Values.Count(x => x.SourceId == sourceId);
        //проверка, что заданные источник и пара соответсвуют друг другу
        if (currentSourceCount == 0) return sourceId;

        if (currentSourceCount == 2) return sourceId;

        var anotherSourceIds = sourcePair.Items.Values.Where(x => x.SourceId != sourceId).ToList();

        //проверка, что пара содержит другой иcточник и что он один
        if (anotherSourceIds.Count != 1 || project.GetSourceOrDefault(anotherSourceIds[0].SourceId) == null)
            throw new ArgumentException("Invalid", nameof(sourcePairId));

        return anotherSourceIds[0].SourceId;
    }

    public static readonly string EnUS = "en-US";

    public static string GetLocalizedName(this SourceModel source) =>
        CultureInfo.CurrentUICulture.Name == EnUS ? source.NameEn.EnsureNotNullOrEmpty(source.Name) : source.Name;

    public static string GetLocalizedName(this SourceAttributeModel attribute) =>
        CultureInfo.CurrentUICulture.Name == EnUS ? attribute.NameEn.EnsureNotNullOrEmpty(attribute.NameRu) : attribute.NameRu;

    public static string GetLocalizedName(this IndicatorModel indicator) =>
        CultureInfo.CurrentUICulture.Name == EnUS ? indicator.NameEn.EnsureNotNullOrEmpty(indicator.Name) : indicator.Name;
    
    public static string GetLocalizedName(this IndicatorRuleModel rule) =>
        CultureInfo.CurrentUICulture.Name == EnUS ? rule.NameEn.EnsureNotNullOrEmpty(rule.Name) : rule.Name;
    
    public static string GetLocalizedName(this UserFieldModel userField) =>
        CultureInfo.CurrentUICulture.Name == EnUS ? userField.NameEn.EnsureNotNullOrEmpty(userField.Name) : userField.Name;

    public static string EnsureNotNullOrEmpty(this string value, string defaultValue)
        => !string.IsNullOrEmpty(value) ? value : defaultValue;

}
