using System;

namespace Entities;

public sealed class GeneratingReportJob : Job
{
    public Guid? ReportTemplateId { get; set; }

    public GeneratingReportJob() => Type = JobType.GeneratingReport;
}
