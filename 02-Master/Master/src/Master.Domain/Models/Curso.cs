namespace Master.Domain.Models;

public class Curso : EntidadeBasica
{
    
    public string? Tituto { get; set; }
    public string? Descricao { get; set; }
    public ICollection<Qualificacao> Qualificacaos { get; set; } = [];
    public ICollection<Preco> Precos { get; set; } = [];
    public ICollection<CursoPreco> CursoPrecos { get; set; } = [];
    public ICollection<Instrutor> Instrutores{ get; set; } = [];
    public ICollection<CursoInstrutor> CursoInstrutors { get; set; } = [];
    public ICollection<Foto> Fotos { get; set; } = [];

}