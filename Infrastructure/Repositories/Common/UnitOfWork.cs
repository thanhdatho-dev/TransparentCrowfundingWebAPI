using Application.Interfaces.Common;
using Infrastructure.Contexts;

namespace Infrastructure.Repositories.Common
{
    public class UnitOfWork(ApplicationDbContext context) : IUnitOfWork
    {
        private readonly ApplicationDbContext _context = context;

        public void Dispose() => _context.Dispose();
        
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
            _context.SaveChangesAsync(cancellationToken);
    }
}
