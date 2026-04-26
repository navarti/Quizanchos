// =============================================================================
// Admin dashboard — users, categories and features management.
// All endpoints are absolute paths so the script works in any environment.
// =============================================================================

const ADMIN_PAGE_SIZE = 15;
const ADMIN_FEATURES_PAGE_SIZE = 10;

let currentSkip = 0;
let categories = [];
let currentFeatures = [];
let currentFeatureType = 0;
let currentCategoryId = '';
let currentPageFeatures = 1;

// ---- View-state helpers (loading / empty / error toggles) -------------------

function show(el)   { if (el) el.hidden = false; }
function hide(el)   { if (el) el.hidden = true;  }
function showInline(el) { if (el) el.style.display = ''; }
function hideInline(el) { if (el) el.style.display = 'none'; }

function setUserTableState(state, errorMessage) {
    const loading = document.getElementById('userTableLoading');
    const empty   = document.getElementById('userTableEmpty');
    const error   = document.getElementById('userTableError');
    const table   = document.getElementById('userTable');
    const pager   = document.querySelector('#userManagementModal .user-pagination');

    [loading, empty, error].forEach(hide);
    if (table) table.style.display = '';
    if (pager) pager.style.display = '';

    if (state === 'loading') {
        if (table) table.style.display = 'none';
        if (pager) pager.style.display = 'none';
        show(loading);
    } else if (state === 'empty') {
        if (table) table.style.display = 'none';
        show(empty);
    } else if (state === 'error') {
        if (table) table.style.display = 'none';
        if (pager) pager.style.display = 'none';
        if (error) error.textContent = errorMessage || 'Could not load users.';
        show(error);
    }
}

function setCategoryTableState(state, errorMessage) {
    const loading = document.getElementById('categoryTableLoading');
    const empty   = document.getElementById('categoryTableEmpty');
    const error   = document.getElementById('categoryTableError');
    const table   = document.getElementById('categoryTable');

    [loading, empty, error].forEach(hide);
    if (table) table.style.display = '';

    if (state === 'loading') {
        if (table) table.style.display = 'none';
        show(loading);
    } else if (state === 'empty') {
        if (table) table.style.display = 'none';
        show(empty);
    } else if (state === 'error') {
        if (table) table.style.display = 'none';
        if (error) error.textContent = errorMessage || 'Could not load categories.';
        show(error);
    }
}

function setFeaturesTableState(state, errorMessage) {
    const loading = document.getElementById('featuresTableLoading');
    const empty   = document.getElementById('featuresTableEmpty');
    const error   = document.getElementById('featuresTableError');
    const table   = document.getElementById('featuresTable');
    const pager   = document.getElementById('featuresPagination');

    [loading, empty, error].forEach(hide);
    if (table) table.style.display = '';
    if (pager) pager.style.display = '';

    if (state === 'loading') {
        if (table) table.style.display = 'none';
        if (pager) pager.style.display = 'none';
        show(loading);
    } else if (state === 'empty') {
        if (table) table.style.display = 'none';
        if (pager) pager.style.display = 'none';
        show(empty);
    } else if (state === 'error') {
        if (table) table.style.display = 'none';
        if (pager) pager.style.display = 'none';
        if (error) error.textContent = errorMessage || 'Could not load features.';
        show(error);
    }
}

// ---- Modal open/close -------------------------------------------------------

function openModal(modalId) {
    const modal = document.getElementById(modalId);
    if (!modal) return;
    modal.style.display = 'block';
    if (modalId === 'userManagementModal') {
        fetchAndRenderUsers();
    } else if (modalId === 'categoryManagementModal') {
        fetchAndRenderCategories();
    }
}

function closeModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) modal.style.display = 'none';
}

// ---- Users ------------------------------------------------------------------

function fetchUsers(name = '', take = ADMIN_PAGE_SIZE, skip = 0) {
    const url = new URL('/Admin/GetUsers', window.location.origin);
    url.searchParams.append('take', take);
    url.searchParams.append('skip', skip);
    if (name) url.searchParams.append('name', name);

    return fetch(url.toString(), {
        method: 'GET',
        headers: { 'Accept': 'application/json' },
    }).then(response => {
        if (!response.ok) {
            return response.text().then(err => { throw new Error(err || 'Failed to fetch users'); });
        }
        return response.json();
    });
}

