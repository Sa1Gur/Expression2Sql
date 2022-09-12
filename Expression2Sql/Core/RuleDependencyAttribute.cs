using System;

namespace QueryingCore.Core;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class RuleDependencyAttribute : Attribute
{
    public RuleDependencyAttribute(string objectId) =>  ObjectId = Guid.Parse(objectId);        

    public Guid ObjectId { get; }
}
