using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using QueryingCore.Core;
using Microsoft.EntityFrameworkCore.Storage;
using QueryingCore.AggregativeIndicatorCore;

namespace SomeNameSpace;

internal class Factory : FactoryBase
{
    public Factory()
    {
        RuleTypes.TryAdd(Project.MTOLSourceId, typeof(Expression<Func<MTOLRecord, bool>>));
        RuleEvaluators.TryAdd(Project.MTOLSourceId, (typeWrapper, ruleId) => QueryingContext<MTOLRecord>.CSharpScriptEvaluate(typeWrapper, ruleId));
        IndicatorQueryBuilders.TryAdd(Project.MTOLSourceId, MTOLQueryBuilder);
        RuleTypes.TryAdd(Project.RDLSourceId, typeof(Expression<Func<RDLRecord, bool>>));
        RuleEvaluators.TryAdd(Project.RDLSourceId, (typeWrapper, ruleId) => QueryingContext<RDLRecord>.CSharpScriptEvaluate(typeWrapper, ruleId));
        IndicatorQueryBuilders.TryAdd(Project.RDLSourceId, RDLQueryBuilder);
        AddContext(new Guid("00000000-1001-0000-0000-000000000000"), new QueryingContext<RDLRecord>());
        AddContext(new Guid("00000000-1002-0000-0000-000000000000"), new QueryingContext<MTOLRecord>());
        AddContext(
            (new Guid("00000000-1001-0000-0000-000000000000"), new Guid("00000000-2001-0000-0000-000000000000")),
            new QueryingContextAggregative<RDLRecord, MTOLRecord>(x => x.RDMTOItems,
                Project.GetRDL_RDMTOItemsSubquery));
        AddContext(
            (new Guid("00000000-1002-0000-0000-000000000000"), new Guid("00000000-2001-0000-0000-000000000000")),
            new QueryingContextAggregative<MTOLRecord, RDLRecord>(x => x.RDMTOItems,
                Project.GetMTOL_RDMTOItemsSubquery));
    }

    public Expression<Func<MTOLRecord, TResult>> MTOLRecordReplaceSurrogates<TResult>(
        Expression<Func<MTOLRecord, TResult>> expression,
        DbContext ctx)
    {
        expression = expression
            .ReplaceSurrogate(x => x.RDMTOItems, Project.GetMTOL_RDMTOItemsSubquery(ctx))
            .ReplaceSurrogate(x => x.PairIndicator1Items, Project.GetMTOLPairIndicator1ItemsSubquery(ctx));
        Expression<Func<RDLRecord, Guid?>> one0 = x => x.RDunique;
        Expression<Func<RDLRecord, Guid?>> another0 = Project.GetRDuniqueIndicatorSubquery(ctx);
        expression = Expression.Lambda<Func<MTOLRecord, TResult>>(
            new RRExpressionVisitor<RDLRecord, Guid?>(one0, another0).Visit(expression.Body),
            expression.Parameters[0]);
        Expression<Func<RDLRecord, Guid?>> one1 = x => x.RDunique1;
        Expression<Func<RDLRecord, Guid?>> another1 = Project.GetRDunique1IndicatorSubquery(ctx);
        expression = Expression.Lambda<Func<MTOLRecord, TResult>>(
            new RRExpressionVisitor<RDLRecord, Guid?>(one1, another1).Visit(expression.Body),
            expression.Parameters[0]);
        Expression<Func<RDLRecord, Guid?>> one2 = x => x.yellow;
        Expression<Func<RDLRecord, Guid?>> another2 = Project.GetyellowIndicatorSubquery(ctx);
        expression = Expression.Lambda<Func<MTOLRecord, TResult>>(
            new RRExpressionVisitor<RDLRecord, Guid?>(one2, another2).Visit(expression.Body),
            expression.Parameters[0]);
        return expression;
    }

