using System;

namespace Entities
{
    /// <summary>
    /// Сущность работы по обработке свуязей между источниками в паре.
    /// </summary>
    public sealed class ProcessingConnectionsJob : Job
    {
        /// <summary>
        /// Получает или устанавлвиает идентификатор пары источникаов в ТФлекс.
        /// </summary>
        public Guid? SourcePairId { get; set; }

        public ProcessingConnectionsJob() => Type = JobType.ProcessingConnections;
    }
}
