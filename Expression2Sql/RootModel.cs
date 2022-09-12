using System;
using System.Collections.Generic;

namespace RestApi.Data.Models;

public class RootModel
{
    // справочники
    public virtual IDictionary<Guid, ColorModel> Colors { get; set; }
    public virtual IDictionary<string, ShapeModel> Shapes { get; set; }

    // проекты
    public virtual IDictionary<Guid, ProjectModel> Projects { get; set; }

    // логи
    public virtual IDictionary<Guid, ScheduleImportOperationModel> ScheduleImportOperations { get; set; }
    public virtual IDictionary<Guid, ReportGenerationOperationModel> ReportGenerationOperations { get; set; }
    public virtual IDictionary<Guid, VersionCalculationOperationModel> VersionCalculationOperations { get; set; }
}
