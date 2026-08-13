using AutoFlow.API.Data;
using AutoFlow.API.DTO.Clientes;
using AutoFlow.API.DTO.Relatorios;
using AutoFlow.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace AutoFlow.API.Services
{
    public class ClienteService
    {
        private readonly AppDbContext _context;
        private readonly TenantProvider _tenant;

        public ClienteService(AppDbContext context, TenantProvider tenant)
        {
            _context = context;
            _tenant = tenant;
        }

        // ========================= GET ALL =========================
        /// <summary>
        /// Retorna todos os clientes ATIVOS do usuário, ordenados por nome.
        /// Soft delete: registros com IsActive = false nunca aparecem.
        /// </summary>
        public async Task<List<ClienteListItemDTO>> GetAll(int userId)
        {
            return await _context.Clientes
                .Where(c => c.IsActive)
                .OrderBy(c => c.Nome)
                .Select(c => new ClienteListItemDTO
                {
                    Id = c.Id,
                    Nome = c.Nome,
                    Telefone = c.Telefone,
                    Email = c.Email,
                    TotalGasto = _context.OrdemServicos
                        .Where(os => os.ClienteId == c.Id &&
                                     (os.Status == StatusOrdemServico.Entregue ||
                                      os.Status == StatusOrdemServico.Finalizado ||
                                      os.Faturado))
                        .Sum(os => (decimal?)os.Valor) ?? 0,
                    Veiculos = c.Veiculos.Where(v => v.IsActive).Select(v => new VeiculoDTO
                    {
                        Id = v.Id,
                        Marca = v.Marca ?? string.Empty,
                        Modelo = v.Modelo ?? string.Empty,
                        Placa = v.Placa ?? string.Empty,
                        Ano = v.Ano
                    }).ToList()
                })
                .ToListAsync();
        }

        // ========================= GET BY ID =========================
        /// <summary>
        /// Retorna o detalhe de um cliente ativo. Retorna null se não encontrado ou se estiver inativo.
        /// </summary>
        public async Task<ClienteDetalheDTO?> GetById(int id, int userId)
        {
            var cliente = await _context.Clientes
                .Include(c => c.Veiculos.Where(v => v.IsActive))
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

            if (cliente == null) return null;

            // Calcula o total gasto pelo cliente em OS finalizadas/entregues/faturadas
            var totalGasto = await _context.OrdemServicos
                .Where(os => os.ClienteId == id &&
                             (os.Status == StatusOrdemServico.Entregue ||
                              os.Status == StatusOrdemServico.Finalizado ||
                              os.Faturado))
                .SumAsync(os => (decimal?)os.Valor) ?? 0;

            return MapToDetalhe(cliente, totalGasto);
        }

        // ========================= CREATE =========================
        public async Task<ClienteDetalheDTO> Create(ClienteInputDTO dto, int userId)
        {
            var cliente = new Cliente
            {
                Nome = dto.Nome.Trim(),
                Telefone = dto.Telefone.Trim(),
                Email = dto.Email?.Trim(),
                Documento = dto.Documento?.Trim(),
                UsuarioId = userId,
                OficinaId = _tenant.OficinaId,
                CriadoEm = DateTime.UtcNow,
                IsActive = true,
                Veiculos = (dto.Veiculos ?? new List<VeiculoDTO>()).Select(v => new Veiculo
                {
                    Marca = v.Marca?.Trim() ?? string.Empty,
                    Modelo = v.Modelo?.Trim() ?? string.Empty,
                    Placa = v.Placa?.Trim() ?? string.Empty,
                    Ano = v.Ano,
                    IsActive = true
                }).ToList()
            };

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            return MapToDetalhe(cliente, 0); // Novo cliente, gasto inicial = 0
        }

        // ========================= UPDATE =========================
        public async Task<ClienteDetalheDTO?> Update(int id, ClienteInputDTO dto, int userId)
        {
            var cliente = await _context.Clientes
                .Include(c => c.Veiculos.Where(v => v.IsActive))
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

            if (cliente == null) return null;

            cliente.Nome = dto.Nome.Trim();
            cliente.Telefone = dto.Telefone.Trim();
            cliente.Email = dto.Email?.Trim();
            cliente.Documento = dto.Documento?.Trim();

            var vList = dto.Veiculos ?? new List<VeiculoDTO>();
            var sentIds = vList.Where(v => v.Id.HasValue).Select(v => v.Id.Value).ToList();
            
            foreach(var v in cliente.Veiculos)
            {
                if (!sentIds.Contains(v.Id)) v.IsActive = false; // Soft delete vehicle
            }
            
            foreach(var vDto in vList)
            {
                if (vDto.Id.HasValue && vDto.Id.Value > 0)
                {
                    var existing = cliente.Veiculos.FirstOrDefault(v => v.Id == vDto.Id.Value);
                    if (existing != null)
                    {
                        existing.Marca = vDto.Marca?.Trim() ?? string.Empty;
                        existing.Modelo = vDto.Modelo?.Trim() ?? string.Empty;
                        existing.Placa = vDto.Placa?.Trim() ?? string.Empty;
                        existing.Ano = vDto.Ano;
                    }
                }
                else
                {
                    cliente.Veiculos.Add(new Veiculo
                    {
                        Marca = vDto.Marca?.Trim() ?? string.Empty,
                        Modelo = vDto.Modelo?.Trim() ?? string.Empty,
                        Placa = vDto.Placa?.Trim() ?? string.Empty,
                        Ano = vDto.Ano,
                        IsActive = true
                    });
                }
            }

            await _context.SaveChangesAsync();

            // Recalcula o total gasto após atualização
            var totalGasto = await _context.OrdemServicos
                .Where(os => os.ClienteId == id &&
                             (os.Status == StatusOrdemServico.Entregue ||
                              os.Status == StatusOrdemServico.Finalizado ||
                              os.Faturado))
                .SumAsync(os => (decimal?)os.Valor) ?? 0;

            return MapToDetalhe(cliente, totalGasto);
        }

        // ========================= DELETE (SOFT) =========================
        /// <summary>
        /// Soft delete: marca o cliente como inativo (IsActive = false).
        /// O registro continua no banco mas não aparece em nenhuma consulta.
        /// </summary>
        public async Task<bool> Delete(int id, int userId)
        {
            var cliente = await _context.Clientes
                .Include(c => c.Veiculos)
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

            if (cliente == null) return false;

            cliente.IsActive = false;
            
            // 🔥 Exclusão em cascata (Soft Delete)
            foreach (var v in cliente.Veiculos)
            {
                v.IsActive = false;
            }

            await _context.SaveChangesAsync();

            return true;
        }

        // ========================= ADICIONAR VEÍCULO INDIVIDUAL =========================
        public async Task<VeiculoDTO?> AddVeiculo(int clienteId, VeiculoDTO dto, int userId)
        {
            var cliente = await _context.Clientes
                .Include(c => c.Veiculos)
                .FirstOrDefaultAsync(c => c.Id == clienteId && c.IsActive);

            if (cliente == null) return null;

            var veiculo = new Veiculo
            {
                Marca = dto.Marca?.Trim() ?? string.Empty,
                Modelo = dto.Modelo?.Trim() ?? string.Empty,
                Placa = dto.Placa?.Trim() ?? string.Empty,
                Ano = dto.Ano,
                ClienteId = clienteId,
                IsActive = true
            };

            _context.Veiculos.Add(veiculo);
            await _context.SaveChangesAsync();

            return new VeiculoDTO
            {
                Id = veiculo.Id,
                Marca = veiculo.Marca,
                Modelo = veiculo.Modelo,
                Placa = veiculo.Placa,
                Ano = veiculo.Ano
            };
        }

        // ========================= MAPPER PRIVADO =========================
        private static ClienteDetalheDTO MapToDetalhe(Cliente c, decimal totalGasto = 0) => new()
        {
            Id = c.Id,
            Nome = c.Nome,
            Telefone = c.Telefone,
            Email = c.Email,
            Documento = c.Documento,
            CriadoEm = c.CriadoEm,
            TotalGasto = totalGasto,
            Veiculos = c.Veiculos?.Where(v => v.IsActive).Select(v => new VeiculoDTO 
            {
                Id = v.Id,
                Marca = v.Marca,
                Modelo = v.Modelo,
                Placa = v.Placa,
                Ano = v.Ano
            }).ToList() ?? new List<VeiculoDTO>()
        };
    }
}
