// ========================================
// Livros Page - CRUD
// ========================================

import { getLivros, criarLivro, atualizarLivro, deletarLivro, getAutores, criarReserva, getFilaReserva } from '../api.js';
import { canManageAcervo, isAluno } from '../auth.js';
import { renderTable, renderLoading } from '../components/table.js';
import { openModal, showConfirm } from '../components/modal.js';
import { showToast } from '../components/toast.js';

let currentPage = 1;
const PAGE_SIZE = 10;
let searchTerm = '';
let searchTimeout = null;

export async function renderLivros() {
  const content = document.getElementById('page-content');
  const canManage = canManageAcervo();

  content.innerHTML = `
    <div class="page-header">
      <div class="page-header__left">
        <h1 class="page-title">Livros</h1>
        <p class="page-subtitle">Gerencie o acervo da biblioteca</p>
      </div>
      <div class="page-header__actions">
        ${canManage ? `
          <button class="btn btn-primary" id="btn-novo-livro">
            <span class="material-icons-round">add</span>
            Novo Livro
          </button>
        ` : ''}
      </div>
    </div>

    <div class="table-card">
      <div class="table-card__header">
        <div class="search-box">
          <span class="material-icons-round search-box__icon">search</span>
          <input
            type="text"
            id="search-livros"
            class="search-box__input"
            placeholder="Buscar por título, ISBN ou autor..."
            value="${searchTerm}"
          />
        </div>
      </div>
      <div id="livros-table-container">
        <div class="loading"><div class="spinner"></div></div>
      </div>
    </div>
  `;

  // Search with debounce
  const searchInput = document.getElementById('search-livros');
  searchInput.addEventListener('input', (e) => {
    clearTimeout(searchTimeout);
    searchTimeout = setTimeout(() => {
      searchTerm = e.target.value.trim();
      currentPage = 1;
      loadLivros();
    }, 350);
  });

  if (canManage) {
    document.getElementById('btn-novo-livro')?.addEventListener('click', () => openLivroForm());
  }

  await loadLivros();

  return () => {
    searchTerm = '';
    currentPage = 1;
  };
}

