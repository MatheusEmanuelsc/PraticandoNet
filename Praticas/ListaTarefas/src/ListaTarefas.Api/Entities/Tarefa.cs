using System.ComponentModel.DataAnnotations;
using ListaTarefas.Api.Entities.Enums;

namespace ListaTarefas.Api.Entities;

public class Tarefa
{
    public int TarefaId { get; set; }
    [Required]
    [StringLength(30)]
    public string Nome { get; set; } =string.Empty;
    [StringLength(250)]
    public string? Descricao { get; set; }
    public Status Status { get; set; }
    public DateTime DataInicio { get; set; } = DateTime.Now;
    public DateTime DataAlteracao { get; set; } = DateTime.Now;
}