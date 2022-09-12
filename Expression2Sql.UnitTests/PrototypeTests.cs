using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using SomeNameSpace;
using RestApi.Data.Models;
using QueryingCore;
using QueryingCore.Core;

namespace QueryingCoreTests;

public class PrototypeTests
{   

    [TestCase]
    public void Test()
    {
        var options = new DbContextOptionsBuilder().UseNpgsql("Host=_;Database=_;Username=_;Password=_);").Options;
        using DbContext dbContext = new DbContext_00000000500100000000000000000000(options);

        FactoryBase factory = new Factory();
        var filterContextKey = new Guid("00000000-1002-0000-0000-000000000000");
        var filterContext = factory.GetContext(filterContextKey);

        var filter = (LambdaExpression)QueryingContext<RDLRecord>.CSharpScriptEvaluate(typeof(FilterWrapper), "_date_00000000300210010000000000000000");
        Guid sourceId = new Guid("00000000-1001-0000-0000-000000000000");
        var aggregativeContextKey = (sourceId,  new Guid("00000000-2001-0000-0000-000000000000"));
        var aggregativeContext = factory.GetContext(aggregativeContextKey);
        
        var expr = (LambdaExpression)QueryingContext<RDLRecord>.CSharpScriptEvaluate(typeof(ExpressionWrapper), "_00000000300210010000000000000000");

        var rulesList = new List<(Guid ruleId, Expression ruleExpression)>();
        rulesList.Add((Guid.NewGuid(), new ConstantsResolver().Visit(expr)));

        var query0 = factory.GetQuery(dbContext, sourceId, rulesList);
        string strRes = query0.ToQueryString();

        Assert.True(true);
    }

    [TestCase]
    public void TestSql()
    {
        var project = JObject.Parse(File.ReadAllText($"PrototypeTests.json"))
            .ToObject<ProjectModel>();

        UserFieldModel userField = new UserFieldModel
        {
            SourceId = new Guid("00000000-1001-0000-0000-000000000000"),
            SourcePairId = new Guid("00000000-2001-0000-0000-000000000000"),
            Expression = "x => true",//"hasIndicator(\"PairIndicator1\",\"grey\") ",
            Filter = null//"col(\"Number\") > 10"
        };

        using var expressionScope = ExpressionScope.Create(project, userField.SourceId, userField.SourcePairId.Value, userField.Expression, userField.Filter);
        Assert.IsEmpty(expressionScope.Errors);

        var userFieldScope = expressionScope.GetUserFieldScope(userField.SourceId, userField.SourcePairId!.Value);

        var updateSql = userFieldScope.GetUpdateSql(true, "fld0000");
    }

