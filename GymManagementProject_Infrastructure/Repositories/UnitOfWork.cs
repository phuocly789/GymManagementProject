using GymManagementProject_Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

public interface IUnitOfWork : IAsyncDisposable
{
    IRepository<T> Repository<T>()
        where T : class;
    Task<int> SaveChangesAsync();
    Task<IDbContextTransaction> BeginTransactionAsync();
    GymDbContext GetDbContext();
}

public class UnitOfWork : IUnitOfWork
{
    private readonly GymDbContext _context;
    private readonly Dictionary<Type, object> _repositories = new();

    public UnitOfWork(GymDbContext context)
    {
        _context = context;
    }

    public IRepository<T> Repository<T>()
        where T : class
    {
        if (!_repositories.ContainsKey(typeof(T)))
        {
            var repositoryInstance = new Repository<T>(_context);
            _repositories[typeof(T)] = repositoryInstance;
        }
        return (IRepository<T>)_repositories[typeof(T)];
    }

    public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return await _context.Database.BeginTransactionAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
    }

    public GymDbContext GetDbContext() => _context;
}
