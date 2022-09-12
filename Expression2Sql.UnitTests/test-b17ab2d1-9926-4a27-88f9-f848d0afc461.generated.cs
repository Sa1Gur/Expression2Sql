namespace SomeNameSpace //для каждой сборки генерируется свой уникальный namespace
{
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ASE.MD.UnifiedSchedule.AggregativeIndicator;
using ASE.MD.UnifiedSchedule.QueryingCore;

internal class Factory: FactoryBase
{
	public Factory()
	{
	_ruleTypes.Add(Project.OneSourceId, typeof(Expression<Func<OneRecord, bool>>));
	_ruleEvaluators.Add(Project.OneSourceId, async (expression, options, ctx, cancellationToken) => (Expression) await CSharpScript.EvaluateAsync<Expression<Func<OneRecord,bool>>>(code: expression, options: options, globals: ctx, cancellationToken: cancellationToken).ConfigureAwait(false));
	_indicatorQueryBuilders.Add(Project.OneSourceId,OneQueryBuilder);
	_ruleTypes.Add(Project.TwoSourceId, typeof(Expression<Func<TwoRecord, bool>>));
	_ruleEvaluators.Add(Project.TwoSourceId, async (expression, options, ctx, cancellationToken) => (Expression) await CSharpScript.EvaluateAsync<Expression<Func<TwoRecord,bool>>>(code: expression, options: options, globals: ctx, cancellationToken: cancellationToken).ConfigureAwait(false));
	_indicatorQueryBuilders.Add(Project.TwoSourceId,TwoQueryBuilder);
AddContext(new Guid("3b6f0dbe-7327-4647-8e51-99483beedb63"), new QueryingContext<OneRecord>(this));
AddContext(new Guid("e5bd56ad-2730-4c40-baa2-158b63a8a221"), new QueryingContext<TwoRecord>(this));
AddContext((new Guid("3b6f0dbe-7327-4647-8e51-99483beedb63"),new Guid("ed80ac50-1967-4134-b842-b7a5977660f6")), new QueryingContextAggregative<OneRecord,TwoRecord>(this, x => x.OneTwoItems, Project.GetOne_OneTwoItemsSubquery));
AddContext((new Guid("e5bd56ad-2730-4c40-baa2-158b63a8a221"),new Guid("ed80ac50-1967-4134-b842-b7a5977660f6")), new QueryingContextAggregative<TwoRecord,OneRecord>(this, x => x.OneTwoItems, Project.GetTwo_OneTwoItemsSubquery));
	}
public Expression<Func<OneRecord,TResult>> OneRecordReplaceSurrogates<TResult>(
Expression<Func<OneRecord,TResult>> expression,
DbContext ctx)
{
expression = expression
.ReplaceSurrogate(x => x.OneTwoItems, Project.GetOne_OneTwoItemsSubquery(ctx))
.ReplaceSurrogate(x => x.напаруItems, Project.GetOneнапаруItemsSubquery(ctx))
;
return expression;
}
private IQueryable<IndicatorResult> OneQueryBuilder( DbContext ctx, IEnumerable<(Guid ruleId, Expression expression)> rules0)
{
var rules = rules0.Select(rule =>
(rule.ruleId, OneRecordReplaceSurrogates((Expression<Func<OneRecord, bool>>)rule.expression, ctx))
).ToArray();


            var selector = rules.ToSelector();

            var query = ctx.Set<OneRecord>().Select(selector).Where(x => x.RuleId != Guid.Empty).Select(x => new IndicatorResult
            { Id = x.Id, RuleId = x.RuleId.ToString() });

            //заменяем суррогаты на подзапрсы
            return query;

}
public Expression<Func<TwoRecord,TResult>> TwoRecordReplaceSurrogates<TResult>(
Expression<Func<TwoRecord,TResult>> expression,
DbContext ctx)
{
expression = expression
.ReplaceSurrogate(x => x.OneTwoItems, Project.GetTwo_OneTwoItemsSubquery(ctx))
.ReplaceSurrogate(x => x.напаруItems, Project.GetTwoнапаруItemsSubquery(ctx))
;
    Expression<Func<OneRecord, Guid?>> one0 = x => x._1;
    Expression<Func<OneRecord, Guid?>> another0 = Project.Get_1IndicatorSubquery(ctx);
    expression = Expression.Lambda<Func<TwoRecord, TResult>>(
        new RRExpressionVisitor<OneRecord, Guid?>(one0, another0).Visit(expression.Body), expression.Parameters[0]);
    Expression<Func<OneRecord, Guid?>> one1 = x => x.Уникальныйнаисточник;
    Expression<Func<OneRecord, Guid?>> another1 = Project.GetУникальныйнаисточникIndicatorSubquery(ctx);
    expression = Expression.Lambda<Func<TwoRecord, TResult>>(
        new RRExpressionVisitor<OneRecord, Guid?>(one1, another1).Visit(expression.Body), expression.Parameters[0]);
return expression;
}
private IQueryable<IndicatorResult> TwoQueryBuilder( DbContext ctx, IEnumerable<(Guid ruleId, Expression expression)> rules0)
{
var rules = rules0.Select(rule =>
(rule.ruleId, TwoRecordReplaceSurrogates((Expression<Func<TwoRecord, bool>>)rule.expression, ctx))
).ToArray();


            var selector = rules.ToSelector();

            var query = ctx.Set<TwoRecord>().Select(selector).Where(x => x.RuleId != Guid.Empty).Select(x => new IndicatorResult
            { Id = x.Id, RuleId = x.RuleId.ToString() });

            //заменяем суррогаты на подзапрсы
            return query;

}
private async Task<Expression> createUserFieldExpression_3b6f0dbe732746478e5199483beedb63_ed80ac5019674134b842b7a5977660f6(
Expression expression, Expression filter, bool continueWithZero)
{
Expression<Func<OneRecord, IEnumerable<TwoRecord>>> items = x => x.OneTwoItems;
var userFieldExpression = expression as Expression<Func<OneRecord, object>>;
var userFieldFilter = filter as Expression<Func<TwoRecord, bool>>;

var exp1 = items.UserField(userFieldExpression, userFieldFilter, continueWithZero);
return exp1;
}
private async Task<Expression> createUserFieldExpression_e5bd56ad27304c40baa2158b63a8a221_ed80ac5019674134b842b7a5977660f6(
Expression expression, Expression filter, bool continueWithZero)
{
Expression<Func<TwoRecord, IEnumerable<OneRecord>>> items = x => x.OneTwoItems;
var userFieldExpression = expression as Expression<Func<TwoRecord, object>>;
var userFieldFilter = filter as Expression<Func<OneRecord, bool>>;

var exp1 = items.UserField(userFieldExpression, userFieldFilter, continueWithZero);
return exp1;
}
}
public static class напаруRules
{
    public static readonly Guid red = Guid.Parse("d9212ce6-a06e-4018-b79c-2a2a6cabd5f2");
}
		public static class _1Rules //rules
		{
		}
		public static class УникальныйнаисточникRules //rules
		{
	public static readonly Guid Правило = Guid.Parse("145d5df5-d1b7-4d14-8c97-40658130ae0a"); //rule
		}
public class Project //project
{
	public static readonly Guid _1IndicatorId = Guid.Parse("8ebf49fa-3698-48df-85f5-c140d471608f");
	public static Expression<Func<OneRecord, Guid?>> Get_1IndicatorSubquery(DbContext ctx)
	{
	return x =>
	ctx.Set<SingleCollisionRecord>()
	.Where(y => y.SourceId == Project.OneSourceId && y.IndicatorId == _1IndicatorId && y.RowId == x.Id)
	.Select(y => y.IndicatorRuleId)
	.FirstOrDefault();
	}

	public static readonly Guid УникальныйнаисточникIndicatorId = Guid.Parse("9c326902-6eba-49b7-ad7e-e76038b6e611");
	public static Expression<Func<OneRecord, Guid?>> GetУникальныйнаисточникIndicatorSubquery(DbContext ctx)
	{
	return x =>
	ctx.Set<SingleCollisionRecord>()
	.Where(y => y.SourceId == Project.OneSourceId && y.IndicatorId == УникальныйнаисточникIndicatorId && y.RowId == x.Id)
	.Select(y => y.IndicatorRuleId)
	.FirstOrDefault();
	}

public static readonly Guid OneTwoItemsId = Guid.Parse("ed80ac50-1967-4134-b842-b7a5977660f6");
	public static readonly Guid OneSourceId = Guid.Parse("3b6f0dbe-7327-4647-8e51-99483beedb63");
	public static Expression<Func<OneRecord, IEnumerable<TwoRecord>>> GetOne_OneTwoItemsSubquery(DbContext ctx)
	{
var oneInPair = ctx.Set<PairConnectionRecord>().Where(x => x.SourcePairId == OneTwoItemsId && x.SourceId == OneSourceId);
var anotherInPair = ctx.Set<PairConnectionRecord>().Where(x => x.SourcePairId == OneTwoItemsId && x.SourceId == TwoSourceId);
var p = oneInPair.Join(anotherInPair, sp2 => sp2.Link, sp3 => sp3.Link, (sp2, sp3) => new { sp2.RowId, extId = sp3.RowId }).Join(ctx.Set<TwoRecord>(), p => p.extId, p => p.Id, (s, ext) => new { s.RowId, ext });
return x => p.Where(z => z.RowId == x.Id).Select(x => x.ext);
	}
	public static readonly Guid TwoSourceId = Guid.Parse("e5bd56ad-2730-4c40-baa2-158b63a8a221");
	public static Expression<Func<TwoRecord, IEnumerable<OneRecord>>> GetTwo_OneTwoItemsSubquery(DbContext ctx)
	{
var oneInPair = ctx.Set<PairConnectionRecord>().Where(x => x.SourcePairId == OneTwoItemsId && x.SourceId == TwoSourceId);
var anotherInPair = ctx.Set<PairConnectionRecord>().Where(x => x.SourcePairId == OneTwoItemsId && x.SourceId == OneSourceId);
var p = oneInPair.Join(anotherInPair, sp2 => sp2.Link, sp3 => sp3.Link, (sp2, sp3) => new { sp2.RowId, extId = sp3.RowId }).Join(ctx.Set<OneRecord>(), p => p.extId, p => p.Id, (s, ext) => new { s.RowId, ext });
return x => p.Where(z => z.RowId == x.Id).Select(x => x.ext);
	}
public static readonly Guid напаруId = Guid.Parse("a79250e2-ef9e-46b6-8040-419faa4835df");
    public static Expression<Func<OneRecord, IEnumerable<Guid>>> GetOneнапаруItemsSubquery(DbContext ctx)
    {
        var oneInPair = ctx.Set<PairConnectionRecord>().Where(x => x.SourcePairId == Project.OneTwoItemsId && x.SourceId == Project.OneSourceId);
        var indicators = ctx.Set<PairCollisionRecord>().Where(x => x.SourcePairId == Project.OneTwoItemsId && x.IndicatorId == Project.напаруId);
        var p = oneInPair.Join(indicators, sp2 => sp2.Link, sp3 => sp3.Link, (sp2, sp3) => new { sp2.RowId, RuleId = sp3.IndicatorRuleId });
        return x => p.Where(z => z.RowId == x.Id).Select(p => p.RuleId);
    }
    public static Expression<Func<TwoRecord, IEnumerable<Guid>>> GetTwoнапаруItemsSubquery(DbContext ctx)
    {
        var oneInPair = ctx.Set<PairConnectionRecord>().Where(x => x.SourcePairId == Project.OneTwoItemsId && x.SourceId == Project.TwoSourceId);
        var indicators = ctx.Set<PairCollisionRecord>().Where(x => x.SourcePairId == Project.OneTwoItemsId && x.IndicatorId == Project.напаруId);
        var p = oneInPair.Join(indicators, sp2 => sp2.Link, sp3 => sp3.Link, (sp2, sp3) => new { sp2.RowId, RuleId = sp3.IndicatorRuleId });
        return x => p.Where(z => z.RowId == x.Id).Select(p => p.RuleId);
    }
}
public class DbContext_b0b820cad9ab47c0a812d94e30a3bff2 : DbContext
{
	public DbContext_b0b820cad9ab47c0a812d94e30a3bff2(DbContextOptions options) : base(options)
	{
	}
	public DateTime CalculatedAt {get {return new DateTime(2020,10,29); } }
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
	modelBuilder.ApplyConfiguration(new UnknownTableRecordConfiguration());
	modelBuilder.ApplyConfiguration(new PairConnectionRecordConfiguration("_b17ab2d199264a2788f9f848d0afc461_1_pair_connections"));
	modelBuilder.ApplyConfiguration(new SingleCollisionRecordConfiguration("_b17ab2d199264a2788f9f848d0afc461_1_single_collisions"));
	modelBuilder.ApplyConfiguration(new PairCollisionRecordConfiguration("_b17ab2d199264a2788f9f848d0afc461_1_pair_collisions"));
	modelBuilder.ApplyConfiguration(new OneRecordConfiguration("_b17ab2d199264a2788f9f848d0afc461_one_1"));
	modelBuilder.ApplyConfiguration(new TwoRecordConfiguration("_b17ab2d199264a2788f9f848d0afc461_two_1"));
	}
}

