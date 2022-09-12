using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using QueryingCore.CodeGeneration;

namespace QueryingCore.SimpleSyntaxScope;

sealed class SimpleSyntaxScope
{
    readonly DataManager _dataManager;
    readonly Guid _sourceId;
    readonly Guid? _sourcePairId;
    string _sourceTag;
    readonly CodeBuilder _codeBuilder;

    Guid _otherSourceId;
    string _linkedItemsPropertyName;
    
    readonly IList<(Group, string)> _errors = new List<(Group, string)>();
    
    public SimpleSyntaxScope(DataManager dataManager, CodeBuilder codeBuilder, Guid sourceId, Guid? sourcePairId)
    {
        _dataManager = dataManager;
        _sourceId = sourceId;
        _sourcePairId = sourcePairId;
        _codeBuilder = codeBuilder;
    }

    public (string expression, IEnumerable<string> errors) ConvertToLambdaSyntax(string rule)
    {
        _errors.Clear(); //todo it's not right to clean errors and fill them in background

        (_, _sourceTag, _) = _dataManager.GetSources().First(x => x.id == _sourceId);

        if (_sourcePairId.HasValue)
        {
            (_linkedItemsPropertyName, _, _otherSourceId) = _dataManager.GetSourceLinkedItems(_sourceId)
            .FirstOrDefault(x => x.sourcePairId == _sourcePairId);
        }

        rule = col_eq.Replace(rule, RuleEvaluator);
        rule = col.Replace(rule, PropertyEvaluator);
        rule = val.Replace(rule, FiltredConstantEvaluator);

        if (_sourcePairId.HasValue)
        {
            rule = sum.Replace(rule, m => AggregateEvaluator(nameof(Enumerable.Sum), m));
            rule = min.Replace(rule, m => AggregateEvaluator(nameof(Enumerable.Min), m));
            rule = max.Replace(rule, m => AggregateEvaluator(nameof(Enumerable.Max), m));
            rule = avg.Replace(rule, m => AggregateEvaluator(nameof(Enumerable.Average), m));

            rule = count.Replace(rule, m => CountEvaluator(m));

            rule = hasIndicator.Replace(rule, HasIndicatorEvaluator);
        }

        return (_errors.Any() ? null : $"x => {rule}", _errors.Select(x => $"{x.Item2}: {x.Item1.Value}"));
    }

    public (string expression, IEnumerable<string> errors) ConvertToLambdaSyntax<T>(string rule)
    {
        _errors.Clear(); //todo it's not right to clean errors and fill them in background

        (_, _sourceTag, _) = _dataManager.GetSources().First(x => x.id == _sourceId);

        if (_sourcePairId.HasValue)
        {
            (_linkedItemsPropertyName, _, _otherSourceId) = _dataManager.GetSourceLinkedItems(_sourceId)
            .FirstOrDefault(x => x.sourcePairId == _sourcePairId);
        }

        rule = col_eq.Replace(rule, RuleEvaluator);
        rule = col.Replace(rule, PropertyEvaluator);
        rule = val.Replace(rule, FiltredConstantEvaluator);

        if (_sourcePairId.HasValue)
        {
            rule = sum.Replace(rule, m => AggregateEvaluator(nameof(Enumerable.Sum), m));
            rule = min.Replace(rule, m => AggregateEvaluator(nameof(Enumerable.Min), m));
            rule = max.Replace(rule, m => AggregateEvaluator(nameof(Enumerable.Max), m));
            rule = avg.Replace(rule, m => AggregateEvaluator(nameof(Enumerable.Average), m));

            rule = count.Replace(rule, m => CountEvaluator(m));

            rule = hasIndicator.Replace(rule, HasIndicatorEvaluator);
        }

        return (_errors.Any() ? null : $"x => ({CodeGenerator.GetTypeName<T>()})({rule})", _errors.Select(x => $"{x.Item2}: {x.Item1.Value}"));
    }

    

    string? ConstantEvaluator(Match match) => match.Groups[1].Value switch
    {
        Constants.VersionCalculatedDate => CodeGenerator.CalculatedAt,
        Constants.CalculatedDate => $"{_sourceTag}{GetSourceAttributeOrUserFieldPropertyName(_sourceId, match.Groups[1])}",
        _ when match.Groups[1].Value.EndsWith($".{Constants.CalculatedDate}", StringComparison.Ordinal) =>
            $"{match.Groups[1].Value.Split('.')[0]}{CodeGenerator.ActualAt}",
        _ when GetConstantPropertyName(match.Groups[1]) == null => match.Value,
        _ => GetConstantPropertyName(match.Groups[1]) ?? "error"
    };

    string FiltredConstantEvaluator(Match match)
    {
        if (_otherSourceId != default)
        {
            return $"x.{_linkedItemsPropertyName}.Min(y => {ConstantEvaluator(match)})";
        }
        return ConstantEvaluator(match);
    }

