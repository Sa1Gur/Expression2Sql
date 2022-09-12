using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using QueryingCore;
using Newtonsoft.Json;
using NUnit.Framework;
using RestApi.Data.Models;

namespace QueryingCoreTests.AggregativeIndicator;

[TestFixture]
public sealed class AggregativeIndicator_Specs
{
    readonly string _defaultPath = "AggregativeIndicator/project_AllData.json";
    public ProjectModel InitializeProjectScopeAndCreadeDbContext(out Guid mtoSourceId, out Guid sourcePairId)
    {
        using var textReader = new StreamReader(_defaultPath);
        using var jsonReader = new JsonTextReader(textReader);
        var project = new JsonSerializer().Deserialize<ProjectModel>(jsonReader);
        
        mtoSourceId = project.Sources.First(x => x.Value.Name == "MTO_learning").Key;
        sourcePairId = project.SourcePairs.First(x => x.Value.Name == "RD_MTO").Key;

        return project;
    }

    [Test]
    [Description("проверяет компиляцию текста в Lambda выражения")]
    [TestCase("x => true")]
    [TestCase("x => x.Code == \"someName\"")]
    [TestCase("x => x.Plan > CalculatedAt")]
    [TestCase("x => x.Plan > MTO_LActualAt")]
    [TestCase("x => x.Number < ic1")]
    [TestCase("x => x.RD_MTOItems.Any(y=>y.RD_unique == RD_uniqueRules.grey)")]
    [TestCase("x => x.UserField_mto > 0")]
    [TestCase("x => date_or_null(x.Plan) != null")]
    public void ShouldCovertStringToLambda(string expressionToTest)
    {
        var project = InitializeProjectScopeAndCreadeDbContext(out Guid mtoSourceId, out Guid sourcePairId);

        using var expressionScope = ExpressionScope.Create(project!, mtoSourceId, sourcePairId, expressionToTest);
        Assert.IsEmpty(expressionScope.Errors);
    }

    [Description("проверяет преобразование текста из SimpleSyntax в LambdaSyntax")]
    private static object? ToObject(Action<Type, object> initializer) => initializer;

    private static object?[][] _testSimpleSyntaxSource =
    {
        new object?[]
        {
            "col(\"Number\") > 10 || col(   \"MTO_L.Name Eq\" \r\n ).Contains(\"a\"  )",
            "x => x.Number > 10 || x.NameEq.Contains(\"a\"  )",
            null,
            false
        },
        new object?[]
        {
            "col(\"MTO_L.Number\"   ) > val(\"ic1\"  ) + val(\"int const 1\")",
            "x => x.Number > ic1 + ic1",
            null,
            false
        },        
        new object?[]
        {
            "col(\"MTO_L.Plan\") > max(\"Plan1\")",
            "x => x.Plan > x.RD_MTOItems.Max(y => y.Plan1)",
            null,
            true
        },
        new object?[]
        {
            "col(\"MTO_L.Plan\") > MiN(\"RDL.PlaN1\")",
            "x => x.Plan > x.RD_MTOItems.Min(y => y.Plan1)",
            null,
            true
        },
        new object?[]
        {
            "hasIndicator(\"RDL.RD_unique\",\"grey\")",
            "x => x.RD_MTOItems.Any(y => y.RD_unique == RD_uniqueRules.grey)",
            null,
            true
        },
        new object?[]
        {
            //идентификатор определяется и по Name, и по NameEn
            "hasIndicator(\"RDL.RD unique1\"  ,\t\r\n \"yellow\") || hasIndicator(\"RDL.yellow\",\"yellow\")",
            "x => x.RD_MTOItems.Any(y => y.RDunique1 == RDunique1Rules.yellow) || x.RD_MTOItems.Any(y => y.yellow == yellowRules.yellow)",
            null,
            true
        },
        new object?[]
        {
            "hasIndicator(\"RD_unique\",\"red\")",
            "x => x.RD_MTOItems.Any(y => y.RD_unique == RD_uniqueRules.red)",
            null,
            true
        },
        new object?[]
        {
            "hasIndicator(\"Инд. ПСД\",\"Серый\")",
            "x => x.RD_MTOItems.Any(y => y.ИндПСД == ИндПСДRules.Серый)",
            null,
            true
        },
        new object?[]
        {
            "hasIndicator(\"RDL.Инд. ПСД\",\"Серый\")",
            "x => x.RD_MTOItems.Any(y => y.ИндПСД == ИндПСДRules.Серый)",
            null,
            true
        },
        // в агрегативной функции атрибут должен распознаваться по Name, NameRu, NameEn с указанием тэга источника и без
        new object?[]
        {
            "max(\"Номер1\")>0 && max(\"Number1\")>0 && max(\"RDL.Номер1\")>0",
            "x => x.RD_MTOItems.Max(y => y.Number1)>0 && x.RD_MTOItems.Max(y => y.Number1)>0 && x.RD_MTOItems.Max(y => y.Number1)>0",
            null,
            true
        },
        // функция count должна распознаваться независимо от 
        new object?[]
        {
            "count(\"Number1\")>0 && count(\"RDL.Номер1\")>0 && count(  )>0",
            "x => x.RD_MTOItems.Count()>0 && x.RD_MTOItems.Count()>0 && x.RD_MTOItems.Count()>0",
            null,
            true
        },
        new object?[]
        {
            "col(\"UserField_mto\") > 0",
            "x => x.UserField_mto > 0",
            null,
            false
        },
        new object?[]
        {
            "date_or_null(col(\"MTO_L.Plan\")) != null",
            "x => date_or_null(x.Plan) != null",
            null,
            false
        },
        new object?[]
        {
            "col_eq  (  col  (  \"MTO_L.Plan\"  ) ,  col  (  \"MTO_L.Fact\"  )  )",
            "x => eq(x.Plan, x.Fact)",
            null,
            false
        },
        new object?[]
        {
            "col_not_eq  (  col  (  \"MTO_L.Plan\"  ) ,  col  (  \"MTO_L.Fact\"  )  )",
            "x => !eq(x.Plan, x.Fact)",
            null,
            false
        }
    };