    private IQueryable<IndicatorResult> MTOLQueryBuilder(DbContext ctx, IEnumerable<(Guid ruleId, Expression expression)> rules0)
    {
        var rules = rules0.Select(rule =>
            (
                rule.ruleId,
                MTOLRecordReplaceSurrogates((Expression<Func<MTOLRecord, bool>>) rule.expression, ctx)
            )
        ).ToArray();

        var selector = rules.ToSelector();

        var query = ctx.Set<MTOLRecord>().Select(selector).Where(x => x.RuleId != Guid.Empty).Select(x =>
            new IndicatorResult
                {Id = x.Id, RuleId = x.RuleId.ToString()});

        //заменяем суррогаты на подзапросы
        return query;
    }

    public Expression<Func<RDLRecord, TResult>> RDLRecordReplaceSurrogates<TResult>(
        Expression<Func<RDLRecord, TResult>> expression,
        DbContext ctx)
    {
        expression = expression
                .ReplaceSurrogate(x => x.RDMTOItems, Project.GetRDL_RDMTOItemsSubquery(ctx))
                .ReplaceSurrogate(x => x.PairIndicator1Items, Project.GetRDLPairIndicator1ItemsSubquery(ctx));

        Expression<Func<MTOLRecord, Guid?>> one0 = x => x.MTOAggregative;
        Expression<Func<MTOLRecord, Guid?>> another0 = Project.GetMTOAggregativeIndicatorSubquery(ctx);
        expression = Expression.Lambda<Func<RDLRecord, TResult>>(
            new RRExpressionVisitor<MTOLRecord, Guid?>(one0, another0).Visit(expression.Body),
            expression.Parameters[0]);
        Expression<Func<MTOLRecord, Guid?>> one1 = x => x.MTOunique;
        Expression<Func<MTOLRecord, Guid?>> another1 = Project.GetMTOuniqueIndicatorSubquery(ctx);
        expression = Expression.Lambda<Func<RDLRecord, TResult>>(
            new RRExpressionVisitor<MTOLRecord, Guid?>(one1, another1).Visit(expression.Body),
            expression.Parameters[0]);
        return expression;
    }

    private IQueryable<IndicatorResult> RDLQueryBuilder(DbContext ctx, IEnumerable<(Guid ruleId, Expression expression)> rules0)
    {
        var rules = rules0.Select(rule =>
            (
                rule.ruleId,
                RDLRecordReplaceSurrogates((Expression<Func<RDLRecord, bool>>) rule.expression, ctx))
        ).ToArray();

        var selector = rules.ToSelector();

        var query = ctx.Set<RDLRecord>().Select(selector).Where(x => x.RuleId != Guid.Empty).Select(x =>
            new IndicatorResult
                {Id = x.Id, RuleId = x.RuleId.ToString()});

        //заменяем суррогаты на подзапросы
        return query;
    }

    private Expression createUserFieldExpression_00000000100100000000000000000000_00000000200100000000000000000000(
            Expression expression, Expression filter, bool continueWithZero)
    {
        Expression<Func<RDLRecord, IEnumerable<MTOLRecord>>> items = x => x.RDMTOItems;
        var userFieldExpression = expression as Expression<Func<RDLRecord, object>>;
        var userFieldFilter = filter as Expression<Func<MTOLRecord, bool>>;

        var exp1 = items.UserField(userFieldExpression, userFieldFilter, continueWithZero);
        return exp1;
    }

    private Expression createUserFieldExpression_00000000100200000000000000000000_00000000200100000000000000000000(
            Expression expression, Expression filter, bool continueWithZero)
    {
        Expression<Func<MTOLRecord, IEnumerable<RDLRecord>>> items = x => x.RDMTOItems;
        var userFieldExpression = expression as Expression<Func<MTOLRecord, object>>;
        var userFieldFilter = filter as Expression<Func<RDLRecord, bool>>;

        var exp1 = items.UserField(userFieldExpression, userFieldFilter, continueWithZero);
        return exp1;
    }
}

