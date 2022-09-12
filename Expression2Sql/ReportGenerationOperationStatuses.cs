namespace RestApi.Data.Models;

public enum ReportGenerationOperationStatuses
{
    None = 0,
    AwaitingExecution = 1,
    Executing = 2,
    CompletedWithErrors = 3,
    CompletedSuccessfully = 4
}
