using ECommerce.Core.Entities;
using ECommerce.Core.Interfaces;
using ECommerce.Core.Specifications;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public class GenericRepository<T>(AppDbContext context) : IGenericRepository<T>
    where T : BaseEntity
{
    private readonly AppDbContext _context = context;

    public async Task<T?> GetByIdAsync(int id)
    {
        return await _context.Set<T>().FindAsync(id);
    }

    public async Task<IReadOnlyList<T>> GetAllAsync()
    {
        return await _context.Set<T>().ToListAsync();
    }

    public async Task<IReadOnlyList<T>> GetAllWithSpecAsync(ISpecification<T> specification)
    {
        var query = SpecificationEvaluator<T>.GetQuery(_context.Set<T>(), specification);
        return await query.ToListAsync();
    }

    public async Task<T?> GetEntityWithSpecAsync(ISpecification<T> specification)
    {
        var query = SpecificationEvaluator<T>.GetQuery(_context.Set<T>(), specification);
        return await query.FirstOrDefaultAsync();
    }

    public async Task<int> CountAsync(ISpecification<T> specification)
    {
        var query = SpecificationEvaluator<T>.GetQuery(_context.Set<T>(), specification);
        return await query.CountAsync();
    }

    public async Task AddAsync(T entity)
    {
        await _context.Set<T>().AddAsync(entity);
    }

    public void Update(T entity)
    {
        _context.Set<T>().Update(entity);
    }

    public void Delete(T entity)
    {
        _context.Set<T>().Remove(entity);
    }
}
