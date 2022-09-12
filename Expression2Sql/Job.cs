using System.Collections.Generic;

namespace Entities;

/// <summary>
/// Сущность выполняемой работы.
/// </summary>
public abstract class Job
{
    /// <summary>
    /// Получает или устанавливает идентификатор.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Получает или устанавливает тип задания.
    /// </summary>
    public JobType Type { get; protected set; }

    /// <summary>
    /// Получает или устанавливает текущий статус работы.
    /// </summary>
    public JobStatus Status { get; set; }

    /// <summary>
    /// Получает или устанавливает идентификатор задачи в очереди.
    /// </summary>
    public int JobQueueId { get; set; }

    /// <summary>
    /// Получает или устанавливает задание в очереди.
    /// </summary>
    public JobQueue JobQueue { get; set; }

    /// <summary>
    /// Получает или устанавливает работы, которые необходимо завершить до выполнения текущей.
    /// </summary>
    public ICollection<JobDependency> ParentJobDependencies { get; } = new HashSet<JobDependency>();

    /// <summary>
    /// Получает или устанавливает работы завиящие от текущей.
    /// </summary>
    public ICollection<JobDependency> ChildJobDependencies { get; } = new HashSet<JobDependency>();
}