async function loadLivros() {
  const container = document.getElementById('livros-table-container');
  if (!container) return;

  renderLoading(container);

  try {
    const result = await getLivros({
      pageNumber: currentPage,
      pageSize: PAGE_SIZE,
      termo: searchTerm || undefined,
    });

    const items = result.items || [];

    const columns = [
      { key: 'titulo', label: 'Título', render: (row) => `<span style="color:var(--text-primary);font-weight:500">${row.titulo || '—'}</span>` },
      { key: 'isbn', label: 'ISBN' },
      { key: 'autor', label: 'Autor', render: (row) => row.autor?.nome || '—' },
      { key: 'anoPublicacao', label: 'Ano' },
      { key: 'quantidade', label: 'Qtd', render: (row) => {
        const qty = row.quantidade ?? 0;
        const badgeClass = qty > 0 ? 'badge--success' : 'badge--danger';
        return `<span class="badge ${badgeClass}">${qty}</span>`;
      }},
    ];

    const canManage = canManageAcervo();
    const isAlunoUser = isAluno();

    if (canManage) {
      columns.push({
        key: 'actions',
        label: 'Ações',
        render: (row) => `
          <div class="table-actions">
            ${(row.quantidade ?? 0) === 0 ? `
              <button class="btn btn-icon btn-secondary" data-queue="${row.id}" title="Ver fila de espera">
                <span class="material-icons-round">bookmarks</span>
              </button>
            ` : ''}
            <button class="btn btn-icon btn-secondary" data-edit="${row.id}" title="Editar">
              <span class="material-icons-round">edit</span>
            </button>
            <button class="btn btn-icon btn-danger" data-delete="${row.id}" title="Excluir">
              <span class="material-icons-round">delete</span>
            </button>
          </div>
        `,
      });
    } else if (isAlunoUser) {
      columns.push({
        key: 'actions',
        label: 'Ações',
        render: (row) => {
          const qty = row.quantidade ?? 0;
          if (qty === 0) {
            return `
              <button class="btn btn-sm btn-secondary" data-reserve="${row.id}" title="Entrar na fila de espera">
                <span class="material-icons-round" style="font-size: 1rem;">bookmark_add</span>
                Reservar
              </button>
            `;
          }
          return `<span class="badge badge--success">Disponível</span>`;
        },
      });
    }

    renderTable(container, {
      columns,
      data: items,
      pagination: {
        pageNumber: result.pageNumber,
        pageSize: result.pageSize,
        totalItems: result.totalItems,
        totalPages: result.totalPages,
        hasPreviousPage: result.hasPreviousPage,
        hasNextPage: result.hasNextPage,
      },
      onPageChange: (page) => {
        currentPage = page;
        loadLivros();
      },
      emptyIcon: 'menu_book',
      emptyText: searchTerm ? 'Nenhum livro encontrado para a busca' : 'Nenhum livro cadastrado',
    });

    // Action handlers for Admin & Bibliotecario
    if (canManage) {
      container.querySelectorAll('[data-edit]').forEach(btn => {
        btn.addEventListener('click', () => {
          const livro = items.find(l => l.id === btn.dataset.edit);
          if (livro) openLivroForm(livro);
        });
      });

      container.querySelectorAll('[data-delete]').forEach(btn => {
        btn.addEventListener('click', async () => {
          const livro = items.find(l => l.id === btn.dataset.delete);
          if (!livro) return;

          const confirmed = await showConfirm({
            title: 'Excluir Livro',
            message: `Tem certeza que deseja excluir "${livro.titulo}"? Esta ação não pode ser desfeita.`,
            confirmText: 'Excluir',
            type: 'danger',
          });

          if (confirmed) {
            try {
              await deletarLivro(livro.id);
              showToast('Livro excluído com sucesso!', 'success');
              loadLivros();
            } catch (err) {
              showToast('Erro ao excluir: ' + err.message, 'error');
            }
          }
        });
      });

      // Queue handler for Bibliotecario and Admin
      container.querySelectorAll('[data-queue]').forEach(btn => {
        btn.addEventListener('click', async () => {
          const livro = items.find(l => l.id === btn.dataset.queue);
          if (!livro) return;
          try {
            const fila = await getFilaReserva(livro.id);
            const contentEl = document.createElement('div');
            if (!fila || fila.length === 0) {
              contentEl.innerHTML = '<p style="color:var(--text-secondary);padding:16px 0;">Nenhum aluno na fila de espera para este livro.</p>';
            } else {
              contentEl.innerHTML = `
                <p style="margin-bottom:12px;color:var(--text-secondary);font-size:0.9rem;">Fila de espera para <strong>${livro.titulo}</strong>:</p>
                <div style="display:flex;flex-direction:column;gap:8px;">
                  ${fila.map((r, i) => `
                    <div style="display:flex;justify-content:space-between;align-items:center;padding:10px 14px;background:var(--bg-glass);border-radius:8px;border:1px solid var(--border-color);">
                      <div>
                        <strong>${i + 1}º - ${r.alunoNome || 'Aluno'}</strong>
                        <span style="font-size:0.8rem;color:var(--text-muted);display:block;">Data: ${new Date(r.dataReserva).toLocaleDateString('pt-BR')}</span>
                      </div>
                      <span class="badge badge--warning">Posição ${r.posicaoFila || i + 1}</span>
                    </div>
                  `).join('')}
                </div>
              `;
            }
            openModal({
              title: `Fila de Espera - ${livro.titulo}`,
              content: contentEl,
            });
          } catch (err) {
            showToast('Erro ao carregar fila de espera: ' + err.message, 'error');
          }
        });
      });
    }

    // Reservation handler for Aluno
    if (isAlunoUser) {
      container.querySelectorAll('[data-reserve]').forEach(btn => {
        btn.addEventListener('click', async () => {
          const livro = items.find(l => l.id === btn.dataset.reserve);
          if (!livro) return;

          const confirmed = await showConfirm({
            title: 'Reservar Livro',
            message: `Deseja entrar na fila de espera prioritária para "${livro.titulo}"?`,
            confirmText: 'Confirmar Reserva',
            type: 'primary',
          });

          if (confirmed) {
            try {
              await criarReserva({ livroId: livro.id });
              showToast('Reserva confirmada! Acompanhe em Minhas Reservas.', 'success');
              loadLivros();
            } catch (err) {
              showToast(err.message || 'Erro ao realizar reserva.', 'error');
            }
          }
        });
      });
    }
  } catch (err) {
    container.innerHTML = `
      <div class="empty-state">
        <span class="material-icons-round empty-state__icon">error_outline</span>
        <p class="empty-state__title">Erro ao carregar livros</p>
        <p class="empty-state__text">${err.message}</p>
      </div>
    `;
    showToast('Erro ao carregar livros: ' + err.message, 'error');
  }
}

