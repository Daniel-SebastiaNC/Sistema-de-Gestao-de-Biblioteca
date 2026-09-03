// ========================================
// Login Page
// ========================================

import { showToast } from '../components/toast.js';
import { login, getDefaultRoute } from '../auth.js';

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

        <div class="login-dev-credentials" style="margin-top: 20px; padding: 12px; background: rgba(0,0,0,0.03); border: 1px solid var(--border, #e2e8f0); border-radius: 8px; font-size: 0.75rem;">
          <p style="font-weight: 600; margin-bottom: 8px; color: var(--text-muted, #64748b); text-align: center;">Usuários Padrão (Ambiente de Desenvolvimento):</p>
          <div style="display: flex; flex-direction: column; gap: 6px;">
            <button type="button" class="btn-demo-user" data-email="admin@smartlib.com" data-pass="Admin@123" style="text-align: left; background: #fff; border: 1px dashed #cbd5e1; padding: 6px 8px; border-radius: 4px; cursor: pointer; display: flex; justify-content: space-between; font-size: 0.75rem;">
              <span><strong>ADMIN:</strong> admin@smartlib.com</span>
              <code>Admin@123</code>
            </button>
            <button type="button" class="btn-demo-user" data-email="biblio@smartlib.com" data-pass="Biblio@123" style="text-align: left; background: #fff; border: 1px dashed #cbd5e1; padding: 6px 8px; border-radius: 4px; cursor: pointer; display: flex; justify-content: space-between; font-size: 0.75rem;">
              <span><strong>BIBLIOTECÁRIO:</strong> biblio@smartlib.com</span>
              <code>Biblio@123</code>
            </button>
            <button type="button" class="btn-demo-user" data-email="aluno@smartlib.com" data-pass="Aluno@123" style="text-align: left; background: #fff; border: 1px dashed #cbd5e1; padding: 6px 8px; border-radius: 4px; cursor: pointer; display: flex; justify-content: space-between; font-size: 0.75rem;">
              <span><strong>ALUNO:</strong> aluno@smartlib.com</span>
              <code>Aluno@123</code>
            </button>
          </div>
        </div>
      </div>
    </div>
  `;

  // Demo user autofill
  document.querySelectorAll('.btn-demo-user').forEach(btn => {
    btn.addEventListener('click', () => {
      document.getElementById('login-email').value = btn.dataset.email;
      document.getElementById('login-password').value = btn.dataset.pass;
    });
  });

  // Form submit
  const form = document.getElementById('login-form');
  form.addEventListener('submit', async (e) => {
    e.preventDefault();

    const email = document.getElementById('login-email').value.trim();
    const password = document.getElementById('login-password').value;

    if (!email || !password) {
      showToast('Preencha todos os campos', 'warning');
      return;
    }

    const submitBtn = document.getElementById('login-submit');
    const originalBtnHtml = submitBtn.innerHTML;

    try {
      submitBtn.disabled = true;
      submitBtn.innerHTML = `
        <span class="spinner" style="width: 16px; height: 16px; border-width: 2px; margin-right: 6px;"></span>
        Autenticando...
      `;

      const data = await login(email, password);
      const user = data.usuario || {};

      showToast(`Bem-vindo(a), ${user.nome || 'ao SmartLib'}!`, 'success');

      setTimeout(() => {
        const defaultRoute = getDefaultRoute();
        window.location.hash = defaultRoute;
        window.location.reload();
      }, 350);
    } catch (err) {
      showToast(err.message || 'Credenciais inválidas: e-mail ou senha incorretos.', 'error');
      submitBtn.disabled = false;
      submitBtn.innerHTML = originalBtnHtml;
    }
  });
}
