using ChatFlow.Application.Repositories.Reads;
using ChatFlow.Persistence.DbContexts;
using ChatFlow.Persistence.Repositories.Commons;
using Microsoft.EntityFrameworkCore;

namespace ChatFlow.Persistence.Repositories.GroupRepo;

public class GroupReadRepository : GenericReadRepository<Domain.Entities.Concretes.Group>, IGroupReadRepository
{
    public GroupReadRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Domain.Entities.Concretes.Group?> GetGroupByNameAsync(string groupName)
    {
        return await _context.Groups
            .Include(g => g.AppUsers)
            .FirstOrDefaultAsync(g => g.GroupName == groupName);
    }

    public async Task<List<Domain.Entities.Concretes.Group>> GetAllGroupsAsync()
    {
        return await _context.Groups.Include(g => g.AppUsers).ToListAsync();
    }
}
