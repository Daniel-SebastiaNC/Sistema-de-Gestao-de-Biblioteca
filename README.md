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
- Redis (Cache Distribuído de Alta Performance)
- AutoMapper
- Swagger / OpenAPI
- Docker & Docker Compose

## Endpoints da API

### 0. Autenticação e Controle de Acesso 🔐
- `POST /api/auth/login` — Autenticação unificada por e-mail e senha, emite JWT Bearer
- `GET /api/auth/me` — Consulta os dados e permissões do usuário autenticado
- `POST /api/auth/usuarios` — Cadastro de novos usuários com perfis específicos (Apenas ADMIN)

#### Perfis de Acesso (RBAC):
1. **ADMIN**: Gestão total, auditoria (`/api/auditoria`), configurações globais e criação de usuários.
2. **BIBLIOTECARIO**: Operações de acervo (Livros e Autores), empréstimos, devoluções e reservas.
3. **ALUNO**: Consultas de acervo, auto-reserva e histórico de seus empréstimos/reservas (`/api/emprestimos/meus`, `/api/reservas/minhas`).

#### Usuários Padrão (Ambiente de Desenvolvimento):
| Perfil | E-mail | Senha |
|---|---|---|
| **ADMIN** | `admin@smartlib.com` | `Admin@123` |
| **BIBLIOTECARIO** | `biblio@smartlib.com` | `Biblio@123` |
| **ALUNO** | `aluno@smartlib.com` | `Aluno@123` |

Todos os endpoints de listagem aceitam parâmetros opcionais de query string para paginação:
- `pageNumber` (padrão: 1)
- `pageSize` (padrão: 10, máximo: 50)

### 1. Gestão do Acervo 📖
- `GET /api/livros?termo=&pageNumber=1&pageSize=10` — busca paginada com filtro por termo (título, autor ou ISBN) (Cache no Redis)
- `GET /api/livros/{id}` — consulta detalhada do livro (Cache no Redis com invalidação em edições/exclusões)
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
- `GET /api/dashboard` — estatísticas consolidadas (Cache no Redis)
- `GET /api/relatorios/populares?top=10` — relatório dos livros mais emprestados (Cache no Redis)
- `GET /api/relatorios/atrasados` — relatório de empréstimos pendentes e atrasados com cálculo de multas estimadas
- `GET /api/relatorios/historico?dataInicio=&dataFim=` — histórico de transações por período

### 5. Auditoria e Monitoramento 📈
- `GET /api/auditoria?pageNumber=1&pageSize=10` — log detalhado de ações críticas realizadas (Quem, O quê, Quando)
- `GET /health` — monitoramento de saúde do sistema (status da API, conexão com PostgreSQL e Redis)

## Estrutura do Projeto

```text
Sistema-de-Gestao-de-Biblioteca/
├── .env
├── .env.example
├── .gitignore
├── docker-compose.yaml
├── README.md
├── backend/               # Código da API .NET 10, Migrations, Testes e Dockerfile
│   ├── Controllers/
│   ├── Data/
│   ├── DTOs/
│   ├── Models/
│   ├── Services/
│   ├── Test/
│   ├── Biblioteca.Api.csproj
│   ├── Dockerfile
│   └── Program.cs
└── frontend/              # Pasta reservada para a aplicação frontend
```

## Configuração de Ambiente (.env)

Copie o arquivo de exemplo e ajuste as variáveis se necessário:

```bash
cp .env.example .env
```

Principais variáveis configuráveis no `.env`:
- `ASPNETCORE_ENVIRONMENT`: Ambiente (`Development`, `Production`)
- `API_PORT`: Porta exposta pela API (ex: `5084`)
- `FRONTEND_PORT`: Porta exposta pelo Frontend (ex: `5173`)
- `CORS_ORIGIN`: Origem permitida no CORS (ex: `http://localhost:5173`)
- `POSTGRES_HOST`, `POSTGRES_PORT`, `POSTGRES_DB`, `POSTGRES_USER`, `POSTGRES_PASSWORD`: Credenciais do banco PostgreSQL
- `REDIS_HOST`, `REDIS_PORT`, `REDIS_CONNECTION_STRING`: Conexão com o servidor Redis
- `ConnectionStrings__DefaultConnection`: String de conexão completa

## Como rodar o projeto

### Opção 1: Com Docker Compose
```bash
# Sobe o banco PostgreSQL, Redis e a API com as variáveis do .env
docker compose up --build
```
> *Nota*: Ao adicionar seu projeto frontend e seu respectivo Dockerfile na pasta `frontend/`, descomente o serviço `frontend` em `docker-compose.yaml` para subir tudo junto.

### Opção 2: Localmente (.NET SDK 10)
```bash
cd backend

# restaurar dependências
dotnet restore

# aplicar migrations no PostgreSQL
dotnet ef database update

# rodar a aplicação
dotnet run
```

Com o ambiente em `Development`, o Swagger fica disponível para testar os endpoints em `http://localhost:5084/swagger`.
