
using ListaTarefas.Api.Entities.Enums;

namespace ListaTarefas.Api.Entities;

public class Tarefa
{
  
    public int TarefaId { get; set; }
    public string Nome { get; set; } =string.Empty;
    public string? Descricao { get; set; }
    public Status Status { get; set; }
    public DateTime DataInicio { get; set; } = DateTime.UtcNow;
    public DateTime DataAlteracao { get; set; } 
}