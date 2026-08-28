// ========================================
// Empréstimos Page
// ========================================

import { getEmprestimos, criarEmprestimo, devolverEmprestimo, getAlunos, getLivros } from '../api.js';
import { renderTable, renderLoading } from '../components/table.js';
import { openModal, showConfirm } from '../components/modal.js';
import { showToast } from '../components/toast.js';

let currentPage = 1;
const PAGE_SIZE = 10;

const STATUS_MAP = {
  0: { label: 'Ativo', class: 'badge--info' },
  1: { label: 'Devolvido', class: 'badge--success' },
  2: { label: 'Atrasado', class: 'badge--danger' },
};

export async function renderEmprestimos() {
  const content = document.getElementById('page-content');

  content.innerHTML = `
    <div class="page-header">
      <div class="page-header__left">
        <h1 class="page-title">Empréstimos</h1>
        <p class="page-subtitle">Gerencie empréstimos e devoluções</p>
      </div>
      <div class="page-header__actions">
        <button class="btn btn-primary" id="btn-novo-emprestimo">
          <span class="material-icons-round">add</span>
          Novo Empréstimo
        </button>
      </div>
    </div>

    <div class="table-card">
      <div id="emprestimos-table-container">
        <div class="loading"><div class="spinner"></div></div>
      </div>
    </div>
  `;

  document.getElementById('btn-novo-emprestimo').addEventListener('click', () => openEmprestimoForm());

  await loadEmprestimos();

  return () => {
    currentPage = 1;
  };
}

async function loadEmprestimos() {
  const container = document.getElementById('emprestimos-table-container');
  if (!container) return;

  renderLoading(container);

  try {
    const result = await getEmprestimos({
      pageNumber: currentPage,
      pageSize: PAGE_SIZE,
    });

    const items = result.items || [];

    renderTable(container, {
      columns: [
        { key: 'aluno', label: 'Aluno', render: (row) => `
          <div>
            <div style="color:var(--text-primary);font-weight:500">${row.aluno?.nome || '—'}</div>
            <div style="font-size:0.75rem;color:var(--text-muted)">${row.aluno?.matricula || ''}</div>
          </div>
        `},
        { key: 'livro', label: 'Livro', render: (row) => `
          <div>
            <div style="color:var(--text-primary);font-weight:500">${row.livro?.titulo || '—'}</div>
            <div style="font-size:0.75rem;color:var(--text-muted)">${row.livro?.autor?.nome || ''}</div>
          </div>
        `},
        { key: 'dataEmprestimo', label: 'Data Empréstimo', render: (row) => formatDate(row.dataEmprestimo) },
        { key: 'dataPrevistaDevolucao', label: 'Previsão Devolução', render: (row) => formatDate(row.dataPrevistaDevolucao) },
        { key: 'dataDevolucao', label: 'Devolvido em', render: (row) => row.dataDevolucao ? formatDate(row.dataDevolucao) : '<span style="color:var(--text-muted)">—</span>' },
        { key: 'status', label: 'Status', render: (row) => {
          const status = STATUS_MAP[row.status] || { label: 'Desconhecido', class: 'badge--neutral' };
          return `<span class="badge ${status.class}">${status.label}</span>`;
        }},
        { key: 'actions', label: 'Ações', render: (row) => {
          if (row.status === 0 || row.status === 2) {
            return `
              <button class="btn btn-sm btn-success" data-devolver="${row.id}">
                <span class="material-icons-round" style="font-size:16px">assignment_return</span>
                Devolver
              </button>
            `;
          }
          return '<span style="color:var(--text-muted);font-size:0.8rem">Concluído</span>';
        }},
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
        loadEmprestimos();
      },
      emptyIcon: 'swap_horiz',
      emptyText: 'Nenhum empréstimo registrado',
    });

    // Devolver handlers
    container.querySelectorAll('[data-devolver]').forEach(btn => {
      btn.addEventListener('click', async () => {
        const emprestimoId = btn.dataset.devolver;
        const emprestimo = items.find(e => e.id === emprestimoId);

        const confirmed = await showConfirm({
          title: 'Confirmar Devolução',
          message: `Confirma a devolução do livro "${emprestimo?.livro?.titulo || ''}" pelo aluno "${emprestimo?.aluno?.nome || ''}"?`,
          confirmText: 'Devolver',
          cancelText: 'Cancelar',
          type: 'info',
        });

        if (confirmed) {
          try {
            btn.disabled = true;
            btn.innerHTML = '<div class="spinner" style="width:14px;height:14px;border-width:2px"></div>';
            const result = await devolverEmprestimo(emprestimoId);
            
            let msg = 'Livro devolvido com sucesso!';
            if (result && result.diasAtraso > 0) {
              msg = `Devolvido com ${result.diasAtraso} dia(s) de atraso. Multa: R$ ${(result.valorMulta || 0).toFixed(2)}`;
              showToast(msg, 'warning', 6000);
            } else {
              showToast(msg, 'success');
            }
            loadEmprestimos();
          } catch (err) {
            showToast('Erro ao devolver: ' + err.message, 'error');
            btn.disabled = false;
            btn.innerHTML = '<span class="material-icons-round" style="font-size:16px">assignment_return</span> Devolver';
          }
        }
      });
    });
  } catch (err) {
    container.innerHTML = `
      <div class="empty-state">
        <span class="material-icons-round empty-state__icon">error_outline</span>
        <p class="empty-state__title">Erro ao carregar empréstimos</p>
        <p class="empty-state__text">${err.message}</p>
      </div>
    `;
    showToast('Erro ao carregar empréstimos: ' + err.message, 'error');
  }
}

