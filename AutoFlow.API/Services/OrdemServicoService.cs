using AutoFlow.API.Data;
using AutoFlow.API.DTO;
using AutoFlow.API.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoFlow.API.Services
{
    public class OrdemServicoService
    {
        private readonly AppDbContext _context;

        public OrdemServicoService(AppDbContext context)
        {
            _context = context;
        }

        // ========================= LOG =========================
        private void RegistrarLog(int ordemId, int userId, string acao)
        {
            var log = new LogAcao
            {
                OrdemServicoId = ordemId,
                UsuarioId = userId,
                Acao = acao,
                Data = DateTime.UtcNow
            };

            _context.LogsAcoes.Add(log);
        }

        // ========================= GET =========================
        public async Task<List<OrdemServicoResponseDTO>> GetAll(int userId, string? status = null)
        {
            var query = _context.OrdemServicos
                .Include(x => x.Cliente)
                .Include(x => x.Veiculo)
                .Where(x => x.UsuarioId == userId)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(status) &&
                Enum.TryParse<StatusOrdemServico>(status, true, out var statusEnum))
            {
                query = query.Where(x => x.Status == statusEnum);
            }

            var ordens = await query
                .OrderByDescending(x => x.Id)
                .ToListAsync();

            return ordens.Select(MapToResponse).ToList();
        }

        // ========================= CREATE =========================
        public async Task<OrdemServicoResponseDTO> Create(OrdemServicoDTO dto, int userId)
        {
            if (dto.Valor <= 0)
                throw new Exception("Valor deve ser maior que zero");

            if (dto.ClienteId == null || dto.ClienteId <= 0)
                throw new Exception("Cliente é obrigatório");

            if (string.IsNullOrWhiteSpace(dto.Servico))
                throw new Exception("Serviço é obrigatório");

            // Validar existência no banco
            var clienteExiste = await _context.Clientes.AnyAsync(c => c.Id == dto.ClienteId && c.UsuarioId == userId && c.IsActive);
            if (!clienteExiste)
                throw new Exception("Cliente inválido ou inativo");

            if (dto.VeiculoId != null && dto.VeiculoId > 0)
            {
                var veiculoExiste = await _context.Veiculos.AnyAsync(v => v.Id == dto.VeiculoId && v.ClienteId == dto.ClienteId && v.IsActive);
                if (!veiculoExiste)
                    throw new Exception("Veículo não pertence ao cliente ou é inválido");
            }
            else
            {
                throw new Exception("Veículo é obrigatório para esta ordem");
            }

            if (!Enum.TryParse<StatusOrdemServico>(dto.Status, true, out var status))
                status = StatusOrdemServico.Pendente;

            if (!Enum.TryParse<MeioPagamento>(dto.MeioPagamento, true, out var meioPagamento))
                meioPagamento = MeioPagamento.Desconhecido;

            var os = new OrdemServico
            {
                ClienteId = dto.ClienteId.Value,
                VeiculoId = dto.VeiculoId.Value,
                Servico = dto.Servico.Trim(),
                Valor = dto.Valor,
                Status = status,
                MeioPagamento = meioPagamento,
                UsuarioId = userId,
                DataCriacao = DateTime.UtcNow,
                DataEntrada = DateTime.UtcNow
            };

            _context.OrdemServicos.Add(os);
            await _context.SaveChangesAsync();
            
            await _context.Entry(os).Reference(x => x.Cliente).LoadAsync();
            await _context.Entry(os).Reference(x => x.Veiculo).LoadAsync();

            return MapToResponse(os);
        }

        // ========================= UPDATE (CORRIGIDO E ESTÁVEL) =========================
        public async Task<OrdemServicoResponseDTO?> Update(int id, OrdemServicoDTO dto, int userId)
        {
            var os = await _context.OrdemServicos
                .Include(x => x.Cliente)
                .Include(x => x.Veiculo)
                .FirstOrDefaultAsync(x => x.Id == id && x.UsuarioId == userId);

            if (os == null) return null;

            // 🔥 BLOQUEIO CRÍTICO
            if (os.Faturado)
                throw new Exception("Ordem faturada não pode ser alterada");

            if (dto.Valor <= 0)
                throw new Exception("Valor deve ser maior que zero");

            if (dto.ClienteId != null && dto.ClienteId != os.ClienteId)
            {
                var clienteExiste = await _context.Clientes.AnyAsync(c => c.Id == dto.ClienteId && c.UsuarioId == userId && c.IsActive);
                if (!clienteExiste) throw new Exception("Cliente inválido ou inativo");
            }

            if (dto.VeiculoId != null && dto.VeiculoId != os.VeiculoId && dto.VeiculoId > 0)
            {
                var veiculoExiste = await _context.Veiculos.AnyAsync(v => v.Id == dto.VeiculoId && v.ClienteId == dto.ClienteId && v.IsActive);
                if (!veiculoExiste) throw new Exception("Veículo inválido");
            }

            if (!Enum.TryParse<StatusOrdemServico>(dto.Status, true, out var status))
                status = os.Status;

            if (!Enum.TryParse<MeioPagamento>(dto.MeioPagamento, true, out var meioPagamento))
                meioPagamento = os.MeioPagamento ?? MeioPagamento.Desconhecido;

            // 🔥 CONTROLE SIMPLES (SEM QUEBRAR SISTEMA)
            if (os.Status == StatusOrdemServico.Entregue &&
                status != StatusOrdemServico.Entregue &&
                status != StatusOrdemServico.Finalizado)
            {
                throw new Exception("Só é permitido voltar para Finalizado após entrega");
            }

            var statusAnterior = os.Status;

            os.ClienteId = dto.ClienteId ?? os.ClienteId;
            os.VeiculoId = dto.VeiculoId ?? os.VeiculoId;
            os.Servico = dto.Servico?.Trim() ?? os.Servico;
            os.Valor = dto.Valor;
            os.Status = status;
            os.MeioPagamento = meioPagamento;

            if (status == StatusOrdemServico.Finalizado && os.DataConclusao == null)
                os.DataConclusao = DateTime.UtcNow;

            // 🔥 LOG DE MUDANÇA DE STATUS
            if (statusAnterior != status)
            {
                RegistrarLog(os.Id, userId, $"STATUS: {statusAnterior} → {status}");
            }

            await _context.SaveChangesAsync();

            await _context.Entry(os).Reference(x => x.Cliente).LoadAsync();
            await _context.Entry(os).Reference(x => x.Veiculo).LoadAsync();

            return MapToResponse(os);
        }

        // ========================= UPDATE STATUS (ESPECÍFICO) =========================
        public async Task<OrdemServicoResponseDTO?> UpdateStatus(int id, string novoStatus, int userId)
        {
            var os = await _context.OrdemServicos
                .Include(x => x.Cliente)
                .Include(x => x.Veiculo)
                .FirstOrDefaultAsync(x => x.Id == id && x.UsuarioId == userId);

            if (os == null) return null;

            if (os.Faturado)
                throw new Exception("Ordem faturada não pode ter o status alterado");

            if (!Enum.TryParse<StatusOrdemServico>(novoStatus, true, out var status))
                throw new Exception("Status inexistente ou inválido");

            var statusAnterior = os.Status;
            
            // 🔥 Impedir retrocessos inválidos (Ex: Entregue -> Pendente)
            if (statusAnterior == StatusOrdemServico.Entregue && status != StatusOrdemServico.Entregue)
                throw new Exception("Ordem entregue não pode voltar de status (apenas desfaturar se necessário)");

            if (statusAnterior == status) return MapToResponse(os);

            os.Status = status;

            if (status == StatusOrdemServico.Finalizado && os.DataConclusao == null)
                os.DataConclusao = DateTime.UtcNow;

            RegistrarLog(os.Id, userId, $"STATUS ALTERADO: {statusAnterior} → {status}");

            await _context.SaveChangesAsync();
            return MapToResponse(os);
        }

        // ========================= DELETE =========================
        public async Task<bool> Delete(int id, int userId)
        {
            var os = await _context.OrdemServicos
                .FirstOrDefaultAsync(x => x.Id == id && x.UsuarioId == userId);

            if (os == null) return false;

            _context.OrdemServicos.Remove(os);
            await _context.SaveChangesAsync();

            return true;
        }

        // ========================= FATURAR =========================
        public async Task<OrdemServicoResponseDTO?> Faturar(int id, int userId, string role)
        {
            var os = await _context.OrdemServicos
                .Include(x => x.Cliente)
                .Include(x => x.Veiculo)
                .FirstOrDefaultAsync(x => x.Id == id && x.UsuarioId == userId);

            if (os == null) return null;

            if (os.Status != StatusOrdemServico.Entregue)
                throw new Exception("Só é possível faturar ordens entregues");

            if (os.Faturado)
                throw new Exception("Ordem já faturada");

            os.Faturado = true;
            os.DataFaturamento = DateTime.UtcNow;
            os.FaturadoPorUserId = userId;

            RegistrarLog(os.Id, userId, "FATURADO");

            await _context.SaveChangesAsync();

            return MapToResponse(os);
        }

        // ========================= DESFATURAR =========================
        public async Task<OrdemServicoResponseDTO?> Desfaturar(int id, int userId, string role, string senha)
        {
            if (role != "Admin")
                throw new Exception("Apenas administradores podem desfaturar");

            var usuario = await _context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (usuario == null)
                throw new Exception("Usuário inválido");

            if (!BCrypt.Net.BCrypt.Verify(senha, usuario.Senha))
                throw new Exception("Senha inválida");

            var ordem = await _context.OrdemServicos
                .Include(x => x.Cliente)
                .Include(x => x.Veiculo)
                .FirstOrDefaultAsync(x => x.Id == id && x.UsuarioId == userId);

            if (ordem == null)
                return null;

            if (!ordem.Faturado)
                throw new Exception("Ordem não está faturada");

            ordem.Faturado = false;
            ordem.DataFaturamento = null;
            ordem.DesfaturadoPorUserId = userId;

            RegistrarLog(ordem.Id, userId, "DESFATURADO");

            await _context.SaveChangesAsync();

            return MapToResponse(ordem);
        }

        // ========================= LOGS =========================
        public async Task<List<LogAcao>> GetLogsByOrdem(int ordemId, int userId)
        {
            return await _context.LogsAcoes
                .Where(x => x.OrdemServicoId == ordemId)
                .OrderByDescending(x => x.Data)
                .ToListAsync();
        }

        // ========================= MAPPER =========================
        private static OrdemServicoResponseDTO MapToResponse(OrdemServico os)
        {
            return new OrdemServicoResponseDTO
            {
                Id = os.Id,
                ClienteId = os.ClienteId,
                Cliente = os.Cliente?.Nome ?? "Desconhecido",
                Telefone = os.Cliente?.Telefone ?? "",
                VeiculoId = os.VeiculoId,
                Veiculo = os.Veiculo != null ? $"{os.Veiculo.Marca} {os.Veiculo.Modelo}" : "",
                Placa = os.Veiculo?.Placa ?? "",
                Servico = os.Servico,
                Valor = os.Valor,
                Status = os.Status.ToString(),
                MeioPagamento = os.MeioPagamento?.ToString() ?? "",
                DataCriacao = os.DataCriacao,
                DataEntrada = os.DataEntrada,
                DataConclusao = os.DataConclusao,
                Faturado = os.Faturado,
                DataFaturamento = os.DataFaturamento
            };
        }
    }
}