public static class PairIndicator1Rules
{
    public static readonly Guid grey = Guid.Parse("00000000-3007-1001-0000-000000000000");
    public static readonly Guid red = Guid.Parse("00000000-3007-1002-0000-000000000000");
}

public static class MTOAggregativeRules //rules
{
    public static readonly Guid rule1 = Guid.Parse("00000000-3004-1001-0000-000000000000"); //rule
    public static readonly Guid rule2 = Guid.Parse("00000000-3004-1002-0000-000000000000"); //rule
}

public static class MTOuniqueRules //rules
{
    public static readonly Guid grey = Guid.Parse("00000000-3006-1001-0000-000000000000"); //rule
    public static readonly Guid red = Guid.Parse("00000000-3006-1002-0000-000000000000"); //rule
}

public static class RDuniqueRules //rules
{
    public static readonly Guid grey = Guid.Parse("00000000-1001-1001-0000-000000000000"); //rule
    public static readonly Guid red = Guid.Parse("00000000-1001-1002-0000-000000000000"); //rule
}

public static class RDunique1Rules //rules
{
    public static readonly Guid rule1 = Guid.Parse("00000000-3002-1001-0000-000000000000"); //rule
    public static readonly Guid yellow = Guid.Parse("00000000-3002-1002-0000-000000000000"); //rule
}

public static class yellowRules //rules
{
    public static readonly Guid rule1 = Guid.Parse("00000000-3003-1001-0000-000000000000"); //rule
    public static readonly Guid yellow = Guid.Parse("00000000-3003-1002-0000-000000000000"); //rule
}

public class Project //project
{
    public static readonly int? ic1 = 500;
    public static readonly Guid MTOAggregativeIndicatorId = Guid.Parse("00000000-3004-0000-0000-000000000000");

    public static Expression<Func<MTOLRecord, Guid?>> GetMTOAggregativeIndicatorSubquery(DbContext ctx)
    {
        return x =>
            ctx.Set<SingleCollisionRecord>()
                .Where(y => y.SourceId == Project.MTOLSourceId && y.IndicatorId == MTOAggregativeIndicatorId &&
                            y.RowId == x.Id)
                .Select(y => y.IndicatorRuleId)
                .FirstOrDefault();
    }

    public static readonly Guid MTOuniqueIndicatorId = Guid.Parse("00000000-3006-0000-0000-000000000000");

    public static Expression<Func<MTOLRecord, Guid?>> GetMTOuniqueIndicatorSubquery(DbContext ctx)
    {
        return x =>
            ctx.Set<SingleCollisionRecord>()
                .Where(y => y.SourceId == Project.MTOLSourceId && y.IndicatorId == MTOuniqueIndicatorId &&
                            y.RowId == x.Id)
                .Select(y => y.IndicatorRuleId)
                .FirstOrDefault();
    }

    public static readonly Guid RDuniqueIndicatorId = Guid.Parse("00000000-3001-0000-0000-000000000000");

    public static Expression<Func<RDLRecord, Guid?>> GetRDuniqueIndicatorSubquery(DbContext ctx)
    {
        return x =>
            ctx.Set<SingleCollisionRecord>()
                .Where(y => y.SourceId == Project.RDLSourceId && y.IndicatorId == RDuniqueIndicatorId &&
                            y.RowId == x.Id)
                .Select(y => y.IndicatorRuleId)
                .FirstOrDefault();
    }

    public static readonly Guid RDunique1IndicatorId = Guid.Parse("00000000-3002-0000-0000-000000000000");

    public static Expression<Func<RDLRecord, Guid?>> GetRDunique1IndicatorSubquery(DbContext ctx)
    {
        return x =>
            ctx.Set<SingleCollisionRecord>()
                .Where(y => y.SourceId == Project.RDLSourceId && y.IndicatorId == RDunique1IndicatorId &&
                            y.RowId == x.Id)
                .Select(y => y.IndicatorRuleId)
                .FirstOrDefault();
    }

