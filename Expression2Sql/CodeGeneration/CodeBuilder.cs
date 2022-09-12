using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;

namespace QueryingCore;

public class CodeBuilder
{
    readonly IDictionary<Guid, SourceBuilder> _sources = new Dictionary<Guid, SourceBuilder>();

    readonly IDictionary<Guid, SourcePairBuilder> _sourcePairs = new Dictionary<Guid, SourcePairBuilder>();

    readonly IDictionary<Guid, SourcePairIndicatorBuilder> _sourcePairIndicators = new Dictionary<Guid, SourcePairIndicatorBuilder>();

    public void AddSource(Guid sourceId, string name) => _sources.Add(
        sourceId,
        new SourceBuilder(sourceId, name));

    public SourceBuilder GetSource(Guid sourceId) => _sources.ContainsKey(sourceId)
        ? _sources[sourceId]
        : throw new ArgumentOutOfRangeException(nameof(sourceId));

    public void AddSourcePair(Guid sourcePairId, string name) => _sourcePairs.Add(
        sourcePairId,
        new SourcePairBuilder(sourcePairId, name));

    public SourcePairBuilder GetSourcePair(Guid sourcePairId) => _sourcePairs.ContainsKey(sourcePairId)
        ? _sourcePairs[sourcePairId]
        : throw new ArgumentOutOfRangeException(nameof(sourcePairId));

    public void AddSourcePairIndicator(Guid indicatorId, string name, SourcePairBuilder sourcePairBuilder) => _sourcePairIndicators.Add(
       indicatorId,
       new SourcePairIndicatorBuilder(indicatorId, name, sourcePairBuilder));

    public SourcePairIndicatorBuilder GetSourcePairIndicator(Guid indicatorId) => _sourcePairIndicators.ContainsKey(indicatorId)
        ? _sourcePairIndicators[indicatorId]
        : throw new ArgumentOutOfRangeException(nameof(indicatorId));

    public Guid FindSourcePairIndicator(string simpleSyntaxIdentifier)
    {
        return _sourcePairIndicators
            .Where(x => x.Value.SimpleSyntaxIdentifiers.Contains(simpleSyntaxIdentifier))
            .Select(x => x.Key)
            .FirstOrDefault();
    }

    public void AppendClasses(IndentedTextWriter writer)
    {
        _sourcePairIndicators.Values.OrderBy(x => x.RulesClassName).ForEach(x => x.AppendRulesClass(writer));
    }

    public void AppendProjectClassContent(IndentedTextWriter writer)
    {
        _sourcePairIndicators.Values.OrderBy(x => x.IndicatorIdConstantName)
            .ForEach(x => x.AppendIndicatorIdConstant(writer));

        _sources.Values.OrderBy(x => x.SourceIdConstantName)
            .ForEach(x => x.AppendProjectClassContent(writer));
    }
}
