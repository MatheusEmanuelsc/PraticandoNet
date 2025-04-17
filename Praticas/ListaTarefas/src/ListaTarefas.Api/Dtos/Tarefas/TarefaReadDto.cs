using ListaTarefas.Api.Entities.Enums;

namespace ListaTarefas.Api.Dtos.Tarefas;

public class TarefaReadDto
{

    public int TarefaId { get; set; }
    public string Nome { get; set; } =string.Empty;
    public string? Descricao { get; set; }
    public Status Status { get; set; }
    public DateTime DataInicio { get; set; } 
    public DateTime DataAlteracao { get; set; } 

}