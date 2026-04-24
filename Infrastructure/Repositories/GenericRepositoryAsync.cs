using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Domain.Interfaces.Repositories;
using Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class GenericRepositoryAsync<T>(ApplicationDbContext context) : IGenericRepositoryAsync<T> where T : class
    {
        private readonly ApplicationDbContext _context = context;

        public async Task AddAsync(T entity) =>
            await _context.Set<T>().AddAsync(entity).ConfigureAwait(false);

        public async Task DeleteAsync(T entity) =>
            _context.Set<T>().Remove(entity);

        public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            await _context.Set<T>().FindAsync([id, cancellationToken], cancellationToken: cancellationToken).ConfigureAwait(false);

        public async Task<IReadOnlyList<T>> ListAsync(ISpecification<T> spec, CancellationToken cancellationToken = default) =>
            await SpecificationEvaluator.Default.GetQuery(_context.Set<T>(), spec).ToListAsync(cancellationToken).ConfigureAwait(false);

        public async Task UpdateAsync(T entity) =>
            _context.Set<T>().Update(entity);
    }
}