async function openEmprestimoForm() {
  // Load alunos and livros for selects
  let alunos = [];
  let livros = [];

  try {
    const [alunosResult, livrosResult] = await Promise.all([
      getAlunos({ pageNumber: 1, pageSize: 200 }),
      getLivros({ pageNumber: 1, pageSize: 200 }),
    ]);
    alunos = alunosResult.items || [];
    livros = (livrosResult.items || []).filter(l => (l.quantidade ?? 0) > 0);
  } catch {
    showToast('Erro ao carregar dados do formulário', 'error');
  }

  const formContent = document.createElement('div');
  formContent.innerHTML = `
    <form id="emprestimo-form">
      <div class="form-group">
        <label class="form-label" for="emp-aluno">Aluno</label>
        <select id="emp-aluno" class="form-select" required>
          <option value="">Selecione um aluno</option>
          ${alunos.map(a => `
            <option value="${a.id}">${a.nome} (${a.matricula || '—'})</option>
          `).join('')}
        </select>
      </div>
      <div class="form-group">
        <label class="form-label" for="emp-livro">Livro</label>
        <select id="emp-livro" class="form-select" required>
          <option value="">Selecione um livro</option>
          ${livros.map(l => `
            <option value="${l.id}">${l.titulo} — ${l.autor?.nome || 'Autor desconhecido'} (${l.quantidade} disp.)</option>
          `).join('')}
        </select>
      </div>
    </form>
  `;

  const footerEl = document.createElement('div');
  footerEl.style.display = 'contents';
  footerEl.innerHTML = `
    <button class="btn btn-secondary" id="modal-cancel">Cancelar</button>
    <button class="btn btn-primary" id="modal-save">
      <span class="material-icons-round">add</span>
      Registrar Empréstimo
    </button>
  `;

  openModal({
    title: 'Novo Empréstimo',
    content: formContent,
    footer: footerEl,
  });

  document.getElementById('modal-cancel')?.addEventListener('click', () => {
    document.getElementById('active-modal')?.remove();
  });

  document.getElementById('modal-save')?.addEventListener('click', async () => {
    const idAluno = document.getElementById('emp-aluno').value;
    const idLivro = document.getElementById('emp-livro').value;

    if (!idAluno || !idLivro) {
      showToast('Selecione o aluno e o livro', 'warning');
      return;
    }

    const saveBtn = document.getElementById('modal-save');
    saveBtn.disabled = true;
    saveBtn.innerHTML = '<div class="spinner" style="width:18px;height:18px;border-width:2px"></div> Registrando...';

    try {
      await criarEmprestimo({ idAluno, idLivro });
      showToast('Empréstimo registrado com sucesso!', 'success');
      document.getElementById('active-modal')?.remove();
      loadEmprestimos();
    } catch (err) {
      showToast('Erro: ' + err.message, 'error');
      saveBtn.disabled = false;
      saveBtn.innerHTML = '<span class="material-icons-round">add</span> Registrar Empréstimo';
    }
  });
}

function formatDate(dateStr) {
  if (!dateStr) return '—';
  try {
    const date = new Date(dateStr);
    return date.toLocaleDateString('pt-BR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
    });
  } catch {
    return dateStr;
  }
}
