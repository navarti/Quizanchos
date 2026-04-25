// Ticket to Ride: Europe — Lobby
document.addEventListener('DOMContentLoaded', async () => {
    await RoomLobbyBase.initialize({
        minigameType: window.minigameConfig.minigameTypeId,
        gameUrlTemplate: window.minigameConfig.gameUrlTemplate,
        ensureLayout: ensureTtrLobbyLayout,
        buildCreateRoomRequest: (minigameType) => {
            const playerCount = parseInt(document.getElementById('playerCount').value, 10);
            return {
                minigameType,
                maxPlayers: playerCount,
                teamCount: playerCount,
                gameParameters: {}
            };
        }
    });
});

function ensureTtrLobbyLayout() {
    if (document.getElementById('createRoomForm')) return;

    const root = document.getElementById('minigame-root');
    if (!root) return;

    root.innerHTML = `
<main class="ttr-lobby">
    <section class="panel create-panel">
        <h1>Ticket to Ride: Europe</h1>
        <p class="subtitle">Claim train routes across the European map and complete your destination tickets.</p>
        <form id="createRoomForm" class="create-form">
            <div class="form-row">
                <div class="form-group">
                    <label for="playerCount">Players</label>
                    <select id="playerCount" class="form-control">
                        <option value="2">2</option>
                        <option value="3" selected>3</option>
                        <option value="4">4</option>
                        <option value="5">5</option>
                    </select>
                </div>
            </div>
            <button type="submit" class="btn-create">Create Room</button>
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
