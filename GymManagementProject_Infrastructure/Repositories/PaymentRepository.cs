using GymManagementProject_Infrastructure.Data;
using GymManagementProject_Infrastructure.Models;

public interface IPaymentRepository : IRepository<Payment>
{
    // Add custom methods for Payment here if needed
}

public class PaymentRepository : Repository<Payment>, IPaymentRepository
{
    public PaymentRepository(GymDbContext context)
        : base(context) { }
}
