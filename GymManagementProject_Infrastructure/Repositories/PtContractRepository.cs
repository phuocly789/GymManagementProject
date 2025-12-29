using GymManagementProject_Infrastructure.Data;
using GymManagementProject_Infrastructure.Models;

public interface IPtContractRepository : IRepository<PtContract>
{
    // Add custom methods for PtContract here if needed
}

public class PtContractRepository : Repository<PtContract>, IPtContractRepository
{
    public PtContractRepository(GymDbContext context)
        : base(context) { }
}
