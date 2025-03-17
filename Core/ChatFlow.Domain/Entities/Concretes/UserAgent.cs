using ChatFlow.Domain.Entities.Commons;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatFlow.Domain.Entities.Concretes;

public class UserAgent : BaseEntity
{
    public string OS { get; set; }
    public string Browser { get; set; }

    [ForeignKey("AppUser")]
    public int UserId { get; set; }

    // Navigation Property
    public AppUser User { get; set; }
}
