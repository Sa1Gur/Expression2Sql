using System;

namespace Entities;

/// <summary>
/// Сущность работы по синхронизации атрибутов источника с таблицей графика.
/// </summary>
public sealed class SyncingAttributesJob : Job
{
    /// <summary>
    /// Получает или устанавливает идентификатор источника.
    /// </summary>
    public Guid? SourceId { get; set; }

    public SyncingAttributesJob() => Type = JobType.SyncingAttributes;
}
