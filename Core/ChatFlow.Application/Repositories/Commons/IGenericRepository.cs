using ChatFlow.Domain.Entities.Commons;

namespace ChatFlow.Application.Repositories.Commons;

public interface IGenericRepository<T> where T : BaseEntity, new()
{
}
