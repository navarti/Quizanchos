(function () {
    const root = document.getElementById('minigame-root');
    if (!root) return;

    const cfg = window.minigameConfig || {};
    const minigameTypeId = cfg.minigameTypeId;
    const gameUrlTemplate = cfg.gameUrlTemplate;

    root.innerHTML = `
        <div class="country-guesser country-guesser__lobby">
            <h2>Country Guesser</h2>
            <p>
                A country is highlighted on the world map each round. Pick the matching name from
                the options before the timer runs out.
            </p>
            <div>
                <label>Rounds: <input type="number" min="3" max="20" value="5" id="cg-rounds" /></label>
                <label>Seconds per round: <input type="number" min="5" max="60" value="20" id="cg-secs" /></label>
            </div>
            <button class="country-guesser__btn" data-start>Start a new game</button>
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
        try {
            const resp = await fetch('/api/Game/create', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                credentials: 'include',
                body: JSON.stringify({
                    minigameType: minigameTypeId,
                    playerIds: [userId],
                    parameters: { totalCards, secondsPerCard, optionCount: 4 },
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
