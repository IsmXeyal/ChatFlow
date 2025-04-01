using ChatFlow.Application.Repositories.Reads;
using ChatFlow.Application.Repositories.Writes;
using ChatFlow.Application.Services;
using ChatFlow.Domain.Entities.Concretes;

namespace ChatFlow.Infrastructure.Services;

public class GroupService : IGroupService
{
    private readonly IGroupReadRepository _readGroupRepository;
    private readonly IGroupWriteRepository _writeGroupRepository;

    public GroupService(IGroupReadRepository readGroupRepository, IGroupWriteRepository writeGroupRepository)
    {
        _readGroupRepository = readGroupRepository;
        _writeGroupRepository = writeGroupRepository;
    }

    public async Task AddGroupAsync(Group group)
    {
        await _writeGroupRepository.AddAsync(group);
        await _writeGroupRepository.SaveChangesAsync();
    }
}
