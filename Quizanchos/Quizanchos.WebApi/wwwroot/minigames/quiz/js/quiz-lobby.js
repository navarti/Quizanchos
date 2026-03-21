const QUIZ_LOBBY_ROUTE = '/Minigame/Quiz';
const CATEGORY_ITEMS_PER_PAGE = 2;

document.addEventListener('DOMContentLoaded', async () => {
    const root = document.getElementById('minigame-root');
    if (!root) return;

    const query = new URLSearchParams(window.location.search);
    const presetCategoryId = query.get('categoryId');
    const initialFilter = (query.get('filter') || 'all').toLowerCase();

    if (!presetCategoryId) {
        await renderCategorySelection(root, initialFilter);
        return;
    }

    renderGameSettings(root);
    await initializeSettingsForm(presetCategoryId);
});

async function loadCategories() {
    const response = await fetch('/QuizCategory/GetAll');
    if (!response.ok) {
        throw new Error('Failed to load quiz categories.');
    }

    return await response.json();
}

function normalizeCategory(category) {
    return {
        id: category.id || category.Id,
        name: category.name || category.Name,
        imageUrl: category.imageUrl || category.ImageUrl,
        authorName: category.authorName || category.AuthorName,
        creationDate: category.creationDate || category.CreationDate,
        questionToDisplay: category.questionToDisplay || category.QuestionToDisplay
    };
}

function slugify(value) {
    return (value || '').toLowerCase().replace(/\s+/g, '-');
}

function formatDate(value) {
    const date = new Date(value);
    if (Number.isNaN(date.getTime())) {
        return '';
    }

    return date.toLocaleDateString();
}

async function renderCategorySelection(root, initialFilter) {
    let categories;
    try {
        categories = (await loadCategories()).map(normalizeCategory);
    } catch (error) {
        console.error('Error loading categories:', error);
        root.innerHTML = '<p>Failed to load categories. Please try again later.</p>';
        return;
    }

    let currentFilter = initialFilter === 'all' ? 'all' : slugify(initialFilter);
    let currentPage = 1;
    const filters = ['all', ...new Set(categories.map(c => slugify(c.name)))];

    const render = () => {
        const filtered = categories.filter(c => currentFilter === 'all' || slugify(c.name) === currentFilter);
        const totalPages = Math.max(1, Math.ceil(filtered.length / CATEGORY_ITEMS_PER_PAGE));
        currentPage = Math.min(currentPage, totalPages);

        const start = (currentPage - 1) * CATEGORY_ITEMS_PER_PAGE;
        const pageItems = filtered.slice(start, start + CATEGORY_ITEMS_PER_PAGE);

        root.innerHTML = `
<div class="container quiz-categories-page">
    <div class="filters">
        ${filters.map(filter => `
            <button class="filter-btn ${filter === currentFilter ? 'active' : ''}" data-filter="${filter}">
                ${filter === 'all' ? 'All' : categories.find(c => slugify(c.name) === filter)?.name || filter}
            </button>
        `).join('')}
    </div>
    <div id="categories">
        ${pageItems.map(category => `
            <div class="category" data-category="${slugify(category.name)}" style="background-image: url('${category.imageUrl}');">
                <div class="category-content">
                    <div class="category-header">
                        <h2 class="category-title">${category.name} Quiz</h2>
                    </div>
                    <div class="meta">
                        <span>${formatDate(category.creationDate)}</span>
                        <span>${category.name}</span>
                        <span>${category.authorName || ''}</span>
                    </div>
                    <p class="description">${category.questionToDisplay || 'Challenge yourself with category-based questions.'}</p>
                    <div class="actions">
                        <a href="${QUIZ_LOBBY_ROUTE}?categoryId=${category.id}" class="quiz-categories-btn quiz-categories-btn-primary">
                            GO
                            <span>→</span>
                        </a>
                        <a href="/Leaderboard" class="quiz-categories-btn quiz-categories-btn-secondary">
                            Leaderboard
                            <span>📊</span>
                        </a>
                    </div>
                </div>
            </div>
        `).join('')}
    </div>
    <div class="pagination">
        <button class="page-btn" id="prevPage" ${currentPage === 1 ? 'disabled' : ''}>Previous</button>
        <span class="page-info">Page ${currentPage} of ${totalPages}</span>
        <button class="page-btn" id="nextPage" ${currentPage === totalPages ? 'disabled' : ''}>Next</button>
    </div>
</div>`;
    };

    render();

    root.addEventListener('click', event => {
        const filterButton = event.target.closest('.filter-btn');
        if (filterButton) {
            currentFilter = filterButton.dataset.filter;
            currentPage = 1;
            render();
            return;
        }

        if (event.target.closest('#prevPage')) {
            currentPage = Math.max(1, currentPage - 1);
            render();
            return;
        }

        if (event.target.closest('#nextPage')) {
            const filteredCount = categories.filter(c => currentFilter === 'all' || slugify(c.name) === currentFilter).length;
            const totalPages = Math.max(1, Math.ceil(filteredCount / CATEGORY_ITEMS_PER_PAGE));
            currentPage = Math.min(totalPages, currentPage + 1);
            render();
        }
    });
}

