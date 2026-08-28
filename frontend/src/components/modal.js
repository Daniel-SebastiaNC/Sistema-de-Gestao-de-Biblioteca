// ========================================
// Modal Component
// ========================================

export function openModal({ title, content, footer, onClose }) {
  // Remove existing modals
  closeModal();

  const overlay = document.createElement('div');
  overlay.className = 'modal-overlay';
  overlay.id = 'active-modal';

  overlay.innerHTML = `
    <div class="modal">
      <div class="modal__header">
        <h2 class="modal__title">${title}</h2>
        <button class="modal__close material-icons-round" id="modal-close-btn">close</button>
      </div>
      <div class="modal__body" id="modal-body"></div>
      ${footer ? '<div class="modal__footer" id="modal-footer"></div>' : ''}
    </div>
  `;

  document.body.appendChild(overlay);

  // Mount content
  const bodyEl = overlay.querySelector('#modal-body');
  if (typeof content === 'string') {
    bodyEl.innerHTML = content;
  } else if (content instanceof HTMLElement) {
    bodyEl.appendChild(content);
  }

  // Mount footer
  if (footer) {
    const footerEl = overlay.querySelector('#modal-footer');
    if (typeof footer === 'string') {
      footerEl.innerHTML = footer;
    } else if (footer instanceof HTMLElement) {
      footerEl.appendChild(footer);
    }
  }

  // Close handlers
  const handleClose = () => {
    closeModal();
    if (onClose) onClose();
  };

  overlay.querySelector('#modal-close-btn').addEventListener('click', handleClose);
  overlay.addEventListener('click', (e) => {
    if (e.target === overlay) handleClose();
  });

  // ESC key
  const escHandler = (e) => {
    if (e.key === 'Escape') {
      handleClose();
      document.removeEventListener('keydown', escHandler);
    }
  };
  document.addEventListener('keydown', escHandler);

  return overlay;
}

export function closeModal() {
  const existing = document.getElementById('active-modal');
  if (existing) existing.remove();
}

export function showConfirm({ title, message, confirmText = 'Confirmar', cancelText = 'Cancelar', type = 'danger' }) {
  return new Promise((resolve) => {
    const overlay = document.createElement('div');
    overlay.className = 'confirm-overlay';

    const iconMap = {
      danger: 'delete_forever',
      warning: 'warning',
      info: 'help',
    };

    overlay.innerHTML = `
      <div class="confirm-dialog">
        <div class="confirm-dialog__icon confirm-dialog__icon--${type}">
          <span class="material-icons-round">${iconMap[type] || iconMap.danger}</span>
        </div>
        <h3 class="confirm-dialog__title">${title}</h3>
        <p class="confirm-dialog__message">${message}</p>
        <div class="confirm-dialog__actions">
          <button class="btn btn-secondary" id="confirm-cancel">${cancelText}</button>
          <button class="btn btn-${type}" id="confirm-ok">${confirmText}</button>
        </div>
      </div>
    `;

    document.body.appendChild(overlay);

    overlay.querySelector('#confirm-cancel').addEventListener('click', () => {
      overlay.remove();
      resolve(false);
    });

    overlay.querySelector('#confirm-ok').addEventListener('click', () => {
      overlay.remove();
      resolve(true);
    });

    overlay.addEventListener('click', (e) => {
      if (e.target === overlay) {
        overlay.remove();
        resolve(false);
      }
    });
  });
}
