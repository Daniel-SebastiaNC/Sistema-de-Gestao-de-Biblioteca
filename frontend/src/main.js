// ========================================
// Main Entry Point
// ========================================

import { registerRoute, initRouter, handleRoute } from './router.js';
import { renderLogin } from './pages/login.js';
import { renderDashboard } from './pages/dashboard.js';
import { renderLivros } from './pages/livros.js';
import { renderEmprestimos } from './pages/emprestimos.js';
import { renderAutores } from './pages/autores.js';
import { renderAlunos } from './pages/alunos.js';
import { renderSidebar, updateActiveLink } from './components/sidebar.js';

function isAuthenticated() {
  const user = localStorage.getItem('biblioteca_user');
  return !!user;
}

function initApp() {
  if (!isAuthenticated()) {
    renderLogin();
    return;
  }

  // Render app layout
  const app = document.getElementById('app');
  app.innerHTML = `
    <div class="app-layout">
      <div id="sidebar-container"></div>
      <main class="main-content" id="page-content"></main>
    </div>
  `;

  renderSidebar(document.getElementById('sidebar-container'));

  // Register routes
  registerRoute('/dashboard', async () => {
    updateActiveLink();
    return await renderDashboard();
  });

  registerRoute('/livros', async () => {
    updateActiveLink();
    return await renderLivros();
  });

  registerRoute('/autores', async () => {
    updateActiveLink();
    return await renderAutores();
  });

  registerRoute('/alunos', async () => {
    updateActiveLink();
    return await renderAlunos();
  });

  registerRoute('/emprestimos', async () => {
    updateActiveLink();
    return await renderEmprestimos();
  });

  // Start router
  initRouter();
}

// Boot
initApp();
