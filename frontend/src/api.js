// ========================================
// API Service - Centralized API calls
// ========================================

import { getToken, logout } from './auth.js';
import { showToast } from './components/toast.js';

const API_BASE = '/api';

async function request(endpoint, options = {}) {
  const url = endpoint.startsWith('http') ? endpoint : `${API_BASE}${endpoint}`;

  const token = getToken();
  const authHeaders = token ? { 'Authorization': `Bearer ${token}` } : {};

  const config = {
    headers: {
      'Content-Type': 'application/json',
      ...authHeaders,
      ...options.headers,
    },
    ...options,
  };

  try {
    const response = await fetch(url, config);

    if (!response.ok) {
      if (response.status === 401) {
        showToast('Sessão expirada. Faça login novamente.', 'warning');
        logout();
        throw new Error('Sessão expirada ou não autorizada.');
      }

      if (response.status === 403) {
        showToast('Acesso negado: você não tem permissão para realizar esta ação.', 'error');
        throw new Error('Acesso negado: perfil sem permissão para esta operação.');
      }

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

export function criarAluno(data) {
  return request('/Aluno', {
    method: 'POST',
    body: JSON.stringify(data),
  });
}

export function deletarAluno(id) {
  return request(`/Aluno/${id}`, {
    method: 'DELETE',
  });
}

// --- Autores ---
export function getAutores({ pageNumber = 1, pageSize = 100 } = {}) {
  const params = new URLSearchParams();
  params.set('PageNumber', pageNumber);
  params.set('PageSize', pageSize);
  return request(`/Autor?${params.toString()}`);
}

export function criarAutor(data) {
  return request('/Autor', {
    method: 'POST',
    body: JSON.stringify(data),
  });
}

export function atualizarAutor(id, data) {
  return request(`/Autor/${id}`, {
    method: 'PUT',
    body: JSON.stringify(data),
  });
}

export function deletarAutor(id) {
  return request(`/Autor/${id}`, {
    method: 'DELETE',
  });
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

export function getReservasGestao() {
  return request('/Reservas');
}

export function cancelarReserva(id) {
  return request(`/Reservas/${id}`, {
    method: 'DELETE',
  });
}

// --- Autenticação & Perfil ---
export function loginApi(email, senha) {
  return request('/auth/login', {
    method: 'POST',
    body: JSON.stringify({ email: email.trim(), senha }),
  });
}

export function getMe() {
  return request('/auth/me');
}

// --- Auditoria (ADMIN Only) ---
export function getAuditoria({ pageNumber = 1, pageSize = 10 } = {}) {
  const params = new URLSearchParams();
  params.set('pageNumber', pageNumber);
  params.set('pageSize', pageSize);
  return request(`/Auditoria?${params.toString()}`);
}

// --- Área do Aluno (ALUNO Only) ---
export function getMeusEmprestimos() {
  return request('/Emprestimos/meus');
}

export function getMinhasReservas() {
  return request('/Reservas/minhas');
}

export function getMeuPerfil() {
  return request('/Aluno/perfil');
}
