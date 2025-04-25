namespace Master.Domain.Models;

public class CursoPreco
{
    public Guid? CursoId { get; set; }
    public Curso? Curso { get; set; }

    public Guid? PrecoId { get; set; }
    public Preco? Preco { get; set; }
}