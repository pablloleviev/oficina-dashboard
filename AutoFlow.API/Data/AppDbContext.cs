using Microsoft.EntityFrameworkCore;
using AutoFlow.API.Models;

namespace AutoFlow.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Servico> Servicos { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<OrdemServico> OrdemServicos { get; set; }

        // ========================= CLIENTES E VEICULOS =========================
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Veiculo> Veiculos { get; set; }

        // ========================= FINANCEIRO =========================
        public DbSet<Despesa> Despesas { get; set; }

        // 🔥 NOVO: LOG DE AÇÕES
        public DbSet<LogAcao> LogsAcoes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ========================= PRECISÃO MONETÁRIA =========================
            modelBuilder.Entity<Servico>()
                .Property(p => p.Valor)
                .HasPrecision(10, 2);

            modelBuilder.Entity<OrdemServico>()
                .Property(p => p.Valor)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Despesa>()
                .Property(p => p.Valor)
                .HasPrecision(10, 2);

            // ========================= ENUMS COMO STRING =========================
            modelBuilder.Entity<OrdemServico>()
                .Property(p => p.Status)
                .HasConversion<string>();

            modelBuilder.Entity<OrdemServico>()
                .Property(p => p.MeioPagamento)
                .HasConversion<string>();

            modelBuilder.Entity<Despesa>()
                .Property(p => p.Categoria)
                .HasConversion<string>();
                
            modelBuilder.Entity<Despesa>()
                .Property(p => p.Status)
                .HasConversion<string>();

            modelBuilder.Entity<Despesa>()
                .Property(p => p.MeioPagamento)
                .HasConversion<string>();

            // ========================= RELACIONAMENTOS ESPECIAIS =========================
            modelBuilder.Entity<Despesa>()
                .HasOne(d => d.OrdemServico)
                .WithMany()
                .HasForeignKey(d => d.OrdemServicoId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configuração das Chaves Estrangeiras da OS para evitar Cascade Paths
            modelBuilder.Entity<OrdemServico>()
                .HasOne(o => o.Cliente)
                .WithMany()
                .HasForeignKey(o => o.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrdemServico>()
                .HasOne(o => o.Veiculo)
                .WithMany()
                .HasForeignKey(o => o.VeiculoId)
                .OnDelete(DeleteBehavior.Restrict);

            // ========================= DATAS PADRÕES =========================
            modelBuilder.Entity<OrdemServico>()
                .Property(p => p.DataCriacao)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity<OrdemServico>()
                .Property(p => p.DataEntrada)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // ========================= LOG DE AÇÕES =========================
            modelBuilder.Entity<LogAcao>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Acao)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(x => x.Data)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Relacionamento opcional com OrdemServico
                entity.HasOne<OrdemServico>()
                    .WithMany()
                    .HasForeignKey(x => x.OrdemServicoId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ========================= CLIENTES E VEICULOS =========================
            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Nome)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(x => x.Telefone)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(x => x.Email)
                    .HasMaxLength(150);

                entity.Property(x => x.Documento)
                    .HasMaxLength(18);

                entity.Property(x => x.CriadoEm)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);
            });

            modelBuilder.Entity<Veiculo>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Marca)
                    .HasMaxLength(50);

                entity.Property(x => x.Modelo)
                    .HasMaxLength(100);

                entity.Property(x => x.Placa)
                    .HasMaxLength(20);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.HasOne(x => x.Cliente)
                    .WithMany(c => c.Veiculos)
                    .HasForeignKey(x => x.ClienteId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}