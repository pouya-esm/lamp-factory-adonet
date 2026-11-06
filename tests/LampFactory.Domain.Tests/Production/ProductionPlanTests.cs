using LampFactory.Domain.Production;

namespace LampFactory.Domain.Tests.Production;

public class ProductionPlanTests
{
    [Fact]
    public void ProductionPlan_WhenCreated_HasNullDatesAndPriority()
    {
        var plan = new ProductionPlan();

        Assert.Null(plan.IssueDate);
        Assert.Null(plan.TargetDate);
        Assert.Null(plan.Priority);
    }

    [Fact]
    public void ProductionPlan_WhenScheduled_RetainsTargetBeforeIssueThrowsNoError()
    {
        var plan = new ProductionPlan
        {
            ProductionPlanId = 1,
            IssueDate = new DateTime(2026, 2, 1),
            TargetDate = new DateTime(2026, 2, 10),
            Priority = 2
        };

        Assert.Equal(1, plan.ProductionPlanId);
        Assert.Equal(new DateTime(2026, 2, 1), plan.IssueDate);
        Assert.Equal(new DateTime(2026, 2, 10), plan.TargetDate);
        Assert.Equal(2, plan.Priority);
    }
}
