using DataContext;
using Microsoft.EntityFrameworkCore;
using Models;

namespace DataContext;

public static class DbInitializer
{
    public static void Initialize(BibliotecaContext context)
    {
        // Garante que o banco está pronto
        context.Database.EnsureCreated();

        // 1. Seed de Administrador (ADMIN)
        var admin = context.Usuarios.FirstOrDefault(u => u.Email == "admin@smartlib.com");
        if (admin == null)
        {
            admin = new Usuario
            {
                Id = Guid.NewGuid(),
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
            admin.SenhaHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");
            admin.Perfil = PerfilUsuario.ADMIN;
            admin.Ativo = true;
        }

        // 2. Seed de Bibliotecário (BIBLIOTECARIO)
        var biblio = context.Usuarios.FirstOrDefault(u => u.Email == "biblio@smartlib.com");
        if (biblio == null)
        {
            biblio = new Usuario
            {
                Id = Guid.NewGuid(),
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
            biblio.SenhaHash = BCrypt.Net.BCrypt.HashPassword("Biblio@123");
            biblio.Perfil = PerfilUsuario.BIBLIOTECARIO;
            biblio.Ativo = true;
        }

        // 3. Seed de Aluno (ALUNO - Usuário + Aluno)
        var alunoUsuario = context.Usuarios.Include(u => u.Aluno).FirstOrDefault(u => u.Email == "aluno@smartlib.com");
        if (alunoUsuario == null)
        {
            alunoUsuario = new Usuario
            {
                Id = Guid.NewGuid(),
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
                Id = Guid.NewGuid(),
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
            alunoUsuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword("Aluno@123");
            alunoUsuario.Perfil = PerfilUsuario.ALUNO;
            alunoUsuario.Ativo = true;

            var alunoEntity = context.Alunos.FirstOrDefault(a => a.UsuarioId == alunoUsuario.Id || a.Email == "aluno@smartlib.com");
            if (alunoEntity == null)
            {
                alunoEntity = new Aluno
                {
                    Id = Guid.NewGuid(),
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
                alunoEntity.UsuarioId = alunoUsuario.Id;
                alunoEntity.Email = alunoUsuario.Email;
            }
        }

        context.SaveChanges();
    }
}
