namespace RestApi.Data.Models;

public enum ScheduleImportOperationStatuses
{
    None = 0,
    Executing = 1,
    CompletedSuccessfully = 2,
    CompletedWithWarnings = 3,
    CompletedWithErrors = 4
}
