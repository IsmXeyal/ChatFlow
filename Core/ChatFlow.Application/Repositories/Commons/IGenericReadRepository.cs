using ChatFlow.Domain.Entities.Commons;
using System.Linq.Expressions;

namespace ChatFlow.Application.Repositories.Commons;

public interface IGenericReadRepository<T> : IGenericRepository<T> where T : BaseEntity, new()
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<IQueryable<T>> GetByExpressionAsync(Expression<Func<T, bool>> expression);
    Task<T> GetByIdAsync(int id);
}
