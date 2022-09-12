using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QueryingCore.Core;

public sealed class PairConnectionRecordConfiguration : IEntityTypeConfiguration<PairConnectionRecord>
{
    readonly string _tableName;

    public PairConnectionRecordConfiguration(string tableName) => _tableName = tableName;

    public void Configure(EntityTypeBuilder<PairConnectionRecord> builder)
    {
        builder.ToTable(_tableName, "fake_table_name");
        builder.HasNoKey();
        builder.Property(x => x.SourcePairId).HasColumnName("source_pair_id");
        builder.Property(x => x.SourceId).HasColumnName("source_id");
        builder.Property(x => x.RowId).HasColumnName("row_id");
        builder.Property(x => x.Link).HasColumnName("link");
        builder.Property(x => x.ConnectionType).HasColumnName("connection_type");
    }
}