public class DbContext_837bdc974e1e4a0da9fd3602b5b34c21 : DbContext
{
	public DbContext_837bdc974e1e4a0da9fd3602b5b34c21(DbContextOptions options) : base(options)
	{
	}
	public DateTime CalculatedAt {get {return new DateTime(2020,10,29); } }
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
	modelBuilder.ApplyConfiguration(new UnknownTableRecordConfiguration());
	modelBuilder.ApplyConfiguration(new PairConnectionRecordConfiguration("_b17ab2d199264a2788f9f848d0afc461_2_pair_connections"));
	modelBuilder.ApplyConfiguration(new SingleCollisionRecordConfiguration("_b17ab2d199264a2788f9f848d0afc461_2_single_collisions"));
	modelBuilder.ApplyConfiguration(new PairCollisionRecordConfiguration("_b17ab2d199264a2788f9f848d0afc461_2_pair_collisions"));
	modelBuilder.ApplyConfiguration(new OneRecordConfiguration("_b17ab2d199264a2788f9f848d0afc461_one_2"));
	modelBuilder.ApplyConfiguration(new TwoRecordConfiguration("_b17ab2d199264a2788f9f848d0afc461_two_2"));
	}
}

public class OneRecord : RecordBase
{
	public bool? Availability { get; set; }
	public int? Codeconnect { get; set; }
	public string Color { get; set; }
	public decimal? Cost { get; set; }
	public DateTime? Dateofbirth { get; set; }
	public double? Height { get; set; }
	public double? Length { get; set; }
	public string Name { get; set; }
	public int? Number { get; set; }
	public double? NumReal { get; set; }
	public decimal? Profit { get; set; }
	public int? Quantityofcoins { get; set; }
	public DateTime? Readydate { get; set; }
	public int? Salary { get; set; }
	public string Surname { get; set; }
	public bool? Truth { get; set; }
[RuleDependency("ed80ac50-1967-4134-b842-b7a5977660f6")]
	public virtual ICollection<TwoRecord> OneTwoItems { get; set; }
[RuleDependency("8ebf49fa-3698-48df-85f5-c140d471608f")]
	public Guid? _1 { get; set; }
[RuleDependency("a79250e2-ef9e-46b6-8040-419faa4835df")]
	public Guid? напару { get; set; }
[RuleDependency("9c326902-6eba-49b7-ad7e-e76038b6e611")]
	public Guid? Уникальныйнаисточник { get; set; }
[RuleDependency("a79250e2-ef9e-46b6-8040-419faa4835df")]
        public ICollection<Guid> напаруItems { get; set; }
}

