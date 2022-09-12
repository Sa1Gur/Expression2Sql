using System;

namespace Entities;

public partial class Log
{
    public int Id { get; set; }

    public DateTimeOffset? StartDateTime { get; set; }

    public DateTimeOffset? EndDateTime { get; set; }

    public int OperationId { get; set; }

    public string? ResultOperation { get; set; }

    public string? Info { get; set; }

    public string? ServiceName { get; set; }
}
