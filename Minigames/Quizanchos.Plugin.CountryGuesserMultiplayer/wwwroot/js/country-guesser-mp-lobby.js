// Country Guesser Multiplayer — lobby. Uses the host's RoomLobbyBase
// (loaded via the descriptor's LobbyScripts) which talks to /api/GameRoom
// and the /hubs/room SignalR hub to manage room creation, joining, team
// switching, and auto-launching when full.
document.addEventListener('DOMContentLoaded', async () => {
    if (typeof RoomLobbyBase === 'undefined') {
        console.error('[CGMP] RoomLobbyBase not loaded — check LobbyScripts order in descriptor.');
        return;
    }

    await RoomLobbyBase.initialize({
        minigameType: window.minigameConfig.minigameTypeId,
        gameUrlTemplate: window.minigameConfig.gameUrlTemplate,
        ensureLayout: ensureLobbyLayout,
        buildCreateRoomRequest: (minigameType) => {
            const playerCount = parseInt(document.getElementById('playerCount').value, 10);

            return {
                minigameType,
                maxPlayers: playerCount,
                teamCount: playerCount, // 1 player per team — free-for-all
                gameParameters: {
                    totalCards: parseInt(document.getElementById('totalCards').value, 10),
                    secondsPerCard: parseInt(document.getElementById('secondsPerCard').value, 10),
                    maxDistanceKm: parseInt(document.getElementById('maxDistanceKm').value, 10),
                },
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
<main class="lobby-container">
    <section class="panel create-panel">
        <h1>Country Guesser — Multiplayer</h1>
        <p class="subtitle">Race other players to point out countries on the world map.</p>
        <form id="createRoomForm" class="create-form">
            <div class="form-row">
                <div class="form-group">
                    <label for="playerCount">Players</label>
                    <select id="playerCount" class="form-control">
                        <option value="2" selected>2</option>
                        <option value="3">3</option>
                        <option value="4">4</option>
                        <option value="5">5</option>
                        <option value="6">6</option>
                    </select>
                </div>
                <div class="form-group">
                    <label for="totalCards">Cards</label>
                    <select id="totalCards" class="form-control">
                        <option value="3">Three</option>
                        <option value="5" selected>Five</option>
                        <option value="10">Ten</option>
                        <option value="15">Fifteen</option>
                    </select>
                </div>
                <div class="form-group">
                    <label for="secondsPerCard">Seconds / card</label>
                    <select id="secondsPerCard" class="form-control">
                        <option value="15">Fifteen</option>
                        <option value="20" selected>Twenty</option>
                        <option value="30">Thirty</option>
                        <option value="45">Forty-five</option>
                    </select>
                </div>
                <div class="form-group">
                    <label for="maxDistanceKm">Allowed radius (km)</label>
                    <select id="maxDistanceKm" class="form-control">
                        <option value="300">300 km — Sharp</option>
                        <option value="600" selected>600 km — Standard</option>
                        <option value="900">900 km — Easy</option>
                        <option value="1500">1500 km — Casual</option>
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
