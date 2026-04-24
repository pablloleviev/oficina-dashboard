using AutoFlow.API.DTO;
using AutoFlow.API.Services;
using AutoFlow.API.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace AutoFlow.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ServicosController : ControllerBase
    {
        private readonly ServicoService _service;

        public ServicosController(ServicoService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var userId = int.Parse(User.FindFirst("UserId").Value);

            var lista = await _service.GetAll(userId);

            return Ok(ApiResponse<object>.SuccessResponse(lista));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ServicoDTO dto)
        {
            var userId = int.Parse(User.FindFirst("UserId").Value);

            var resultado = await _service.Create(dto, userId);

            return Ok(ApiResponse<object>.SuccessResponse(resultado));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ServicoDTO dto)
        {
            var userId = int.Parse(User.FindFirst("UserId").Value);

            var resultado = await _service.Update(id, dto, userId);

            if (resultado == null)
                return NotFound(ApiResponse<string>.ErrorResponse(
                    new List<string> { "Serviço não encontrado ou não pertence a você" }
                ));

            return Ok(ApiResponse<object>.SuccessResponse(resultado));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = int.Parse(User.FindFirst("UserId").Value);

            var sucesso = await _service.Delete(id, userId);

            if (!sucesso)
                return NotFound(ApiResponse<string>.ErrorResponse(
                    new List<string> { "Serviço não encontrado ou não pertence a você" }
                ));

            return Ok(ApiResponse<string>.SuccessResponse("Deletado com sucesso"));
        }
    }
}
