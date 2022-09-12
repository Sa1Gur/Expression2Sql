using System;
using System.Collections.Generic;

namespace RestApi.Data.Models;

public class UserFieldModel
{
    public virtual string NameEn { get; set; }
    public virtual string Name { get; set; }
    public virtual string ColumnName { get; set; }
    public virtual DateTime CreatedAt { get; set; }
    public virtual string Filter { get; set; }
    public virtual string Expression { get; set; }
    public virtual DateTime ModifiedAt { get; set; }
    public virtual bool ContinueWithZero { get; set; }
    public virtual Guid SourceId { get; set; }
    
    public virtual Guid? SourcePairId { get; set; }
    public virtual DataTypes Type { get; set; }
    public virtual string LocalId { get; set; }

    public virtual IDictionary<string, DependencyModel> Dependencies { get; set; }
}
