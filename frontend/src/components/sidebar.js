// ========================================
// Sidebar Component
// ========================================

import { navigateTo, getCurrentRoute } from '../router.js';
import { getUser, logout, hasAnyRole } from '../auth.js';

const ALL_NAV_ITEMS = [
  { path: '/dashboard', icon: 'dashboard', label: 'Dashboard', roles: ['ADMIN', 'BIBLIOTECARIO'] },
  { path: '/livros', icon: 'menu_book', label: 'Livros', roles: ['ADMIN', 'BIBLIOTECARIO', 'ALUNO'] },
  { path: '/autores', icon: 'person', label: 'Autores', roles: ['ADMIN', 'BIBLIOTECARIO'] },
  { path: '/alunos', icon: 'school', label: 'Alunos', roles: ['ADMIN', 'BIBLIOTECARIO'] },
  { path: '/emprestimos', icon: 'swap_horiz', label: 'Empréstimos', roles: ['ADMIN', 'BIBLIOTECARIO'] },
  { path: '/reservas', icon: 'bookmarks', label: 'Reservas', roles: ['ADMIN', 'BIBLIOTECARIO'] },
  { path: '/meus-emprestimos', icon: 'history_edu', label: 'Meus Empréstimos', roles: ['ALUNO'] },
  { path: '/minhas-reservas', icon: 'bookmark', label: 'Minhas Reservas', roles: ['ALUNO'] },
  { path: '/auditoria', icon: 'security', label: 'Auditoria', roles: ['ADMIN'] },
];

const ROLE_LABELS = {
  ADMIN: { label: 'Administrador', badgeClass: 'badge--warning' },
  BIBLIOTECARIO: { label: 'Bibliotecário', badgeClass: 'badge--info' },
  ALUNO: { label: 'Aluno', badgeClass: 'badge--success' },
};

export function renderSidebar(container) {
  const user = getUser() || { nome: 'Usuário', email: '', perfil: 'ALUNO' };
  const userRole = (user.perfil || 'ALUNO').toUpperCase();
  const roleConfig = ROLE_LABELS[userRole] || { label: userRole, badgeClass: 'badge--neutral' };
  const initial = (user.nome || user.email || 'U').charAt(0).toUpperCase();

  const visibleNavItems = ALL_NAV_ITEMS.filter(item => hasAnyRole(item.roles));

  const sidebar = document.createElement('aside');
  sidebar.className = 'sidebar';
  sidebar.id = 'sidebar';

  sidebar.innerHTML = `
    <div class="sidebar__brand">
      <div class="sidebar__brand-icon">
        <span class="material-icons-round">local_library</span>
      </div>
      <span class="sidebar__brand-text">SmartLib</span>
    </div>

    <nav class="sidebar__nav" id="sidebar-nav">
      ${visibleNavItems.map(item => `
        <button class="sidebar__link" data-path="${item.path}">
          <span class="material-icons-round">${item.icon}</span>
          <span>${item.label}</span>
        </button>
      `).join('')}
    </nav>

    <div class="sidebar__footer">
      <div class="sidebar__user">
        <div class="sidebar__avatar" style="font-weight: 700;">${initial}</div>
        <div class="sidebar__user-info">
          <div class="sidebar__user-name" title="${user.nome || user.email}">${user.nome || user.email}</div>
          <div style="margin-top: 2px;">
            <span class="badge ${roleConfig.badgeClass}" style="font-size: 0.68rem; padding: 2px 6px;">
              ${roleConfig.label}
            </span>
            ${user.matricula ? `<span style="font-size: 0.68rem; color: var(--text-muted); margin-left: 4px;">#${user.matricula}</span>` : ''}
          </div>
        </div>
      </div>
      <button class="sidebar__link" id="logout-btn" style="margin-top: 8px;">
        <span class="material-icons-round">logout</span>
        <span>Sair</span>
      </button>
    </div>
  `;

  container.appendChild(sidebar);

  // Nav click handlers
  sidebar.querySelector('#sidebar-nav').addEventListener('click', (e) => {
    const link = e.target.closest('.sidebar__link');
    if (link) {
      navigateTo(link.dataset.path);
    }
  });

  // Logout handler
  sidebar.querySelector('#logout-btn').addEventListener('click', () => {
    logout();
  });

  // Highlight active link
  updateActiveLink();
}

export function updateActiveLink() {
  const currentPath = getCurrentRoute();
  const links = document.querySelectorAll('.sidebar__link[data-path]');
  links.forEach(link => {
    link.classList.toggle('active', link.dataset.path === currentPath);
  });
}
