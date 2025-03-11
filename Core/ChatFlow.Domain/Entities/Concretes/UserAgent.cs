using ChatFlow.Domain.Entities.Commons;

namespace ChatFlow.Domain.Entities.Concretes;

public class UserAgent : BaseEntity
{
    public string OS { get; set; }
    public string Browser { get; set; }
    public int UserId { get; set; }

    // Navigation Property
    public virtual AppUser User { get; set; }
}