    string? GetConstantPropertyName(Group group)
    {
        if (string.IsNullOrWhiteSpace(group.Value))
        {
            _errors.Add((group, "ConstantIdentifierUndefined"));
            return null;
        }

        var (_, name, _, _) = _dataManager.GetConstants().SingleOrDefault(
            x => x.identifiers.Any(i =>
                string.Equals(i, group.Value, StringComparison.InvariantCultureIgnoreCase)));

        if (name == null)
        {
            _errors.Add((group, "ConstantIdentifierUnknown"));
        }

        return name;
    }

    string PropertyEvaluator(Match match)
    {
        var propertyName = GetSourceAttributeOrUserFieldPropertyName(_sourceId, match.Groups[1]);

        return (propertyName == null ? match.Value : $"x.{propertyName}");
    }

    string RuleEvaluator(Match match) => match.Groups[0].Value switch
    {
        _ when match.Groups[0].Value
            .StartsWith("col_eq") => $"eq(col(\"{match.Groups[1].Value.Trim()}\"), col(\"{match.Groups[2].Value.Trim()}\"))",
        _ when match.Groups[0].Value
            .StartsWith("col_not_eq") => $"!eq(col(\"{match.Groups[1].Value.Trim()}\"), col(\"{match.Groups[2].Value.Trim()}\"))",
        _ => throw new ArgumentOutOfRangeException()
    };

