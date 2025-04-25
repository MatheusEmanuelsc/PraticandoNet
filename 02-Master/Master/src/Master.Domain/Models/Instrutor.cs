namespace Master.Domain.Models;

public class Instrutor : EntidadeBasica
{
    
    public string? Nome { get; set; }
    public string? Apelido { get; set; }
    public string?  GrauAcademico { get; set; }
    public ICollection<Curso> Cursos { get; set; } = [];
}