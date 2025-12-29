using GymManagementProject_Infrastructure.Data;
using GymManagementProject_Infrastructure.Models;

public interface ITrainerRepository : IRepository<Trainer>
{
    // Add custom methods for Trainer here if needed
}

public class TrainerRepository : Repository<Trainer>, ITrainerRepository
{
    public TrainerRepository(GymDbContext context)
        : base(context) { }
}
