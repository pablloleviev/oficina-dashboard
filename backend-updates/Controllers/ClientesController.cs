using AutoFlow.API.DTO;
using AutoFlow.API.Responses;
using AutoFlow.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoFlow.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClientesController : ControllerBase
    {
        private readonly ClienteService _clienteService;

        public ClientesController(ClienteService clienteService)
        {
            _clienteService = clienteService;
        }

        // GET /api/clientes
        // Response: ApiResponse<List<ClienteListItemDTO>>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var clientes = await _clienteService.GetAllAsync();
            return Ok(new ApiResponse<List<ClienteListItemDTO>>(true, "Clientes carregados", clientes));
        }

        // GET /api/clientes/stats
        // Response: ApiResponse<ClienteStatsDTO>  →  { totalClientes, clientesAtivos, veiculosCadastrados }
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var stats = await _clienteService.GetStatsAsync();
            return Ok(new ApiResponse<ClienteStatsDTO>(true, "Stats carregadas", stats));
        }

        // GET /api/clientes/{id}
        // Response: ApiResponse<ClienteDetalheDTO>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var cliente = await _clienteService.GetByIdAsync(id);
            if (cliente is null)
                return NotFound(new ApiResponse<object>(false, "Cliente não encontrado", null));

            return Ok(new ApiResponse<ClienteDetalheDTO>(true, "Cliente carregado", cliente));
        }

        // POST /api/clientes
        // Request:  { nome, telefone }
        // Response: ApiResponse<ClienteDetalheDTO>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateClienteDTO dto)
        {
            var cliente = await _clienteService.CreateAsync(dto);
            return Ok(new ApiResponse<ClienteDetalheDTO>(true, "Cliente criado com sucesso", cliente));
        }

        // PUT /api/clientes/{id}
        // Request:  { nome, telefone }
        // Response: ApiResponse<ClienteDetalheDTO>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateClienteDTO dto)
        {
            var cliente = await _clienteService.UpdateAsync(id, dto);
            if (cliente is null)
                return NotFound(new ApiResponse<object>(false, "Cliente não encontrado", null));

            return Ok(new ApiResponse<ClienteDetalheDTO>(true, "Cliente atualizado", cliente));
        }

        // DELETE /api/clientes/{id}   →  soft-delete (Ativo = false)
        // Response: ApiResponse<object>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Deactivate(int id)
        {
            var ok = await _clienteService.DeactivateAsync(id);
            if (!ok)
                return NotFound(new ApiResponse<object>(false, "Cliente não encontrado", null));

            return Ok(new ApiResponse<object>(true, "Cliente desativado", null));
        }

        // GET /api/clientes/{id}/veiculos
        // Response: ApiResponse<List<VeiculoDTO>>
        [HttpGet("{id}/veiculos")]
        public async Task<IActionResult> GetVeiculos(int id)
        {
            var veiculos = await _clienteService.GetVeiculosAsync(id);
            return Ok(new ApiResponse<List<VeiculoDTO>>(true, "Veículos carregados", veiculos));
        }

        // POST /api/clientes/{id}/veiculos
        // Request:  { nome, placa, ano }
        // Response: ApiResponse<VeiculoDTO>
        [HttpPost("{id}/veiculos")]
        public async Task<IActionResult> AddVeiculo(int id, [FromBody] CreateVeiculoDTO dto)
        {
            var veiculo = await _clienteService.AddVeiculoAsync(id, dto);
            if (veiculo is null)
                return NotFound(new ApiResponse<object>(false, "Cliente não encontrado", null));

            return Ok(new ApiResponse<VeiculoDTO>(true, "Veículo adicionado", veiculo));
        }

        // DELETE /api/clientes/veiculos/{veiculoId}
        // Response: ApiResponse<object>
        [HttpDelete("veiculos/{veiculoId}")]
        public async Task<IActionResult> RemoveVeiculo(int veiculoId)
        {
            var ok = await _clienteService.RemoveVeiculoAsync(veiculoId);
            if (!ok)
                return NotFound(new ApiResponse<object>(false, "Veículo não encontrado", null));

            return Ok(new ApiResponse<object>(true, "Veículo removido", null));
        }
    }
}
