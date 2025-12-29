using GymManagementProject_Infrastructure.Data;
using GymManagementProject_Infrastructure.Models;

public interface IBranchRepository : IRepository<Branch>
{
    // Add custom methods for Branch here if needed
}

public class BranchRepository : Repository<Branch>, IBranchRepository
{
    public BranchRepository(GymDbContext context)
        : base(context) { }
}
