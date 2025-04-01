using ChatFlow.Application.Repositories.Writes;
using ChatFlow.Persistence.DbContexts;
using ChatFlow.Persistence.Repositories.Commons;

namespace ChatFlow.Persistence.Repositories.GroupRepo;

public class GroupWriteRepository : GenericWriteRepository<Domain.Entities.Concretes.Group>, IGroupWriteRepository
{
    public GroupWriteRepository(AppDbContext context) : base(context)
    {
    }
}
