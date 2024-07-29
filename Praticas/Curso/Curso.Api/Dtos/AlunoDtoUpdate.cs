namespace Curso.Api.Dtos
{
    public class AlunoDtoUpdate
    {
        public string Nome { get; set; } = string.Empty;
        public double Nota { get; set; }
        public int? DisciplinaId { get; set; }
    }
}
