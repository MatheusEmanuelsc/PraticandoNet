using ListaTarefas.Api.Entities.Enums;

namespace ListaTarefas.Api.Dtos;

public class TarefaCreateDto
{
    
    
    
    public string Nome { get; set; } =string.Empty;
    public string? Descricao { get; set; }
    public Status Status { get; set; }
    

}