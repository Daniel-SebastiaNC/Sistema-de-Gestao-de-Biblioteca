# Sistema de Gestão de Biblioteca

API REST em C# / ASP.NET Core para gerenciar autores, livros, alunos e empréstimos de uma biblioteca.

## O que o sistema faz

- Cadastro de **autores** (nome, data de nascimento, nacionalidade)
- Cadastro de **livros** (ISBN, título, ano de publicação, quantidade em estoque), vinculados a um autor
- Cadastro de **alunos** (nome, matrícula, e-mail)
- Controle de **empréstimos**: ao emprestar um livro o estoque é reduzido em 1; ao devolver, o estoque volta a aumentar em 1

## Tecnologias

- .NET 10 / ASP.NET Core Web API
- Entity Framework Core + SQLite
- AutoMapper
- Swagger / OpenAPI

## Endpoints principais

### Autores — `/api/autor`
- `GET /api/autor` — lista autores
- `GET /api/autor/{id}` — busca por id
- `POST /api/autor` — cadastra
- `PUT /api/autor/{id}` — atualiza
- `DELETE /api/autor/{id}` — remove

### Livros — `/api/livros`
- `GET /api/livros?titulo=&autor=` — busca por título e/ou autor
- `GET /api/livros/{id}` — busca por id
- `POST /api/livros` — cadastra

### Alunos — `/api/aluno`
- `GET /api/aluno` — lista alunos
- `GET /api/aluno/{id}` — busca por id
- `POST /api/aluno` — cadastra
- `DELETE /api/aluno/{id}` — remove

### Empréstimos — `/api/emprestimos`
- `POST /api/emprestimos` — registra um empréstimo
- `PUT /api/emprestimos/{id}/devolucao` — registra a devolução

## Como rodar o projeto

Pré-requisito: [.NET SDK 10](https://dotnet.microsoft.com/download).

```bash
# clonar o repositório
git clone https://github.com/Daniel-SebastiaNC/Sistema-de-Gestao-de-Biblioteca.git
cd Sistema-de-Gestao-de-Biblioteca

# restaurar dependências
dotnet restore

# aplicar as migrations e criar o banco SQLite (biblioteca.db)
dotnet ef database update

# rodar a aplicação
dotnet run
```

Com o ambiente em `Development`, o Swagger fica disponível para testar os endpoints (normalmente em `https://localhost:<porta>/swagger`).
