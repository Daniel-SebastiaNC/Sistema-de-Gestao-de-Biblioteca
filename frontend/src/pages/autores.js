// ========================================
// Autores Page - CRUD
// ========================================

import { getAutores } from '../api.js';
import { renderTable, renderLoading } from '../components/table.js';
import { openModal, showConfirm } from '../components/modal.js';
import { showToast } from '../components/toast.js';

const API_BASE = '/api';

let currentPage = 1;
const PAGE_SIZE = 10;

export async function renderAutores() {
  const content = document.getElementById('page-content');

  content.innerHTML = `
    <div class="page-header">
      <div class="page-header__left">
        <h1 class="page-title">Autores</h1>
        <p class="page-subtitle">Gerencie os autores cadastrados</p>
      </div>
      <div class="page-header__actions">
        <button class="btn btn-primary" id="btn-novo-autor">
          <span class="material-icons-round">add</span>
          Novo Autor
        </button>
      </div>
    </div>

    <div class="table-card">
      <div id="autores-table-container">
        <div class="loading"><div class="spinner"></div></div>
      </div>
    </div>
  `;

  document.getElementById('btn-novo-autor').addEventListener('click', () => openAutorForm());

  await loadAutores();

  return () => {
    currentPage = 1;
  };
}

async function loadAutores() {
  const container = document.getElementById('autores-table-container');
  if (!container) return;

  renderLoading(container);

  try {
    const result = await getAutores({
      pageNumber: currentPage,
      pageSize: PAGE_SIZE,
    });

    const items = result.items || [];

    renderTable(container, {
      columns: [
        { key: 'nome', label: 'Nome', render: (row) => `<span style="color:var(--text-primary);font-weight:500">${row.nome || '—'}</span>` },
        { key: 'nacionalidade', label: 'Nacionalidade', render: (row) => row.nacionalidade || '—' },
        { key: 'dataNascimento', label: 'Data de Nascimento', render: (row) => formatDate(row.dataNascimento) },
        { key: 'actions', label: 'Ações', render: (row) => `
          <div class="table-actions">
            <button class="btn btn-icon btn-secondary" data-edit="${row.id}" title="Editar">
              <span class="material-icons-round">edit</span>
            </button>
            <button class="btn btn-icon btn-danger" data-delete="${row.id}" title="Excluir">
              <span class="material-icons-round">delete</span>
            </button>
          </div>
        `},
      ],
      data: items,
      pagination: {
        pageNumber: result.pageNumber,
        pageSize: result.pageSize,
        totalItems: result.totalItems,
        totalPages: result.totalPages,
        hasPreviousPage: result.hasPreviousPage,
        hasNextPage: result.hasNextPage,
      },
      onPageChange: (page) => {
        currentPage = page;
        loadAutores();
      },
      emptyIcon: 'person',
      emptyText: 'Nenhum autor cadastrado',
    });

    // Edit handlers
    container.querySelectorAll('[data-edit]').forEach(btn => {
      btn.addEventListener('click', () => {
        const autor = items.find(a => a.id === btn.dataset.edit);
        if (autor) openAutorForm(autor);
      });
    });

    // Delete handlers
    container.querySelectorAll('[data-delete]').forEach(btn => {
      btn.addEventListener('click', async () => {
        const autor = items.find(a => a.id === btn.dataset.delete);
        if (!autor) return;

        const confirmed = await showConfirm({
          title: 'Excluir Autor',
          message: `Tem certeza que deseja excluir "${autor.nome}"? Esta ação não pode ser desfeita.`,
          confirmText: 'Excluir',
          type: 'danger',
        });

        if (confirmed) {
          try {
            await fetch(`${API_BASE}/Autor/${autor.id}`, { method: 'DELETE' });
            showToast('Autor excluído com sucesso!', 'success');
            loadAutores();
          } catch (err) {
            showToast('Erro ao excluir: ' + err.message, 'error');
          }
        }
      });
    });
  } catch (err) {
    container.innerHTML = `
      <div class="empty-state">
        <span class="material-icons-round empty-state__icon">error_outline</span>
        <p class="empty-state__title">Erro ao carregar autores</p>
        <p class="empty-state__text">${err.message}</p>
      </div>
    `;
    showToast('Erro ao carregar autores: ' + err.message, 'error');
  }
}

