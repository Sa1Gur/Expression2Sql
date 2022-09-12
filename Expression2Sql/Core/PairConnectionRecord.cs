using System;

namespace QueryingCore.Core;

public class PairConnectionRecord
{
    public Guid SourcePairId { get; set; }
    public Guid SourceId { get; set; }
    public int RowId { get; set; }
    public int Link { get; set; }
    public PairConnectionType ConnectionType { get; set; }
}
