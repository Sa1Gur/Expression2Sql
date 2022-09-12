using System;

namespace QueryingCore;

public class SourcePairBuilder
{
    private readonly Guid _sourcePairId;
    private readonly string _name;

    public SourcePairBuilder(Guid sourcePairId, string name)
    {
        _sourcePairId = sourcePairId;
        _name = name;
    }

    public Guid SourcePairId => _sourcePairId;
    public string SourcePairIdConstantName => $"{_name}ItemsId";

    public string SourcePairIdConstantPath => $"Project.{SourcePairIdConstantName}";
}
