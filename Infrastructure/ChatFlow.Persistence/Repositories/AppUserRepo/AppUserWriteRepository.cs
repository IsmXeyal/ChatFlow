using ChatFlow.Application.Repositories.Writes;
using ChatFlow.Persistence.DbContexts;
using ChatFlow.Persistence.Repositories.Commons;

namespace ChatFlow.Persistence.Repositories.AppUser;

public class AppUserWriteRepository : GenericWriteRepository<Domain.Entities.Concretes.AppUser>, IAppUserWriteRepository
{
    public AppUserWriteRepository(AppDbContext context) : base(context)
    {
    }
}
