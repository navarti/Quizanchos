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
        buildCreateRoomRequest: (minigameType) => {
            return {
                minigameType,
                maxPlayers: 2,
                teamCount: 2, // 1 player per team — head-to-head
                gameParameters: {},
            };
        },
    });
});

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
    <section class="panel create-panel">
        <h1>Caravan — Multiplayer</h1>
        <p class="subtitle">Head-to-head card battle from Fallout: New Vegas. Build three caravans worth 21–26 before your opponent does.</p>
        <ul class="caravan-mp-rules">
            <li>Number cards 2–10 must be placed in strict ascending or descending order on a column; matching the suit lets you reverse direction. Aces can open a column.</li>
            <li>Face cards attach to existing slots — Jack discards a card, Queen flips direction and changes the active suit, King doubles the slot's value, Joker on an Ace nukes that suit, Joker on a number nukes that value across the whole table.</li>
        </ul>
        <form id="createRoomForm" class="create-form">
            <button type="submit" class="btn-create">Create Room (1v1)</button>
        </form>
    </section>
    <section class="panel rooms-panel">
        <div class="rooms-header">
            <h2>Available Rooms</h2>
            <button id="refreshBtn" class="btn-refresh" title="Refresh">⟳</button>
        </div>
        <div id="roomsList" class="rooms-list"><p class="empty-msg">Loading rooms...</p></div>
    </section>
    <section class="panel room-detail-panel" id="roomDetail" style="display:none;">
        <h2 id="roomTitle">Room</h2>
        <p id="roomStatus" class="room-status"></p>
        <div id="teamsGrid" class="teams-grid"></div>
        <div class="room-actions">
            <button id="leaveRoomBtn" class="btn-leave">Leave Room</button>
        </div>
    </section>
</main>`;
}
