using AutoFlow.API.DTO.Auth;
using AutoFlow.API.Services;
using Microsoft.AspNetCore.Mvc;

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
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ========================= LOGIN =========================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            try
            {
                var token = await _service.Login(dto);

                if (token == null)
                    return Unauthorized(new { error = "Email ou senha inválidos" });

                return Ok(new { token });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
