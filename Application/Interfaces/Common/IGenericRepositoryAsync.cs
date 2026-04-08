using Ardalis.Specification;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Common
{
    public interface IGenericRepositoryAsync<T> where T : class
    {
        Task<T?> GetByIdAsync(Guid id);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task<IReadOnlyList<T>> ListAsync(ISpecification<T> specification);
        //Task<int> CountAsync(ISpecification<T> specification);
    }
}