    string? GetSourceAttributeOrUserFieldPropertyName(Guid expectedSource, Group group)
    {
        var arr = group.Value.Split('.');
        if (arr.Length > 2)
        {
            _errors.Add((group, "SourceIdentifierInvalid"));
            return null;
        }

        if (arr.Length > 1)
        {
            if (string.IsNullOrWhiteSpace(arr[0]))
            {
                _errors.Add((group, "SourceIdentifierUndefined"));
                return null;
            }
            var (_, _, actualSourceId) = _dataManager.GetSources()
                .FirstOrDefault(x => x.identifiers.Any(i => string.Equals(i, arr[0], StringComparison.InvariantCultureIgnoreCase)));

            if (actualSourceId == default)
            {
                _errors.Add((group, "SourceIdentifierUnknown"));
                return null;
            }

            if (actualSourceId != expectedSource)
            {
                _errors.Add((@group, "SourceIdentifierUnexpected"));
                return null;
            }
            return Finish(expectedSource, arr[1]);
        }

        return Finish(expectedSource, arr[0]);

        string? Finish(Guid sourceId, string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                _errors.Add((group, "SourceAttribute or UserField Identifier undefined"));
                return null;
            }

            var (_, propertyName, _, _) = _dataManager.GetSourceAttributes(sourceId)
           .SingleOrDefault(x => x.identifiers.Any(i => string.Equals(i, identifier, StringComparison.InvariantCultureIgnoreCase)));
            if (propertyName != null) return propertyName;

            var userField = _dataManager.GetUserFields(sourceId).SingleOrDefault(x => x.Equals(identifier, StringComparison.InvariantCultureIgnoreCase));
            if (userField != null)
                return _dataManager.Corrector.Correct(userField);

            if (Constants.CalculatedDate.Equals(identifier, StringComparison.InvariantCultureIgnoreCase))
                return CodeGenerator.ActualAt;

            _errors.Add((group, "SourceAttribute or UserField Identifier Unknown"));
            
            return null;
        }
    }

    string CountEvaluator(Match match)
    {
        if (match.Groups[1].Success &&
            GetSourceAttributeOrUserFieldPropertyName(_otherSourceId, match.Groups[2]) is null)
        {
            return match.Value;
        }
        
        return $"x.{_linkedItemsPropertyName}.Count()";
    }

    string AggregateEvaluator(string method, Match match)
    {
        string? propertyName = GetSourceAttributeOrUserFieldPropertyName(_otherSourceId, match.Groups[1]);
        if (propertyName == null)
        {
            return match.Value;
        }
        return $"x.{_linkedItemsPropertyName}.{method}(y => y.{propertyName})";
    }

    //TODO what is it?
    const string SourcePairIndicatorSign = "метка-индикатора-на-пару-{7F37DA9B-4463-4C46-B733-16C4CB0C4A3A}";
    string HasIndicatorEvaluator(Match match)
    {
        string methodName = nameof(Enumerable.Any);

        var (indicatorName, indicatorId) = GetIndicator(_otherSourceId, match.Groups[1]);

        if (indicatorName == null)
        {
            return match.Value;
        }

        string? indicatorRuleName = GetIndicatorRuleName(indicatorId, match.Groups[2]);
        if (indicatorRuleName == null)
        {
            return match.Value;
        }

        if (indicatorName == SourcePairIndicatorSign)
        {
            var builder = _codeBuilder.GetSourcePairIndicator(indicatorId);
            return $"x.{builder.RecordPropertyName}.{methodName}(y => y == {builder.RulesClassName}.{indicatorRuleName})";
        }

        return
            $"x.{_linkedItemsPropertyName}.{methodName}(y => y.{indicatorName} == {indicatorName}Rules.{indicatorRuleName})";

    }
    (string? name, Guid id) GetIndicator(Guid expectedSource, Group group)
    {
        var (_, name, id) = _dataManager.GetSourceIndicators(expectedSource)
            .FirstOrDefault(x => x.identifiers.Any(i => string.Equals(i, group.Value, StringComparison.InvariantCultureIgnoreCase)));

        if (id != default)
        {
            return Finish(expectedSource, group.Value);
        }

        var arr = group.Value.Split('.');
        if (arr.Length > 1)
        {
            if (string.IsNullOrWhiteSpace(arr[0]))
            {
                _errors.Add((group, "SourceIdentifierUndefined"));
                return default((string, Guid));
            }

            var (_, _, actualSourceId) = _dataManager.GetSources()
                .FirstOrDefault(x =>
                    x.identifiers.Any(i => string.Equals(i, arr[0], StringComparison.InvariantCultureIgnoreCase)));
        
            if (actualSourceId == default)
            {
                _errors.Add((group, "SourceIdentifierUnknown"));
                return default((string, Guid));
            }
            
            if (actualSourceId != expectedSource)
            {
                _errors.Add((group, "SourceIdentifierUnexpected"));
                return default((string, Guid));
            }

            return Finish(expectedSource, group.Value.Substring(arr[0].Length + 1));
        }

        return Finish(expectedSource, arr[0]);

        (string name, Guid id) Finish(Guid sourceId, string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                _errors.Add((group, "IndicatorIdentifierUndefined"));
                return default((string, Guid));
            }

            var sourcePairIndicatorId = _codeBuilder.FindSourcePairIndicator(identifier);
            if (sourcePairIndicatorId != default)
            {
                return (SourcePairIndicatorSign, sourcePairIndicatorId);
            }

            var (_, name, id) = _dataManager.GetSourceIndicators(sourceId)
                .FirstOrDefault(x => x.identifiers.Any(i => string.Equals(i, identifier, StringComparison.InvariantCultureIgnoreCase)));

            if (name == null)
            {
                _errors.Add((group, "IndicatorIdentifierUnknown"));
                return default((string, Guid));
            }

            return (name, id);
        }
    }
    string? GetIndicatorRuleName(Guid indicatorId, Group group)
    {
        if (string.IsNullOrWhiteSpace(group.Value))
        {
            _errors.Add((group, "IndicatorRuleIdentifierUndefined"));
        }

        var (_, name, _) = _dataManager.GetSourceIndicatorRules(indicatorId)
            .Where(x => x.identifiers.Any(i => string.Equals(i, group.Value, StringComparison.InvariantCultureIgnoreCase)))
            .FirstOrDefault();

        if (name == null)
        {
            _errors.Add((group, "IndicatorIdentifierUnknown"));
        }
        return name;
    }

    const string ValueInBraketsAndQuotes = "\\s*\\(\\s*\"([^\"\\)]*)\"\\s*\\)";
    static readonly string twoColValueInBrakets = $"\\s*\\(\\s*col{ValueInBraketsAndQuotes}\\s*,\\s*col{ValueInBraketsAndQuotes}\\s*\\)";
    readonly Regex col_eq = new Regex($"col_(?:not_)?eq{twoColValueInBrakets}", RegexOptions.IgnoreCase);
    readonly Regex col = new Regex($"col{ValueInBraketsAndQuotes}", RegexOptions.IgnoreCase);
    readonly Regex val = new Regex($"val{ValueInBraketsAndQuotes}", RegexOptions.IgnoreCase);
    readonly Regex sum = new Regex($"sum{ValueInBraketsAndQuotes}", RegexOptions.IgnoreCase);
    readonly Regex min = new Regex($"min{ValueInBraketsAndQuotes}", RegexOptions.IgnoreCase);
    readonly Regex max = new Regex($"max{ValueInBraketsAndQuotes}", RegexOptions.IgnoreCase);
    readonly Regex avg = new Regex($"avg{ValueInBraketsAndQuotes}", RegexOptions.IgnoreCase);
    
    readonly Regex count = new Regex("count\\s*\\(\\s*(\"([^\"\\)]*)\"\\s*)?\\)", RegexOptions.IgnoreCase);

    readonly Regex hasIndicator = new Regex("hasIndicator\\s*\\(\\s*\"([^\"\\)]*)\"\\s*,\\s*\"([^\"\\)]*)\"\\s*\\)", RegexOptions.IgnoreCase);
}