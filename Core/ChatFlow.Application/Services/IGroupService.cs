using ChatFlow.Domain.Entities.Concretes;

namespace ChatFlow.Application.Services;

public interface IGroupService
{
    Task AddGroupAsync(Group group);
}
