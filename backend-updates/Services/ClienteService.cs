using AutoFlow.API.Data;
using AutoFlow.API.DTO;
using AutoFlow.API.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoFlow.API.Services
{
    public class ClienteService
    {
        private readonly AppDbContext _db;

        public ClienteService(AppDbContext db)
        {
            _db = db;
        }

        // ── Listagem com contagem de ordens ─────────────────────────────────
        public async Task<List<ClienteListItemDTO>> GetAllAsync()
        {
            var clientes = await _db.Clientes
                .Include(c => c.Veiculos)
                .ToListAsync();

            var result = new List<ClienteListItemDTO>();

            foreach (var c in clientes)
            {
                // AVISO: match por string enquanto OrdemServico.ClienteId não existir.
                // Substituir por FK quando a migração for feita.
                var ordens = await _db.OrdensServico
                    .Where(o => o.Cliente == c.Nome)
                    .ToListAsync();

                var veiculo = c.Veiculos.FirstOrDefault();
                var veiculoPrincipal = veiculo != null
                    ? $"{veiculo.Nome} · {veiculo.Placa}"
                    : string.Empty;

                result.Add(new ClienteListItemDTO
                {
                    Id = c.Id,
                    Nome = c.Nome,
                    Telefone = c.Telefone,
                    DataCadastro = c.DataCadastro,
                    Ativo = c.Ativo,
                    VeiculoPrincipal = veiculoPrincipal,
                    TotalOrdens = ordens.Count,
                    GastoTotal = ordens
                        .Where(o => o.Faturado)
                        .Sum(o => o.Valor)
                });
            }

            return result;
        }

        // ── Stats para os 3 cards do topo ───────────────────────────────────
        public async Task<ClienteStatsDTO> GetStatsAsync()
        {
            return new ClienteStatsDTO
            {
                TotalClientes = await _db.Clientes.CountAsync(),
                ClientesAtivos = await _db.Clientes.CountAsync(c => c.Ativo),
                VeiculosCadastrados = await _db.Veiculos.CountAsync()
            };
        }

        // ── Detalhe (drawer lateral) ────────────────────────────────────────
        public async Task<ClienteDetalheDTO?> GetByIdAsync(int id)
        {
            var c = await _db.Clientes
                .Include(c => c.Veiculos)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (c is null) return null;

            var ordens = await _db.OrdensServico
                .Where(o => o.Cliente == c.Nome)
                .OrderByDescending(o => o.DataFaturamento)
                .ToListAsync();

            return new ClienteDetalheDTO
            {
                Id = c.Id,
                Nome = c.Nome,
                Telefone = c.Telefone,
                DataCadastro = c.DataCadastro,
                Ativo = c.Ativo,
                TotalGasto = ordens
                    .Where(o => o.Faturado)
                    .Sum(o => o.Valor),
                OrdensAbertas = ordens.Count(o =>
                    o.Status == StatusOrdemServico.Pendente ||
                    o.Status == StatusOrdemServico.EmAndamento),
                OrdensFinalizadas = ordens.Count(o =>
                    o.Status == StatusOrdemServico.Finalizado ||
                    o.Status == StatusOrdemServico.Entregue),
                UltimoServico = ordens
                    .FirstOrDefault(o => o.DataFaturamento.HasValue)
                    ?.DataFaturamento,
                Veiculos = c.Veiculos.Select(v => new VeiculoDTO
                {
                    Id = v.Id,
                    Nome = v.Nome,
                    Placa = v.Placa,
                    Ano = v.Ano
                }).ToList()
            };
        }

        // ── Criar ───────────────────────────────────────────────────────────
        public async Task<ClienteDetalheDTO> CreateAsync(CreateClienteDTO dto)
        {
            var cliente = new Cliente
            {
                Nome = dto.Nome,
                Telefone = dto.Telefone,
                DataCadastro = DateTime.UtcNow,
                Ativo = true
            };

            _db.Clientes.Add(cliente);
            await _db.SaveChangesAsync();

            return (await GetByIdAsync(cliente.Id))!;
        }

        // ── Atualizar ────────────────────────────────────────────────────────
        public async Task<ClienteDetalheDTO?> UpdateAsync(int id, CreateClienteDTO dto)
        {
            var cliente = await _db.Clientes.FindAsync(id);
            if (cliente is null) return null;

            cliente.Nome = dto.Nome;
            cliente.Telefone = dto.Telefone;

            await _db.SaveChangesAsync();
            return await GetByIdAsync(id);
        }

        // ── Desativar (soft delete) ──────────────────────────────────────────
        public async Task<bool> DeactivateAsync(int id)
        {
            var cliente = await _db.Clientes.FindAsync(id);
            if (cliente is null) return false;

            cliente.Ativo = false;
            await _db.SaveChangesAsync();
            return true;
        }

        // ── Veículos ─────────────────────────────────────────────────────────
        public async Task<List<VeiculoDTO>> GetVeiculosAsync(int clienteId)
        {
            return await _db.Veiculos
                .Where(v => v.ClienteId == clienteId)
                .Select(v => new VeiculoDTO
                {
                    Id = v.Id,
                    Nome = v.Nome,
                    Placa = v.Placa,
                    Ano = v.Ano
                })
                .ToListAsync();
        }

        public async Task<VeiculoDTO?> AddVeiculoAsync(int clienteId, CreateVeiculoDTO dto)
        {
            if (!await _db.Clientes.AnyAsync(c => c.Id == clienteId))
                return null;

            var veiculo = new Veiculo
            {
                ClienteId = clienteId,
                Nome = dto.Nome,
                Placa = dto.Placa,
                Ano = dto.Ano
            };

            _db.Veiculos.Add(veiculo);
            await _db.SaveChangesAsync();

            return new VeiculoDTO
            {
                Id = veiculo.Id,
                Nome = veiculo.Nome,
                Placa = veiculo.Placa,
                Ano = veiculo.Ano
            };
        }

        public async Task<bool> RemoveVeiculoAsync(int veiculoId)
        {
            var veiculo = await _db.Veiculos.FindAsync(veiculoId);
            if (veiculo is null) return false;

            _db.Veiculos.Remove(veiculo);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
