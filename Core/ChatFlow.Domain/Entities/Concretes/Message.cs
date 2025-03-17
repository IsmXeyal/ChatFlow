using ChatFlow.Domain.Entities.Commons;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatFlow.Domain.Entities.Concretes;

public class Message : BaseEntity
{
    public string Content { get; set; }
    public DateTime SentDate { get; set; } = DateTime.Now;

    // Foreign Keys
    [ForeignKey("AppUser")]
    public int AppUserId { get; set; }

    [ForeignKey("Group")]
    public int GroupId { get; set; }

    // Navigation Properties
    public AppUser AppUser { get; set; } 
    public Group Group { get; set; }
}
