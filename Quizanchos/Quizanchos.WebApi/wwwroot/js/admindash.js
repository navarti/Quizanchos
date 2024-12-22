let currentPage = 1;
let currentSkip = 0;
const take = 15; // Количество пользователей на странице

// Открытие модального окна
function openModal(modalId) {
    document.getElementById(modalId).style.display = "block";
    if (modalId === 'userManagementModal') {
        fetchAndRenderUsers(); 
    } else if (modalId === 'categoryManagementModal') {
        renderFeaturesTable(); 
    }
    else {
        
    }
}

// Закрытие модального окна
function closeModal(modalId) {
    document.getElementById(modalId).style.display = "none";
}

function fetchUsers(name = "", take = 15, skip = 0) {
    const url = new URL('/Admin/GetUsers');
    url.searchParams.append('take', take);
    url.searchParams.append('skip', skip);
    if (name) url.searchParams.append('name', name);

    console.log(`Sending request to: ${url.toString()}`);
    return fetch(url.toString(), {
        method: 'GET',
        headers: {
            'Accept': '*/*',
        },
    })
        .then(response => {
            if (!response.ok) {
                return response.text().then(err => {
                    throw new Error(err || 'Failed to fetch users');
                });
            }
            return response.json();
        })
        .catch(error => {
            console.error("Error in fetchUsers:", error);
            throw error;
        });
}

function fetchAndRenderUsers() {
    console.log("Fetching and rendering users...");
    const searchTerm = document.getElementById('userSearchInput').value.trim();
    console.log(`Search term: ${searchTerm}`);

    fetchUsers(searchTerm, take, currentSkip)
        .then(users => {
            console.log("Fetched users:", users);

            const tableBody = document.querySelector('#userTable tbody');
            tableBody.innerHTML = ''; 

            users.forEach(user => {
                const row = tableBody.insertRow();
                row.insertCell(0).textContent = user.userName; 
                row.insertCell(1).textContent = user.score; 

                const actionCell = row.insertCell(2);
                const deleteButton = document.createElement('button');
                deleteButton.textContent = 'Delete';
                deleteButton.onclick = () => deleteUser(user.userName);
                actionCell.appendChild(deleteButton);
            });
        })
        .catch(error => {
            console.error("Error fetching users:", error);
        });
}

function deleteUser(userEmail) {
    fetch('/Admin/DeleteUser', {
        method: "POST",
        headers: {
            "Accept": "*/*",
            "Content-Type": "application/json",
        },
        body: JSON.stringify(userEmail),
    })
        .then(response => {
            if (response.ok) {
                alert("User deleted successfully!");
                fetchAndRenderUsers(); 
            } else {
                response.text().then(errorMessage => {
                    console.error("Failed to delete user:", errorMessage);
                    alert("Failed to delete user. Please try again.");
                });
            }
        })
        .catch(error => {
            console.error("Error deleting user:", error);
            alert("An error occurred while deleting the user.");
        });
}

function changePage(direction) {
    currentSkip += direction * take; // Увеличиваем или уменьшаем skip
    if (currentSkip < 0) currentSkip = 0; // Не допускаем отрицательных значений
    fetchAndRenderUsers(); // Обновляем таблицу
}

// Обработчик ввода в поле поиска
document.getElementById('userSearchInput').addEventListener('input', () => {
    currentSkip = 0; 
    fetchAndRenderUsers();
});

// Инициализация
document.addEventListener('DOMContentLoaded', () => {
    fetchAndRenderUsers();
    fetchAndRenderCategories()
});

let categories = [];

function closeModal(modalId) {
    document.getElementById(modalId).style.display = 'none';
}

function fetchAndRenderCategories() {
    fetch('/QuizCategory/GetAll', {
        method: 'GET',
        headers: {
            'Accept': '*/*',
        },
    })
        .then(response => response.json())
        .then(data => {
            categories = data;
            renderCategoryTable();
        })
        .catch(error => console.error('Error fetching categories:', error));
}

function renderCategoryTable() {
    const tableBody = document.querySelector('#categoryTable tbody');
    tableBody.innerHTML = '';

    categories.forEach(category => {
        const row = tableBody.insertRow();

        // Name
        const nameCell = row.insertCell(0);
        const nameInput = document.createElement('input');
        nameInput.type = 'text';
        nameInput.value = category.name;
        nameCell.appendChild(nameInput);

        // Feature Type
        const featureTypeCell = row.insertCell(1);
        const featureTypeInput = document.createElement('input');
        featureTypeInput.type = 'number';
        featureTypeInput.value = category.featureType;
        featureTypeInput.disabled = true; // Disable initially
        featureTypeCell.appendChild(featureTypeInput);

        // Image
        const imageCell = row.insertCell(2);
        const imageInput = document.createElement('input');
        imageInput.type = 'url';
        imageInput.value = category.imageUrl;
        imageCell.appendChild(imageInput);

        // Author Name
        const authorCell = row.insertCell(3);
        const authorInput = document.createElement('input');
        authorInput.type = 'text';
        authorInput.value = category.authorName;
        authorCell.appendChild(authorInput);

        // Actions
        const actionsCell = row.insertCell(4);

        // Save Button
        const saveButton = document.createElement('button');
        saveButton.textContent = 'Save';
        saveButton.onclick = () => updateCategory({
            id: category.id,
            name: nameInput.value,
            featureType: parseInt(featureTypeInput.value),
            imageUrl: imageInput.value,
            authorName: authorInput.value,
            creationDate: category.creationDate,
            questionToDisplay: category.questionToDisplay
        });
        actionsCell.appendChild(saveButton);

        // Delete Button
        const deleteButton = document.createElement('button');
        deleteButton.textContent = 'Delete';
        deleteButton.onclick = () => {
            if (confirm(`Are you sure you want to delete category "${category.name}"?`)) {
                deleteCategory(category.id);
            }
        };
        actionsCell.appendChild(deleteButton);
        
        const editFeaturesButton = document.createElement('button');
        editFeaturesButton.textContent = 'Edit Features';
        editFeaturesButton.onclick = () => openEditFeaturesModal(category.id, category.featureType);
        actionsCell.appendChild(editFeaturesButton);
    });
}


