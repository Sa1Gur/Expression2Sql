using System;


namespace QueryingCore.Core;

[AttributeUsage(AttributeTargets.Property)]
public class SourcePairIdAttribute : Attribute
{
    public SourcePairIdAttribute(string objectId) => Value = Guid.Parse(objectId);

    public Guid Value { get; }
}
