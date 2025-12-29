using GymManagementProject_Infrastructure.Data;
using GymManagementProject_Infrastructure.Models;

public interface ICheckinRepository : IRepository<Checkin>
{
    // Add custom methods for Checkin here if needed
}

public class CheckinRepository : Repository<Checkin>, ICheckinRepository
{
    public CheckinRepository(GymDbContext context)
        : base(context) { }
}
