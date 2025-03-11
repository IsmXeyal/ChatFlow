using ChatFlow.Domain.Entities.Commons;

namespace ChatFlow.Domain.Entities.Concretes;

public class Token : BaseToken
{
    public int UserId { get; set; }

    // Navigation Property
    public virtual AppUser AppUser { get; set; }
}
