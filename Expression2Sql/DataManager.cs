using System;
using System.Collections.Generic;
using System.Linq;
using QueryingCore.CodeGeneration;
using RestApi.Data.Models;

namespace QueryingCore;

public class DataManager
{
    public Corrector Corrector { get; }

    readonly ProjectModel _data;

    public CodeGenerator CodeGenerator { get; internal set; }
    
    public Guid TheVersion => _data.Versions.OrderByDescending(x => x.Value.Number).First().Key;
    public DataManager(Corrector corrector, ProjectModel data) => (Corrector, _data) = (corrector, data);

    public IEnumerable<(IEnumerable<string> identifiers, string? name, Type type, string value)> GetConstants() =>
        _data.Parameters
           .Values.Select(x => (GetIdentifiers(x), name: GetName(x), GetType(x), GetValue(x)))
           .OrderBy(x => x.name)
           .Where(x => x.Item3 != typeof(Types.Undefined));

    public IEnumerable<(IEnumerable<string> identifiers, string tag, Guid id)> GetSources() =>
        _data
            .Sources
            .Select(x => (GetIdentifiers(x.Value), tag: GetTag(x.Value), x.Key))
            .OrderBy(x => x.tag);

    static IEnumerable<string> GetIdentifiers(SourceModel source)
    {
        yield return source.Tag;

        if (!string.IsNullOrWhiteSpace(source.Name))
        {
            yield return source.Name;
        }
        if (!string.IsNullOrWhiteSpace(source.NameEn))
        {
            yield return source.NameEn;
        }
    }

    public IEnumerable<(IEnumerable<string> identifiers, string? propertyName, Type propertyType, string columnName)> GetSourceAttributes(Guid sourceId)
    {
        var source = _data.Sources.Single(x => x.Key == sourceId).Value;
        var attributes = source.Attributes.Select(z => z.Value);
        var allAttributes = attributes.Select(x => (getName(x), propertyName: getPropertyName(x), getPropertyType(x), getColumnName(x)))
            .OrderBy(x => x.propertyName);
        return allAttributes.Where(x => x.Item3 != typeof(Types.Undefined));

        IEnumerable<string> getName(SourceAttributeModel attribute)
        {
            yield return attribute.Name;

            if (!string.IsNullOrWhiteSpace(attribute.NameRu))
            {
                yield return attribute.NameRu;
            }

            if (!string.IsNullOrWhiteSpace(attribute.NameEn))
            {
                yield return attribute.NameEn;
            }
        }

        string getPropertyName(SourceAttributeModel attribute) => CodeGenerator.GetPropertyName(attribute);

        Type getPropertyType(SourceAttributeModel attribute) => Types.GetNetType(attribute.Type);

        string getColumnName(SourceAttributeModel attribute) => attribute.ColumnName;
    }
    
    public IEnumerable<string> GetUserFields(Guid sourceId) => _data.UserFields.Values.Select(x => x.Name);

    public IEnumerable<(IEnumerable<string> identifiers, string? name, Guid id)> GetSourceIndicators(Guid sourceId) =>
        _data
           .Indicators.Where(x => x.Value.SourceId == sourceId && x.Value.Type != IndicatorTypes.GroupIndicator)
           .Select(x => (GetIdentifiers(x.Value), name: GetName(x.Value), x.Key))
           .OrderBy(x => x.name);

    IEnumerable<string> GetIdentifiers(IndicatorModel indicator)
    {
        yield return indicator.Name;

        if (!string.IsNullOrWhiteSpace(indicator.NameEn))
            yield return indicator.NameEn;
    }

    public IEnumerable<(IEnumerable<string> identifiers, string name, Guid id)> GetSourceIndicatorRules(Guid indicatorId)
    {
        var indicator = _data.Indicators.Single(z => z.Key == indicatorId).Value;
        return indicator.Rules.Select(x => (getIdentifiers(x.Value), name: GetName(x.Value), x.Key))
           .OrderBy(x => x.name);

        IEnumerable<string> getIdentifiers(IndicatorRuleModel rule)
        {
            yield return rule.Text;

            if (!string.IsNullOrWhiteSpace(rule.Name))
                yield return rule.Name;

            if (!string.IsNullOrWhiteSpace(rule.NameEn))
                yield return rule.NameEn;
        }
    }

    public IEnumerable<(string linkedItemsPropertyName, Guid sourcePairId, Guid otherSourceId)> GetSourceLinkedItems(Guid sourceId)
    {
        return _data.SourcePairs
            .Where(z => z.Value.Items.Values.Any(x => x.SourceId == sourceId))
            .Select(x => (name: GetName(x.Value), x.Key, getOtherSourceId(x.Value)))
            .OrderBy(x => x.name);

        Guid getOtherSourceId(SourcePairModel sourcePair) => sourcePair.Items.Values.FirstOrDefault(z => z.SourceId != sourceId)?.SourceId ?? sourceId;
    }

    public (string, string, string, DateTime, IDictionary<Guid, DateTime>) GetVersion(Guid versionId)
    {
        if (!_data.Versions.ContainsKey(versionId))
            return default;

        var version = _data.Versions[versionId];

        return (version.SourcePairConnections, version.SourceCollisions, version.SourcePairCollisions,
            version.CalculatedAt!.Value,
            version.Schedules.Select(x => new KeyValuePair<Guid, DateTime>(x.Value.SourceId, x.Value.ActualAt)).ToDictionary(x => x.Key, x => x.Value));
    }

    public string GetSchedule(Guid sourceId, Guid versionId) =>
        _data.Versions.Where(x => x.Key == versionId).SelectMany(z => z.Value.Schedules.Values).Where(x => x.SourceId == sourceId).Select(x => x.TableName).FirstOrDefault();

    public IDictionary<Guid, VersionModel> GetAllVersions() => _data.Versions;

    static IEnumerable<string> GetIdentifiers(ProjectParameterModel parameter)
    {
        yield return parameter.Tag;

        yield return parameter.Name;

        yield return Constants.CalculatedDate;

        if (!string.IsNullOrWhiteSpace(parameter.NameEn))
            yield return parameter.NameEn;
    }

    string GetTag(SourceModel source) => Corrector.Correct(source.Tag);

    string GetName(ProjectParameterModel parameter) => CodeGenerator.GetProjectConstantName(parameter);

    string GetName(IndicatorModel indicator) => Corrector.Correct(indicator.Name);

    string GetName(IndicatorRuleModel rule) => Corrector.Correct(rule.Text);

    string GetName(SourcePairModel sourcePair) => CodeGenerator.GetLinkedItemsPropertyName(sourcePair);

    static Type GetType(ProjectParameterModel parameter) => Types.GetNetType(parameter.Type);

    static string GetValue(ProjectParameterModel parameter) => parameter.Value;
}
