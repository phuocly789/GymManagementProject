using GymManagementProject_Infrastructure.Data;
using GymManagementProject_Infrastructure.Models;

public interface IPermissionRepository : IRepository<Permission>
{
    // Add custom methods for Permission here if needed
}

public class PermissionRepository : Repository<Permission>, IPermissionRepository
{
    public PermissionRepository(GymDbContext context)
        : base(context) { }
}