    [TestCase("date_or_null(col(\"RDL.Plan1\"))", @"UPDATE fake_table_name._0862b7ab73434b6f83ab4efa73920847_rd_1 SET (fld0000) = (SELECT ""Value"" FROM (
SELECT _.id AS ""Id"", date_trunc('day', _.col2643, 'UTC') AS ""Value""
FROM fake_table_name._0862b7ab73434b6f83ab4efa73920847_rd_1 AS _

) AS tttt WHERE tttt.""Id"" = id)", TestName = "date_or_null_1")]
    [TestCase("date_or_null(col(\"RDL.Plan1\"))== date_or_null(col(\"RDL.Fact1\")) ? col(\"RDL.Plan1\") : col(\"RDL.Fact1\")", @"UPDATE fake_table_name._0862b7ab73434b6f83ab4efa73920847_rd_1 SET (fld0000) = (SELECT ""Value"" FROM (
SELECT _.id AS ""Id"", CASE
    WHEN (CASE
        WHEN (_.col2643 IS NOT NULL) THEN date_trunc('day', _.col2643, 'UTC')
        ELSE NULL
    END = CASE
        WHEN (_.col2644 IS NOT NULL) THEN date_trunc('day', _.col2644, 'UTC')
        ELSE NULL
    END) OR (((CASE
        WHEN (_.col2643 IS NOT NULL) THEN date_trunc('day', _.col2643, 'UTC')
        ELSE NULL
    END IS NULL)) AND ((CASE
        WHEN (_.col2644 IS NOT NULL) THEN date_trunc('day', _.col2644, 'UTC')
        ELSE NULL
    END IS NULL))) THEN _.col2643
    ELSE _.col2644
END AS ""Value""
FROM fake_table_name._0862b7ab73434b6f83ab4efa73920847_rd_1 AS _

) AS tttt WHERE tttt.""Id"" = id)", TestName = "date_or_null_2")]
    [TestCase("val(\"RDL.Calculated_Date\") ", @"UPDATE fake_table_name._0862b7ab73434b6f83ab4efa73920847_rd_1 SET (fld0000) = (SELECT ""Value"" FROM (
SELECT _.id AS ""Id"", (
    SELECT MIN(TIMESTAMPTZ '2021-01-25 11:16:42Z')
    FROM fake_table_name._0862b7ab73434b6f83ab4efa73920847_1_pair_connections AS _0
    INNER JOIN (
        SELECT _2.connection_type, _2.link, _2.row_id, _2.source_id, _2.source_pair_id
        FROM fake_table_name._0862b7ab73434b6f83ab4efa73920847_1_pair_connections AS _2
        WHERE (_2.source_pair_id = '00000000-2001-0000-0000-000000000000') AND (_2.source_id = '00000000-1002-0000-0000-000000000000')
    ) AS t ON _0.link = t.link
    INNER JOIN fake_table_name._0862b7ab73434b6f83ab4efa73920847_mto_1 AS _1 ON t.row_id = _1.id
    WHERE ((_0.source_pair_id = '00000000-2001-0000-0000-000000000000') AND (_0.source_id = '00000000-1001-0000-0000-000000000000')) AND (_0.row_id = _.id)) AS ""Value""
FROM fake_table_name._0862b7ab73434b6f83ab4efa73920847_rd_1 AS _

) AS tttt WHERE tttt.""Id"" = id)", TestName = "val")]
    [TestCase("x => x.Plan1 > CalculatedAt", @"UPDATE fake_table_name._0862b7ab73434b6f83ab4efa73920847_rd_1 SET (fld0000) = (SELECT ""Value"" FROM (
SELECT _.id AS ""Id"", _.col2643 > TIMESTAMPTZ '2020-08-14 19:03:35Z' AS ""Value""
FROM fake_table_name._0862b7ab73434b6f83ab4efa73920847_rd_1 AS _

) AS tttt WHERE tttt.""Id"" = id)", TestName = "CalculatedAt")]
    [TestCase("col(\"RDL.Plan1\") > val(\"Version_Calculated_Date\")", @"UPDATE fake_table_name._0862b7ab73434b6f83ab4efa73920847_rd_1 SET (fld0000) = (SELECT ""Value"" FROM (
SELECT _.id AS ""Id"", _.col2643 > (
    SELECT MIN(TIMESTAMPTZ '2020-08-14 19:03:35Z')
    FROM fake_table_name._0862b7ab73434b6f83ab4efa73920847_1_pair_connections AS _0
    INNER JOIN (
        SELECT _2.connection_type, _2.link, _2.row_id, _2.source_id, _2.source_pair_id
        FROM fake_table_name._0862b7ab73434b6f83ab4efa73920847_1_pair_connections AS _2
        WHERE (_2.source_pair_id = '00000000-2001-0000-0000-000000000000') AND (_2.source_id = '00000000-1002-0000-0000-000000000000')
    ) AS t ON _0.link = t.link
    INNER JOIN fake_table_name._0862b7ab73434b6f83ab4efa73920847_mto_1 AS _1 ON t.row_id = _1.id
    WHERE ((_0.source_pair_id = '00000000-2001-0000-0000-000000000000') AND (_0.source_id = '00000000-1001-0000-0000-000000000000')) AND (_0.row_id = _.id)) AS ""Value""
FROM fake_table_name._0862b7ab73434b6f83ab4efa73920847_rd_1 AS _

) AS tttt WHERE tttt.""Id"" = id)", TestName = "Version_Calculated_Date")]
    [TestCase("any_null(col(\"RDL.Plan1\"), col(\"RDL.Plan1\"))", @"UPDATE fake_table_name._0862b7ab73434b6f83ab4efa73920847_rd_1 SET (fld0000) = (SELECT ""Value"" FROM (
SELECT _.id AS ""Id"", (_.col2643 IS NULL) AS ""Value""
FROM fake_table_name._0862b7ab73434b6f83ab4efa73920847_rd_1 AS _

) AS tttt WHERE tttt.""Id"" = id)", TestName = "any_null")]
    [TestCase("eq(col(\"RDL.Plan1\"), col(\"RDL.Fact1\"))", @"UPDATE fake_table_name._0862b7ab73434b6f83ab4efa73920847_rd_1 SET (fld0000) = (SELECT ""Value"" FROM (
SELECT _.id AS ""Id"", (((_.col2643 IS NOT NULL)) AND ((_.col2644 IS NOT NULL))) AND (_.col2643 = _.col2644) AS ""Value""
FROM fake_table_name._0862b7ab73434b6f83ab4efa73920847_rd_1 AS _

) AS tttt WHERE tttt.""Id"" = id)", TestName = "eq")]
    [TestCase("DateDiffDays(col(\"RDL.Plan1\").Value,col(\"RDL.Fact1\").Value)", @"UPDATE fake_table_name._0862b7ab73434b6f83ab4efa73920847_rd_1 SET (fld0000) = (SELECT ""Value"" FROM (
SELECT _.id AS ""Id"", DATE_PART('day', _.col2643 - _.col2644) AS ""Value""
FROM fake_table_name._0862b7ab73434b6f83ab4efa73920847_rd_1 AS _

) AS tttt WHERE tttt.""Id"" = id)", TestName = "DateSubtractionMethod")]
    [TestCase("sum(\"MTOL.Number\") + 22", @"UPDATE fake_table_name._0862b7ab73434b6f83ab4efa73920847_rd_1 SET (fld0000) = (SELECT ""Value"" FROM (
SELECT _.id AS ""Id"", (
    SELECT COALESCE(SUM(_1.col2645), 0)::INT
    FROM fake_table_name._0862b7ab73434b6f83ab4efa73920847_1_pair_connections AS _0
    INNER JOIN (
        SELECT _2.connection_type, _2.link, _2.row_id, _2.source_id, _2.source_pair_id
        FROM fake_table_name._0862b7ab73434b6f83ab4efa73920847_1_pair_connections AS _2
        WHERE (_2.source_pair_id = '00000000-2001-0000-0000-000000000000') AND (_2.source_id = '00000000-1002-0000-0000-000000000000')
    ) AS t ON _0.link = t.link
    INNER JOIN fake_table_name._0862b7ab73434b6f83ab4efa73920847_mto_1 AS _1 ON t.row_id = _1.id
    WHERE ((_0.source_pair_id = '00000000-2001-0000-0000-000000000000') AND (_0.source_id = '00000000-1001-0000-0000-000000000000')) AND (_0.row_id = _.id)) + 22 AS ""Value""
FROM fake_table_name._0862b7ab73434b6f83ab4efa73920847_rd_1 AS _

) AS tttt WHERE tttt.""Id"" = id)", TestName = "Error ПП")]
    [TestCase("col(\"RDL.Plan1\") < new DateTime(val(\"Version_Calculated_Date\").Year, val(\"Version_Calculated_Date\").Month, DateTime.DaysInMonth(val(\"Version_Calculated_Date\").Year, val(\"Version_Calculated_Date\").Month))",
        @"UPDATE fake_table_name._0862b7ab73434b6f83ab4efa73920847_rd_1 SET (fld0000) = (SELECT ""Value"" FROM (
-- @__CalculatedAt_0='2020-08-14T19:03:35.0000000Z' (DbType = DateTime)
SELECT _.id, _.col2643, date_part('year', (
    SELECT MIN(@__CalculatedAt_0)
    FROM fake_table_name._0862b7ab73434b6f83ab4efa73920847_1_pair_connections AS _0
    INNER JOIN (
        SELECT _2.connection_type, _2.link, _2.row_id, _2.source_id, _2.source_pair_id
        FROM fake_table_name._0862b7ab73434b6f83ab4efa73920847_1_pair_connections AS _2
        WHERE (_2.source_pair_id = '00000000-2001-0000-0000-000000000000') AND (_2.source_id = '00000000-1002-0000-0000-000000000000')
    ) AS t ON _0.link = t.link
    INNER JOIN fake_table_name._0862b7ab73434b6f83ab4efa73920847_mto_1 AS _1 ON t.row_id = _1.id
    WHERE ((_0.source_pair_id = '00000000-2001-0000-0000-000000000000') AND (_0.source_id = '00000000-1001-0000-0000-000000000000')) AND (_0.row_id = _.id)))::INT, date_part('month', (
    SELECT MIN(@__CalculatedAt_0)
    FROM fake_table_name._0862b7ab73434b6f83ab4efa73920847_1_pair_connections AS _3
    INNER JOIN (
        SELECT _5.connection_type, _5.link, _5.row_id, _5.source_id, _5.source_pair_id
        FROM fake_table_name._0862b7ab73434b6f83ab4efa73920847_1_pair_connections AS _5
        WHERE (_5.source_pair_id = '00000000-2001-0000-0000-000000000000') AND (_5.source_id = '00000000-1002-0000-0000-000000000000')
    ) AS t0 ON _3.link = t0.link
    INNER JOIN fake_table_name._0862b7ab73434b6f83ab4efa73920847_mto_1 AS _4 ON t0.row_id = _4.id
    WHERE ((_3.source_pair_id = '00000000-2001-0000-0000-000000000000') AND (_3.source_id = '00000000-1001-0000-0000-000000000000')) AND (_3.row_id = _.id)))::INT, date_part('year', (
    SELECT MIN(@__CalculatedAt_0)
    FROM fake_table_name._0862b7ab73434b6f83ab4efa73920847_1_pair_connections AS _6
    INNER JOIN (
        SELECT _8.connection_type, _8.link, _8.row_id, _8.source_id, _8.source_pair_id
        FROM fake_table_name._0862b7ab73434b6f83ab4efa73920847_1_pair_connections AS _8
        WHERE (_8.source_pair_id = '00000000-2001-0000-0000-000000000000') AND (_8.source_id = '00000000-1002-0000-0000-000000000000')
    ) AS t1 ON _6.link = t1.link
    INNER JOIN fake_table_name._0862b7ab73434b6f83ab4efa73920847_mto_1 AS _7 ON t1.row_id = _7.id
    WHERE ((_6.source_pair_id = '00000000-2001-0000-0000-000000000000') AND (_6.source_id = '00000000-1001-0000-0000-000000000000')) AND (_6.row_id = _.id)))::INT, date_part('month', (
    SELECT MIN(@__CalculatedAt_0)
    FROM fake_table_name._0862b7ab73434b6f83ab4efa73920847_1_pair_connections AS _9
    INNER JOIN (
        SELECT _11.connection_type, _11.link, _11.row_id, _11.source_id, _11.source_pair_id
        FROM fake_table_name._0862b7ab73434b6f83ab4efa73920847_1_pair_connections AS _11
        WHERE (_11.source_pair_id = '00000000-2001-0000-0000-000000000000') AND (_11.source_id = '00000000-1002-0000-0000-000000000000')
    ) AS t2 ON _9.link = t2.link
    INNER JOIN fake_table_name._0862b7ab73434b6f83ab4efa73920847_mto_1 AS _10 ON t2.row_id = _10.id
    WHERE ((_9.source_pair_id = '00000000-2001-0000-0000-000000000000') AND (_9.source_id = '00000000-1001-0000-0000-000000000000')) AND (_9.row_id = _.id)))::INT
FROM fake_table_name._0862b7ab73434b6f83ab4efa73920847_rd_1 AS _

) AS tttt WHERE tttt.""Id"" = id)", TestName = "Constant resolver")]

    public void Should_Return_Expected_Sql(string expression, string expectedSql)
    {
        var project = JObject.Parse(File.ReadAllText($"PrototypeTests.json"))
            .ToObject<ProjectModel>();

        var sourcePairId = project.SourcePairs.First().Key;
        var sourceId = project.SourcePairs[sourcePairId].Items.First().Value.SourceId;

        UserFieldModel userField = new UserFieldModel
        {
            SourceId = sourceId,
            SourcePairId = sourcePairId,
            Expression = expression,
            Filter = null,
            ContinueWithZero = true
        };

        using var expressionScope = ExpressionScope.Create(project, userField.SourceId, userField.SourcePairId, userField.Expression);
        Assert.IsEmpty(expressionScope.Errors);

        var userFieldScope = expressionScope.GetUserFieldScope(userField.SourceId, userField.SourcePairId!.Value);

        var updateSql = userFieldScope.GetUpdateSql(true, "fld0000");
        Assert.AreEqual(expectedSql, updateSql);
    }  
}
