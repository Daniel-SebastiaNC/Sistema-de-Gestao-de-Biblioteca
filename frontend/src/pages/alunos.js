// ========================================
// Alunos Page - CRUD
// ========================================

import { getAlunos } from '../api.js';
import { renderTable, renderLoading } from '../components/table.js';
import { openModal, showConfirm } from '../components/modal.js';
import { showToast } from '../components/toast.js';

const API_BASE = '/api';

let currentPage = 1;
const PAGE_SIZE = 10;

export async function renderAlunos() {
  const content = document.getElementById('page-content');

  content.innerHTML = `
    <div class="page-header">
      <div class="page-header__left">
        <h1 class="page-title">Alunos</h1>
        <p class="page-subtitle">Gerencie os alunos cadastrados</p>
      </div>
      <div class="page-header__actions">
        <button class="btn btn-primary" id="btn-novo-aluno">
          <span class="material-icons-round">add</span>
          Novo Aluno
        </button>
      </div>
    </div>

    <div class="table-card">
      <div id="alunos-table-container">
        <div class="loading"><div class="spinner"></div></div>
      </div>
    </div>
  `;

  document.getElementById('btn-novo-aluno').addEventListener('click', () => openAlunoForm());

  await loadAlunos();

  return () => {
    currentPage = 1;
  };
}