    public static readonly Guid yellowIndicatorId = Guid.Parse("00000000-3003-0000-0000-000000000000");

    public static Expression<Func<RDLRecord, Guid?>> GetyellowIndicatorSubquery(DbContext ctx)
    {
        return x =>
            ctx.Set<SingleCollisionRecord>()
                .Where(y => y.SourceId == Project.RDLSourceId && y.IndicatorId == yellowIndicatorId &&
                            y.RowId == x.Id)
                .Select(y => y.IndicatorRuleId)
                .FirstOrDefault();
    }

    public static readonly Guid RDMTOItemsId = Guid.Parse("00000000-2001-0000-0000-000000000000");
    public static readonly Guid MTOLSourceId = Guid.Parse("00000000-1002-0000-0000-000000000000");

    public static Expression<Func<MTOLRecord, IEnumerable<RDLRecord>>> GetMTOL_RDMTOItemsSubquery(DbContext ctx)
    {
        Guid RDMTOItemsId = Guid.Parse("00000000-2001-0000-0000-000000000000");
        var oneInPair = ctx.Set<PairConnectionRecord>()
            .Where(x => x.SourcePairId == RDMTOItemsId && x.SourceId == MTOLSourceId);
        var anotherInPair = ctx.Set<PairConnectionRecord>()
            .Where(x => x.SourcePairId == RDMTOItemsId && x.SourceId == RDLSourceId);
        var p = oneInPair
            .Join(anotherInPair, sp2 => sp2.Link, sp3 => sp3.Link, (sp2, sp3) => new {sp2.RowId, extId = sp3.RowId})
            .Join(ctx.Set<RDLRecord>(), p => p.extId, p => p.Id, (s, ext) => new {s.RowId, ext});
        return x => p.Where(z => z.RowId == x.Id).Select(x => x.ext);
    }

    public static readonly Guid RDLSourceId = Guid.Parse("00000000-1001-0000-0000-000000000000");

    public static Expression<Func<RDLRecord, IEnumerable<MTOLRecord>>> GetRDL_RDMTOItemsSubquery(DbContext ctx)
    {
        Guid RDMTOItemsId = Guid.Parse("00000000-2001-0000-0000-000000000000");
        var oneInPair = ctx.Set<PairConnectionRecord>()
            .Where(x => x.SourcePairId == RDMTOItemsId && x.SourceId == RDLSourceId);
        var anotherInPair = ctx.Set<PairConnectionRecord>()
            .Where(x => x.SourcePairId == RDMTOItemsId && x.SourceId == MTOLSourceId);
        var p = oneInPair
            .Join(anotherInPair, sp2 => sp2.Link, sp3 => sp3.Link, (sp2, sp3) => new {sp2.RowId, extId = sp3.RowId})
            .Join(ctx.Set<MTOLRecord>(), p => p.extId, p => p.Id, (s, ext) => new {s.RowId, ext});
        return x => p.Where(z => z.RowId == x.Id).Select(x => x.ext);
    }

    public static readonly Guid PairIndicator1Id = Guid.Parse("00000000-3007-0000-0000-000000000000");

    public static Expression<Func<MTOLRecord, IEnumerable<Guid>>> GetMTOLPairIndicator1ItemsSubquery(DbContext ctx)
    {
        var oneInPair = ctx.Set<PairConnectionRecord>().Where(x =>
            x.SourcePairId == Project.RDMTOItemsId && x.SourceId == Project.MTOLSourceId);
        var indicators = ctx.Set<PairCollisionRecord>().Where(x =>
            x.SourcePairId == Project.RDMTOItemsId && x.IndicatorId == Project.PairIndicator1Id);
        var p = oneInPair.Join(indicators, sp2 => sp2.Link, sp3 => sp3.Link,
            (sp2, sp3) => new {sp2.RowId, RuleId = sp3.IndicatorRuleId});
        return x => p.Where(z => z.RowId == x.Id).Select(p => p.RuleId);
    }

