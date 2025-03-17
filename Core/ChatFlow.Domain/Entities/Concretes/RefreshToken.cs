
using ChatFlow.Domain.Entities.Commons;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatFlow.Domain.Entities.Concretes;

public class RefreshToken : BaseToken
{
    [ForeignKey("AppUser")]
    public int UserId { get; set; }

    // Navigation Property
    public AppUser AppUser { get; set; }
}
