namespace Entities;

public partial class JobDependency
{
    public int Id { get; set; }

    public int ParentJobId { get; set; }

    public Job ParentJob { get; set; }

    public int ChildJobId { get; set; }

    public Job ChildJob { get; set; }
}
