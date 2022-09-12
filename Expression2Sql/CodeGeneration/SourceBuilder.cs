using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;

namespace QueryingCore;

public class SourceBuilder
{
    readonly Guid _id;

    readonly string _name;

    // список индикаторов на пару по идентификатору индикатора
    readonly IDictionary<Guid, SourcePairIndicatorBuilder> _sourcePairIndicators = new Dictionary<Guid, SourcePairIndicatorBuilder>();

    public SourceBuilder(Guid id, string name)
    {
        _id = id;
        _name = name;
    }

    public string RecordTypeName => $"{_name}Record";
    public string SourceIdConstantName => $"{_name}SourceId";
    public string SourceIdConstantPath => $"Project.{SourceIdConstantName}";

    public void Add(Guid indicatorId, SourcePairIndicatorBuilder item)
    {
        _sourcePairIndicators.Add(indicatorId, item);
    }

    public void AppendRecordClassContent(IndentedTextWriter writer)
    {
        _sourcePairIndicators.Values.OrderBy(x => x.RecordPropertyName)
            .ForEach(x => x.AppendRecordProperty(writer));
    }

    public void AppendConfigureContent(IndentedTextWriter writer)
    {
        _sourcePairIndicators.Values.OrderBy(x => x.RecordPropertyName)
            .ForEach(x => x.AppendConfigure(writer));
    }

    public void AppendReplaceSurrogatesMethodContent(IndentedTextWriter writer)
    {
        foreach (var item in _sourcePairIndicators)
        {
            string from = $"x => x.{item.Value.RecordPropertyName}";
            string to = $"Project.{GetSourcePairIndicatorSubqueryMethodName(item.Key)}(ctx)";
            writer.WriteLine($".ReplaceSurrogate({from}, {to})");
        }
    }

    public string GetSourcePairIndicatorSubqueryMethodName(Guid indicatorId)
    {
        return $"Get{_name}{_sourcePairIndicators[indicatorId].RecordPropertyName}Subquery";
    }

    public void AppendSourcePairIndicatorSubqueryMethod(Guid indicatorId, IndentedTextWriter writer)
    {
        string itemType = _sourcePairIndicators[indicatorId].RecordPropertyItemType;
        string itemsType = $"IEnumerable<{itemType}>";
        writer.Write($"public static Expression<Func<{RecordTypeName}, {itemsType}>> ");
        writer.WriteLine($"{GetSourcePairIndicatorSubqueryMethodName(indicatorId)}(DbContext ctx)");
        writer.WriteLine("{");
        writer.Indent++;

        string sourcePairId = _sourcePairIndicators[indicatorId].SourcePair.SourcePairIdConstantPath;
        string indicatorIdConst = _sourcePairIndicators[indicatorId].IndicatorIdConstantPath;

        writer.Write("var oneInPair = ctx.Set<PairConnectionRecord>().Where(x => ");
        writer.WriteLine($"x.SourcePairId == {sourcePairId} && x.SourceId == {SourceIdConstantPath});");

        writer.Write("var indicators = ctx.Set<PairCollisionRecord>().Where(x => ");
        writer.WriteLine($"x.SourcePairId == {sourcePairId} && x.IndicatorId == {indicatorIdConst});");

        writer.Write("var p = oneInPair.Join(indicators, sp2 => sp2.Link, sp3 => sp3.Link, ");
        writer.WriteLine("(sp2, sp3) => new { sp2.RowId, RuleId = sp3.IndicatorRuleId });");

        writer.WriteLine("return x => p.Where(z => z.RowId == x.Id).Select(p => p.RuleId);");

        writer.Indent--;
        writer.WriteLine("}");
    }

    public void AppendProjectClassContent(IndentedTextWriter writer)
    {
        _sourcePairIndicators.OrderBy(x => x.Value.RecordPropertyName)
            .ForEach(x => AppendSourcePairIndicatorSubqueryMethod(x.Key, writer));
    }
}