using ChatFlow.Application.Repositories.Commons;
using ChatFlow.Domain.Entities.Concretes;

namespace ChatFlow.Application.Repositories.Reads;

public interface IGroupReadRepository : IGenericReadRepository<Group>
{
    Task<Group?> GetGroupByNameAsync(string groupName);
    Task<List<Group>> GetAllGroupsAsync();
    Task<List<Group>> GetGroupsByUserIdAsync(int userId);
}
