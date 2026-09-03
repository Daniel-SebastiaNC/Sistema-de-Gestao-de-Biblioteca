// ========================================
// Main Entry Point
// ========================================

import { registerRoute, initRouter } from './router.js';
import { isAuthenticated, checkSession } from './auth.js';
import { renderLogin } from './pages/login.js';
import { renderDashboard } from './pages/dashboard.js';
import { renderLivros } from './pages/livros.js';
import { renderEmprestimos } from './pages/emprestimos.js';
import { renderAutores } from './pages/autores.js';
import { renderAlunos } from './pages/alunos.js';
import { renderAuditoria } from './pages/auditoria.js';
import { renderReservas } from './pages/reservas.js';
import { renderMeusEmprestimos, renderMinhasReservas } from './pages/alunoArea.js';
import { renderSidebar, updateActiveLink } from './components/sidebar.js';

async function initApp() {
  if (!isAuthenticated()) {
    renderLogin();
    return;
  }

  // Render app shell layout
  const app = document.getElementById('app');
  app.innerHTML = `
    <div class="app-layout">
      <div id="sidebar-container"></div>
      <main class="main-content" id="page-content"></main>
    </div>
  `;

  renderSidebar(document.getElementById('sidebar-container'));

  // 1. Dashboard (ADMIN, BIBLIOTECARIO)
  registerRoute('/dashboard', async () => {
    updateActiveLink();
    return await renderDashboard();
  }, { roles: ['ADMIN', 'BIBLIOTECARIO'], requiresAuth: true });

  // 2. Livros (ADMIN, BIBLIOTECARIO, ALUNO)
  registerRoute('/livros', async () => {
    updateActiveLink();
    return await renderLivros();
  }, { roles: ['ADMIN', 'BIBLIOTECARIO', 'ALUNO'], requiresAuth: true });

  // 3. Autores (ADMIN, BIBLIOTECARIO)
  registerRoute('/autores', async () => {
    updateActiveLink();
    return await renderAutores();
  }, { roles: ['ADMIN', 'BIBLIOTECARIO'], requiresAuth: true });

  // 4. Alunos (ADMIN, BIBLIOTECARIO)
  registerRoute('/alunos', async () => {
    updateActiveLink();
    return await renderAlunos();
  }, { roles: ['ADMIN', 'BIBLIOTECARIO'], requiresAuth: true });

  // 5. Empréstimos (ADMIN, BIBLIOTECARIO)
  registerRoute('/emprestimos', async () => {
    updateActiveLink();
    return await renderEmprestimos();
  }, { roles: ['ADMIN', 'BIBLIOTECARIO'], requiresAuth: true });

  // 6. Reservas (ADMIN, BIBLIOTECARIO)
  registerRoute('/reservas', async () => {
    updateActiveLink();
    return await renderReservas();
  }, { roles: ['ADMIN', 'BIBLIOTECARIO'], requiresAuth: true });

  // 7. Auditoria (ADMIN Only)
  registerRoute('/auditoria', async () => {
    updateActiveLink();
    return await renderAuditoria();
  }, { roles: ['ADMIN'], requiresAuth: true });

  // 7. Meus Empréstimos (ALUNO Only)
  registerRoute('/meus-emprestimos', async () => {
    updateActiveLink();
    return await renderMeusEmprestimos();
  }, { roles: ['ALUNO'], requiresAuth: true });

  // 8. Minhas Reservas (ALUNO Only)
  registerRoute('/minhas-reservas', async () => {
    updateActiveLink();
    return await renderMinhasReservas();
  }, { roles: ['ALUNO'], requiresAuth: true });

  // Start router
  initRouter();

  // Background token check
  checkSession().catch(() => {});
}

// Boot application
initApp();
