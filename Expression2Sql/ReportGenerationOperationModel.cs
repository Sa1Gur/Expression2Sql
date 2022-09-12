using System;

namespace RestApi.Data.Models;

/// <summary>
/// Операция генерации отчета, соответсвует <see cref="TFlexEntities.ReportGenerationOperation"/>
/// </summary>
public class ReportGenerationOperationModel
{
    public virtual Guid? ProjectId { get; set; }
    public virtual Guid? VersionId { get; set; }
    public virtual Guid? ReportTemplateId { get; set; }
    public virtual Guid? GeneratedReportId { get; set; }

    public virtual ReportGenerationOperationStatuses Status { get; set; }
    public virtual DateTime? BeginAt { get; set; }
    public virtual DateTime? EndAt { get; set; }

    public virtual string Message { get; set; }
    public virtual string Details { get; set; }
}
