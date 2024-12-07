let currentPage = 1;
const itemsPerPage = 2;
let currentFilter = 'all';

function renderCategories() {
    const categories = document.querySelectorAll('#categories .category');

    // Apply filter and pagination logic
    let visibleCategories = Array.from(categories).filter(category => {
        const type = category.getAttribute('data-category');
        const isVisible = currentFilter === 'all' || type === currentFilter;
        category.style.display = isVisible ? 'block' : 'none';
        return isVisible;
    });

    // Apply pagination
    const start = (currentPage - 1) * itemsPerPage;
    const end = start + itemsPerPage;
    visibleCategories.forEach((cat, index) => {
        cat.style.display = index >= start && index < end ? 'block' : 'none';
    });

    updatePagination(visibleCategories.length);
}

function updatePagination(totalItems) {
    const totalPages = Math.ceil(totalItems / itemsPerPage);
    const pageInfo = document.getElementById('pageInfo');
    pageInfo.textContent = `Page ${currentPage} of ${totalPages}`;

    // Disable or enable pagination buttons
    document.getElementById('prevPage').disabled = currentPage === 1;
    document.getElementById('nextPage').disabled = currentPage === totalPages;
}

function filterCategories(filter) {
    currentFilter = filter; // Update filter
    currentPage = 1; // Reset to first page
    renderCategories();

    // Update active filter button
    document.querySelectorAll('.filter-btn').forEach(btn => {
        btn.classList.remove('active');
        if (btn.dataset.category === filter) {
            btn.classList.add('active');
        }
    });
}

function changePage(delta) {
    const categories = document.querySelectorAll('#categories .category');
    const visibleCategories = Array.from(categories).filter(cat => cat.style.display !== 'none');

    const totalPages = Math.ceil(visibleCategories.length / itemsPerPage);
    currentPage = Math.max(1, Math.min(currentPage + delta, totalPages));
    renderCategories();
}

function startQuiz(categoryId) {
    console.log(`Starting quiz: ${categoryId}`);
}

// Attach event listeners to filter buttons
document.querySelectorAll('.filter-btn').forEach(btn => {
    btn.addEventListener('click', () => filterCategories(btn.dataset.category));
});

// Attach event listeners to pagination buttons
document.getElementById('prevPage').addEventListener('click', () => changePage(-1));
document.getElementById('nextPage').addEventListener('click', () => changePage(1));

// Initial render
renderCategories();

document.addEventListener('DOMContentLoaded', () => {
    const urlParams = new URLSearchParams(window.location.search);
    const filter = urlParams.get('filter');

    if (filter) {
        // Apply the filter
        filterCategories(filter);

        // Highlight the active filter button
        document.querySelectorAll('.filter-btn').forEach(btn => {
            btn.classList.remove('active');
            if (btn.dataset.category === filter) {
                btn.classList.add('active');
            }
        });
    }
});