async function openAutorForm(autor = null) {
  const isEdit = !!autor;

  // Format date for the input (YYYY-MM-DD)
  let dateValue = '';
  if (autor?.dataNascimento) {
    try {
      dateValue = new Date(autor.dataNascimento).toISOString().split('T')[0];
    } catch { /* ignore */ }
  }

  const formContent = document.createElement('div');
  formContent.innerHTML = `
    <form id="autor-form">
      <div class="form-group">
        <label class="form-label" for="autor-nome">Nome</label>
        <input type="text" id="autor-nome" class="form-input" placeholder="Nome completo do autor" value="${autor?.nome || ''}" required />
      </div>
      <div class="form-group">
        <label class="form-label" for="autor-nascimento">Data de Nascimento</label>
        <input type="date" id="autor-nascimento" class="form-input" value="${dateValue}" required />
      </div>
      <div class="form-group">
        <label class="form-label" for="autor-nacionalidade">Nacionalidade</label>
        <input type="text" id="autor-nacionalidade" class="form-input" placeholder="Ex: Brasileiro" value="${autor?.nacionalidade || ''}" required />
      </div>
    </form>
  `;

  const footerEl = document.createElement('div');
  footerEl.style.display = 'contents';
  footerEl.innerHTML = `
    <button class="btn btn-secondary" id="modal-cancel">Cancelar</button>
    <button class="btn btn-primary" id="modal-save">
      <span class="material-icons-round">${isEdit ? 'save' : 'add'}</span>
      ${isEdit ? 'Salvar' : 'Cadastrar'}
    </button>
  `;

  openModal({
    title: isEdit ? 'Editar Autor' : 'Novo Autor',
    content: formContent,
    footer: footerEl,
  });

  // Cancel
  document.getElementById('modal-cancel')?.addEventListener('click', () => {
    document.getElementById('active-modal')?.remove();
  });

  // Save
  document.getElementById('modal-save')?.addEventListener('click', async () => {
    const nome = document.getElementById('autor-nome').value.trim();
    const dataNascimentoRaw = document.getElementById('autor-nascimento').value;
    const nacionalidade = document.getElementById('autor-nacionalidade').value.trim();

    if (!nome || !dataNascimentoRaw || !nacionalidade) {
      showToast('Preencha todos os campos obrigatórios', 'warning');
      return;
    }

    const dataNascimento = new Date(dataNascimentoRaw).toISOString();

    const saveBtn = document.getElementById('modal-save');
    saveBtn.disabled = true;
    saveBtn.innerHTML = '<div class="spinner" style="width:18px;height:18px;border-width:2px"></div> Salvando...';

    try {
      const body = JSON.stringify({ nome, dataNascimento, nacionalidade });
      const headers = { 'Content-Type': 'application/json' };

      if (isEdit) {
        const res = await fetch(`${API_BASE}/Autor/${autor.id}`, { method: 'PUT', headers, body });
        if (!res.ok) {
          const err = await res.text();
          throw new Error(err || `Erro ${res.status}`);
        }
        showToast('Autor atualizado com sucesso!', 'success');
      } else {
        const res = await fetch(`${API_BASE}/Autor`, { method: 'POST', headers, body });
        if (!res.ok) {
          const err = await res.text();
          throw new Error(err || `Erro ${res.status}`);
        }
        showToast('Autor cadastrado com sucesso!', 'success');
      }

      document.getElementById('active-modal')?.remove();
      loadAutores();
    } catch (err) {
      showToast('Erro: ' + err.message, 'error');
      saveBtn.disabled = false;
      saveBtn.innerHTML = `<span class="material-icons-round">${isEdit ? 'save' : 'add'}</span> ${isEdit ? 'Salvar' : 'Cadastrar'}`;
    }
  });
}

function formatDate(dateStr) {
  if (!dateStr) return '—';
  try {
    return new Date(dateStr).toLocaleDateString('pt-BR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
    });
  } catch {
    return dateStr;
  }
}
