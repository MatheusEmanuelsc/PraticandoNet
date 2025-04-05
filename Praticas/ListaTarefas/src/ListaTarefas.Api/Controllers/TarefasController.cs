using AutoMapper;
using ListaTarefas.Api.Dtos;
using ListaTarefas.Api.Entities;
using ListaTarefas.Api.Repository;

using Microsoft.AspNetCore.Mvc;

namespace ListaTarefas.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TarefasController : ControllerBase
    {
        private readonly IRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TarefasController( IRepository repository, IUnitOfWork unitOfWork, IMapper mapper )
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
           var result =await _repository.GetALlTarefasAsync();
           return Ok(_mapper.Map<IEnumerable<TarefaReadDto>>(result));
          
        }

        [HttpGet("{id}",Name ="GetTarefaById")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _repository.GetTarefaByIdAsync(id);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(_mapper.Map<TarefaReadDto>(result));
            
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] TarefaCreateDto tarefaCreateDto)
        {
             var tarefa = _mapper.Map<Tarefa>(tarefaCreateDto);
             tarefa.DataAlteracao = DateTime.Now;
             await _repository.AddTarefaAsync(tarefa);
             await _unitOfWork.CommitAsync();var tarefaRead = _mapper.Map<TarefaReadDto>(tarefa);
             return CreatedAtRoute("GetTarefaById", new { id = tarefa.TarefaId }, tarefaRead);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] TarefaUpdateDto tarefaUpdateDto)
        {
           
            var tarefaEntity = await _repository.GetTarefaByIdAsync(id);
            if (tarefaEntity == null)
            {
               return BadRequest();
            }
            
            var tarefa = _mapper.Map<Tarefa>(tarefaUpdateDto);
            tarefa.DataAlteracao = DateTime.Now;
            _repository.UpdateTarefa(tarefa);
            await _unitOfWork.CommitAsync();
            return Ok(tarefa);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var tarefaEntity = await _repository.GetTarefaByIdAsync(id);
            if (tarefaEntity == null)
            {
                return BadRequest();
            }
                
            _repository.DeleteTarefa(tarefaEntity);
            await _unitOfWork.CommitAsync();
            return Ok();
        }
    }
}