function updateCategory(category) {
    fetch('/QuizCategory/Update', {
        method: 'POST',
        headers: {
            'Accept': '*/*',
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(category),
    })
        .then(response => {
            if (response.ok) {
                alert('Category updated successfully!');
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

let currentFeatures = [];
let currentFeatureType = 0;
let currentCategoryId = '';
let currentPageFeatures = 1; // Текущая страница для features
const itemsPerPageFeatures = 10; // Количество features на странице

function openEditFeaturesModal(categoryId, featureType) {
    document.getElementById('editFeaturesModal').style.display = "block"; // Исправлено на строку 'editFeaturesModal'
    currentCategoryId = categoryId;
    currentFeatureType = featureType;
    currentPageFeatures = 1; // Сбрасываем на первую страницу

    const url = featureType === 1
        ? `/FeatureInt/GetAllByCategory?categoryId=${categoryId}`
        : `/FeatureFloat/GetAllByCategory?categoryId=${categoryId}`;

    fetch(url, {
        method: 'GET',
        headers: {
            'Accept': '*/*',
        },
    })
        .then(response => response.json())
        .then(features => {
            currentFeatures = features;
            renderFeaturesTable();
        })
        .catch(error => console.error('Error fetching features:', error));
}

function renderFeaturesTable() {
    const tableBody = document.querySelector('#featuresTable tbody');
    tableBody.innerHTML = '';

    const startIndex = (currentPageFeatures - 1) * itemsPerPageFeatures;
    const endIndex = startIndex + itemsPerPageFeatures;
    const paginatedFeatures = currentFeatures.slice(startIndex, endIndex);

    console.log('Rendering paginated features:', paginatedFeatures);

    paginatedFeatures.forEach(feature => {
        const row = tableBody.insertRow();

        // Значение
        const valueCell = row.insertCell(0);
        const valueInput = document.createElement('input');
        valueInput.type = 'number';
        valueInput.value = feature.value;
        valueCell.appendChild(valueInput);

        // Имя варианта
        const entityCell = row.insertCell(1);
        entityCell.textContent = 'Loading...'; // Плейсхолдер

        fetch(`/QuizEntity/GetById?id=${feature.quizEntityId}`, {
            method: 'GET',
            headers: {
                'Accept': '*/*',
            },
        })
            .then(response => {
                if (!response.ok) {
                    console.error(`Failed to fetch entity name for ID: ${feature.quizEntityId}`);
                    entityCell.textContent = 'Error';
                    return;
                }
                return response.json();
            })
            .then(entity => {
                if (entity) {
                    entityCell.textContent = entity.name;
                } else {
                    entityCell.textContent = 'Not Found';
                }
            })
            .catch(error => {
                console.error(`Error fetching entity name for ID ${feature.quizEntityId}:`, error);
                entityCell.textContent = 'Error';
            });

        // Действия
        const actionsCell = row.insertCell(2);

        const saveButton = document.createElement('button');
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
        deleteButton.textContent = 'Delete';
        deleteButton.onclick = () => deleteFeature(feature.id);
        actionsCell.appendChild(deleteButton);
    });

    renderFeaturesPagination();
}


function renderFeaturesPagination() {
    const paginationContainer = document.querySelector('#featuresPagination');
    paginationContainer.innerHTML = '';

    const totalPages = Math.ceil(currentFeatures.length / itemsPerPageFeatures);

    const prevButton = document.createElement('button');
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
    nextButton.textContent = 'Next';
    nextButton.disabled = currentPageFeatures === totalPages;
    nextButton.onclick = () => {
        if (currentPageFeatures < totalPages) {
            currentPageFeatures++;
            renderFeaturesTable();
        }
    };
    paginationContainer.appendChild(nextButton);
}

function updateFeature(feature) {
    const url = currentFeatureType === 1
        ? `/FeatureInt/Update`
        : `/FeatureFloat/Update`;
    const payload = {
        Id: feature.id, // Переименовать в "Id" вместо "id", если требуется
        Value: feature.value, // Переименовать в "Value" вместо "value"
        QuizCategoryId: feature.quizCategoryId, // Переименовать в "QuizCategoryId"
        QuizEntityId: feature.quizEntityId // Переименовать в "QuizEntityId"
    };
    fetch(url, {
        method: 'POST',
        headers: {
            'Accept': '*/*',
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(payload),
    })
        .then(response => {
            if (response.ok) {
                alert('Feature updated successfully!');
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

    fetch(url, {
        method: 'POST',
        headers: {
            'Accept': '*/*',
        },
    })
        .then(response => {
            if (response.ok) {
                alert('Feature deleted successfully!');
                currentFeatures = currentFeatures.filter(feature => feature.id !== featureId);
                renderFeaturesTable();
            } else {
                alert('Failed to delete feature.');
            }
        })
        .catch(error => console.error('Error deleting feature:', error));
}