function fetchAndRenderUsers() {
    const searchInput = document.getElementById('userSearchInput');
    const searchTerm = (searchInput?.value || '').trim();

    setUserTableState('loading');

    fetchUsers(searchTerm, ADMIN_PAGE_SIZE, currentSkip)
        .then(users => {
            const tableBody = document.querySelector('#userTable tbody');
            if (!tableBody) return;
            tableBody.innerHTML = '';

            if (!users || users.length === 0) {
                setUserTableState('empty');
                return;
            }

            users.forEach(user => {
                const row = tableBody.insertRow();
                row.insertCell(0).textContent = user.userName;
                row.insertCell(1).textContent = user.score;

                const actionCell = row.insertCell(2);
                const deleteButton = document.createElement('button');
                deleteButton.type = 'button';
                deleteButton.className = 'admin-row-btn danger';
                deleteButton.textContent = 'Delete';
                deleteButton.onclick = () => deleteUser(user.userName);
                actionCell.appendChild(deleteButton);
            });
            setUserTableState('ready');
        })
        .catch(error => {
            console.error('Error fetching users:', error);
            setUserTableState('error', 'Could not load users. Please retry.');
        });
}

function deleteUser(userEmail) {
    fetch('/Admin/DeleteUser', {
        method: 'POST',
        headers: { 'Accept': 'application/json', 'Content-Type': 'application/json' },
        body: JSON.stringify(userEmail),
    })
        .then(response => {
            if (response.ok) {
                fetchAndRenderUsers();
            } else {
                response.text().then(errorMessage => {
                    console.error('Failed to delete user:', errorMessage);
                    alert('Failed to delete user. Please try again.');
                });
            }
        })
        .catch(error => {
            console.error('Error deleting user:', error);
            alert('An error occurred while deleting the user.');
        });
}

function changePage(direction) {
    currentSkip += direction * ADMIN_PAGE_SIZE;
    if (currentSkip < 0) currentSkip = 0;
    fetchAndRenderUsers();
}

// ---- Categories -------------------------------------------------------------

function fetchAndRenderCategories() {
    setCategoryTableState('loading');
    fetch('/QuizCategory/GetAll', {
        method: 'GET',
        headers: { 'Accept': 'application/json' },
    })
        .then(response => {
            if (!response.ok) throw new Error('Failed to fetch categories');
            return response.json();
        })
        .then(data => {
            categories = data || [];
            if (categories.length === 0) {
                setCategoryTableState('empty');
            } else {
                setCategoryTableState('ready');
                renderCategoryTable();
            }
        })
        .catch(error => {
            console.error('Error fetching categories:', error);
            setCategoryTableState('error', 'Could not load categories. Please retry.');
        });
}

function renderCategoryTable() {
    const tableBody = document.querySelector('#categoryTable tbody');
    if (!tableBody) return;
    tableBody.innerHTML = '';

    categories.forEach(category => {
        const row = tableBody.insertRow();

        const nameCell = row.insertCell(0);
        const nameInput = document.createElement('input');
        nameInput.type = 'text';
        nameInput.value = category.name;
        nameCell.appendChild(nameInput);

        const featureTypeCell = row.insertCell(1);
        const featureTypeInput = document.createElement('input');
        featureTypeInput.type = 'number';
        featureTypeInput.value = category.featureType;
        featureTypeInput.disabled = true;
        featureTypeCell.appendChild(featureTypeInput);

        const imageCell = row.insertCell(2);
        const imageInput = document.createElement('input');
        imageInput.type = 'url';
        imageInput.value = category.imageUrl;
        imageCell.appendChild(imageInput);

        const authorCell = row.insertCell(3);
        const authorInput = document.createElement('input');
        authorInput.type = 'text';
        authorInput.value = category.authorName;
        authorCell.appendChild(authorInput);

        const actionsCell = row.insertCell(4);

        const saveButton = document.createElement('button');
        saveButton.type = 'button';
        saveButton.className = 'admin-row-btn';
        saveButton.textContent = 'Save';
        saveButton.onclick = () => updateCategory({
            id: category.id,
            name: nameInput.value,
            featureType: parseInt(featureTypeInput.value),
            imageUrl: imageInput.value,
            authorName: authorInput.value,
            creationDate: category.creationDate,
            questionToDisplay: category.questionToDisplay,
        });
        actionsCell.appendChild(saveButton);

        const deleteButton = document.createElement('button');
        deleteButton.type = 'button';
        deleteButton.className = 'admin-row-btn danger';
        deleteButton.textContent = 'Delete';
        deleteButton.onclick = () => {
            if (confirm(`Are you sure you want to delete category "${category.name}"?`)) {
                deleteCategory(category.id);
            }
        };
        actionsCell.appendChild(deleteButton);

        const editFeaturesButton = document.createElement('button');
        editFeaturesButton.type = 'button';
        editFeaturesButton.className = 'admin-row-btn';
        editFeaturesButton.textContent = 'Edit Features';
        editFeaturesButton.onclick = () => openEditFeaturesModal(category.id, category.featureType);
        actionsCell.appendChild(editFeaturesButton);
    });
}

