using ChatFlow.Application.Repositories.Commons;
using ChatFlow.Domain.Entities.Commons;
using ChatFlow.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace ChatFlow.Persistence.Repositories.Commons;

public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity, new()
{
    protected AppDbContext _context;
    protected DbSet<T> _entity;

    public GenericRepository(AppDbContext context)
    {
        _context = context;
        _entity = context.Set<T>();
    }
}
