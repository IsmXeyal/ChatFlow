﻿using ChatFlow.Application.Repositories.Commons;
using ChatFlow.Domain.Entities.Concretes;

namespace ChatFlow.Application.Repositories.Reads;

public interface IAppUserReadRepository : IGenericReadRepository<AppUser>
{
    Task<AppUser?> GetUserByEmailWithRelationsAsync(string email);
    Task<AppUser?> GetUserByUserName(string userName);
    Task<AppUser?> GetUserByRefreshToken(string refreshToken);
    Task<AppUser?> GetUserByRePasswordToken(string rePasswordToken);
    Task<AppUser?> GetUserByEmailConfirmToken(string emailConfrmToken);
    Task<AppUser?> GetUserByUserNameWithRelationsAsync(string userName);
    Task<AppUser?> GetUserByIdAsync(int userId);
    Task<AppUser?> GetUserByConnectionIdWithRelationsAsync(string connectionId);
}
