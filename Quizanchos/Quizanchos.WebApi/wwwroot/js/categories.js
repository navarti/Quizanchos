let currentPage = 1;
const itemsPerPage = 2;
let currentFilter = 'all';

function renderCategories() {
    const categories = document.querySelectorAll('#categories .category');

    let visibleCategories = Array.from(categories).filter(category => {
        const type = category.getAttribute('data-category');
        return currentFilter === 'all' || type === currentFilter;
    });

    categories.forEach(cat => cat.style.display = 'none');

    const start = (currentPage - 1) * itemsPerPage;
    const end = start + itemsPerPage;
    visibleCategories.slice(start, end).forEach(cat => {
        cat.style.display = 'block';
    });

    updatePagination(visibleCategories.length);
}

function updatePagination(totalItems) {
    const totalPages = Math.ceil(totalItems / itemsPerPage);
    const pageInfo = document.getElementById('pageInfo');
    pageInfo.textContent = `Page ${currentPage} of ${totalPages}`;

    document.getElementById('prevPage').disabled = currentPage === 1;
    document.getElementById('nextPage').disabled = currentPage === totalPages;
}

function filterCategories(filter) {
    currentFilter = filter;
    currentPage = 1;
    renderCategories();

    document.querySelectorAll('.filter-btn').forEach(btn => {
        btn.classList.remove('active');
        if (btn.dataset.category === filter) {
            btn.classList.add('active');
        }
    });
}

function changePage(delta) {
    const categories = document.querySelectorAll('#categories .category');
    const visibleCategories = Array.from(categories).filter(cat => {
        const type = cat.getAttribute('data-category');
        return currentFilter === 'all' || type === currentFilter;
    });

    const totalPages = Math.ceil(visibleCategories.length / itemsPerPage);
    currentPage += delta;
    currentPage = Math.max(1, Math.min(currentPage, totalPages));
    renderCategories();
}

function startQuiz(categoryId) {
    console.log(`Starting quiz: ${categoryId}`);
}

document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('.filter-btn').forEach(btn => {
        btn.addEventListener('click', () => filterCategories(btn.dataset.category));
    });

    document.getElementById('prevPage').addEventListener('click', () => {
        changePage(-1);
    });
    document.getElementById('nextPage').addEventListener('click', () => {
        changePage(1);
    });

    const urlParams = new URLSearchParams(window.location.search);
    const filter = urlParams.get('filter');

    if (filter) {
        filterCategories(filter);
    } else {
        renderCategories(); 
    }
});

