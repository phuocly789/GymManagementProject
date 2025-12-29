using GymManagementProject_Infrastructure.Data;
using GymManagementProject_Infrastructure.Models;

public interface IRoleRepository : IRepository<Role>
{
    // Add custom methods for Role here if needed
}

public class RoleRepository : Repository<Role>, IRoleRepository
{
    public RoleRepository(GymDbContext context)
        : base(context) { }
}
