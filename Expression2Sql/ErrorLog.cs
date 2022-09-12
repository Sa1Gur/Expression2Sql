using System;

namespace Entities;

//Сущность Журнал ошибок
public partial class ErrorLog
{
    public int Id { get; set; }

    public int? LogId { get; set; }

    public DateTimeOffset? DateError { get; set; }

    public string? Message { get; set; }

    public string? Description { get; set; }

    public virtual Log Log { get; set; }
}
