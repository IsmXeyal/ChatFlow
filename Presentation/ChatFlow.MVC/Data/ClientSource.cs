using ChatFlow.Domain.ViewModels;
using ChatFlow.MVC.Models;

namespace ChatFlow.MVC.Data;

public class ClientSource
{
    public static List<AppUserVM> Clients { get; set; } = new();
}
