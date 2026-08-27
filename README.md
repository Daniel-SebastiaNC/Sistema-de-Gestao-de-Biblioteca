# Sistema de Gestão de Biblioteca

API REST em C# / ASP.NET Core para gerenciar autores, livros, alunos e empréstimos de uma biblioteca.

## O que o sistema faz

- Cadastro de **autores** (nome, data de nascimento, nacionalidade)
- Cadastro de **livros** (ISBN, título, ano de publicação, quantidade em estoque), vinculados a um autor
- Cadastro de **alunos** (nome, matrícula, e-mail)
- Controle de **empréstimos**: ao emprestar um livro o estoque é reduzido em 1; ao devolver, o estoque volta a aumentar em 1

## Tecnologias

- .NET 10 / ASP.NET Core Web API
- Entity Framework Core + PostgreSQL
- AutoMapper
- Swagger / OpenAPI
- Docker & Docker Compose

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

## Configuração de Ambiente (.env)

Copie o arquivo de exemplo e ajuste as variáveis se necessário:

```bash
cp .env.example .env
```

Principais variáveis configuráveis no `.env`:
- `ASPNETCORE_ENVIRONMENT`: Ambiente (`Development`, `Production`)
- `API_PORT`: Porta exposta pela API (ex: `5084`)
- `CORS_ORIGIN`: Origem permitida no CORS (ex: `http://localhost:5173`)
- `POSTGRES_HOST`, `POSTGRES_PORT`, `POSTGRES_DB`, `POSTGRES_USER`, `POSTGRES_PASSWORD`: Credenciais do banco PostgreSQL
- `ConnectionStrings__DefaultConnection`: String de conexão completa

## Como rodar o projeto

### Opção 1: Com Docker Compose
```bash
# Sobe o banco PostgreSQL e a API com as variáveis do .env
docker compose up --build
```

### Opção 2: Localmente (.NET SDK 10)
```bash
# restaurar dependências
dotnet restore

# aplicar migrations no PostgreSQL
dotnet ef database update

# rodar a aplicação
dotnet run
```

Com o ambiente em `Development`, o Swagger fica disponível para testar os endpoints (normalmente em `http://localhost:5084/swagger`).
