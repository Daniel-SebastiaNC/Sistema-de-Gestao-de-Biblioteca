// ========================================
// Sidebar Component
// ========================================

import { navigateTo, getCurrentRoute } from '../router.js';

const NAV_ITEMS = [
  { path: '/dashboard', icon: 'dashboard', label: 'Dashboard' },
  { path: '/livros', icon: 'menu_book', label: 'Livros' },
  { path: '/autores', icon: 'person', label: 'Autores' },
  { path: '/alunos', icon: 'school', label: 'Alunos' },
  { path: '/emprestimos', icon: 'swap_horiz', label: 'Empréstimos' },
];

export function renderSidebar(container) {
  const sidebar = document.createElement('aside');
  sidebar.className = 'sidebar';
  sidebar.id = 'sidebar';

  sidebar.innerHTML = `
    <div class="sidebar__brand">
      <div class="sidebar__brand-icon">
        <span class="material-icons-round">local_library</span>
      </div>
      <span class="sidebar__brand-text">Biblioteca</span>
    </div>

    <nav class="sidebar__nav" id="sidebar-nav">
      ${NAV_ITEMS.map(item => `
        <button class="sidebar__link" data-path="${item.path}">
          <span class="material-icons-round">${item.icon}</span>
          <span>${item.label}</span>
        </button>
      `).join('')}
    </nav>

    <div class="sidebar__footer">
      <div class="sidebar__user">
        <div class="sidebar__avatar">B</div>
        <div class="sidebar__user-info">
          <div class="sidebar__user-name">Bibliotecário</div>
          <div class="sidebar__user-role">Administrador</div>
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
    localStorage.removeItem('biblioteca_user');
    window.location.hash = '';
    window.location.reload();
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
