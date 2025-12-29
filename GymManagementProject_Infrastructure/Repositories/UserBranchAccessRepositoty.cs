using GymManagementProject_Infrastructure.Data;
using GymManagementProject_Infrastructure.Models;

public interface IUserBranchAccessRepository : IRepository<UserBranchAccess>
{
    // Add custom methods for UserBranchAccess here if needed
}

public class UserBranchAccessRepository : Repository<UserBranchAccess>, IUserBranchAccessRepository
{
    public UserBranchAccessRepository(GymDbContext context)
        : base(context) { }
}
