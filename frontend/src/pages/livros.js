// ========================================
// Livros Page - CRUD
// ========================================

import { getLivros, criarLivro, atualizarLivro, deletarLivro, getAutores } from '../api.js';
import { renderTable, renderLoading } from '../components/table.js';
import { openModal, showConfirm } from '../components/modal.js';
import { showToast } from '../components/toast.js';

let currentPage = 1;
const PAGE_SIZE = 10;
let searchTerm = '';
let searchTimeout = null;

export async function renderLivros() {
  const content = document.getElementById('page-content');

  content.innerHTML = `
    <div class="page-header">
      <div class="page-header__left">
        <h1 class="page-title">Livros</h1>
        <p class="page-subtitle">Gerencie o acervo da biblioteca</p>
      </div>
      <div class="page-header__actions">
        <div class="search-bar">
          <span class="material-icons-round search-bar__icon">search</span>
          <input
            type="text"
            class="search-bar__input"
            id="livros-search"
            placeholder="Buscar por título, autor..."
            value="${searchTerm}"
          />
        </div>
        <button class="btn btn-primary" id="btn-novo-livro">
          <span class="material-icons-round">add</span>
          Novo Livro
        </button>
      </div>
    </div>

    <div class="table-card">
      <div id="livros-table-container">
        <div class="loading"><div class="spinner"></div></div>
      </div>
    </div>
  `;

  // Event listeners
  document.getElementById('btn-novo-livro').addEventListener('click', () => openLivroForm());

  document.getElementById('livros-search').addEventListener('input', (e) => {
    clearTimeout(searchTimeout);
    searchTimeout = setTimeout(() => {
      searchTerm = e.target.value.trim();
      currentPage = 1;
      loadLivros();
    }, 400);
  });

  await loadLivros();

  // Return cleanup
  return () => {
    clearTimeout(searchTimeout);
    currentPage = 1;
    searchTerm = '';
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

    renderTable(container, {
      columns: [
        { key: 'titulo', label: 'Título', render: (row) => `<span style="color:var(--text-primary);font-weight:500">${row.titulo || '—'}</span>` },
        { key: 'isbn', label: 'ISBN' },
        { key: 'autor', label: 'Autor', render: (row) => row.autor?.nome || '—' },
        { key: 'anoPublicacao', label: 'Ano' },
        { key: 'quantidade', label: 'Qtd', render: (row) => {
          const qty = row.quantidade ?? 0;
          const badgeClass = qty > 0 ? 'badge--success' : 'badge--danger';
          return `<span class="badge ${badgeClass}">${qty}</span>`;
        }},
        { key: 'actions', label: 'Ações', render: (row) => `
          <div class="table-actions">
            <button class="btn btn-icon btn-secondary" data-edit="${row.id}" title="Editar">
              <span class="material-icons-round">edit</span>
            </button>
            <button class="btn btn-icon btn-danger" data-delete="${row.id}" title="Excluir">
              <span class="material-icons-round">delete</span>
            </button>
          </div>
        `},
      ],
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

    // Edit/Delete handlers
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