    public static Expression<Func<RDLRecord, IEnumerable<Guid>>> GetRDLPairIndicator1ItemsSubquery(DbContext ctx)
    {
        var oneInPair = ctx.Set<PairConnectionRecord>().Where(x =>
            x.SourcePairId == Project.RDMTOItemsId && x.SourceId == Project.RDLSourceId);
        var indicators = ctx.Set<PairCollisionRecord>().Where(x =>
            x.SourcePairId == Project.RDMTOItemsId && x.IndicatorId == Project.PairIndicator1Id);
        var p = oneInPair.Join(indicators, sp2 => sp2.Link, sp3 => sp3.Link,
            (sp2, sp3) => new {sp2.RowId, RuleId = sp3.IndicatorRuleId});
        return x => p.Where(z => z.RowId == x.Id).Select(p => p.RuleId);
    }
}

public class DbContext_00000000500100000000000000000000 : DbContext
{
    public DbContext_00000000500100000000000000000000(DbContextOptions options) : base(options)
    {
    }

    public DateTime CalculatedAt
    {
        get { return new DateTime(2020, 8, 14, 0, 0, 0, DateTimeKind.Utc); }
    }

    public int? ic1
    {
        get { return Project.ic1; }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UnknownTableRecordConfiguration());
        modelBuilder.ApplyConfiguration(
            new PairConnectionRecordConfiguration("_0862b7ab73434b6f83ab4efa73920847_1_pair_connections"));
        modelBuilder.ApplyConfiguration(
            new SingleCollisionRecordConfiguration("_0862b7ab73434b6f83ab4efa73920847_1_single_collisions"));
        modelBuilder.ApplyConfiguration(
            new PairCollisionRecordConfiguration("_0862b7ab73434b6f83ab4efa73920847_1_pair_collisions"));
        modelBuilder.ApplyConfiguration(new MTOLRecordConfiguration("_0862b7ab73434b6f83ab4efa73920847_mto_1"));
        modelBuilder.ApplyConfiguration(new RDLRecordConfiguration("_0862b7ab73434b6f83ab4efa73920847_rd_2"));

        modelBuilder.HasDbFunction(ModelBuilderExtensions.DateDiffDaysMethodInfo)
            .HasTranslation(args =>
                    new SqlFunctionExpression("DATE_PART",
                    new SqlExpression[]
                    {
                        new SqlConstantExpression(
                            Expression.Constant("day"),
                            new StringTypeMapping("string", DbType.String)
                            ),
                        new SqlBinaryExpression(
                            ExpressionType.Subtract,
                            args.First(),
                            args.Skip(1).First(),
                            args.First().Type,
                            args.First().TypeMapping)
                    },
                    nullable: true,
                    argumentsPropagateNullability: new[] { true, true },
                    type: typeof(int?),
                    typeMapping: new IntTypeMapping("int", DbType.Int32))
            );
    }
}

public class MTOLRecord : RecordBase
{
    public string Code { get; set; }
    public DateTime? Fact { get; set; }
    public string NameEq { get; set; }
    public int? Number { get; set; }
    public DateTime? Plan { get; set; }

    [RuleDependency("00000000-2001-0000-0000-000000000000")]
    public virtual ICollection<RDLRecord> RDMTOItems { get; set; }

    [RuleDependency("00000000-3004-0000-0000-000000000000")]
    public Guid? MTOAggregative { get; set; }

    [RuleDependency("00000000-3006-0000-0000-000000000000")]
    public Guid? MTOunique { get; set; }

    // User fields (1)
    [RuleDependency("00000000-4002-0000-0000-000000000000")]
    public string MTOuserfield1 { get; set; }

    [RuleDependency("00000000-3007-0000-0000-000000000000")]
    public ICollection<Guid> PairIndicator1Items { get; set; }
}

internal sealed class MTOLRecordConfiguration : IEntityTypeConfiguration<MTOLRecord>
{
    private readonly string _tableName;

