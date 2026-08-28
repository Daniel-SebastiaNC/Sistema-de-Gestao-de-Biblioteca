// ========================================
// Login Page
// ========================================

import { showToast } from '../components/toast.js';

export function renderLogin() {
  const app = document.getElementById('app');

  app.innerHTML = `
    <div class="login-page">
      <div class="login-card">
        <div class="login-card__icon">
          <span class="material-icons-round">local_library</span>
        </div>
        <h1 class="login-card__title">Biblioteca</h1>
        <p class="login-card__subtitle">Sistema de Gerenciamento</p>

        <form id="login-form">
          <div class="form-group">
            <label class="form-label" for="login-email">Email</label>
            <input
              type="email"
              id="login-email"
              class="form-input"
              placeholder="seu@email.com"
              required
              autocomplete="email"
            />
          </div>

          <div class="form-group">
            <label class="form-label" for="login-password">Senha</label>
            <input
              type="password"
              id="login-password"
              class="form-input"
              placeholder="••••••••"
              required
              autocomplete="current-password"
            />
          </div>

          <button type="submit" class="btn btn-primary btn-full" id="login-submit" style="margin-top: 12px;">
            <span class="material-icons-round">login</span>
            Entrar
          </button>
        </form>

        <p style="text-align: center; margin-top: 20px; font-size: 0.8rem; color: var(--text-muted);">
          Insira qualquer email e senha para acessar
        </p>
      </div>
    </div>
  `;

  // Form submit
  const form = document.getElementById('login-form');
  form.addEventListener('submit', (e) => {
    e.preventDefault();

    const email = document.getElementById('login-email').value.trim();
    const password = document.getElementById('login-password').value;

    if (!email || !password) {
      showToast('Preencha todos os campos', 'warning');
      return;
    }

    // Simulate login
    const user = {
      email,
      name: email.split('@')[0],
      loggedAt: new Date().toISOString(),
    };

    localStorage.setItem('biblioteca_user', JSON.stringify(user));
    showToast('Login realizado com sucesso!', 'success');

    // Reload to trigger auth guard
    setTimeout(() => {
      window.location.hash = '/dashboard';
      window.location.reload();
    }, 300);
  });
}
