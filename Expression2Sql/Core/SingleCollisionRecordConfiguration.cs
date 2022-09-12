using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QueryingCore.Core;

public class SingleCollisionRecordConfiguration : IEntityTypeConfiguration<SingleCollisionRecord>
{
    readonly string _tableName;

    public SingleCollisionRecordConfiguration(string tableName) => _tableName = tableName;

    public void Configure(EntityTypeBuilder<SingleCollisionRecord> builder)
    {
        builder.ToTable(_tableName, "fake_table_name");
        builder.HasNoKey();
        builder.Property(x => x.SourceId).HasColumnName("source_id");
        builder.Property(x => x.RowId).HasColumnName("row_id");
        builder.Property(x => x.IndicatorId).HasColumnName("indicator_id");
        builder.Property(x => x.IndicatorRuleId).HasColumnName("indicator_rule_id");
    }
}
