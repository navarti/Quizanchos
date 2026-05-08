(function () {
    const root = document.getElementById('minigame-root');
    if (!root) return;

    const cfg = window.minigameConfig || {};
    const minigameTypeId = cfg.minigameTypeId;
    const gameUrlTemplate = cfg.gameUrlTemplate;

    root.innerHTML = `
        <div class="country-guesser-mp country-guesser-mp__lobby">
            <h2>Country Guesser — Multiplayer</h2>
            <p>
                Race other players in real time to identify highlighted countries on the world map.
                Each round shows a circle around the target country; pick the matching name from the
                options before the timer runs out.
            </p>
            <p>
                Solo testing is fine too — start a room and play it through alone.
            </p>
            <button class="country-guesser-mp__btn" data-start>Start a room</button>
        </div>
    `;

    root.querySelector('[data-start]').addEventListener('click', async () => {
        const userId = document.body.getAttribute('data-user-id');
        if (!userId) {
            alert('Please log in to play.');
            return;
        }
        try {
            const resp = await fetch('/api/Game/create', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                credentials: 'include',
                body: JSON.stringify({
                    minigameType: minigameTypeId,
                    playerIds: [userId],
                    parameters: { totalCards: 5, secondsPerCard: 20, optionCount: 4 },
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
