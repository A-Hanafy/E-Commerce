using ECommerce.Core.Entities;
using ECommerce.Core.Specifications;

namespace ECommerce.Core.Interfaces;

public interface IGenericRepository<T>
    where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id);

    Task<IReadOnlyList<T>> GetAllAsync();

    Task<IReadOnlyList<T>> GetAllWithSpecAsync(ISpecification<T> specification);

    Task<T?> GetEntityWithSpecAsync(ISpecification<T> specification);

    Task<int> CountAsync(ISpecification<T> specification);

    Task AddAsync(T entity);

    void Update(T entity);

    void Delete(T entity);
}