function renderGameSettings(root) {
    root.innerHTML = `
<div class="container game-session-container">
    <div class="header">
        <h1 class="game-title" id="categoryName">Choose category</h1>
        <p class="game-subtitle">Customize your quest below!</p>
    </div>

    <form id="gameSettingsForm" class="game-settings-form">
        <div class="form-group">
            <label for="categoryId">Category:</label>
            <select id="categoryId" name="categoryId" class="form-control">
                <option value="">Loading...</option>
            </select>
        </div>

        <div class="form-group">
            <label for="gameLevel">Game Level:</label>
            <select id="gameLevel" name="gameLevel" class="form-control">
                <option value="0">Easy</option>
                <option value="1">Medium</option>
                <option value="2">Hard</option>
            </select>
        </div>

        <div class="form-group">
            <label for="cardsCount">Number of Cards:</label>
            <select id="cardsCount" name="cardsCount" class="form-control">
                <option value="5">Five</option>
                <option value="10" selected>Ten</option>
                <option value="15">Fifteen</option>
            </select>
        </div>

        <div class="form-group">
            <label for="secondsPerCard">Time per Card (seconds):</label>
            <select id="secondsPerCard" name="secondsPerCard" class="form-control">
                <option value="10">Ten</option>
                <option value="15" selected>Fifteen</option>
                <option value="20">Twenty</option>
            </select>
        </div>

        <div class="form-group">
            <label for="optionCount">Number of Options per Card:</label>
            <select id="optionCount" name="optionCount" class="form-control">
                <option value="2">Two</option>
                <option value="3">Three</option>
                <option value="4" selected>Four</option>
            </select>
        </div>

        <div class="action-buttons">
            <button type="button" id="startQuestButton" class="btn-start-quiz">Start Quest</button>
            <a href="${QUIZ_LOBBY_ROUTE}" class="btn-back-to-quizzes">Back to Categories</a>
        </div>
    </form>
</div>`;
}

async function initializeSettingsForm(presetCategoryId) {
    try {
        const categories = (await loadCategories()).map(normalizeCategory);
        const categorySelect = document.getElementById('categoryId');
        categorySelect.innerHTML = '';

        categories.forEach(category => {
            const option = document.createElement('option');
            option.value = category.id;
            option.textContent = category.name;
            if (presetCategoryId && option.value.toLowerCase() === presetCategoryId.toLowerCase()) {
                option.selected = true;
            }
            categorySelect.appendChild(option);
        });

        updateCategoryTitle(categorySelect);

        categorySelect.addEventListener('change', () => {
            updateCategoryTitle(categorySelect);
        });
    } catch (error) {
        console.error('Error loading categories:', error);
    }

    document.getElementById('startQuestButton')?.addEventListener('click', async () => {
        const userId = document.body.getAttribute('data-user-id');
        if (!userId) {
            alert('User information is missing. Please log in again.');
            return;
        }

        const categoryId = document.getElementById('categoryId').value;
        if (!categoryId) {
            alert('Please choose category.');
            return;
        }

        const gameLevel = parseInt(document.getElementById('gameLevel').value, 10);
        const cardsCount = parseInt(document.getElementById('cardsCount').value, 10);
        const secondsPerCard = parseInt(document.getElementById('secondsPerCard').value, 10);
        const optionCount = parseInt(document.getElementById('optionCount').value, 10);

        try {
            const gameResponse = await quizClient.createQuizGame(
                userId,
                cardsCount,
                {
                    gameLevel,
                    secondsPerCard,
                    optionCount,
                    categoryId
                }
            );

            if (gameResponse?.gameId) {
                window.location.href = window.minigameConfig.gameUrlTemplate.replace('{gameId}', gameResponse.gameId);
            } else {
                alert('Unexpected response format. Please try again.');
            }
        } catch (error) {
            console.error('Error:', error);
            alert(error.message || 'An error occurred. Please try again later.');
        }
    });
}

function updateCategoryTitle(categorySelect) {
    const selected = categorySelect.options[categorySelect.selectedIndex];
    if (!selected) {
        return;
    }

    const title = document.getElementById('categoryName');
    if (title) {
        title.textContent = `${selected.textContent} Quiz`;
    }
}
