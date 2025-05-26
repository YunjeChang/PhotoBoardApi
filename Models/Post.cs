namespace PhotoBoardApi.Models;

public class Post
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string ImageUrl { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public ICollection<Comment>? Comments { get; set; }
}