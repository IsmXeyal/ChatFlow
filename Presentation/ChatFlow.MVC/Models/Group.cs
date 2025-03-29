using ChatFlow.Domain.ViewModels;

namespace ChatFlow.MVC.Models;

public class Group
{
    public string GroupName { get; set; }

    public List<AppUserVM> Clients { get; set; } = new();
}
