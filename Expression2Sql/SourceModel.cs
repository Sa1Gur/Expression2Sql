using RestApi.Data.Models;
using System;
using System.Collections.Generic;

namespace RestApi.Data.Models;

public class SourceModel
{
    public virtual int SourceType { get; set; }
    public virtual string RemoteDirectory { get; set; }
    public virtual string Tag { get; set; }
    public virtual string NameEn { get; set; }
    public virtual string Name { get; set; }
    public virtual SourceWbsModel Wbs { get; set; }
    public virtual IDictionary<Guid, SourceAttributeModel> Attributes { get; set; }
    public virtual string LocalId { get; set; }
    public virtual int AttributeCounter { get; set; }
}
