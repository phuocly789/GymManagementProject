using GymManagementProject_Infrastructure.Data;
using GymManagementProject_Infrastructure.Models;

public interface IAuditLogRepository : IRepository<AuditLog>
{
    // Add custom methods for AuditLog here if needed
}

public class AuditLogRepository : Repository<AuditLog>, IAuditLogRepository
{
    public AuditLogRepository(GymDbContext context)
        : base(context) { }
}
