using GymManagementProject_Infrastructure.Data;
using GymManagementProject_Infrastructure.Models;

public interface INotificationLogRepository : IRepository<NotificationLog>
{
    // Add custom methods for NotificationLog here if needed
}

public class NotificationLogRepository : Repository<NotificationLog>, INotificationLogRepository
{
    public NotificationLogRepository(GymDbContext context)
        : base(context) { }
}
