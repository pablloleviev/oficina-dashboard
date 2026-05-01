using AutoFlow.API.Data;
using AutoFlow.API.DTO;
using AutoFlow.API.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoFlow.API.Services
{
    public class ServicoService
    {
        private readonly AppDbContext _context;

        public ServicoService(AppDbContext context)
        {
            _context = context;
        }

        // ========================= GET =========================
        public async Task<List<Servico>> GetAll(int userId)
        {
            return await _context.Servicos
                .Where(s => s.UsuarioId == userId)
                .OrderByDescending(s => s.Id)
                .ToListAsync();
        }

        // ========================= CREATE =========================
        public async Task<Servico> Create(ServicoDTO dto, int userId)
        {
            if (dto.Valor <= 0)
                throw new Exception("Valor deve ser maior que zero");

            if (string.IsNullOrWhiteSpace(dto.Cliente))
                throw new Exception("Cliente é obrigatório");

            if (string.IsNullOrWhiteSpace(dto.NomeServico))
                throw new Exception("Nome do serviço é obrigatório");

            var servico = new Servico
            {
                Cliente = dto.Cliente.Trim(),
                Veiculo = dto.Veiculo?.Trim(),
                Placa = dto.Placa?.Trim(),
                NomeServico = dto.NomeServico.Trim(),
                Valor = dto.Valor,
                Status = string.IsNullOrWhiteSpace(dto.Status) ? "Pendente" : dto.Status,
                UsuarioId = userId
            };

            _context.Servicos.Add(servico);
            await _context.SaveChangesAsync();

            return servico;
        }

        // ========================= UPDATE =========================
        public async Task<Servico?> Update(int id, ServicoDTO dto, int userId)
        {
            var servico = await _context.Servicos
                .FirstOrDefaultAsync(s => s.Id == id && s.UsuarioId == userId);

            if (servico == null)
                return null;

            if (dto.Valor <= 0)
                throw new Exception("Valor deve ser maior que zero");

            servico.Cliente = dto.Cliente?.Trim();
            servico.Veiculo = dto.Veiculo?.Trim();
            servico.Placa = dto.Placa?.Trim();
            servico.NomeServico = dto.NomeServico?.Trim();
            servico.Valor = dto.Valor;

            if (!string.IsNullOrWhiteSpace(dto.Status))
                servico.Status = dto.Status;

            await _context.SaveChangesAsync();

            return servico;
        }

        // ========================= DELETE =========================
        public async Task<bool> Delete(int id, int userId)
        {
            var servico = await _context.Servicos
                .FirstOrDefaultAsync(s => s.Id == id && s.UsuarioId == userId);

            if (servico == null)
                return false;

            _context.Servicos.Remove(servico);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}