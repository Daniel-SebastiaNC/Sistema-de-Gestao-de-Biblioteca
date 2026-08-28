// ========================================
// Toast Notification Component
// ========================================

const ICONS = {
  success: 'check_circle',
  error: 'error',
  warning: 'warning',
  info: 'info',
};

export function showToast(message, type = 'info', duration = 4000) {
  const container = document.getElementById('toast-container');
  if (!container) return;

  const toast = document.createElement('div');
  toast.className = `toast toast--${type}`;
  toast.innerHTML = `
    <span class="material-icons-round toast__icon">${ICONS[type] || ICONS.info}</span>
    <span class="toast__message">${message}</span>
    <button class="toast__close material-icons-round" onclick="this.closest('.toast').remove()">close</button>
  `;

  container.appendChild(toast);

  setTimeout(() => {
    toast.classList.add('toast--removing');
    setTimeout(() => toast.remove(), 300);
  }, duration);
}
