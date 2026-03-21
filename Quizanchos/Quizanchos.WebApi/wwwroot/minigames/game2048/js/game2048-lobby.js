document.addEventListener('DOMContentLoaded', () => {
    const root = document.getElementById('minigame-root');
    if (!root) return;

    root.innerHTML = `
<div class="game2048-landing">
    <h1>2048</h1>
    <p class="subtitle">Join the tiles, get to <strong>2048!</strong></p>

    <div class="game2048-start-section">
        <div class="form-group">
            <label for="boardSize">Board Size:</label>
            <select id="boardSize" class="form-control">
                <option value="4" selected>4 × 4</option>
                <option value="5">5 × 5</option>
                <option value="6">6 × 6</option>
            </select>
        </div>
        <div class="action-buttons">
            <button type="button" id="startGame2048" class="btn-start-game">Start Game</button>
            <a href="/" class="btn-back">Back to Home</a>
        </div>
    </div>
</div>`;

    document.getElementById('startGame2048')?.addEventListener('click', async () => {
        const size = parseInt(document.getElementById('boardSize').value, 10);
        const userId = document.body.getAttribute('data-user-id');
        if (!userId) {
            alert('Please log in to play.');
            return;
        }

        try {
            const result = await game2048Client.createGame([userId], { size });
            if (result?.gameId) {
                window.location.href = window.minigameConfig.gameUrlTemplate.replace('{gameId}', result.gameId);
            }
        } catch (error) {
            alert('Failed to create game: ' + error.message);
        }
    });
});
