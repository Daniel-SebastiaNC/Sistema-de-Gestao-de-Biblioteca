using Microsoft.EntityFrameworkCore;
using Models;

namespace DataContext;

public class BibliotecaContext : DbContext
{
    public BibliotecaContext(DbContextOptions<BibliotecaContext> options) : base(options)
    {
    }

    public DbSet<Aluno> Alunos { get; set; }
    public DbSet<Autor> Autores { get; set; }
    public DbSet<Livro> Livros { get; set; }
    public DbSet<Emprestimo> Emprestimos { get; set; }
    public DbSet<Reserva> Reservas { get; set; }
    public DbSet<Auditoria> Auditorias { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Perfil).HasConversion<string>();
        });

        modelBuilder.Entity<Aluno>(entity =>
        {
            entity.HasOne(a => a.Usuario)
                  .WithOne(u => u.Aluno)
                  .HasForeignKey<Aluno>(a => a.UsuarioId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }
}