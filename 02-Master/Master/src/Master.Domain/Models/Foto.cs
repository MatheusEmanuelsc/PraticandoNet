namespace Master.Domain.Models;

public class Foto : EntidadeBasica
{
    public string? Url { get; set; }
    public Guid? CursoId { get; set; }
    public Curso? Curso { get; set; }
}