async function loadAlunos() {
  const container = document.getElementById('alunos-table-container');
  if (!container) return;

  renderLoading(container);

  try {
    const result = await getAlunos({
      pageNumber: currentPage,
      pageSize: PAGE_SIZE,
    });

    const items = result.items || [];

    renderTable(container, {
      columns: [
        { key: 'nome', label: 'Nome', render: (row) => `<span style="color:var(--text-primary);font-weight:500">${row.nome || '—'}</span>` },
        { key: 'matricula', label: 'Matrícula', render: (row) => `<code style="background:var(--bg-glass);padding:2px 8px;border-radius:4px;font-size:0.85rem">${row.matricula || '—'}</code>` },
        { key: 'email', label: 'E-mail', render: (row) => row.email || '—' },
        { key: 'emprestimos', label: 'Empréstimos', render: (row) => {
          const count = row.emprestimos?.length || 0;
          if (count === 0) return '<span class="badge badge--neutral">0</span>';
          return `<span class="badge badge--info">${count}</span>`;
        }},
        { key: 'actions', label: 'Ações', render: (row) => `
          <div class="table-actions">
            <button class="btn btn-icon btn-secondary" data-view="${row.id}" title="Ver detalhes">
              <span class="material-icons-round">visibility</span>
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
        loadAlunos();
      },
      emptyIcon: 'school',
      emptyText: 'Nenhum aluno cadastrado',
    });

    // View details handler
    container.querySelectorAll('[data-view]').forEach(btn => {
      btn.addEventListener('click', () => {
        const aluno = items.find(a => a.id === btn.dataset.view);
        if (aluno) openAlunoDetails(aluno);
      });
    });

    // Delete handler
    container.querySelectorAll('[data-delete]').forEach(btn => {
      btn.addEventListener('click', async () => {
        const aluno = items.find(a => a.id === btn.dataset.delete);
        if (!aluno) return;

        const confirmed = await showConfirm({
          title: 'Excluir Aluno',
          message: `Tem certeza que deseja excluir "${aluno.nome}"? Esta ação não pode ser desfeita.`,
          confirmText: 'Excluir',
          type: 'danger',
        });

        if (confirmed) {
          try {
            const res = await fetch(`${API_BASE}/Aluno/${aluno.id}`, { method: 'DELETE' });
            if (!res.ok) {
              const err = await res.text();
              throw new Error(err || `Erro ${res.status}`);
            }
            showToast('Aluno excluído com sucesso!', 'success');
            loadAlunos();
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
        <p class="empty-state__title">Erro ao carregar alunos</p>
        <p class="empty-state__text">${err.message}</p>
      </div>
    `;
    showToast('Erro ao carregar alunos: ' + err.message, 'error');
  }
}

function openAlunoDetails(aluno) {
  const emprestimos = aluno.emprestimos || [];

  const content = document.createElement('div');
  content.innerHTML = `
    <div style="display:flex;flex-direction:column;gap:16px">
      <div style="display:grid;grid-template-columns:1fr 1fr;gap:12px">
        <div>
          <div class="form-label" style="margin-bottom:4px">Nome</div>
          <div style="color:var(--text-primary);font-weight:500">${aluno.nome || '—'}</div>
        </div>
        <div>
          <div class="form-label" style="margin-bottom:4px">Matrícula</div>
          <div><code style="background:var(--bg-glass);padding:2px 8px;border-radius:4px">${aluno.matricula || '—'}</code></div>
        </div>
        <div style="grid-column:1/-1">
          <div class="form-label" style="margin-bottom:4px">E-mail</div>
          <div style="color:var(--text-secondary)">${aluno.email || '—'}</div>
        </div>
      </div>

      ${emprestimos.length > 0 ? `
        <div>
          <div class="form-label" style="margin-bottom:8px">Empréstimos (${emprestimos.length})</div>
          <div style="display:flex;flex-direction:column;gap:8px">
            ${emprestimos.map(emp => {
              const statusMap = { 0: { label: 'Ativo', cls: 'badge--info' }, 1: { label: 'Devolvido', cls: 'badge--success' }, 2: { label: 'Atrasado', cls: 'badge--danger' } };
              const status = statusMap[emp.status] || { label: '?', cls: 'badge--neutral' };
              return `
                <div style="background:var(--bg-glass);border:1px solid var(--border-color);border-radius:var(--radius-md);padding:12px;display:flex;justify-content:space-between;align-items:center">
                  <div>
                    <div style="color:var(--text-primary);font-weight:500;font-size:0.9rem">${emp.livro?.titulo || '—'}</div>
                    <div style="font-size:0.75rem;color:var(--text-muted);margin-top:2px">
                      ${formatDate(emp.dataEmprestimo)} → ${formatDate(emp.dataPrevistaDevolucao)}
                    </div>
                  </div>
                  <span class="badge ${status.cls}">${status.label}</span>
                </div>
              `;
            }).join('')}
          </div>
        </div>
      ` : `
        <div style="text-align:center;padding:16px;color:var(--text-muted);font-size:0.85rem">
          Nenhum empréstimo registrado
        </div>
      `}
    </div>
  `;

  openModal({
    title: `Detalhes do Aluno`,
    content,
  });
}

function openAlunoForm() {
  const formContent = document.createElement('div');
  formContent.innerHTML = `
    <form id="aluno-form">
      <div class="form-group">
        <label class="form-label" for="aluno-nome">Nome</label>
        <input type="text" id="aluno-nome" class="form-input" placeholder="Nome completo do aluno" required />
      </div>
      <div class="form-group">
        <label class="form-label" for="aluno-matricula">Matrícula</label>
        <input type="text" id="aluno-matricula" class="form-input" placeholder="Ex: 2024001" required />
      </div>
      <div class="form-group">
        <label class="form-label" for="aluno-email">E-mail</label>
        <input type="email" id="aluno-email" class="form-input" placeholder="aluno@email.com" required />
      </div>
    </form>
  `;

  const footerEl = document.createElement('div');
  footerEl.style.display = 'contents';
  footerEl.innerHTML = `
    <button class="btn btn-secondary" id="modal-cancel">Cancelar</button>
    <button class="btn btn-primary" id="modal-save">
      <span class="material-icons-round">add</span>
      Cadastrar
    </button>
  `;

  openModal({
    title: 'Novo Aluno',
    content: formContent,
    footer: footerEl,
  });

  document.getElementById('modal-cancel')?.addEventListener('click', () => {
    document.getElementById('active-modal')?.remove();
  });

  document.getElementById('modal-save')?.addEventListener('click', async () => {
    const nome = document.getElementById('aluno-nome').value.trim();
    const matricula = document.getElementById('aluno-matricula').value.trim();
    const email = document.getElementById('aluno-email').value.trim();

    if (!nome || !matricula || !email) {
      showToast('Preencha todos os campos obrigatórios', 'warning');
      return;
    }

    const saveBtn = document.getElementById('modal-save');
    saveBtn.disabled = true;
    saveBtn.innerHTML = '<div class="spinner" style="width:18px;height:18px;border-width:2px"></div> Salvando...';

    try {
      const res = await fetch(`${API_BASE}/Aluno`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ nome, matricula, email }),
      });

      if (!res.ok) {
        const err = await res.text();
        throw new Error(err || `Erro ${res.status}`);
      }

      showToast('Aluno cadastrado com sucesso!', 'success');
      document.getElementById('active-modal')?.remove();
      loadAlunos();
    } catch (err) {
      showToast('Erro: ' + err.message, 'error');
      saveBtn.disabled = false;
      saveBtn.innerHTML = '<span class="material-icons-round">add</span> Cadastrar';
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
