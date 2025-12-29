using GymManagementProject_Infrastructure.Data;
using GymManagementProject_Infrastructure.Models;

public interface IInvoiceRepository : IRepository<Invoice>
{
    // Add custom methods for Invoice here if needed
}

public class InvoiceRepository : Repository<Invoice>, IInvoiceRepository
{
    public InvoiceRepository(GymDbContext context)
        : base(context) { }
}
