let quizCategoryId = null;
let entities = [];
let features = [];
let quizEntityId = null;

// Category Form Handler
document.getElementById('categoryForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    const categoryData = {
        name: document.getElementById('categoryName').value,
        featureType: parseInt(document.getElementById('featureType').value),
        imageUrl: document.getElementById('imageUrl').value,
        authorName: document.getElementById('authorName').value,
        creationDate: new Date().toISOString(),
        questionToDisplay: document.getElementById('questionToDisplay').value
    };

    try {
        const response = await fetch('/QuizCategory/Create', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(categoryData)
        });

        if (!response.ok) throw new Error('Failed to create category');
        const result = await response.json();
        quizCategoryId = result.id;
        alert('Category created successfully!');
    } catch (error) {
        console.error('Error:', error);
        alert('Failed to create category');
    }
});

// Entity Form Handler
document.getElementById('entityForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    if (!quizCategoryId) {
        alert('Please create a category first');
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
        console.error('Error:', error);
        alert('Failed to create entity');
    }
});

// Add an event listener for the entity select dropdown
document.getElementById('entitySelect').addEventListener('change', (e) => {
    quizEntityId = e.target.value;
});

document.getElementById('featureForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    if (!quizCategoryId) {
        alert('Please create a category first');
        return;
    }

    if (!quizEntityId) {
        alert('Please select an entity');
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
        features.push({
            entityId: quizEntityId,
            value,
            entityName: entities.find(e => e.id === quizEntityId)?.name
        });
        updateFeatureList();
        document.getElementById('featureValue').value = '';
    } catch (error) {
        console.error('Error:', error);
        alert('Failed to create feature');
    }
});

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