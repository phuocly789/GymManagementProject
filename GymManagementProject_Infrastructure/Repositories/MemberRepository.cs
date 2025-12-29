using GymManagementProject_Infrastructure.Data;
using GymManagementProject_Infrastructure.Models;

public interface IMemberRepository : IRepository<Member>
{
    // Add custom methods for Member here if needed
}

public class MemberRepository : Repository<Member>, IMemberRepository
{
    public MemberRepository(GymDbContext context)
        : base(context) { }
}
