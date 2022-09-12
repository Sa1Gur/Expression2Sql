namespace RestApi.Data.Models;

public enum VersionCalculationOperationStatuses
{
    None = 0,

    AwaitingExecution = 1,

    Executing = 2,

    CompletedSuccessfully = 3,

    CompletedWithErrors = 4
}
