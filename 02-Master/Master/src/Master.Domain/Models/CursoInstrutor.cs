namespace Master.Domain.Models;

public class CursoInstrutor
{
    public Guid? CursoId { get; set; }
    public Curso? Curso { get; set; }

    public Guid? InstrutorId { get; set; }
    public Instrutor? Instrutor { get; set; }
}