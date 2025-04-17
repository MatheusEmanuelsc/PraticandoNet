using AutoMapper;
using ListaTarefas.Api.Dtos;
using ListaTarefas.Api.Dtos.Tarefas;
using ListaTarefas.Api.Entities;
using ListaTarefas.Api.Repository;
using ListaTarefas.Api.Validators;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace ListaTarefas.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TarefasController : ControllerBase
    {
        
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TarefasController(  IUnitOfWork unitOfWork, IMapper mapper )
        {
            
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
           var result =await _unitOfWork.Tarefa.GetALlTarefasAsync();
           return Ok(_mapper.Map<IEnumerable<TarefaReadDto>>(result));
          
        }

        [HttpGet("{id}",Name ="GetTarefaById")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _unitOfWork.Tarefa.GetTarefaByIdAsync(id);
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
             await _unitOfWork.Tarefa.AddTarefaAsync(tarefa);
             await _unitOfWork.CommitAsync();var tarefaRead = _mapper.Map<TarefaReadDto>(tarefa);
             return CreatedAtRoute("GetTarefaById", new { id = tarefa.TarefaId }, tarefaRead);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] TarefaUpdateDto tarefaUpdateDto)
        {
           
            var tarefaEntity = await _unitOfWork.Tarefa.GetTarefaByIdAsync(id);
            if (tarefaEntity == null)
            {
               return BadRequest();
            }
            //
            
            _mapper.Map(tarefaUpdateDto, tarefaEntity);
            tarefaEntity.DataAlteracao = DateTime.Now;
            
            //
            _unitOfWork.Tarefa.UpdateTarefa(tarefaEntity);
            await _unitOfWork.CommitAsync();
            
            var tarefaRead = _mapper.Map<TarefaReadDto>(tarefaEntity);
            return Ok(tarefaRead);

        }

        [HttpPatch("{id}")]
        
        public async Task<IActionResult> Patch(int id, [FromBody] JsonPatchDocument<TarefaPatchDto>? patchDoc)
        {
            if (patchDoc == null)
                return BadRequest();

            var tarefaEntity = await _unitOfWork.Tarefa.GetTarefaByIdAsync(id);
            if (tarefaEntity == null)
                return NotFound();

            // Mapear entidade para DTO
            var tarefaToPatch = _mapper.Map<TarefaPatchDto>(tarefaEntity);

            // Aplicar o patch no DTO
            patchDoc.ApplyTo(tarefaToPatch, ModelState);

            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);


            // Validar com FluentValidation (opcional, mas recomendado)
            var validator = new TarefaPatchDtoValidator();
            var validationResult = validator.Validate(tarefaToPatch);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            // Mapear DTO atualizado de volta para entidade
            _mapper.Map(tarefaToPatch, tarefaEntity);

            // Atualizar no banco
            _unitOfWork.Tarefa.UpdateTarefa(tarefaEntity);
            await _unitOfWork.CommitAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var tarefaEntity = await _unitOfWork.Tarefa.GetTarefaByIdAsync(id);
            if (tarefaEntity == null)
            {
                return BadRequest();
            }
                
            _unitOfWork.Tarefa.DeleteTarefa(tarefaEntity);
            await _unitOfWork.CommitAsync();
            return Ok();
        }
    }
}
