using GymManagementProject_Infrastructure.Data;
using GymManagementProject_Infrastructure.Models;

public interface ITenantRepository : IRepository<Tenant>
{
    // Add custom methods for Tenant here if needed
}

public class TenantRepository : Repository<Tenant>, ITenantRepository
{
    public TenantRepository(GymDbContext context)
        : base(context) { }
}
