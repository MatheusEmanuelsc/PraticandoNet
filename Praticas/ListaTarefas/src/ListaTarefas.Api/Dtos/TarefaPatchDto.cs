using ListaTarefas.Api.Entities.Enums;

namespace ListaTarefas.Api.Dtos;

public class TarefaPatchDto
{
    public string? Nome { get; set; } 
    public string? Descricao { get; set; }
    public Status? Status { get; set; } 
}