async function openLivroForm(livro = null) {
  const isEdit = !!livro;

  // Load authors for the select
  let autores = [];
  try {
    const result = await getAutores({ pageNumber: 1, pageSize: 200 });
    autores = result.items || [];
  } catch {
    showToast('Erro ao carregar autores', 'warning');
  }

  const formContent = document.createElement('div');
  formContent.innerHTML = `
    <form id="livro-form">
      <div class="form-group">
        <label class="form-label" for="livro-titulo">Título</label>
        <input type="text" id="livro-titulo" class="form-input" placeholder="Nome do livro" value="${livro?.titulo || ''}" required />
      </div>
      <div class="form-group">
        <label class="form-label" for="livro-isbn">ISBN</label>
        <input type="text" id="livro-isbn" class="form-input" placeholder="978-3-16-148410-0" value="${livro?.isbn || ''}" required />
      </div>
      <div class="form-group">
        <label class="form-label" for="livro-ano">Ano de Publicação</label>
        <input type="number" id="livro-ano" class="form-input" placeholder="2024" value="${livro?.anoPublicacao || ''}" required />
      </div>
      <div class="form-group">
        <label class="form-label" for="livro-autor">Autor</label>
        <select id="livro-autor" class="form-select" required>
          <option value="">Selecione um autor</option>
          ${autores.map(a => `
            <option value="${a.id}" ${livro?.autor?.id === a.id ? 'selected' : ''}>${a.nome}</option>
          `).join('')}
        </select>
      </div>
      <div class="form-group">
        <label class="form-label" for="livro-qtd">Quantidade</label>
        <input type="number" id="livro-qtd" class="form-input" placeholder="1" min="0" value="${livro?.quantidade ?? ''}" required />
      </div>
    </form>
  `;

  const footerEl = document.createElement('div');
  footerEl.style.display = 'contents';
  footerEl.innerHTML = `
    <button class="btn btn-secondary" id="modal-cancel">Cancelar</button>
    <button class="btn btn-primary" id="modal-save">
      <span class="material-icons-round">${isEdit ? 'save' : 'add'}</span>
      ${isEdit ? 'Salvar' : 'Cadastrar'}
    </button>
  `;

  openModal({
    title: isEdit ? 'Editar Livro' : 'Novo Livro',
    content: formContent,
    footer: footerEl,
  });

  // Cancel
  document.getElementById('modal-cancel')?.addEventListener('click', () => {
    document.getElementById('active-modal')?.remove();
  });

  // Save
  document.getElementById('modal-save')?.addEventListener('click', async () => {
    const titulo = document.getElementById('livro-titulo').value.trim();
    const isbn = document.getElementById('livro-isbn').value.trim();
    const anoPublicacao = parseInt(document.getElementById('livro-ano').value);
    const autorId = document.getElementById('livro-autor').value;
    const quantidade = parseInt(document.getElementById('livro-qtd').value);

    if (!titulo || !isbn || !anoPublicacao || !autorId || isNaN(quantidade)) {
      showToast('Preencha todos os campos obrigatórios', 'warning');
      return;
    }

    const saveBtn = document.getElementById('modal-save');
    saveBtn.disabled = true;
    saveBtn.innerHTML = '<div class="spinner" style="width:18px;height:18px;border-width:2px"></div> Salvando...';

    try {
      if (isEdit) {
        await atualizarLivro(livro.id, { titulo, isbn, anoPublicacao, quantidade, quantidadeDisponivel: quantidade, autorId });
        showToast('Livro atualizado com sucesso!', 'success');
      } else {
        await criarLivro({ titulo, isbn, anoPublicacao, autorId, quantidade, quantidadeDisponivel: quantidade });
        showToast('Livro cadastrado com sucesso!', 'success');
      }
      document.getElementById('active-modal')?.remove();
      loadLivros();
    } catch (err) {
      showToast('Erro: ' + err.message, 'error');
      saveBtn.disabled = false;
      saveBtn.innerHTML = `<span class="material-icons-round">${isEdit ? 'save' : 'add'}</span> ${isEdit ? 'Salvar' : 'Cadastrar'}`;
    }
  });
}
