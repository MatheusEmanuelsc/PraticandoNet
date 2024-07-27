using AutoMapper;
using Curso.Api.Dtos;
using Curso.Api.Models;
using Curso.Api.Repositorys;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Curso.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DisciplinasController : ControllerBase
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DisciplinasController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet("disciplina/lista")]
        public async Task<ActionResult<IEnumerable<DisciplinaDto>>> listaDisciplinas() {

            var listaDisciplinas = await _unitOfWork.DisciplinaRepository.GetAllAsync();

            if (listaDisciplinas is null)
            {
                return NotFound("Não existem disciplinas...");
            }

            var disciplinasDto = _mapper.Map<IEnumerable<DisciplinaDto>>(listaDisciplinas);
            return Ok(disciplinasDto);

        }

        [HttpGet("disciplina/{id}", Name = "ObterDisciplina")]
        public async Task<ActionResult<DisciplinaDto>> Disciplina(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Valor invalido");
            }

            var disciplina = await _unitOfWork.DisciplinaRepository.GetAsync(d => d.DisciplinaId == id);

            if (disciplina is null)
            {
                return NotFound("Disciplina não encontrada....");
            }

            return Ok(disciplina);
        }

        [HttpPost]
        public async Task<ActionResult<DisciplinaDto>> createDisciplina(DisciplinaDto disciplinaDto) {

            if (disciplinaDto is null)
            {
                return BadRequest("Valor invalido");
            }
            var disciplina = _mapper.Map<Disciplina>(disciplinaDto);

            var novaDisciplina = _unitOfWork.DisciplinaRepository.Create(disciplina);
            await _unitOfWork.CommitAsync();


            var novaDisciplinaDto = _mapper.Map<DisciplinaDto>(disciplina);

            return new CreatedAtRouteResult("ObterDisciplina",
                new { id = novaDisciplinaDto.DisciplinaId }, novaDisciplinaDto);
        }


        [HttpPut("{id:int}")]
        public async Task<ActionResult<DisciplinaDto>> AtualizaDisciplina(int id, DisciplinaDto disciplinaDto)
        {
            if (id != disciplinaDto.DisciplinaId)
            {
                return BadRequest("Valor invalido");
            }

            var disciplina = _mapper.Map<Disciplina>(disciplinaDto);

            var disciplinaAtualizada = _unitOfWork.DisciplinaRepository.Update(disciplina);
            await _unitOfWork.CommitAsync();

            var disciplinaAtualizadaDto = _mapper.Map<DisciplinaDto>(disciplinaAtualizada);

            return Ok(disciplinaAtualizadaDto);

        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<DisciplinaDto>>DeletarDisciplina(int id)
        {

            var disciplina = await _unitOfWork.DisciplinaRepository.GetAsync(c => c.DisciplinaId == id);
            if (disciplina is null)
            {
                return NotFound(); 
            }

           _unitOfWork.DisciplinaRepository.Delete(disciplina);
           await _unitOfWork.CommitAsync();

           var disciplinaDeletada = _mapper.Map<DisciplinaDto>(disciplina);
            return Ok(disciplinaDeletada);

        }
    }
}