function updateCategory(category) {
    fetch('/QuizCategory/Update', {
        method: 'POST',
        headers: { 'Accept': 'application/json', 'Content-Type': 'application/json' },
        body: JSON.stringify(category),
    })
        .then(response => {
            if (response.ok) {
                fetchAndRenderCategories();
            } else {
                response.text().then(errorMessage => {
                    console.error('Failed to update category:', errorMessage);
                    alert('Failed to update category.');
                });
            }
        })
        .catch(error => {
            console.error('Error updating category:', error);
            alert('An error occurred while updating the category.');
        });
}

function deleteCategory(categoryId) {
    fetch(`/QuizCategory/Delete?id=${categoryId}`, {
        method: 'DELETE',
        headers: { 'Accept': 'application/json' },
    })
        .then(response => {
            if (response.ok || response.status === 204) {
                fetchAndRenderCategories();
            } else {
                alert('Failed to delete category.');
            }
        })
        .catch(error => {
            console.error('Error deleting category:', error);
            alert('An error occurred while deleting the category.');
        });
}

// ---- Features ---------------------------------------------------------------

function openEditFeaturesModal(categoryId, featureType) {
    const modal = document.getElementById('editFeaturesModal');
    if (!modal) return;
    modal.style.display = 'block';
    currentCategoryId = categoryId;
    currentFeatureType = featureType;
    currentPageFeatures = 1;

    const url = featureType === 1
        ? `/FeatureInt/GetAllByCategory?categoryId=${categoryId}`
        : `/FeatureFloat/GetAllByCategory?categoryId=${categoryId}`;

    setFeaturesTableState('loading');

    fetch(url, { method: 'GET', headers: { 'Accept': 'application/json' } })
        .then(response => {
            if (!response.ok) throw new Error('Failed to fetch features');
            return response.json();
        })
        .then(features => {
            currentFeatures = features || [];
            if (currentFeatures.length === 0) {
                setFeaturesTableState('empty');
            } else {
                setFeaturesTableState('ready');
                renderFeaturesTable();
            }
        })
        .catch(error => {
            console.error('Error fetching features:', error);
            setFeaturesTableState('error', 'Could not load features. Please retry.');
        });
}