internal sealed class OneRecordConfiguration : IEntityTypeConfiguration<OneRecord>
{
private readonly string _tableName;
public OneRecordConfiguration(string tableName)
{
_tableName = tableName;
}
	public void Configure(EntityTypeBuilder<OneRecord> builder)
	{
		builder.ToTable(_tableName, "unifiedschedule");
		builder.HasKey(x => x.Id);
		builder.Property(x => x.Id).HasColumnName("id");
		builder.Property(x => x.Availability).HasColumnName("col30229");
		builder.Property(x => x.Codeconnect).HasColumnName("col30225");
		builder.Property(x => x.Color).HasColumnName("col30231");
		builder.Property(x => x.Cost).HasColumnName("col30234");
		builder.Property(x => x.Dateofbirth).HasColumnName("col30223");
		builder.Property(x => x.Height).HasColumnName("col30232");
		builder.Property(x => x.Length).HasColumnName("col30233");
		builder.Property(x => x.Name).HasColumnName("col30221");
		builder.Property(x => x.Number).HasColumnName("col30226");
		builder.Property(x => x.NumReal).HasColumnName("col30236");
		builder.Property(x => x.Profit).HasColumnName("col30235");
		builder.Property(x => x.Quantityofcoins).HasColumnName("col30227");
		builder.Property(x => x.Readydate).HasColumnName("col30224");
		builder.Property(x => x.Salary).HasColumnName("col30228");
		builder.Property(x => x.Surname).HasColumnName("col30222");
		builder.Property(x => x.Truth).HasColumnName("col30230");
		builder.Ignore(x => x.OneTwoItems);
		builder.Ignore(x => x._1);
		builder.Ignore(x => x.напару);
		builder.Ignore(x => x.Уникальныйнаисточник);
builder.Ignore(x => x.напаруItems);
	}
}

