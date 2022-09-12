using System;

namespace Entities;

public class QueryProfile
{
    /// <summary>
    /// Id
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Наименование сущности
    /// </summary>
    public string Entity { get; set; }

    /// <summary>
    /// Идентификатор сущности
    /// </summary>
    public Guid EntityId { get; set; }

    /// <summary>
    /// Текст запроса
    /// </summary>
    public string? Query { get; set; }

    /// <summary>
    /// Json со списом параметров
    /// </summary>
    public string? ParamsAsJson { get; set; }

    /// <summary>
    /// Получает или устанавливает момент начала выполнения.
    /// </summary>
    public DateTimeOffset? StartedAt { get; set; }

    /// <summary>
    /// Получает или устанавливает момент завершения выполнения.
    /// </summary>
    public DateTimeOffset? EndedAt { get; set; }
}
