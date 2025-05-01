namespace Artigos.Entidades;

public class Article
{
    public int Id { get; set; }
    public string Title { get; set; } = String.Empty;
    public string Content { get; set; }  = String.Empty;
    public bool IsPublic { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public required string AuthorId { get; set; }
    public ApplicationUser Author { get; set; }
}