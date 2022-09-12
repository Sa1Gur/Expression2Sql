using System;

namespace Entities;

/// <summary>
/// Сущность работы по инициализации ганта.
/// </summary>
public sealed class InitializationGanttJob : Job
{
    public InitializationGanttJob() => Type = JobType.InitializationGantt;
}