    public static bool TestLambda<TSource>(TSource record, Expression expression)
    {
        var lambda = (Expression<Func<TSource, bool>>) expression;
        var method = lambda.Compile();
        return method(record);
    }

    [Test]
    [TestCaseSource(nameof(_testSimpleSyntaxSource))]
    public void TestSimpleSyntax(
        string simpleSyntax,
        string lambdaSyntaxExpected,
        Action<Type, object>? recordInitializer,
        bool expectedResult)
    {
        var project = InitializeProjectScopeAndCreadeDbContext(out Guid mtoSourceId, out Guid sourcePairId);

        using var expressionScope = ExpressionScope.Create(project!, mtoSourceId, sourcePairId, simpleSyntax);
        Assert.IsEmpty(expressionScope.Errors);
        
        if (recordInitializer == null) return;

        var recordType = expressionScope.Ctx.GetType().Assembly
            .GetType($"{expressionScope.Ctx.GetType().Namespace}.MTO_LRecord");
        var record = Activator.CreateInstance(recordType);
        recordInitializer(recordType, record);
        var (lambda, err) = expressionScope.CovertToLambda(
            mtoSourceId,
            sourcePairId);
        Assert.IsEmpty(err);

        var m = GetType().GetMethod(nameof(TestLambda));
        var genM = m.MakeGenericMethod(recordType);

        var actualResult = (bool) genM.Invoke(null, new[] {record, lambda});
        Assert.AreEqual(expectedResult, actualResult);
    }

    private static object[] _testSimpleSyntaxUniqueSource =
    {
        new object?[]
        {
            "col(\"Number\") > 10 || col(   \"MTO_L.Name Eq\" \r\n ).Contains(\"a\"  )",
            "x => x.Number > 10 || x.NameEq.Contains(\"a\"  )",
            null,
            false
        },
        new object?[]
        {
            "col(\"MTO_L.Number\"   ) > val(\"ic1\"  ) + val(\"int const 1\")",
            "x => x.Number > ic1 + ic1",
            null,
            false
        },        
        new object?[]
        {
            "col(\"UserField_mto\") > 0",
            "x => x.UserField_mto > 0",
            null,
            false
        }
    };

    [Test]
    [TestCaseSource(nameof(_testSimpleSyntaxUniqueSource))]
    public void TestSimpleSyntaxUniqueSource(
        string simpleSyntax,
        string lambdaSyntaxExpected,
        Action<Type, object>? recordInitializer,
        bool expectedResult)
    {
        var project = InitializeProjectScopeAndCreadeDbContext(out Guid mtoSourceId, out Guid sourcePairId);

        using var expressionScope = ExpressionScope.Create(project!, mtoSourceId, sourcePairId, simpleSyntax);
        Assert.IsEmpty(expressionScope.Errors);
        
        if (recordInitializer == null) return;

        var recordType = expressionScope.Ctx.GetType().Assembly
            .GetType($"{expressionScope.Ctx.GetType().Namespace}.MTO_LRecord");
        var record = Activator.CreateInstance(recordType);
        recordInitializer(recordType, record);
        var (lambda, err) = expressionScope.CovertToLambda(
            mtoSourceId,
            sourcePairId);
        Assert.IsEmpty(err);

        var m = GetType().GetMethod(nameof(TestLambda));
        var genM = m.MakeGenericMethod(recordType);

        var actualResult = (bool) genM.Invoke(null, new[] {record, lambda});
        Assert.AreEqual(expectedResult, actualResult);
    }    
}
