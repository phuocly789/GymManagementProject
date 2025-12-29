using GymManagementProject_Infrastructure.Data;
using GymManagementProject_Infrastructure.Models;

public interface IBookingRepository : IRepository<Booking>
{
    // Add custom methods for Booking here if needed
}

public class BookingRepository : Repository<Booking>, IBookingRepository
{
    public BookingRepository(GymDbContext context)
        : base(context) { }
}
