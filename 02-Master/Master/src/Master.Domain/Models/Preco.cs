namespace Master.Domain.Models;

public class Preco: EntidadeBasica
{
   
    public string? Nome { get; set; }
    public decimal PrecoAtual { get; set; }
    public decimal PrecoPromocao { get; set; }
    public ICollection<Curso> Cursos { get; set; } = [];
    public ICollection<CursoPreco> CursoPrecos { get; set; } = [];
}