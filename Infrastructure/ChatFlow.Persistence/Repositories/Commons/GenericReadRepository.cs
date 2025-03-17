using ChatFlow.Application.Repositories.Commons;
using ChatFlow.Domain.Entities.Commons;
using ChatFlow.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ChatFlow.Persistence.Repositories.Commons;

public class GenericReadRepository<T> : GenericRepository<T>, IGenericReadRepository<T> where T : BaseEntity, new()
{
    public GenericReadRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _entity.ToListAsync();
    }

    public async Task<IQueryable<T>> GetByExpressionAsync(Expression<Func<T, bool>> expression)
    {
        return _entity.Where(expression);
    }

    public async Task<T> GetByIdAsync(int id)
    {
        return await _entity.FirstOrDefaultAsync(x => x.Id == id);
    }
}