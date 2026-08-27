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

## Endpoints da API

Todos os endpoints de listagem aceitam parâmetros opcionais de query string para paginação:
- `pageNumber` (padrão: 1)
- `pageSize` (padrão: 10, máximo: 50)

### 1. Gestão do Acervo 📖
- `GET /api/livros?termo=&pageNumber=1&pageSize=10` — busca paginada com filtro por termo (título, autor ou ISBN)
- `GET /api/livros/{id}` — consulta detalhada do livro
- `POST /api/livros` — cadastro de novos livros
- `PUT /api/livros/{id}` — atualização cadastral do livro
- `DELETE /api/livros/{id}` — exclusão de livro do catálogo (com validação de empréstimos ativos)
- `GET /api/livros/all` — lista todos os livros sem paginação

### 2. Autores e Alunos
- `GET /api/autor?pageNumber=1&pageSize=10` — lista autores
- `GET /api/autor/{id}` — busca por id
- `POST /api/autor` — cadastra
- `PUT /api/autor/{id}` — atualiza
- `DELETE /api/autor/{id}` — remove
- `GET /api/aluno?pageNumber=1&pageSize=10` — lista alunos
- `GET /api/aluno/{id}` — busca por id
- `POST /api/aluno` — cadastra
- `DELETE /api/aluno/{id}` — remove

### 3. Empréstimos e Reservas ⏳
- `POST /api/emprestimos` — registra a saída de um livro para um aluno
- `POST /api/emprestimos/devolver` — processa a devolução com cálculo automático de atrasos e multas (R$ 2,00/dia)
- `PUT /api/emprestimos/{id}/devolucao` — devolução simplificada por ID
- `GET /api/emprestimos?pageNumber=1&pageSize=10` — lista empréstimos paginados
- `POST /api/reservas` — permite reservar um livro indisponível
- `GET /api/reservas/fila/{livroId}` — retorna a fila de espera prioritária do livro

### 4. Dashboard e Relatórios 📊
- `GET /api/dashboard` — estatísticas consolidadas (Total de Livros, Usuários Ativos, Empréstimos Ativos, Livros Atrasados, Reservas Ativas)
- `GET /api/relatorios/populares?top=10` — relatório dos livros mais emprestados
- `GET /api/relatorios/atrasados` — relatório de empréstimos pendentes e atrasados com cálculo de multas estimadas
- `GET /api/relatorios/historico?dataInicio=&dataFim=` — histórico de transações por período

### 5. Auditoria e Monitoramento 📈
- `GET /api/auditoria?pageNumber=1&pageSize=10` — log detalhado de ações críticas realizadas (Quem, O quê, Quando)
- `GET /health` — monitoramento de saúde do sistema (status da API e conexão com PostgreSQL)

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
