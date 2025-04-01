using ChatFlow.Domain.Entities.Concretes;

namespace ChatFlow.Domain.ViewModels;

public class GroupVM
{
    public string GroupName { get; set; }

    public List<AppUserVM> AppUsers { get; set; }
    public List<Message> Messages { get; set; }
}