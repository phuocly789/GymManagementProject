using GymManagementProject_Infrastructure.Data;
using GymManagementProject_Infrastructure.Models;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    // Add custom methods for RefreshToken here if needed
}

public class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(GymDbContext context)
        : base(context) { }
}
