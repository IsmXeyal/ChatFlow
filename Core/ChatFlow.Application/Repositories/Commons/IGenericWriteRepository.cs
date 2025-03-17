using ChatFlow.Domain.Entities.Commons;

namespace ChatFlow.Application.Repositories.Commons;

public interface IGenericWriteRepository<T> : IGenericRepository<T> where T : BaseEntity, new()
{
    Task AddAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
    Task DeleteAsync(T entity);
    Task SaveChangesAsync();
}