function renderFeaturesTable() {
    const tableBody = document.querySelector('#featuresTable tbody');
    if (!tableBody) return;
    tableBody.innerHTML = '';

    const startIndex = (currentPageFeatures - 1) * ADMIN_FEATURES_PAGE_SIZE;
    const endIndex = startIndex + ADMIN_FEATURES_PAGE_SIZE;
    const paginatedFeatures = currentFeatures.slice(startIndex, endIndex);

    paginatedFeatures.forEach(feature => {
        const row = tableBody.insertRow();

        const valueCell = row.insertCell(0);
        const valueInput = document.createElement('input');
        valueInput.type = 'number';
        valueInput.value = feature.value;
        valueCell.appendChild(valueInput);

        const entityCell = row.insertCell(1);
        entityCell.textContent = 'Loading…';

        fetch(`/QuizEntity/GetById?id=${feature.quizEntityId}`, {
            method: 'GET',
            headers: { 'Accept': 'application/json' },
        })
            .then(response => {
                if (!response.ok) {
                    entityCell.textContent = 'Error';
                    return null;
                }
                return response.json();
            })
            .then(entity => {
                if (entity) {
                    entityCell.textContent = entity.name;
                } else if (entityCell.textContent === 'Loading…') {
                    entityCell.textContent = 'Not Found';
                }
            })
            .catch(() => { entityCell.textContent = 'Error'; });

        const actionsCell = row.insertCell(2);

        const saveButton = document.createElement('button');
        saveButton.type = 'button';
        saveButton.className = 'admin-row-btn';
        saveButton.textContent = 'Save';
        saveButton.onclick = () => {
            updateFeature({
                id: feature.id,
                value: parseFloat(valueInput.value),
                quizCategoryId: feature.quizCategoryId,
                quizEntityId: feature.quizEntityId,
            });
        };
        actionsCell.appendChild(saveButton);

        const deleteButton = document.createElement('button');
        deleteButton.type = 'button';
        deleteButton.className = 'admin-row-btn danger';
        deleteButton.textContent = 'Delete';
        deleteButton.onclick = () => deleteFeature(feature.id);
        actionsCell.appendChild(deleteButton);
    });

    renderFeaturesPagination();
}

function renderFeaturesPagination() {
    const paginationContainer = document.querySelector('#featuresPagination');
    if (!paginationContainer) return;
    paginationContainer.innerHTML = '';

    const totalPages = Math.max(1, Math.ceil(currentFeatures.length / ADMIN_FEATURES_PAGE_SIZE));

    const prevButton = document.createElement('button');
    prevButton.type = 'button';
    prevButton.textContent = 'Previous';
    prevButton.disabled = currentPageFeatures === 1;
    prevButton.onclick = () => {
        if (currentPageFeatures > 1) {
            currentPageFeatures--;
            renderFeaturesTable();
        }
    };
    paginationContainer.appendChild(prevButton);

    const nextButton = document.createElement('button');
    nextButton.type = 'button';
    nextButton.textContent = 'Next';
    nextButton.disabled = currentPageFeatures >= totalPages;
    nextButton.onclick = () => {
        if (currentPageFeatures < totalPages) {
            currentPageFeatures++;
            renderFeaturesTable();
        }
    };
    paginationContainer.appendChild(nextButton);
}

function updateFeature(feature) {
    const url = currentFeatureType === 1 ? '/FeatureInt/Update' : '/FeatureFloat/Update';
    const payload = {
        Id: feature.id,
        Value: feature.value,
        QuizCategoryId: feature.quizCategoryId,
        QuizEntityId: feature.quizEntityId,
    };
    fetch(url, {
        method: 'POST',
        headers: { 'Accept': 'application/json', 'Content-Type': 'application/json' },
        body: JSON.stringify(payload),
    })
        .then(response => {
            if (response.ok) {
                openEditFeaturesModal(currentCategoryId, currentFeatureType);
            } else {
                alert('Failed to update feature.');
            }
        })
        .catch(error => console.error('Error updating feature:', error));
}

function deleteFeature(featureId) {
    const url = currentFeatureType === 1
        ? `/FeatureInt/Delete?id=${featureId}`
        : `/FeatureFloat/Delete?id=${featureId}`;

    fetch(url, { method: 'POST', headers: { 'Accept': 'application/json' } })
        .then(response => {
            if (response.ok) {
                currentFeatures = currentFeatures.filter(feature => feature.id !== featureId);
                if (currentFeatures.length === 0) {
                    setFeaturesTableState('empty');
                } else {
                    renderFeaturesTable();
                }
            } else {
                alert('Failed to delete feature.');
            }
        })
        .catch(error => console.error('Error deleting feature:', error));
}

// ---- Init -------------------------------------------------------------------

document.addEventListener('DOMContentLoaded', () => {
    // On the standalone /Admin/Users page the user panel is rendered inline,
    // so we should auto-load it. On the dashboard, modals load on open.
    const inlineUserPanel = document.querySelector('#userManagementModal.admin-inline-panel');
    if (inlineUserPanel) {
        fetchAndRenderUsers();
    }

    const searchInput = document.getElementById('userSearchInput');
    if (searchInput) {
        searchInput.addEventListener('input', () => {
            currentSkip = 0;
            fetchAndRenderUsers();
        });
    }
});
