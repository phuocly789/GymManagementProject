using GymManagementProject_Infrastructure.Data;
using GymManagementProject_Infrastructure.Models;

public interface IMessageTemplateRepository : IRepository<MessageTemplate>
{
    // Add custom methods for MessageTemplate here if needed
}

public class MessageTemplateRepository : Repository<MessageTemplate>, IMessageTemplateRepository
{
    public MessageTemplateRepository(GymDbContext context)
        : base(context) { }
}
