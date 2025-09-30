using Microsoft.EntityFrameworkCore;
using ChallengeYardFlow.Modelo;

namespace ChallengeYardFlow.Data
{
    public class LocadoraContext : DbContext
    {
        public LocadoraContext(DbContextOptions<LocadoraContext> opts)
            : base(opts) { }

        // DbSets
        public DbSet<Moto> Motos { get; set; } = null!;
        public DbSet<Locacao> Locacoes { get; set; } = null!;
        public DbSet<Usuario> Usuarios { get; set; } = null!; 

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
            var moto = modelBuilder.Entity<Moto>();
            moto.ToTable("Motos");
            moto.HasKey(v => v.Id);
            moto.Property(v => v.ValorDiaria)
                 .HasPrecision(18, 2);

            
            var locacao = modelBuilder.Entity<Locacao>();
            locacao.ToTable("Locacoes");
            locacao.HasKey(l => l.Id);
            locacao.HasOne(l => l.Moto)
                   .WithMany()
                   .HasForeignKey(l => l.MotoId)
                   .OnDelete(DeleteBehavior.Restrict);

            locacao.Property(l => l.ValorFinal)
                   .HasPrecision(18, 2);

            
            var usuario = modelBuilder.Entity<Usuario>();
            usuario.ToTable("Usuarios");
            usuario.HasKey(u => u.Id);
            usuario.Property(u => u.Nome)
                   .IsRequired()
                   .HasMaxLength(100);
            usuario.Property(u => u.Email)
                   .IsRequired()
                   .HasMaxLength(100);
            usuario.Property(u => u.Senha)
                   .IsRequired()
                   .HasMaxLength(100);
            usuario.Property(u => u.Funcao)
                   .IsRequired()
                   .HasMaxLength(50);
        }
    }
}
