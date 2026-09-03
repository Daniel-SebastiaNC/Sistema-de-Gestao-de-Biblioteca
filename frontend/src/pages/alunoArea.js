// ========================================
// Aluno Area Pages - ALUNO Exclusive
// ========================================

import { getMeusEmprestimos, getMinhasReservas } from '../api.js';
import { renderTable, renderLoading } from '../components/table.js';
import { showToast } from '../components/toast.js';

const STATUS_EMPRESTIMO_MAP = {
  0: { label: 'Ativo', class: 'badge--info' },
  1: { label: 'Devolvido', class: 'badge--success' },
  2: { label: 'Atrasado', class: 'badge--danger' },
};

const STATUS_RESERVA_MAP = {
  0: { label: 'Ativa na Fila', class: 'badge--warning' },
  1: { label: 'Atendida', class: 'badge--success' },
  2: { label: 'Cancelada', class: 'badge--neutral' },
  Ativa: { label: 'Ativa na Fila', class: 'badge--warning' },
  Atendida: { label: 'Atendida', class: 'badge--success' },
  Cancelada: { label: 'Cancelada', class: 'badge--neutral' },
};

function formatDate(isoStr) {
  if (!isoStr) return '—';
  try {
    const d = new Date(isoStr);
    return d.toLocaleDateString('pt-BR');
  } catch {
    return isoStr;
  }
}

// -------------------------------------------------------------------
// 1. Meus Empréstimos
// -------------------------------------------------------------------
export async function renderMeusEmprestimos() {
  const content = document.getElementById('page-content');

  content.innerHTML = `
    <div class="page-header">
      <div class="page-header__left">
        <h1 class="page-title">Meus Empréstimos</h1>
        <p class="page-subtitle">Acompanhe seus livros emprestados, prazos e histórico de devoluções</p>
      </div>
      <div class="page-header__actions">
        <button class="btn btn-secondary" id="btn-recarregar-meus-emprestimos">
          <span class="material-icons-round">refresh</span>
          Atualizar
        </button>
      </div>
    </div>

    <div class="table-card">
      <div id="meus-emprestimos-table-container">
        <div class="loading"><div class="spinner"></div></div>
      </div>
    </div>
  `;

  document.getElementById('btn-recarregar-meus-emprestimos')?.addEventListener('click', () => {
    loadMeusEmprestimos();
  });

  await loadMeusEmprestimos();
}

async function loadMeusEmprestimos() {
  const container = document.getElementById('meus-emprestimos-table-container');
  if (!container) return;

  renderLoading(container);

  try {
    const data = await getMeusEmprestimos();
    const items = Array.isArray(data) ? data : (data?.items || []);

    renderTable(container, {
      columns: [
        {
          key: 'livroTitulo',
          label: 'Livro',
          render: (row) => `
            <div style="display: flex; align-items: center; gap: 8px;">
              <span class="material-icons-round" style="color: var(--accent-primary);">menu_book</span>
              <span style="font-weight: 500; color: var(--text-primary);">${row.livroTitulo || 'Livro'}</span>
            </div>
          `,
        },
        {
          key: 'dataEmprestimo',
          label: 'Data de Saída',
          render: (row) => formatDate(row.dataEmprestimo),
        },
        {
          key: 'dataDevolucaoPrevista',
          label: 'Devolução Prevista',
          render: (row) => {
            const isLate = row.status === 2 || (row.status === 0 && new Date(row.dataDevolucaoPrevista) < new Date());
            const color = isLate ? 'var(--danger)' : 'var(--text-secondary)';
            return `<span style="color: ${color}; font-weight: ${isLate ? '600' : '400'};">${formatDate(row.dataDevolucaoPrevista)}</span>`;
          },
        },
        {
          key: 'dataDevolucaoReal',
          label: 'Devolvido Em',
          render: (row) => formatDate(row.dataDevolucaoReal),
        },
        {
          key: 'multa',
          label: 'Multa',
          render: (row) => {
            const valor = row.multa ?? 0;
            if (valor > 0) {
              return `<span class="badge badge--danger">R$ ${valor.toFixed(2)}</span>`;
            }
            return `<span style="color: var(--text-muted);">R$ 0,00</span>`;
          },
        },
        {
          key: 'status',
          label: 'Status',
          render: (row) => {
            const config = STATUS_EMPRESTIMO_MAP[row.status] || { label: 'Ativo', class: 'badge--info' };
            return `<span class="badge ${config.class}">${config.label}</span>`;
          },
        },
      ],
      data: items,
      emptyIcon: 'history_edu',
      emptyText: 'Você ainda não possui nenhum empréstimo registrado',
    });
  } catch (err) {
    container.innerHTML = `
      <div class="empty-state">
        <span class="material-icons-round empty-state__icon" style="color: var(--danger);">error_outline</span>
        <p class="empty-state__title">Não foi possível carregar seus empréstimos</p>
        <p style="color: var(--text-muted); font-size: 0.85rem;">${err.message}</p>
      </div>
    `;
    showToast('Erro ao consultar seus empréstimos', 'error');
  }
}

