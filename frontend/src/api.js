// ========================================
// API Service - Centralized API calls
// ========================================

const API_BASE = '/api';

async function request(endpoint, options = {}) {
  const url = endpoint.startsWith('http') ? endpoint : `${API_BASE}${endpoint}`;

  const config = {
    headers: {
      'Content-Type': 'application/json',
      ...options.headers,
    },
    ...options,
  };

  try {
    const response = await fetch(url, config);

    if (!response.ok) {
      const statusMessages = {
        400: 'Dados inválidos',
        401: 'Não autorizado',
        403: 'Sem permissão',
        404: 'Registro não encontrado',
        409: 'Conflito — verifique se o registro já existe ou se há dependências',
        500: 'Erro interno no servidor',
      };
      let errorMessage = statusMessages[response.status] || `Erro ${response.status}`;
      try {
        const text = await response.text();
        if (text) {
          try {
            const errorData = JSON.parse(text);
            // Handle C# ProblemDetails, ValidationProblemDetails, and custom error formats
            errorMessage = errorData.detail
              || errorData.message
              || errorData.title
              || (errorData.errors ? Object.values(errorData.errors).flat().join('. ') : null)
              || errorMessage;
          } catch {
            // Response is plain text
            if (text.length < 300) {
              errorMessage = text;
            }
          }
        }
      } catch {
        // ignore parse error
      }
      throw new Error(errorMessage);
    }

    const contentType = response.headers.get('content-type');
    if (contentType && contentType.includes('application/json')) {
      return await response.json();
    }
    return null;
  } catch (error) {
    if (error.name === 'TypeError' && error.message === 'Failed to fetch') {
      throw new Error('Não foi possível conectar ao servidor. Verifique se a API está rodando.');
    }
    throw error;
  }
}

// --- Dashboard ---
export function getDashboard() {
  return request('/Dashboard');
}

// --- Livros ---
export function getLivros({ pageNumber = 1, pageSize = 10, termo, titulo, autor } = {}) {
  const params = new URLSearchParams();
  params.set('PageNumber', pageNumber);
  params.set('PageSize', pageSize);
  if (termo) params.set('termo', termo);
  if (titulo) params.set('titulo', titulo);
  if (autor) params.set('autor', autor);
  return request(`/Livros?${params.toString()}`);
}

export function getLivroById(id) {
  return request(`/Livros/${id}`);
}

export function getAllLivros() {
  return request('/Livros/all');
}

export function criarLivro(data) {
  return request('/Livros', {
    method: 'POST',
    body: JSON.stringify(data),
  });
}

export function atualizarLivro(id, data) {
  return request(`/Livros/${id}`, {
    method: 'PUT',
    body: JSON.stringify(data),
  });
}

export function deletarLivro(id) {
  return request(`/Livros/${id}`, {
    method: 'DELETE',
  });
}

// --- Empréstimos ---
export function getEmprestimos({ pageNumber = 1, pageSize = 10 } = {}) {
  const params = new URLSearchParams();
  params.set('PageNumber', pageNumber);
  params.set('PageSize', pageSize);
  return request(`/Emprestimos?${params.toString()}`);
}

export function getAllEmprestimos() {
  return request('/Emprestimos/all');
}

export function criarEmprestimo(data) {
  return request('/Emprestimos', {
    method: 'POST',
    body: JSON.stringify(data),
  });
}

export function devolverEmprestimo(emprestimoId) {
  return request('/Emprestimos/devolver', {
    method: 'POST',
    body: JSON.stringify({ emprestimoId }),
  });
}

// --- Alunos ---
export function getAlunos({ pageNumber = 1, pageSize = 100 } = {}) {
  const params = new URLSearchParams();
  params.set('PageNumber', pageNumber);
  params.set('PageSize', pageSize);
  return request(`/Aluno?${params.toString()}`);
}

export function getAlunoById(id) {
  return request(`/Aluno/${id}`);
}

// --- Autores ---
export function getAutores({ pageNumber = 1, pageSize = 100 } = {}) {
  const params = new URLSearchParams();
  params.set('PageNumber', pageNumber);
  params.set('PageSize', pageSize);
  return request(`/Autor?${params.toString()}`);
}

// --- Relatórios ---
export function getLivrosPopulares(top = 10) {
  return request(`/Relatorios/populares?top=${top}`);
}

export function getEmprestimosAtrasados() {
  return request('/Relatorios/atrasados');
}

export function getHistorico(dataInicio, dataFim) {
  const params = new URLSearchParams();
  if (dataInicio) params.set('dataInicio', dataInicio);
  if (dataFim) params.set('dataFim', dataFim);
  return request(`/Relatorios/historico?${params.toString()}`);
}

// --- Reservas ---
export function criarReserva(data) {
  return request('/Reservas', {
    method: 'POST',
    body: JSON.stringify(data),
  });
}

export function getFilaReserva(livroId) {
  return request(`/Reservas/fila/${livroId}`);
}
