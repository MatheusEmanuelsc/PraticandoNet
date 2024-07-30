using AutoMapper;
using Curso.Api.Dtos;
using Curso.Api.Models;
using Curso.Api.Repositorys;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace Curso.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AlunosController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AlunosController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }


        [HttpGet("lista")]
        public async Task<ActionResult<IEnumerable<AlunoDto>>> GetListaAlunos()
        {
            var listaAlunos = await _unitOfWork.AlunoRepository.GetAllAsync();
            if (listaAlunos == null) { NotFound(); }
            var listaAlunosDto = _mapper.Map<IEnumerable<AlunoDto>>(listaAlunos);
            return Ok(listaAlunosDto);

        }

        [HttpGet("{id:int}",Name ="ObterAluno")]
        public async Task<ActionResult<AlunoDto>> GetAluno(int id)
        {
            var aluno = await _unitOfWork.AlunoRepository.GetAsync(a => a.AlunoId == id);
            if (aluno == null) { NotFound(); }
            var alunoDto=_mapper.Map<AlunoDto>(aluno);
            return Ok(alunoDto);

        }

        [HttpGet("disciplina/{id:int}")]
        public async Task<ActionResult<IEnumerable<AlunoDto>>>GetAlunoDisciplina(int id)
        {
            var aluno = await _unitOfWork.AlunoRepository.GetAlunoPorDisciplinaAsync(id);
            if (aluno == null) {return NotFound(); }

            var alunoDto = _mapper.Map<IEnumerable<AlunoDto>>(aluno);
            return Ok(alunoDto);
        }

        [HttpPost]
        public async Task<ActionResult<AlunoDto>> CriarAluno(AlunoDto alunoDto)
        {
            if (alunoDto == null) { return BadRequest(); }
            var aluno= _mapper.Map<Aluno>(alunoDto);
            _unitOfWork.AlunoRepository.Create(aluno);
            await _unitOfWork.CommitAsync();

            var novoAlunoDto = _mapper.Map<AlunoDto>(aluno);

            return new CreatedAtRouteResult("ObterAluno",
                new { id = novoAlunoDto.AlunoId }, novoAlunoDto);

        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<AlunoDto>>AtualizaAluno(int id, AlunoDto alunoDto)
        {
            if (id != alunoDto.AlunoId) { return BadRequest(); }
            var alunoAtualizado =_mapper.Map<Aluno>(alunoDto);
            _unitOfWork.AlunoRepository.Update(alunoAtualizado);
            await _unitOfWork.CommitAsync();
            return Ok(alunoAtualizado);

        }

        [HttpPatch("{id:int}/UpdateParcial")]
        public async Task<ActionResult<AlunoDto>>AtualizaAlunoParcial(int id, JsonPatchDocument<AlunoDtoUpdate> alunoDtoUpdate)
        {
            if (id <= 0 || alunoDtoUpdate is null)
            {
                return BadRequest();
            }

            var aluno = await _unitOfWork.AlunoRepository.GetAsync(a => a.AlunoId == id);

            if (aluno is null)
            {
                return NotFound();
            }

            var AlunoUpdateRequest = _mapper.Map<AlunoDtoUpdate>(aluno);
            alunoDtoUpdate.ApplyTo(AlunoUpdateRequest,ModelState);

            if (!ModelState.IsValid || !TryValidateModel(AlunoUpdateRequest))
            {
                return BadRequest();
            }

            _mapper.Map(AlunoUpdateRequest, aluno);
            await _unitOfWork.CommitAsync();

            return Ok(_mapper.Map<AlunoDto>(aluno));
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<AlunoDto>>DeletarAluno(int id)
        {
           var aluno = await _unitOfWork.AlunoRepository.GetAsync(a=> a.AlunoId == id);
           if (aluno is null) { return BadRequest(); }
           _unitOfWork.AlunoRepository.Delete(aluno);
           await _unitOfWork.CommitAsync();
           var alunoDeletado = _mapper.Map<AlunoDto>(aluno);

           return Ok(alunoDeletado);


        }
    }
}
