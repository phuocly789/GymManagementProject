using GymManagementProject_Infrastructure.Data;
using GymManagementProject_Infrastructure.Models;

public interface IUserRepository : IRepository<User>
{
    // Add custom methods for User here if needed
}

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(GymDbContext context)
        : base(context) { }
}
