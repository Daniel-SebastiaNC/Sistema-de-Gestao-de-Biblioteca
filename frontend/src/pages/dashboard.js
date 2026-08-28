// ========================================
// Dashboard Page
// ========================================

import { getDashboard, getLivrosPopulares, getEmprestimosAtrasados } from '../api.js';
import { showToast } from '../components/toast.js';

export async function renderDashboard() {
  const content = document.getElementById('page-content');

  content.innerHTML = `
    <div class="page-header">
      <div class="page-header__left">
        <h1 class="page-title">Dashboard</h1>
        <p class="page-subtitle">Visão geral do sistema da biblioteca</p>
      </div>
    </div>

    <div class="stats-grid" id="stats-grid">
      ${Array(3).fill('').map(() => '<div class="stat-card"><div class="skeleton skeleton--card"></div></div>').join('')}
    </div>

    <div class="dashboard-grid">
      <div class="table-card" id="populares-card">
        <div class="table-card__header">
          <h3 class="table-card__title">📚 Livros Mais Populares</h3>
        </div>
        <div class="loading"><div class="spinner"></div></div>
      </div>
      <div class="table-card" id="atrasados-card">
        <div class="table-card__header">
          <h3 class="table-card__title">⚠️ Empréstimos Atrasados</h3>
        </div>
        <div class="loading"><div class="spinner"></div></div>
      </div>
    </div>
  `;

  // Load data in parallel
  await Promise.allSettled([
    loadStats(),
    loadPopulares(),
    loadAtrasados(),
  ]);
}

async function loadStats() {
  const grid = document.getElementById('stats-grid');
  if (!grid) return;

  try {
    const data = await getDashboard();

    const stats = [
      { label: 'Total de Livros', value: data.totalLivros, icon: 'menu_book', color: 'primary' },
      { label: 'Usuários Ativos', value: data.totalUsuariosAtivos, icon: 'people', color: 'success' },
      { label: 'Empréstimos Ativos', value: data.totalEmprestimosAtivos, icon: 'swap_horiz', color: 'info' },
    ];

    grid.innerHTML = stats.map(stat => `
      <div class="stat-card">
        <div class="stat-card__header">
          <div class="stat-card__icon stat-card__icon--${stat.color}">
            <span class="material-icons-round">${stat.icon}</span>
          </div>
        </div>
        <div class="stat-card__value">${stat.value ?? 0}</div>
        <div class="stat-card__label">${stat.label}</div>
      </div>
    `).join('');
  } catch (err) {
    grid.innerHTML = `
      <div class="stat-card" style="grid-column: 1 / -1;">
        <div class="empty-state">
          <span class="material-icons-round empty-state__icon">cloud_off</span>
          <p class="empty-state__title">Não foi possível carregar as estatísticas</p>
          <p class="empty-state__text">${err.message}</p>
        </div>
      </div>
    `;
    showToast('Erro ao carregar dashboard: ' + err.message, 'error');
  }
}

async function loadPopulares() {
  const card = document.getElementById('populares-card');
  if (!card) return;

  try {
    const data = await getLivrosPopulares(5);

    const header = card.querySelector('.table-card__header').outerHTML;

    if (!data || data.length === 0) {
      card.innerHTML = `
        ${header}
        <div class="empty-state">
          <span class="material-icons-round empty-state__icon">auto_stories</span>
          <p class="empty-state__title">Sem dados de popularidade</p>
        </div>
      `;
      return;
    }

    card.innerHTML = `
      ${header}
      <div class="table-wrapper">
        <table class="data-table">
          <thead>
            <tr>
              <th>#</th>
              <th>Título</th>
              <th>Autor</th>
              <th>Empréstimos</th>
            </tr>
          </thead>
          <tbody>
            ${data.map((item, i) => `
              <tr>
                <td><span class="badge badge--${i < 3 ? 'warning' : 'neutral'}">${i + 1}º</span></td>
                <td style="color: var(--text-primary); font-weight: 500;">${item.titulo || '—'}</td>
                <td>${item.autorNome || '—'}</td>
                <td><strong>${item.totalEmprestimos}</strong></td>
              </tr>
            `).join('')}
          </tbody>
        </table>
      </div>
    `;
  } catch (err) {
    const header = card.querySelector('.table-card__header')?.outerHTML || '';
    card.innerHTML = `
      ${header}
      <div class="empty-state">
        <span class="material-icons-round empty-state__icon">error_outline</span>
        <p class="empty-state__text">${err.message}</p>
      </div>
    `;
  }
}

async function loadAtrasados() {
  const card = document.getElementById('atrasados-card');
  if (!card) return;

  try {
    const data = await getEmprestimosAtrasados();

    const header = card.querySelector('.table-card__header').outerHTML;

    if (!data || data.length === 0) {
      card.innerHTML = `
        ${header}
        <div class="empty-state">
          <span class="material-icons-round empty-state__icon">check_circle</span>
          <p class="empty-state__title">Nenhum empréstimo atrasado</p>
          <p class="empty-state__text">Tudo em dia! 🎉</p>
        </div>
      `;
      return;
    }

    card.innerHTML = `
      ${header}
      <div class="table-wrapper">
        <table class="data-table">
          <thead>
            <tr>
              <th>Aluno</th>
              <th>Livro</th>
              <th>Dias Atraso</th>
              <th>Multa</th>
            </tr>
          </thead>
          <tbody>
            ${data.map(item => `
              <tr>
                <td style="color: var(--text-primary); font-weight: 500;">${item.alunoNome || '—'}</td>
                <td>${item.livroTitulo || '—'}</td>
                <td><span class="badge badge--danger">${item.diasAtraso} dias</span></td>
                <td style="color: var(--danger); font-weight: 600;">R$ ${(item.multaEstimada || 0).toFixed(2)}</td>
              </tr>
            `).join('')}
          </tbody>
        </table>
      </div>
    `;
  } catch (err) {
    const header = card.querySelector('.table-card__header')?.outerHTML || '';
    card.innerHTML = `
      ${header}
      <div class="empty-state">
        <span class="material-icons-round empty-state__icon">error_outline</span>
        <p class="empty-state__text">${err.message}</p>
      </div>
    `;
  }
}
