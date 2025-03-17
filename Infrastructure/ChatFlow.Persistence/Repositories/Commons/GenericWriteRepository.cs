using ChatFlow.Application.Repositories.Commons;
using ChatFlow.Domain.Entities.Commons;
using ChatFlow.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace ChatFlow.Persistence.Repositories.Commons;

public class GenericWriteRepository<T> : GenericRepository<T>, IGenericWriteRepository<T>
    where T : BaseEntity, new()
{
    public GenericWriteRepository(AppDbContext context) : base(context)
    {
    }

    public async Task AddAsync(T entity)
    {
        await _entity.AddAsync(entity);
    }

    public async Task AddRangeAsync(IEnumerable<T> entities)
    {
        await _entity.AddRangeAsync(entities);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _entity.FirstOrDefaultAsync(x => x.Id == id);
        if (entity != null)
        {
            _entity.Remove(entity);
        }
    }

    public Task DeleteAsync(T entity)
    {
        _entity.Remove(entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(T entity)
    {
        _entity.Update(entity);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
