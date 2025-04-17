using ListaTarefas.Api.Entities.Enums;

namespace ListaTarefas.Api.Dtos.Tarefas;

public class TarefaPatchDto
{
    public string? Nome { get; set; } 
    public string? Descricao { get; set; }
    public Status? Status { get; set; } 
}