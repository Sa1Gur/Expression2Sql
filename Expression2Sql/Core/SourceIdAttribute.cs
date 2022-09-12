using System;

namespace QueryingCore.Core;

/// <summary>
/// Вешается при кодогенерации класс записи графика
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class SourceIdAttribute : Attribute
{
    public SourceIdAttribute(string objectId) => Value = Guid.Parse(objectId);

    public Guid Value { get; }
}
