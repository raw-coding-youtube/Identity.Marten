namespace Identity.Marten;

public class UserRole<TKey>
{
    public int Id { get; set; }
    public TKey UserId { get; set; }
    public string RoleId { get; set; }
}