// -------------------------------------------------------------------
// 2. Minhas Reservas
// -------------------------------------------------------------------
export async function renderMinhasReservas() {
  const content = document.getElementById('page-content');

  content.innerHTML = `
    <div class="page-header">
      <div class="page-header__left">
        <h1 class="page-title">Minhas Reservas</h1>
        <p class="page-subtitle">Acompanhe sua posição na fila de espera para livros indisponíveis</p>
      </div>
      <div class="page-header__actions">
        <button class="btn btn-secondary" id="btn-recarregar-minhas-reservas">
          <span class="material-icons-round">refresh</span>
          Atualizar
        </button>
      </div>
    </div>

    <div class="table-card">
      <div id="minhas-reservas-table-container">
        <div class="loading"><div class="spinner"></div></div>
      </div>
    </div>
  `;

  document.getElementById('btn-recarregar-minhas-reservas')?.addEventListener('click', () => {
    loadMinhasReservas();
  });

  await loadMinhasReservas();
}

async function loadMinhasReservas() {
  const container = document.getElementById('minhas-reservas-table-container');
  if (!container) return;

  renderLoading(container);

  try {
    const data = await getMinhasReservas();
    const items = Array.isArray(data) ? data : (data?.items || []);

    renderTable(container, {
      columns: [
        {
          key: 'livroTitulo',
          label: 'Livro Solicitado',
          render: (row) => `
            <div style="display: flex; align-items: center; gap: 8px;">
              <span class="material-icons-round" style="color: var(--warning);">bookmark</span>
              <span style="font-weight: 500; color: var(--text-primary);">${row.livroTitulo || 'Livro'}</span>
            </div>
          `,
        },
        {
          key: 'dataReserva',
          label: 'Data da Reserva',
          render: (row) => formatDate(row.dataReserva),
        },
        {
          key: 'posicaoFila',
          label: 'Posição na Fila',
          render: (row) => {
            const pos = row.posicaoFila ?? 1;
            const badgeClass = pos === 1 ? 'badge--success' : 'badge--warning';
            return `<span class="badge ${badgeClass}">${pos}º lugar</span>`;
          },
        },
        {
          key: 'status',
          label: 'Status da Reserva',
          render: (row) => {
            const config = STATUS_RESERVA_MAP[row.status] || { label: 'Ativa na Fila', class: 'badge--warning' };
            return `<span class="badge ${config.class}">${config.label}</span>`;
          },
        },
      ],
      data: items,
      emptyIcon: 'bookmark_border',
      emptyText: 'Você não possui nenhuma reserva de livro na fila no momento',
    });
  } catch (err) {
    container.innerHTML = `
      <div class="empty-state">
        <span class="material-icons-round empty-state__icon" style="color: var(--danger);">error_outline</span>
        <p class="empty-state__title">Não foi possível carregar suas reservas</p>
        <p style="color: var(--text-muted); font-size: 0.85rem;">${err.message}</p>
      </div>
    `;
    showToast('Erro ao consultar suas reservas', 'error');
  }
}
