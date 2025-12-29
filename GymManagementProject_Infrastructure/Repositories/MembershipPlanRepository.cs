using GymManagementProject_Infrastructure.Data;
using GymManagementProject_Infrastructure.Models;

public interface IMembershipPlanRepository : IRepository<MembershipPlan>
{
    // Add custom methods for MembershipPlan here if needed
}

public class MembershipPlanRepository : Repository<MembershipPlan>, IMembershipPlanRepository
{
    public MembershipPlanRepository(GymDbContext context)
        : base(context) { }
}
