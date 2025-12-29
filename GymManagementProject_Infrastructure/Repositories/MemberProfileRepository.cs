using GymManagementProject_Infrastructure.Data;
using GymManagementProject_Infrastructure.Models;

public interface IMemberProfileRepository : IRepository<MemberProfile>
{
    // Add custom methods for MemberProfile here if needed
}

public class MemberProfileRepository : Repository<MemberProfile>, IMemberProfileRepository
{
    public MemberProfileRepository(GymDbContext context)
        : base(context) { }
}