    public MTOLRecordConfiguration(string tableName)
    {
        _tableName = tableName;
    }

    public void Configure(EntityTypeBuilder<MTOLRecord> builder)
    {
        builder.ToTable(_tableName, "fake_table_name");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Code).HasColumnName("col2647");
        builder.Property(x => x.Fact).HasColumnName("col2649");
        builder.Property(x => x.NameEq).HasColumnName("col2646");
        builder.Property(x => x.Number).HasColumnName("col2645");
        builder.Property(x => x.Plan).HasColumnName("col2648");
        builder.Ignore(x => x.RDMTOItems);
        builder.Ignore(x => x.MTOAggregative);
        builder.Ignore(x => x.MTOunique);
        // Append user fields configuration (1)
        builder.Property(x => x.MTOuserfield1).HasColumnName("fld4002");
        builder.Ignore(x => x.PairIndicator1Items);
    }
}

public class RDLRecord : RecordBase
{
    public string Code1 { get; set; }
    public DateTime? Fact1 { get; set; }
    public string Name1 { get; set; }
    public int? Number1 { get; set; }
    public DateTime? Plan1 { get; set; }

    [RuleDependency("00000000-2001-0000-0000-000000000000")]
    public virtual ICollection<MTOLRecord> RDMTOItems { get; set; }

    [RuleDependency("00000000-3001-0000-0000-000000000000")]
    public Guid? RDunique { get; set; }

    [RuleDependency("00000000-3002-0000-0000-000000000000")]
    public Guid? RDunique1 { get; set; }

    [RuleDependency("00000000-3003-0000-0000-000000000000")]
    public Guid? yellow { get; set; }

    // User fields (1)
    [RuleDependency("00000000-4001-0000-0000-000000000000")]
    public string RDuserfield1 { get; set; }

    [RuleDependency("00000000-3007-0000-0000-000000000000")]
    public ICollection<Guid> PairIndicator1Items { get; set; }
}

internal sealed class RDLRecordConfiguration : IEntityTypeConfiguration<RDLRecord>
{
    private readonly string _tableName;

    public RDLRecordConfiguration(string tableName)
    {
        _tableName = tableName;
    }

    public void Configure(EntityTypeBuilder<RDLRecord> builder)
    {
        builder.ToTable(_tableName, "fake_table_name");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Code1).HasColumnName("col2642");
        builder.Property(x => x.Fact1).HasColumnName("col2644");
        builder.Property(x => x.Name1).HasColumnName("col2641");
        builder.Property(x => x.Number1).HasColumnName("col2640");
        builder.Property(x => x.Plan1).HasColumnName("col2643");
        builder.Ignore(x => x.RDMTOItems);
        builder.Ignore(x => x.RDunique);
        builder.Ignore(x => x.RDunique1);
        builder.Ignore(x => x.yellow);
        // Append user fields configuration (1)
        builder.Property(x => x.RDuserfield1).HasColumnName("fld4001");
        builder.Ignore(x => x.PairIndicator1Items);
    }
}

public class ExpressionWrapper
{
    public System.DateTime CalculatedAt => new DateTime(2020, 08, 14, 0, 0, 0, DateTimeKind.Utc);
    public System.DateTime MTOLActualAt => new DateTime(0001, 01, 01, 0, 0, 0, DateTimeKind.Utc);
    public System.DateTime RDLActualAt => new DateTime(2021, 01, 25, 0, 0, 0, DateTimeKind.Utc);
    public Expression<Func<RDLRecord, bool>> _00000000300210010000000000000000 => x => (bool)(x.Number1 >= 30);   
    public Expression<Func<RDLRecord, bool>> _00000000300210020000000000000000 => x => (bool)(x.Number1 >= 20);
}

public class FilterWrapper
{
    public Expression<Func<RDLRecord, DateTime>> _date_00000000300210010000000000000000 => x => DateTime.UtcNow;        
}
