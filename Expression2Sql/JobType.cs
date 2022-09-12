namespace Entities;

public enum JobType
{
    ProcessingConnections = 0,
    DetectingCollision = 1,
    GeneratingReport = 2,
    CreatingVersion = 3,
    DeletingSourcePair = 4,
    DeletingIndicator = 5,
    DeletingProject = 6,
    DeletingSource = 7,
    DeletingVersion = 8,
    CopyingProject = 9,
    CalculatingVersion = 10,
    ProcessingAggregations = 11,
    CalculatingUserField = 12,
    SyncingAttributes = 13,
    InitializationGantt = 14
}
