using System;
using System.CodeDom.Compiler;

namespace QueryingCore;

public class IndicatorRuleBuilder
{
    private readonly Guid _ruleId;
    private readonly string _name;

    public IndicatorRuleBuilder(Guid ruleId, string name)
    {
        _ruleId = ruleId;
        _name = name;
    }

    public string RuleIdConstantName => $"{_name}";
    public string RuleIdConstantType => Types.GetNetTypeString(typeof(Guid));
    public string RuleIdConstantValue => $"Guid.Parse(\"{_ruleId}\")";

    public void AppendRuleIdConstant(IndentedTextWriter writer)
    {
        writer.WriteLine($"public static readonly {RuleIdConstantType} {RuleIdConstantName} = {RuleIdConstantValue};");
    }

}
