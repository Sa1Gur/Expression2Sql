using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QueryingCore.Core;

public sealed class PairCollisionRecordConfiguration : IEntityTypeConfiguration<PairCollisionRecord>
{
    readonly string _tableName;

    public PairCollisionRecordConfiguration(string tableName) => _tableName = tableName;

    public void Configure(EntityTypeBuilder<PairCollisionRecord> builder)
    {
        builder.ToTable(_tableName, "fake_table_name");
        builder.HasNoKey();
        builder.Property(x => x.SourcePairId).HasColumnName("source_pair_id");
        builder.Property(x => x.Link).HasColumnName("link");
        builder.Property(x => x.IndicatorId).HasColumnName("indicator_id");
        builder.Property(x => x.IndicatorRuleId).HasColumnName("indicator_rule_id");

    }
}
