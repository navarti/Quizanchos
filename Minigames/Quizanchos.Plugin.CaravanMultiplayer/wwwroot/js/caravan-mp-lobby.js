// Caravan Multiplayer — lobby. Uses the host's RoomLobbyBase to manage room
// creation, joining, and auto-launching when the room hits 2 players.
document.addEventListener('DOMContentLoaded', async () => {
    if (typeof RoomLobbyBase === 'undefined') {
        console.error('[CaravanMp] RoomLobbyBase not loaded — check LobbyScripts order in descriptor.');
        return;
    }

    await RoomLobbyBase.initialize({
        minigameType: window.minigameConfig.minigameTypeId,
        gameUrlTemplate: window.minigameConfig.gameUrlTemplate,
        ensureLayout: ensureLobbyLayout,
        onAfterLayout: wireLobbyExtras,
        buildCreateRoomRequest: (minigameType) => {
            const gameParameters = {};
            const seedInput = document.getElementById('caravanSeed');
            if (seedInput && seedInput.value.trim() !== '') {
                const parsedSeed = parseInt(seedInput.value, 10);
                if (!Number.isNaN(parsedSeed) && parsedSeed !== 0) {
                    gameParameters.seed = parsedSeed;
                }
            }

            return {
                minigameType,
                maxPlayers: 2,
                teamCount: 2,
                gameParameters,
            };
        },
    });
});

function wireLobbyExtras() {
    const seedRandomBtn = document.getElementById('caravanSeedRandom');
    const seedInput = document.getElementById('caravanSeed');
    if (seedRandomBtn && seedInput) {
        seedRandomBtn.addEventListener('click', (event) => {
            event.preventDefault();
            seedInput.value = String(Math.floor(Math.random() * 999999) + 1);
        });
    }

    const seedClearBtn = document.getElementById('caravanSeedClear');
    if (seedClearBtn && seedInput) {
        seedClearBtn.addEventListener('click', (event) => {
            event.preventDefault();
            seedInput.value = '';
        });
    }
}

function ensureLobbyLayout() {
    if (document.getElementById('createRoomForm')) {
        return;
    }

    const root = document.getElementById('minigame-root');
    if (!root) {
        return;
    }

    root.innerHTML = `
<main class="lobby-container caravan-mp-lobby">
    <section class="panel create-panel caravan-mp-create-panel">
        <div class="caravan-mp-hero">
            <span class="caravan-mp-hero__eyebrow">Fallout: New Vegas</span>
            <h1>Caravan — Multiplayer</h1>
            <p class="subtitle">Head-to-head card battle. Sell three caravans worth 21 – 26 before your opponent does.</p>
            <div class="caravan-mp-hero__chips">
                <span class="caravan-mp-chip">1v1</span>
                <span class="caravan-mp-chip">~10 min</span>
                <span class="caravan-mp-chip">Turn-based</span>
            </div>
        </div>

        <details class="caravan-mp-rules">
            <summary class="caravan-mp-rules__head">
                <span>How to play</span>
                <span class="caravan-mp-rules__chev" aria-hidden="true">▾</span>
            </summary>
            <div class="caravan-mp-rules__grid">
                <div class="caravan-mp-rule">
                    <span class="caravan-mp-rule__icon">🎯</span>
                    <div>
                        <strong>Win two of three caravans</strong>
                        <p>A caravan "sells" once it sums to 21 – 26. Highest selling caravan beats the opponent's same column.</p>
                    </div>
                </div>
                <div class="caravan-mp-rule">
                    <span class="caravan-mp-rule__icon">📈</span>
                    <div>
                        <strong>Number cards (Ace – 10)</strong>
                        <p>Stack on your own caravan in strict ascending or descending order. Match the suit to reverse direction.</p>
                    </div>
                </div>
                <div class="caravan-mp-rule">
                    <span class="caravan-mp-rule__icon">🃏</span>
                    <div>
                        <strong>Face cards attach to slots</strong>
                        <p>Jack discards a card · Queen flips direction & changes suit · King doubles the slot · Joker (on Ace) nukes a suit, (on number) nukes a value.</p>
                    </div>
                </div>
                <div class="caravan-mp-rule">
                    <span class="caravan-mp-rule__icon">🏳️</span>
                    <div>
                        <strong>Surrender any turn</strong>
                        <p>Stuck? Concede on your turn to hand the win to your opponent and start fresh.</p>
                    </div>
                </div>
            </div>
        </details>

        <form id="createRoomForm" class="create-form caravan-mp-form">
            <div class="caravan-mp-form__row">
                <div class="caravan-mp-form__field">
                    <label for="caravanSeed">Seed <span class="caravan-mp-form__hint">(optional, for reproducible matches)</span></label>
                    <div class="caravan-mp-seed-input">
                        <input id="caravanSeed" name="caravanSeed" type="number" min="1" max="999999" placeholder="random"
                               inputmode="numeric" autocomplete="off" />
                        <button id="caravanSeedRandom" type="button" class="caravan-mp-seed-btn" title="Roll a random seed">🎲</button>
                        <button id="caravanSeedClear" type="button" class="caravan-mp-seed-btn" title="Clear seed">✕</button>
                    </div>
                </div>
            </div>
            <button type="submit" class="btn-create caravan-mp-btn-create">
                <span>⚔️</span>
                <span>Create 1v1 Room</span>
            </button>
            <p class="caravan-mp-form__footnote">Share the room with a friend, or wait for an opponent in the list to your right.</p>
        </form>
    </section>

    <section class="panel rooms-panel caravan-mp-rooms-panel">
        <div class="rooms-header">
            <h2>Available Rooms</h2>
            <button id="refreshBtn" class="btn-refresh" title="Refresh rooms" aria-label="Refresh rooms">⟳</button>
        </div>
        <div id="roomsList" class="rooms-list"><p class="empty-msg">Loading rooms…</p></div>
    </section>

    <section class="panel room-detail-panel caravan-mp-room-detail" id="roomDetail" style="display:none;">
        <h2 id="roomTitle">Room</h2>
        <p id="roomStatus" class="room-status"></p>
        <div id="teamsGrid" class="teams-grid"></div>
        <div class="room-actions">
            <button id="leaveRoomBtn" class="btn-leave">Leave Room</button>
        </div>
    </section>
</main>`;
}
