using DataContext;
using Microsoft.EntityFrameworkCore;
using Models;

namespace DataContext;

public static class DbInitializer
{
    public static readonly Guid AdminId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid BiblioId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid AlunoId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    public static void Initialize(BibliotecaContext context)
    {
        // 1. Limpeza preventiva de possíveis livros com ISBNs duplicados (dados legados de testes)
        var livrosDuplicados = context.Livros.ToList()
            .GroupBy(l => l.ISBN?.Trim().ToLower())
            .Where(g => !string.IsNullOrEmpty(g.Key) && g.Count() > 1);

        foreach (var grupo in livrosDuplicados)
        {
            var livros = grupo.OrderBy(l => l.Id).ToList();
            for (int i = 1; i < livros.Count; i++)
            {
                livros[i].ISBN = $"{livros[i].ISBN}-DUP{i}";
            }
            context.SaveChanges();
        }

        // 2. Seed de Administrador (ADMIN) com ID determinístico
        var admin = context.Usuarios.FirstOrDefault(u => u.Email == "admin@smartlib.com" || u.Id == AdminId);
        if (admin == null)
        {
            admin = new Usuario
            {
                Id = AdminId,
                Nome = "Administrador do Sistema",
                Email = "admin@smartlib.com",
                SenhaHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Perfil = PerfilUsuario.ADMIN,
                Ativo = true,
                DataCriacao = DateTime.UtcNow
            };
            context.Usuarios.Add(admin);
        }
        else
        {
            admin.Nome = "Administrador do Sistema";
            admin.Email = "admin@smartlib.com";
            admin.SenhaHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");
            admin.Perfil = PerfilUsuario.ADMIN;
            admin.Ativo = true;
        }

        // 3. Seed de Bibliotecário (BIBLIOTECARIO) com ID determinístico
        var biblio = context.Usuarios.FirstOrDefault(u => u.Email == "biblio@smartlib.com" || u.Id == BiblioId);
        if (biblio == null)
        {
            biblio = new Usuario
            {
                Id = BiblioId,
                Nome = "Bibliotecário Chefe",
                Email = "biblio@smartlib.com",
                SenhaHash = BCrypt.Net.BCrypt.HashPassword("Biblio@123"),
                Perfil = PerfilUsuario.BIBLIOTECARIO,
                Ativo = true,
                DataCriacao = DateTime.UtcNow
            };
            context.Usuarios.Add(biblio);
        }
        else
        {
            biblio.Nome = "Bibliotecário Chefe";
            biblio.Email = "biblio@smartlib.com";
            biblio.SenhaHash = BCrypt.Net.BCrypt.HashPassword("Biblio@123");
            biblio.Perfil = PerfilUsuario.BIBLIOTECARIO;
            biblio.Ativo = true;
        }

        // 4. Seed de Aluno (ALUNO - Usuário e Aluno com o MESMO ID determinístico)
        var alunoUsuario = context.Usuarios.Include(u => u.Aluno).FirstOrDefault(u => u.Email == "aluno@smartlib.com" || u.Id == AlunoId);
        if (alunoUsuario == null)
        {
            alunoUsuario = new Usuario
            {
                Id = AlunoId,
                Nome = "Aluno Demonstração",
                Email = "aluno@smartlib.com",
                SenhaHash = BCrypt.Net.BCrypt.HashPassword("Aluno@123"),
                Perfil = PerfilUsuario.ALUNO,
                Ativo = true,
                DataCriacao = DateTime.UtcNow
            };
            context.Usuarios.Add(alunoUsuario);

            var alunoEntity = new Aluno
            {
                Id = AlunoId,
                Nome = alunoUsuario.Nome,
                Matricula = "20260001",
                Email = alunoUsuario.Email,
                UsuarioId = AlunoId,
                Usuario = alunoUsuario
            };

            context.Alunos.Add(alunoEntity);
        }
        else
        {
            alunoUsuario.Nome = "Aluno Demonstração";
            alunoUsuario.Email = "aluno@smartlib.com";
            alunoUsuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword("Aluno@123");
            alunoUsuario.Perfil = PerfilUsuario.ALUNO;
            alunoUsuario.Ativo = true;

            var alunoEntity = context.Alunos.FirstOrDefault(a => a.UsuarioId == alunoUsuario.Id || a.Id == AlunoId || a.Email == "aluno@smartlib.com");
            if (alunoEntity == null)
            {
                alunoEntity = new Aluno
                {
                    Id = alunoUsuario.Id,
                    Nome = alunoUsuario.Nome,
                    Matricula = "20260001",
                    Email = alunoUsuario.Email,
                    UsuarioId = alunoUsuario.Id,
                    Usuario = alunoUsuario
                };
                context.Alunos.Add(alunoEntity);
            }
            else
            {
                alunoEntity.Nome = alunoUsuario.Nome;
                alunoEntity.Matricula = "20260001";
                alunoEntity.Email = alunoUsuario.Email;
                alunoEntity.UsuarioId = alunoUsuario.Id;
            }
        }

        context.SaveChanges();
    }
}
