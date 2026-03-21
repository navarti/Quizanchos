document.addEventListener('DOMContentLoaded', async () => {
    const root = document.getElementById('minigame-root');
    if (!root) return;

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
            <a href="/" class="btn-back-to-quizzes">Back to Quizzes</a>
        </div>
    </form>
</div>`;

    const query = new URLSearchParams(window.location.search);
    const presetCategoryId = query.get('categoryId');

    try {
        const response = await fetch('/QuizCategory/GetAll');
        if (response.ok) {
            const categories = await response.json();
            const categorySelect = document.getElementById('categoryId');
            categorySelect.innerHTML = '';

            categories.forEach(c => {
                const option = document.createElement('option');
                option.value = c.id || c.Id;
                option.textContent = c.name || c.Name;
                if (presetCategoryId && option.value.toLowerCase() === presetCategoryId.toLowerCase()) {
                    option.selected = true;
                }
                categorySelect.appendChild(option);
            });

            const selected = categorySelect.options[categorySelect.selectedIndex];
            if (selected) {
                document.getElementById('categoryName').textContent = selected.textContent + ' Quiz';
            }

            categorySelect.addEventListener('change', () => {
                const next = categorySelect.options[categorySelect.selectedIndex];
                if (next) {
                    document.getElementById('categoryName').textContent = next.textContent + ' Quiz';
                }
            });
        }
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
});
