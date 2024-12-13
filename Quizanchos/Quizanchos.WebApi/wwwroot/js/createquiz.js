// Global variables to store data
let quizCategoryId = null;
let entities = [];
let features = [];
let quizEntityId = null;

// Event Listener for Category Form Submission
document.getElementById('categoryForm').addEventListener('submit', async (e) => {
    e.preventDefault();

    // Check if we are updating an existing category or creating a new one
    const isUpdating = !!quizCategoryId; // If `quizCategoryId` exists, it's an update

    // Prepare the JSON payload
    const categoryData = {
        id: quizCategoryId || null, // Use `quizCategoryId` for updates, otherwise `null`
        name: document.getElementById('categoryName').value,
        featureType: parseInt(document.getElementById('featureType').value),
        imageUrl: document.getElementById('imageUrl').value,
        authorName: document.getElementById('authorName').value,
        creationDate: new Date().toISOString(), // Use current date for creation or updates
        questionToDisplay: document.getElementById('questionToDisplay').value
    };

    try {
        const response = await fetch('/QuizCategory/Update', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(categoryData)
        });

        if (!response.ok) throw new Error('Failed to update category');
        const result = await response.json();

        // Update the quizCategoryId if it's a new creation
        quizCategoryId = result.id;

        alert('Category updated successfully!');
        fetchAndRenderCategories(); // Refresh category dropdown
    } catch (error) {
        console.error('Error updating category:', error);
        alert('Failed to update category');
    }
});

// Event Listener for Entity Form Submission
document.getElementById('entityForm').addEventListener('submit', async (e) => {
    e.preventDefault();

    if (!quizCategoryId) {
        alert('Please create or select a category first.');
        return;
    }

    const entityName = document.getElementById('entityName').value;
    try {
        const response = await fetch('/QuizEntity/Create', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ name: entityName })
        });

        if (!response.ok) throw new Error('Failed to create entity');
        const result = await response.json();

        entities.push({ id: result.id, name: entityName });
        updateEntityList();
        updateEntitySelect();
        document.getElementById('entityName').value = '';
    } catch (error) {
        console.error('Error creating entity:', error);
        alert('Failed to create entity');
    }
});

// Event Listener for Feature Form Submission
document.getElementById('featureForm').addEventListener('submit', async (e) => {
    e.preventDefault();

    // Get the selected entity ID from the dropdown
    quizEntityId = document.getElementById('entitySelect').value;

    if (!quizCategoryId) {
        alert('Please create or select a category first.');
        return;
    }

    if (!quizEntityId) {
        alert('Please select an entity.');
        return;
    }

    const value = parseInt(document.getElementById('featureValue').value);

    try {
        const response = await fetch('/FeatureInt/Create', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                Value: value,
                QuizCategoryId: quizCategoryId,
                QuizEntityId: quizEntityId
            })
        });

        if (!response.ok) throw new Error('Failed to create feature');
        const result = await response.json();

        features.push({
            entityId: quizEntityId,
            value,
            entityName: entities.find(e => e.id === quizEntityId)?.name
        });

        updateFeatureList();
        document.getElementById('featureValue').value = '';
    } catch (error) {
        console.error('Error creating feature:', error);
        alert('Failed to create feature');
    }
});


// Fetch and Render Existing Categories
async function fetchAndRenderCategories() {
    try {
        const response = await fetch('/QuizCategory/GetAll', {
            method: 'GET',
            headers: { 'Accept': 'application/json' }
        });

        if (!response.ok) throw new Error('Failed to fetch categories');
        const categories = await response.json();

        const select = document.getElementById('existingCategorySelect');
        select.innerHTML = '<option value="">Select Existing Category</option>';
        categories.forEach(category => {
            const option = document.createElement('option');
            option.value = category.id;
            option.textContent = category.name;
            select.appendChild(option);
        });
    } catch (error) {
        console.error('Error fetching categories:', error);
    }
}

// Load Selected Category Details
document.getElementById('loadCategoryButton').addEventListener('click', async () => {
    const selectedCategoryId = document.getElementById('existingCategorySelect').value;

    if (!selectedCategoryId) {
        alert('Please select a category.');
        return;
    }

    try {
        const response = await fetch(`https://localhost:7020/QuizCategory/GetById?id=${selectedCategoryId}`, {
            method: 'GET',
            headers: { 'Accept': '*/*' }
        });

        if (!response.ok) {
            throw new Error('Failed to fetch category details');
        }

        const category = await response.json();

        // Store the ID for potential updates
        quizCategoryId = category.id;

        // Populate the form fields
        document.getElementById('categoryName').value = category.name;
        document.getElementById('featureType').value = category.featureType;
        document.getElementById('imageUrl').value = category.imageUrl;
        document.getElementById('authorName').value = category.authorName || ''; // Handle empty strings
        document.getElementById('questionToDisplay').value = category.questionToDisplay;

        // Disable inputs to prevent accidental edits
        setCategoryFormDisabled(true);

        alert('Category loaded successfully!');
    } catch (error) {
        console.error('Error fetching category details:', error);
        alert('Failed to load category details');
    }
});

// Enable Editing of the Category Form
document.getElementById('enableEditButton').addEventListener('click', () => {
    setCategoryFormDisabled(false);
});

// Helper Function to Disable/Enable Category Form
function setCategoryFormDisabled(disabled) {
    document.getElementById('categoryName').disabled = disabled;
    document.getElementById('featureType').disabled = disabled;
    document.getElementById('imageUrl').disabled = disabled;
    document.getElementById('authorName').disabled = disabled;
    document.getElementById('questionToDisplay').disabled = disabled;
}

// Update Entity List
function updateEntityList() {
    const list = document.getElementById('entityList');
    list.innerHTML = '';
    entities.forEach(entity => {
        const li = document.createElement('li');
        li.className = 'entity-item';
        li.textContent = entity.name;
        list.appendChild(li);
    });
}

// Update Entity Dropdown
function updateEntitySelect() {
    const select = document.getElementById('entitySelect');
    select.innerHTML = '<option value="">Select Entity</option>';
    entities.forEach(entity => {
        const option = document.createElement('option');
        option.value = entity.id;
        option.textContent = entity.name;
        select.appendChild(option);
    });
}

// Update Feature List
function updateFeatureList() {
    const list = document.getElementById('featureList');
    list.innerHTML = '';
    features.forEach(feature => {
        const li = document.createElement('li');
        li.className = 'feature-item';
        li.textContent = `${feature.entityName}: ${feature.value}`;
        list.appendChild(li);
    });
}

// Fetch and render categories when the page loads
document.addEventListener('DOMContentLoaded', () => {
    fetchAndRenderCategories();
});
