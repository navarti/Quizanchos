(function () {
    const root = document.getElementById('minigame-root');
    if (!root) return;

    const cfg = window.minigameConfig || {};
    const minigameTypeId = cfg.minigameTypeId;
    const gameUrlTemplate = cfg.gameUrlTemplate;

    root.innerHTML = `
        <div class="minigame-card minigame-card--lobby minigame-card--narrow">
            <h2 class="minigame-title">Country Guesser</h2>
            <p class="minigame-prose">
                You'll see a country name each round — click on the world map where you
                think it is. Land within range of the centroid to score.
            </p>
            <div class="minigame-form">
                <div class="minigame-field">
                    <label for="cg-rounds">Rounds</label>
                    <input type="number" min="3" max="20" value="5" id="cg-rounds" />
                </div>
                <div class="minigame-field">
                    <label for="cg-secs">Seconds per round</label>
                    <input type="number" min="5" max="60" value="20" id="cg-secs" />
                </div>
                <div class="minigame-field">
                    <label for="cg-radius">Allowed radius (km)</label>
                    <input type="number" min="100" max="2000" step="50" value="600" id="cg-radius" />
                </div>
            </div>
            <button class="minigame-btn" data-start>Start a new game</button>
        </div>
    `;

    root.querySelector('[data-start]').addEventListener('click', async () => {
        const userId = document.body.getAttribute('data-user-id');
        if (!userId) {
            alert('Please log in to play.');
            return;
        }
        const totalCards = parseInt(document.getElementById('cg-rounds').value, 10) || 5;
        const secondsPerCard = parseInt(document.getElementById('cg-secs').value, 10) || 20;
        const maxDistanceKm = parseInt(document.getElementById('cg-radius').value, 10) || 600;
        try {
            const resp = await fetch('/api/Game/create', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                credentials: 'include',
                body: JSON.stringify({
                    minigameType: minigameTypeId,
                    playerIds: [userId],
                    parameters: { totalCards, secondsPerCard, maxDistanceKm },
                }),
            });
            if (!resp.ok) {
                const err = await resp.json().catch(() => ({}));
                throw new Error(err.message || `HTTP ${resp.status}`);
            }
            const data = await resp.json();
            const gameId = data.gameId || data.GameId;
            if (gameId && gameUrlTemplate) {
                window.location.href = gameUrlTemplate.replace('{gameId}', gameId);
            }
        } catch (err) {
            alert('Failed to start: ' + err.message);
        }
    });
})();
