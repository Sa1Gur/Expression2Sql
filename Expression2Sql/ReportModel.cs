using System;

namespace RestApi.Data.Models;

public class ReportModel
{
    public virtual string Name { get; set; }

    public virtual DateTime CreatedAt { get; set; }

    public virtual Guid? ReportTemplateId { get; set; }
    public virtual string OriginalReportTemplateName { get; set; }

    public virtual string FileId { get; set; }
    public virtual string LocalId { get; set; }
}
