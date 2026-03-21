// Quiz Multiplayer Lobby
document.addEventListener('DOMContentLoaded', async () => {
    await RoomLobbyBase.initialize({
        minigameType: window.minigameConfig.minigameTypeId,
        gameUrlTemplate: window.minigameConfig.gameUrlTemplate,
        ensureLayout: ensureQuizMultiplayerLobbyLayout,
        onAfterLayout: loadCategories,
        buildCreateRoomRequest: (minigameType) => {
            const teamCount = parseInt(document.getElementById('teamCount').value, 10);
            const playersPerTeam = parseInt(document.getElementById('playersPerTeam').value, 10);

            return {
                minigameType,
                maxPlayers: teamCount * playersPerTeam,
                teamCount,
                gameParameters: {
                    categoryId: document.getElementById('categorySelect').value,
                    totalCards: parseInt(document.getElementById('totalCards').value, 10),
                    secondsPerCard: parseInt(document.getElementById('secondsPerCard').value, 10),
                    optionCount: parseInt(document.getElementById('optionCount').value, 10),
                    gameLevel: parseInt(document.getElementById('gameLevel').value, 10)
                }
            };
        }
    });
});

async function loadCategories() {
    try {
        const response = await fetch('/QuizCategory/GetAll');
        if (!response.ok) {
            return;
        }

        const categories = await response.json();
        const select = document.getElementById('categorySelect');
        if (!select) {
            return;
        }

        select.innerHTML = '';
        categories.forEach(category => {
            const option = document.createElement('option');
            option.value = category.id || category.Id;
            option.textContent = category.name || category.Name;
            select.appendChild(option);
        });
    } catch (_) {
    }
}

function ensureQuizMultiplayerLobbyLayout() {
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
        <h1>Quiz Multiplayer</h1>
        <p class="subtitle">Create a new room or join an existing one</p>
        <form id="createRoomForm" class="create-form">
            <div class="form-row">
                <div class="form-group"><label for="categorySelect">Category</label><select id="categorySelect" class="form-control"><option value="">Loading...</option></select></div>
                <div class="form-group"><label for="teamCount">Teams</label><select id="teamCount" class="form-control"><option value="2" selected>2</option><option value="3">3</option><option value="4">4</option></select></div>
                <div class="form-group"><label for="playersPerTeam">Players / team</label><select id="playersPerTeam" class="form-control"><option value="1">1</option><option value="2" selected>2</option><option value="3">3</option><option value="4">4</option><option value="5">5</option></select></div>
            </div>
            <div class="form-row">
                <div class="form-group"><label for="totalCards">Cards</label><select id="totalCards" class="form-control"><option value="5">Five</option><option value="10" selected>Ten</option><option value="15">Fifteen</option></select></div>
                <div class="form-group"><label for="secondsPerCard">Seconds / card</label><select id="secondsPerCard" class="form-control"><option value="10">Ten</option><option value="15" selected>Fifteen</option><option value="20">Twenty</option></select></div>
                <div class="form-group"><label for="optionCount">Options</label><select id="optionCount" class="form-control"><option value="2">Two</option><option value="3">Three</option><option value="4" selected>Four</option></select></div>
                <div class="form-group"><label for="gameLevel">Level</label><select id="gameLevel" class="form-control"><option value="0">Easy</option><option value="1">Medium</option><option value="2">Hard</option></select></div>
            </div>
            <button type="submit" class="btn-create">Create Room</button>
        </form>
    </section>
    <section class="panel rooms-panel">
        <div class="rooms-header"><h2>Available Rooms</h2><button id="refreshBtn" class="btn-refresh" title="Refresh">?</button></div>
        <div id="roomsList" class="rooms-list"><p class="empty-msg">Loading rooms...</p></div>
    </section>
    <section class="panel room-detail-panel" id="roomDetail" style="display:none;">
        <h2 id="roomTitle">Room</h2>
        <p id="roomStatus" class="room-status"></p>
        <div id="teamsGrid" class="teams-grid"></div>
        <div class="room-actions"><button id="leaveRoomBtn" class="btn-leave">Leave Room</button></div>
    </section>
</main>`;
}
