using System;

namespace RestApi.Data.Models;

    public class SourceWbsModel
    {
	public virtual Guid? WbsId { get; set; }
	public virtual Guid? ParentWbsId { get; set; }
	public virtual Guid? WbsOrder { get; set; }
	public virtual Guid? StartDate { get; set; }
	public virtual Guid? FinishDate { get; set; }
	public virtual Guid? SummaryTaskFlag { get; set; }
	public virtual Guid? CompletionPercentage { get; set; }
	public virtual Guid? PlanStartDate { get; set; }
	public virtual Guid? PlanFinishDate { get; set; }
	public virtual Guid? MilestoneFlag { get; set; }
}
