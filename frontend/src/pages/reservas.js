// ========================================
// Reservas Page - ADMIN & BIBLIOTECARIO
// ========================================

import { getReservasGestao, cancelarReserva } from '../api.js';
import { renderTable, renderLoading } from '../components/table.js';
import { showToast } from '../components/toast.js';
import { showConfirm } from '../components/modal.js';

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
    return d.toLocaleString('pt-BR');
  } catch {
    return isoStr;
  }
}

export async function renderReservas() {
  const content = document.getElementById('page-content');

  content.innerHTML = `
    <div class="page-header">
      <div class="page-header__left">
        <h1 class="page-title">Gestão de Reservas</h1>
        <p class="page-subtitle">Acompanhe a fila de espera prioritária e gerencie as reservas de livros</p>
      </div>
      <div class="page-header__actions">
        <button class="btn btn-secondary" id="btn-recarregar-reservas">
          <span class="material-icons-round">refresh</span>
          Atualizar
        </button>
      </div>
    </div>

    <div class="table-card">
      <div id="reservas-table-container">
        <div class="loading"><div class="spinner"></div></div>
      </div>
    </div>
  `;

  document.getElementById('btn-recarregar-reservas')?.addEventListener('click', () => {
    loadReservas();
  });

  await loadReservas();
}

async function loadReservas() {
  const container = document.getElementById('reservas-table-container');
  if (!container) return;

  renderLoading(container);

  try {
    const data = await getReservasGestao();
    const items = Array.isArray(data) ? data : (data?.items || []);

    renderTable(container, {
      columns: [
        {
          key: 'livro',
          label: 'Livro Solicitado',
          render: (row) => `
            <div style="display: flex; flex-direction: column; gap: 2px;">
              <span style="font-weight: 500; color: var(--text-primary); display: flex; align-items: center; gap: 6px;">
                <span class="material-icons-round" style="color: var(--accent-primary); font-size: 1.1rem;">menu_book</span>
                ${row.livroTitulo || row.livro?.titulo || 'Livro'}
              </span>
              ${row.livro?.isbn ? `<span style="font-size: 0.75rem; color: var(--text-muted); margin-left: 24px;">ISBN: ${row.livro.isbn}</span>` : ''}
            </div>
          `,
        },
        {
          key: 'aluno',
          label: 'Aluno Solicitante',
          render: (row) => `
            <div style="display: flex; flex-direction: column; gap: 2px;">
              <span style="color: var(--text-primary); font-weight: 500; display: flex; align-items: center; gap: 6px;">
                <span class="material-icons-round" style="color: var(--text-muted); font-size: 1.1rem;">person</span>
                ${row.alunoNome || row.aluno?.nome || 'Aluno'}
              </span>
              ${row.aluno?.matricula ? `<span style="font-size: 0.75rem; color: var(--text-muted); margin-left: 24px;">Matrícula: ${row.aluno.matricula}</span>` : ''}
            </div>
          `,
        },
        {
          key: 'dataReserva',
          label: 'Data da Solicitação',
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
          label: 'Status',
          render: (row) => {
            const config = STATUS_RESERVA_MAP[row.status] || { label: 'Ativa', class: 'badge--warning' };
            return `<span class="badge ${config.class}">${config.label}</span>`;
          },
        },
        {
          key: 'acoes',
          label: 'Ações',
          render: (row) => {
            const isAtiva = row.status === 0 || row.status === 'Ativa';
            if (isAtiva) {
              return `
                <button class="btn btn-sm btn-danger" data-cancel-reserva="${row.id}" title="Cancelar reserva">
                  <span class="material-icons-round" style="font-size: 1rem;">close</span>
                  Cancelar
                </button>
              `;
            }
            return `<span style="color: var(--text-muted); font-size: 0.8rem;">—</span>`;
          },
        },
      ],
      data: items,
      emptyIcon: 'bookmarks',
      emptyText: 'Nenhuma reserva de livro registrada no sistema no momento',
    });

    // Action handlers for cancellation
    container.querySelectorAll('[data-cancel-reserva]').forEach(btn => {
      btn.addEventListener('click', async () => {
        const id = btn.dataset.cancelReserva;
        const reserva = items.find(r => r.id === id);
        if (!reserva) return;

        const confirmed = await showConfirm({
          title: 'Cancelar Reserva',
          message: `Tem certeza que deseja cancelar a reserva do livro "${reserva.livroTitulo || reserva.livro?.titulo || 'Livro'}" para o aluno "${reserva.alunoNome || reserva.aluno?.nome || 'Aluno'}"?`,
          confirmText: 'Cancelar Reserva',
          type: 'danger',
        });

        if (confirmed) {
          try {
            await cancelarReserva(id);
            showToast('Reserva cancelada com sucesso!', 'success');
            loadReservas();
          } catch (err) {
            showToast('Erro ao cancelar reserva: ' + err.message, 'error');
          }
        }
      });
    });
  } catch (err) {
    container.innerHTML = `
      <div class="empty-state">
        <span class="material-icons-round empty-state__icon" style="color: var(--danger);">error_outline</span>
        <p class="empty-state__title">Não foi possível carregar as reservas</p>
        <p style="color: var(--text-muted); font-size: 0.85rem;">${err.message}</p>
      </div>
    `;
    showToast('Erro ao carregar reservas', 'error');
  }
}
