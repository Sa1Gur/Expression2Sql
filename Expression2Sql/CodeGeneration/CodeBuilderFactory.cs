using System.Linq;
using QueryingCore.CodeGeneration;
using RestApi.Data.Models;

namespace QueryingCore;

public class CodeBuilderFactory
{
    readonly Corrector _corrector;

    public CodeBuilderFactory(Corrector corrector) => _corrector = corrector;

    public CodeBuilder Create(ProjectModel project)
    {
        var codeBuilder = new CodeBuilder();

        project.Sources.ForEach(x => codeBuilder.AddSource(x.Key, _corrector.Correct(x.Value.Tag)));
        project.SourcePairs.ForEach(x => codeBuilder.AddSourcePair(x.Key, _corrector.Correct(x.Value.Name)));
        project.Indicators.Where(x => x.Value.Type == IndicatorTypes.SourcePair)
            .ForEach(x =>
            {
                var pair = project.GetSourcePairOrDefault(x.Value.SourcePairId!.Value);
                var sourcePairBuilder = codeBuilder.GetSourcePair(x.Value.SourcePairId!.Value);

                codeBuilder.AddSourcePairIndicator(x.Key, _corrector.Correct(x.Value.Name), sourcePairBuilder);
                var builder = codeBuilder.GetSourcePairIndicator(x.Key);
                x.Value.Rules.ForEach(r => builder.AddRule(r.Key, _corrector.Correct(r.Value.Text)));

                foreach (var item in pair!.Items.Values)
                {
                    var sourceBuilder = codeBuilder.GetSource(item.SourceId);
                    sourceBuilder.Add(x.Key, builder);
                }

                builder.SimpleSyntaxIdentifiers.Add(_corrector.Correct(x.Value.Name));
                builder.SimpleSyntaxIdentifiers.Add(x.Value.Name);
                if (!string.IsNullOrWhiteSpace(x.Value.NameEn))
                {
                    builder.SimpleSyntaxIdentifiers.Add(x.Value.NameEn);
                }
            });

        return codeBuilder;
    }
}
