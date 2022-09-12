using Entities;
using Microsoft.EntityFrameworkCore;

namespace EntityFramework;

public partial class DataSourceContext : DbContext, IUnitOfWork
{
    public DbSet<ErrorLog> ErrorLogs { get; set; }
    public DbSet<JobQueue> JobQueues { get; set; }
    public DbSet<Log> Logs { get; set; }
    public DbSet<DetectingCollisionsJob> DetectingCollisionsJobs { get; set; }
    public DbSet<ProcessingConnectionsJob> ProcessingConnectionsJobs { get; set; }
    public DbSet<ProcessingAggregationsJob> ProcessingAggregationsJobs { get; set; }
    public DbSet<GeneratingReportJob> GeneratingReportJobs { get; set; }
    public DbSet<InitializationGanttJob> InitializationGanttJobs { get; set; }
    public DbSet<CreatingVersionJob> CreatingVersionJobs { get; set; }
    public DbSet<CalculatingUserFieldJob> CalculatingUserFieldJobs { get; set; }
    public DbSet<DeletingSourcePairJob> DeletingSourcePairJobs { get; set; }
    public DbSet<DeletingIndicatorJob> DeletingIndicatorJobs { get; set; }
    public DbSet<DeletingProjectJob> DeletingProjectJobs { get; set; }
    public DbSet<DeletingSourceJob> DeletingSourceJobs { get; set; }
    public DbSet<DeletingVersionJob> DeletingVersionJobs { get; set; }
    public DbSet<CopyingProjectJob> CopyingProjectJobs { get; set; }
    public DbSet<SyncingAttributesJob> SyncingAttributesJobs { get; set; }
    public DbSet<Job> Jobs { get; set; }
    public DbSet<QueryProfile> QueryProfiles { get; set; }
    public DataSourceContext(DbContextOptions<DataSourceContext> options) : base(options) {}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
