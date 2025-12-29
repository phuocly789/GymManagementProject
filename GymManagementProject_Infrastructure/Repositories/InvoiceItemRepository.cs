using GymManagementProject_Infrastructure.Data;
using GymManagementProject_Infrastructure.Models;

public interface IInvoiceItemRepository : IRepository<InvoiceItem>
{
    // Add custom methods for InvoiceItem here if needed
}

public class InvoiceItemRepository : Repository<InvoiceItem>, IInvoiceItemRepository
{
    public InvoiceItemRepository(GymDbContext context)
        : base(context) { }
}
