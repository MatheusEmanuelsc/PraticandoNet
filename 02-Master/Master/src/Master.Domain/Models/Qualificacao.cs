namespace Master.Domain.Models;

public class Qualificacao : EntidadeBasica
{
  
    public string? Aluno { get; set; }
    public int Pontuacao { get; set; }
    public string? Comentario { get; set; }
    public Guid? CursoId { get; set; }
    public Curso? Curso { get; set; }
}