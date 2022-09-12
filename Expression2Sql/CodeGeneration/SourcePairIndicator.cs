using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace QueryingCore;

public class SourcePairIndicatorBuilder
{
    private readonly Guid _indicatorId;
    private readonly string _name;
    private readonly SourcePairBuilder _sourcePairBuilder;

    public SourcePairIndicatorBuilder(Guid indicatorId, string name, SourcePairBuilder sourcePairBuilder)
    {
        _indicatorId = indicatorId;
        _name = name;
        _sourcePairBuilder = sourcePairBuilder;
    }

    public void AddRule(Guid id, string name)
    {
        _rules.Add(id, new IndicatorRuleBuilder(id, name));
    }

    public SourcePairBuilder SourcePair => _sourcePairBuilder;
    public ISet<string> SimpleSyntaxIdentifiers => _simpleSyntaxIdentifiers;

    public string RecordPropertyName => $"{_name}Items";
    public string RecordPropertyItemType => Types.GetNetTypeString(typeof(Guid));
    public string RecordPropertyType => $"ICollection<{RecordPropertyItemType}>";

    public string IndicatorIdConstantName => $"{_name}Id";

    public string IndicatorIdConstantType => Types.GetNetTypeString(typeof(Guid));
    public string IndicatorIdConstantPath => $"Project.{IndicatorIdConstantName}";
    public string IndicatorIdConstantValue => $"Guid.Parse(\"{_indicatorId}\")";

    public string RulesClassName => $"{_name}Rules";
    public string SubqueryMethodName => $"Get{_name}Subquery";


    public void AppendIndicatorIdConstant(IndentedTextWriter writer)
    {
        writer.WriteLine($"public static readonly {IndicatorIdConstantType} {IndicatorIdConstantName} = {IndicatorIdConstantValue};");
    }


    public void AppendRecordProperty(IndentedTextWriter writer)
    {
        writer.WriteLine($"[SourcePairIndicator(\"{_sourcePairBuilder.SourcePairId}\",\"{_indicatorId}\")]");
        writer.WriteLine($"[RuleDependency(\"{_indicatorId}\")]");
        writer.WriteLine($"public {RecordPropertyType} {RecordPropertyName} {{ get; set; }}");
    }

    public void AppendConfigure(IndentedTextWriter writer)
    {
        writer.WriteLine($"builder.Ignore(x => x.{RecordPropertyName});");
    }

    public void AppendRulesClass(IndentedTextWriter writer)
    {
        writer.WriteLine($"public static class {RulesClassName}");
        writer.WriteLine("{");
        writer.Indent++;

        _rules.Values.ForEach(x => x.AppendRuleIdConstant(writer));

        writer.Indent--;
        writer.WriteLine("}");
    }

    public void AppendReplaceSubqueryMethod(IndentedTextWriter writer)
    {
        //var expressionFrom = $"x => x.{RecordPropertyName}";
        //// Expression<Func<TMain, TExt>>
        //var expressionTo = $"{GetProjectClassName()}.Get{GetStaticClassName(sourceName)}_{linkedItemsPropertyName}Subquery(ctx)";
        //_sb.AppendLine($".ReplaceSurrogate({expressionFrom}, {expressionTo})");
    }
    // добавляет колонку IQueryable<Guid?> SomeIndicator1Items в основной график


    // hasIndicator("SomeIndicator1","red") превращает в формулу x=>x.SomeIndicator1Items.Contains(SomeIndicator1Rules.Red)
    // x=>x.SomeIndicator1Items заменяет на подзапрос
    //


    private readonly IDictionary<Guid, IndicatorRuleBuilder> _rules = new Dictionary<Guid, IndicatorRuleBuilder>();
    private readonly ISet<string> _simpleSyntaxIdentifiers = new HashSet<string>();

}
