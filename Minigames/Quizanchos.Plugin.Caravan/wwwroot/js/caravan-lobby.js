(function () {
    const root = document.getElementById('minigame-root');
    if (!root) return;

    const cfg = window.minigameConfig || {};
    const minigameTypeId = cfg.minigameTypeId;
    const gameUrlTemplate = cfg.gameUrlTemplate;

    root.innerHTML = `
        <div class="caravan caravan__lobby">
            <h2>Caravan</h2>
            <p>
                Build three caravans, each worth 21–26, before the AI does the same. Number cards
                2–10 must be placed in strict ascending or descending order on a column; matching
                the suit lets you reverse direction. Aces can open a column.
            </p>
            <p>
                Face cards attach to existing slots — Jack discards a card, Queen flips direction
                and changes the active suit, King doubles the slot's value, Joker on an Ace nukes
                that suit, Joker on a number nukes that value across the whole table.
            </p>
            <button class="caravan__btn" data-start>Start a new game</button>
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
                    parameters: {},
                }),
            });
            if (!resp.ok) {
                const err = await resp.json().catch(() => ({}));
                throw new Error(err.message || `Server returned ${resp.status}`);
            }
            const data = await resp.json();
            const gameId = data.gameId || data.GameId;
            if (gameId && gameUrlTemplate) {
                window.location.href = gameUrlTemplate.replace('{gameId}', gameId);
            }
        } catch (err) {
            console.error('Failed to start Caravan game', err);
            alert('Failed to start game: ' + err.message);
        }
    });
})();
