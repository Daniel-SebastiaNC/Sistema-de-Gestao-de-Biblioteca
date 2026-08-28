// ========================================
// Table Component with Pagination
// ========================================

/**
 * Renders a data table with pagination.
 * @param {HTMLElement} container
 * @param {Object} options
 * @param {string} options.title
 * @param {Array<{key: string, label: string, render?: Function}>} options.columns
 * @param {Array} options.data
 * @param {Object} options.pagination - { pageNumber, pageSize, totalItems, totalPages, hasPreviousPage, hasNextPage }
 * @param {Function} options.onPageChange - callback(pageNumber)
 * @param {string} options.emptyIcon
 * @param {string} options.emptyText
 */
export function renderTable(container, options) {
  const {
    columns = [],
    data = [],
    pagination = null,
    emptyIcon = 'inbox',
    emptyText = 'Nenhum registro encontrado',
  } = options;

  // Table
  if (data.length === 0) {
    container.innerHTML = `
      <div class="empty-state">
        <span class="material-icons-round empty-state__icon">${emptyIcon}</span>
        <p class="empty-state__title">${emptyText}</p>
      </div>
    `;
    return;
  }

  let html = `
    <div class="table-wrapper">
      <table class="data-table">
        <thead>
          <tr>
            ${columns.map(col => `<th>${col.label}</th>`).join('')}
          </tr>
        </thead>
        <tbody>
          ${data.map((row, index) => `
            <tr>
              ${columns.map(col => {
                const value = col.render ? col.render(row, index) : (row[col.key] ?? '—');
                return `<td>${value}</td>`;
              }).join('')}
            </tr>
          `).join('')}
        </tbody>
      </table>
    </div>
  `;

  // Pagination
  if (pagination && pagination.totalPages > 1) {
    const { pageNumber, totalItems, totalPages, hasPreviousPage, hasNextPage, pageSize } = pagination;
    const start = ((pageNumber - 1) * pageSize) + 1;
    const end = Math.min(pageNumber * pageSize, totalItems);

    html += `
      <div class="pagination">
        <span class="pagination__info">
          Mostrando ${start}-${end} de ${totalItems} registros
        </span>
        <div class="pagination__controls">
          <button class="pagination__btn" data-page="${pageNumber - 1}" ${!hasPreviousPage ? 'disabled' : ''}>
            <span class="material-icons-round" style="font-size:18px">chevron_left</span>
          </button>
          ${generatePageButtons(pageNumber, totalPages)}
          <button class="pagination__btn" data-page="${pageNumber + 1}" ${!hasNextPage ? 'disabled' : ''}>
            <span class="material-icons-round" style="font-size:18px">chevron_right</span>
          </button>
        </div>
      </div>
    `;
  }

  container.innerHTML = html;

  // Pagination click handlers
  if (options.onPageChange) {
    container.querySelectorAll('.pagination__btn[data-page]').forEach(btn => {
      btn.addEventListener('click', () => {
        const page = parseInt(btn.dataset.page);
        if (!isNaN(page) && !btn.disabled) {
          options.onPageChange(page);
        }
      });
    });
  }
}

function generatePageButtons(current, total) {
  const pages = [];
  const maxVisible = 5;

  let start = Math.max(1, current - Math.floor(maxVisible / 2));
  let end = Math.min(total, start + maxVisible - 1);

  if (end - start + 1 < maxVisible) {
    start = Math.max(1, end - maxVisible + 1);
  }

  if (start > 1) {
    pages.push(`<button class="pagination__btn" data-page="1">1</button>`);
    if (start > 2) {
      pages.push(`<span class="pagination__btn" style="border:none;cursor:default;color:var(--text-muted)">...</span>`);
    }
  }

  for (let i = start; i <= end; i++) {
    pages.push(`<button class="pagination__btn ${i === current ? 'active' : ''}" data-page="${i}">${i}</button>`);
  }

  if (end < total) {
    if (end < total - 1) {
      pages.push(`<span class="pagination__btn" style="border:none;cursor:default;color:var(--text-muted)">...</span>`);
    }
    pages.push(`<button class="pagination__btn" data-page="${total}">${total}</button>`);
  }

  return pages.join('');
}

export function renderLoading(container) {
  container.innerHTML = `
    <div class="loading">
      <div class="spinner"></div>
    </div>
  `;
}
