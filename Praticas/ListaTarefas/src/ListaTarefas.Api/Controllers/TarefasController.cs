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

        public TarefasController( IRepository repository, IUnitOfWork unitOfWork )
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
           var result =await _repository.GetALlTarefasAsync();
           return Ok(result);
        }

        [HttpGet("{id}",Name ="GetTarefaById")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _repository.GetTarefaByIdAsync(id);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Tarefa tarefa)
        {
             await _repository.AddTarefaAsync(tarefa);
             await _unitOfWork.CommitAsync();
             return CreatedAtRoute("GetTarefaById", new { id = tarefa.TarefaId }, tarefa);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Tarefa tarefa)
        {
            var tarefaEntity = await _repository.GetTarefaByIdAsync(id);
            if (tarefaEntity == null)
            {
               return BadRequest();
            }
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
