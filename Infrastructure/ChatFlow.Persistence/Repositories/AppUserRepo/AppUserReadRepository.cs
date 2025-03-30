using ChatFlow.Application.Repositories.Reads;
using ChatFlow.Domain.Entities.Concretes;
using ChatFlow.Persistence.DbContexts;
using ChatFlow.Persistence.Repositories.Commons;
using Microsoft.EntityFrameworkCore;

namespace ChatFlow.Persistence.Repositories.AppUserRepo;

public class AppUserReadRepository : GenericReadRepository<Domain.Entities.Concretes.AppUser>, IAppUserReadRepository
{
    public AppUserReadRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Domain.Entities.Concretes.AppUser?> GetUserByEmailWithRelationsAsync(string email)
    {
        return await _context.AppUsers
            .Include(u => u.RefreshToken)
            .Include(u => u.EmailConfirmToken)
            .Include(u => u.RePasswordToken)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<Domain.Entities.Concretes.AppUser?> GetUserByRefreshToken(string refreshToken)
    {
        return await _entity
        .Include(u => u.RefreshToken)
        .FirstOrDefaultAsync(p => p.RefreshToken.Token == refreshToken);
    }

    public async Task<Domain.Entities.Concretes.AppUser?> GetUserByEmailConfirmToken(string emailConfirmToken)
    {
        return await _entity
        .Include(u => u.EmailConfirmToken)
        .FirstOrDefaultAsync(p => p.EmailConfirmToken.Token == emailConfirmToken);
    }

    public async Task<Domain.Entities.Concretes.AppUser?> GetUserByRePasswordToken(string rePasswordToken)
    {
        return await _entity
        .Include(u => u.RePasswordToken)
        .FirstOrDefaultAsync(p => p.RePasswordToken.Token == rePasswordToken);
    }

    public async Task<Domain.Entities.Concretes.AppUser?> GetUserByUserName(string userName)
    {
        return await _entity.FirstOrDefaultAsync(p => p.UserName == userName);
    }

    public async Task<Domain.Entities.Concretes.AppUser?> GetUserByUserNameWithRelationsAsync(string userName)
    {
        return await _context.AppUsers
            .Include(u => u.RefreshToken) 
            .Include(u => u.EmailConfirmToken) 
            .Include(u => u.RePasswordToken) 
            .FirstOrDefaultAsync(u => u.UserName == userName);
    }

    public async Task<Domain.Entities.Concretes.AppUser?> GetUserByIdAsync(int userId)
    {
        return await _entity.FirstOrDefaultAsync(p => p.Id == userId);
    }
}