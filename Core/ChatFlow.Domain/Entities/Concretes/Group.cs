using ChatFlow.Domain.Entities.Commons;

namespace ChatFlow.Domain.Entities.Concretes;

public class Group : BaseEntity
{
    public string GroupName { get; set; }

    public ICollection<AppUser> AppUsers { get; set; }
    public ICollection<Message> Messages { get; set; }
}
