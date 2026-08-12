using AutoFlow.API.DTO.Auth;
using AutoFlow.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AutoFlow.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _service;

        public AuthController(AuthService service)
        {
            _service = service;
        }

        // ========================= REGISTER =========================
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO dto)
        {
            try
            {
                var success = await _service.Register(dto);

                if (!success)
                    return BadRequest(new { error = "Usuário já existe" });

                return Ok(new { message = "Usuário registrado com sucesso" });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "Erro interno do servidor" });
            }
        }

        // ========================= LOGIN =========================
        [HttpPost("login")]
        [EnableRateLimiting("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            try
            {
                var token = await _service.Login(dto);

                if (token == null)
                    return Unauthorized(new { error = "Email ou senha inválidos" });

                return Ok(new { token });
            }
            catch (Exception)
            {
                // Não expor detalhe técnico ao cliente (A10/A02). Detalhe fica no handler global/log.
                return StatusCode(500, new { error = "Erro interno do servidor" });
            }
        }
    }
}