public class TwoRecord : RecordBase
{
	public bool? Availability { get; set; }
	public int? Codeconnect { get; set; }
	public string Color { get; set; }
	public decimal? Cost { get; set; }
	public DateTime? Dateofbirth { get; set; }
	public double? Height { get; set; }
	public double? Length { get; set; }
	public string Name { get; set; }
	public int? Number { get; set; }
	public double? NumReal { get; set; }
	public decimal? Profit { get; set; }
	public int? Quantityofcoins { get; set; }
	public DateTime? Readydate { get; set; }
	public int? Salary { get; set; }
	public string Surname { get; set; }
	public bool? Truth { get; set; }
[RuleDependency("ed80ac50-1967-4134-b842-b7a5977660f6")]
	public virtual ICollection<OneRecord> OneTwoItems { get; set; }
[RuleDependency("a79250e2-ef9e-46b6-8040-419faa4835df")]
        public ICollection<Guid> напаруItems { get; set; }
}

internal sealed class TwoRecordConfiguration : IEntityTypeConfiguration<TwoRecord>
{
private readonly string _tableName;
public TwoRecordConfiguration(string tableName)
{
_tableName = tableName;
}
	public void Configure(EntityTypeBuilder<TwoRecord> builder)
	{
		builder.ToTable(_tableName, "unifiedschedule");
		builder.HasKey(x => x.Id);
		builder.Property(x => x.Id).HasColumnName("id");
		builder.Property(x => x.Availability).HasColumnName("col30245");
		builder.Property(x => x.Codeconnect).HasColumnName("col30241");
		builder.Property(x => x.Color).HasColumnName("col30247");
		builder.Property(x => x.Cost).HasColumnName("col30250");
		builder.Property(x => x.Dateofbirth).HasColumnName("col30239");
		builder.Property(x => x.Height).HasColumnName("col30248");
		builder.Property(x => x.Length).HasColumnName("col30249");
		builder.Property(x => x.Name).HasColumnName("col30237");
		builder.Property(x => x.Number).HasColumnName("col30242");
		builder.Property(x => x.NumReal).HasColumnName("col30252");
		builder.Property(x => x.Profit).HasColumnName("col30251");
		builder.Property(x => x.Quantityofcoins).HasColumnName("col30243");
		builder.Property(x => x.Readydate).HasColumnName("col30240");
		builder.Property(x => x.Salary).HasColumnName("col30244");
		builder.Property(x => x.Surname).HasColumnName("col30238");
		builder.Property(x => x.Truth).HasColumnName("col30246");
		builder.Ignore(x => x.OneTwoItems);
builder.Ignore(x => x.напаруItems);
	}
}


}