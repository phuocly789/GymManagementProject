using GymManagementProject_Infrastructure.Data;
using GymManagementProject_Infrastructure.Models;

public interface IEmailVerificationTokenRepository : IRepository<EmailVerificationToken>
{
    // Add custom methods for EmailVerificationToken here if needed
}

public class EmailVerificationTokenRepository
    : Repository<EmailVerificationToken>,
        IEmailVerificationTokenRepository
{
    public EmailVerificationTokenRepository(GymDbContext context)
        : base(context) { }
}
