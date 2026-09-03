// ========================================
// Auditoria Page - ADMIN Exclusive
// ========================================

import { getAuditoria } from '../api.js';
import { renderTable, renderLoading } from '../components/table.js';
import { showToast } from '../components/toast.js';

let currentPage = 1;
const PAGE_SIZE = 10;

const ACTION_COLORS = {
  LOGIN: 'badge--info',
  LOGOUT: 'badge--neutral',
  CRIAR_USUARIO: 'badge--warning',
  CRIAR_LIVRO: 'badge--success',
  ATUALIZAR_LIVRO: 'badge--warning',
  EXCLUIR_LIVRO: 'badge--danger',
  EMPRESTIMO: 'badge--info',
  DEVOLUCAO: 'badge--success',
  RESERVA: 'badge--warning',
  DEFAULT: 'badge--neutral',
};

export async function renderAuditoria() {
  const content = document.getElementById('page-content');

  content.innerHTML = `
    <div class="page-header">
      <div class="page-header__left">
        <h1 class="page-title">Logs de Auditoria</h1>
        <p class="page-subtitle">Rastreamento de ações críticas e segurança no sistema</p>
      </div>
      <div class="page-header__actions">
        <button class="btn btn-secondary" id="btn-recarregar-auditoria">
          <span class="material-icons-round">refresh</span>
          Atualizar
        </button>
      </div>
    </div>

    <div class="table-card">
      <div id="auditoria-table-container">
        <div class="loading"><div class="spinner"></div></div>
      </div>
    </div>
  `;

  document.getElementById('btn-recarregar-auditoria')?.addEventListener('click', () => {
    loadAuditoria();
  });

  await loadAuditoria();

  return () => {
    currentPage = 1;
  };
}

async function loadAuditoria() {
  const container = document.getElementById('auditoria-table-container');
  if (!container) return;

  renderLoading(container);

  try {
    const result = await getAuditoria({
      pageNumber: currentPage,
      pageSize: PAGE_SIZE,
    });

    const items = result.items || [];

    renderTable(container, {
      columns: [
        {
          key: 'dataHora',
          label: 'Data / Hora',
          render: (row) => {
            if (!row.dataHora) return '—';
            const date = new Date(row.dataHora);
            return `<span style="font-size: 0.85rem; color: var(--text-secondary);">${date.toLocaleString('pt-BR')}</span>`;
          },
        },
        {
          key: 'usuario',
          label: 'Usuário',
          render: (row) => `
            <div style="display: flex; align-items: center; gap: 8px;">
              <span class="material-icons-round" style="font-size: 1.1rem; color: var(--text-muted);">account_circle</span>
              <span style="color: var(--text-primary); font-weight: 500;">${row.usuario || 'Sistema'}</span>
            </div>
          `,
        },
        {
          key: 'acao',
          label: 'Ação',
          render: (row) => {
            const acao = (row.acao || 'OPERACAO').toUpperCase();
            const badgeCls = ACTION_COLORS[acao] || ACTION_COLORS.DEFAULT;
            return `<span class="badge ${badgeCls}">${acao}</span>`;
          },
        },
        {
          key: 'detalhes',
          label: 'Detalhes da Operação',
          render: (row) => `<span style="color: var(--text-secondary); font-size: 0.88rem;">${row.detalhes || '—'}</span>`,
        },
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
        loadAuditoria();
      },
      emptyIcon: 'security',
      emptyText: 'Nenhum log de auditoria registrado no momento',
    });
  } catch (err) {
    container.innerHTML = `
      <div class="empty-state">
        <span class="material-icons-round empty-state__icon" style="color: var(--danger);">error_outline</span>
        <p class="empty-state__title">Não foi possível carregar os logs de auditoria</p>
        <p style="color: var(--text-muted); font-size: 0.85rem;">${err.message}</p>
      </div>
    `;
    showToast('Erro ao carregar logs de auditoria', 'error');
  }
}
