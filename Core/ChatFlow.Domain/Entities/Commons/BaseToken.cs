namespace ChatFlow.Domain.Entities.Commons;

public abstract class BaseToken : BaseEntity
{
    public string Token { get; set; }
    public DateTime CreateTime { get; set; } = DateTime.Now;
    public DateTime ExpireTime { get; set; }
}
