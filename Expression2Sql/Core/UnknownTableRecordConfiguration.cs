using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QueryingCore.Core;

public class UnknownTableRecordConfiguration : IEntityTypeConfiguration<UnknownTableRecord>
{
    public const string TableName = "unknown_table";
    public void Configure(EntityTypeBuilder<UnknownTableRecord> builder)
    {
        builder.ToTable(TableName, "fake_table_name");
        builder.HasNoKey();
        builder.Property(x => x.Id).HasColumnName("id");
    }
}

