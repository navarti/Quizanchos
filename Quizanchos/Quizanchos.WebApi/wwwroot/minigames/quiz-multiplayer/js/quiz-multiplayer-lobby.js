// Quiz Multiplayer Lobby
document.addEventListener('DOMContentLoaded', async () => {
    ensureQuizMultiplayerLobbyLayout();

    const MINIGAME_TYPE = window.minigameConfig.minigameTypeId;
    const GAME_URL_TEMPLATE = window.minigameConfig.gameUrlTemplate;
    const API = '/api/GameRoom';
    let currentRoomId = null;

    function navigateToGame(gameId) {
        window.location.href = GAME_URL_TEMPLATE.replace('{gameId}', gameId);
    }

    // ?? SignalR ??????????????????????????????????????????????????????
    const roomHub = new signalR.HubConnectionBuilder()
        .withUrl('/hubs/room')
        .withAutomaticReconnect()
        .build();

    roomHub.on('RoomUpdated', (room) => {
        if (currentRoomId && room.roomId === currentRoomId) {
            renderRoomDetail(room);
        }
    });

function ensureQuizMultiplayerLobbyLayout() {
    if (document.getElementById('createRoomForm')) return;

    const root = document.getElementById('minigame-root');
    if (!root) return;

    root.innerHTML = `
<main class="lobby-container">
    <section class="panel create-panel">
        <h1>Quiz Multiplayer</h1>
        <p class="subtitle">Create a new room or join an existing one</p>
        <form id="createRoomForm" class="create-form">
            <div class="form-row">
                <div class="form-group"><label for="categorySelect">Category</label><select id="categorySelect" class="form-control"><option value="">Loading…</option></select></div>
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
        <div id="roomsList" class="rooms-list"><p class="empty-msg">Loading rooms…</p></div>
    </section>
    <section class="panel room-detail-panel" id="roomDetail" style="display:none;">
        <h2 id="roomTitle">Room</h2>
        <p id="roomStatus" class="room-status"></p>
        <div id="teamsGrid" class="teams-grid"></div>
        <div class="room-actions"><button id="leaveRoomBtn" class="btn-leave">Leave Room</button></div>
    </section>
</main>`;
}

    roomHub.on('PlayerJoinedRoom', (_) => {
        if (currentRoomId) refreshRoomDetail();
    });

    roomHub.on('PlayerLeftRoom', (_) => {
        if (currentRoomId) refreshRoomDetail();
    });

    roomHub.on('GameStarting', (payload) => {
        const gameId = payload.gameId || payload.GameId;
        if (gameId) {
            navigateToGame(gameId);
        }
    });

    roomHub.on('RoomClosed', (_) => {
        alert('The room has been closed.');
        leaveCurrentRoom(true);
    });

    try {
        await roomHub.start();
    } catch (err) {
        console.error('SignalR room hub error', err);
    }

    // ?? Load categories ?????????????????????????????????????????????
    try {
        const resp = await fetch('/QuizCategory/GetAll');
        if (resp.ok) {
            const categories = await resp.json();
            const sel = document.getElementById('categorySelect');
            sel.innerHTML = '';
            categories.forEach(c => {
                const opt = document.createElement('option');
                opt.value = c.id || c.Id;
                opt.textContent = c.name || c.Name;
                sel.appendChild(opt);
            });
        }
    } catch (_) { /* ignore */ }

    // ?? Load rooms ??????????????????????????????????????????????????
    await loadRooms();

    document.getElementById('refreshBtn').addEventListener('click', loadRooms);

    // ?? Create room ?????????????????????????????????????????????????
    document.getElementById('createRoomForm').addEventListener('submit', async (e) => {
        e.preventDefault();

        const teamCount = parseInt(document.getElementById('teamCount').value);
        const playersPerTeam = parseInt(document.getElementById('playersPerTeam').value);
        const maxPlayers = teamCount * playersPerTeam;

        const body = {
            minigameType: MINIGAME_TYPE,
            maxPlayers: maxPlayers,
            teamCount: teamCount,
            gameParameters: {
                categoryId: document.getElementById('categorySelect').value,
                totalCards: parseInt(document.getElementById('totalCards').value),
                secondsPerCard: parseInt(document.getElementById('secondsPerCard').value),
                optionCount: parseInt(document.getElementById('optionCount').value),
                gameLevel: parseInt(document.getElementById('gameLevel').value)
            }
        };

        try {
            const resp = await fetch(API, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(body)
            });

            if (!resp.ok) {
                const err = await resp.json();
                alert(err.message || 'Failed to create room');
                return;
            }

            const room = await resp.json();
            await enterRoom(room.roomId || room.RoomId);
        } catch (err) {
            alert('Error creating room: ' + err.message);
        }
    });

    // ?? Leave room button ???????????????????????????????????????????
    document.getElementById('leaveRoomBtn').addEventListener('click', () => leaveCurrentRoom(false));

    // ?? Helpers ??????????????????????????????????????????????????????

    async function loadRooms() {
        const container = document.getElementById('roomsList');
        try {
            const resp = await fetch(`${API}?minigameType=${MINIGAME_TYPE}`);
            if (!resp.ok) {
                container.innerHTML = '<p class="empty-msg">Failed to load rooms.</p>';
                return;
            }
            const rooms = await resp.json();
            if (rooms.length === 0) {
                container.innerHTML = '<p class="empty-msg">No rooms available. Create one!</p>';
                return;
            }
            container.innerHTML = rooms.map(r => roomCardHtml(r)).join('');

            // Attach join buttons
            container.querySelectorAll('.btn-join').forEach(btn => {
                btn.addEventListener('click', async () => {
                    const roomId = btn.dataset.roomId;
                    const teamIndex = parseInt(btn.dataset.teamIndex);
                    await joinRoom(roomId, teamIndex);
                });
            });
        } catch (err) {
            container.innerHTML = '<p class="empty-msg">Error loading rooms.</p>';
        }
    }

    function roomCardHtml(room) {
        const id = room.roomId || room.RoomId;
        const teams = room.teams || room.Teams || [];
        const current = room.currentPlayerCount ?? room.CurrentPlayerCount ?? 0;
        const max = room.maxPlayers ?? room.MaxPlayers ?? 0;

        const teamsHtml = teams.map(t => {
            const tIdx = t.teamIndex ?? t.TeamIndex ?? 0;
            const tName = t.name || t.Name || `Team ${tIdx + 1}`;
            const players = t.players || t.Players || [];
            const tMax = t.maxSize ?? t.MaxSize ?? 0;
            const full = t.isFull ?? t.IsFull ?? false;
            return `<div class="room-team">
                        <span>${tName} (${players.length}/${tMax})</span>
                        ${full
                            ? '<span class="tag-full">Full</span>'
                            : `<button class="btn-join" data-room-id="${id}" data-team-index="${tIdx}">Join</button>`}
                    </div>`;
        }).join('');

        return `<div class="room-card">
                    <div class="room-card-header">
                        <span class="room-id">${id.substring(0, 8)}…</span>
                        <span class="room-players">${current}/${max} players</span>
                    </div>
                    <div class="room-teams">${teamsHtml}</div>
                </div>`;
    }

    async function joinRoom(roomId, teamIndex) {
        try {
            const resp = await fetch(`${API}/${roomId}/join`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ teamIndex: teamIndex })
            });
            if (!resp.ok) {
                const err = await resp.json();
                alert(err.message || 'Failed to join room');
                return;
            }
            await enterRoom(roomId);
        } catch (err) {
            alert('Error joining room: ' + err.message);
        }
    }

    async function enterRoom(roomId) {
        currentRoomId = roomId;

        // Subscribe to real-time updates
        try {
            await roomHub.invoke('SubscribeToRoom', roomId);
        } catch (_) { /* ignore */ }

        // Show detail panel, hide others
        document.querySelector('.create-panel').style.display = 'none';
        document.querySelector('.rooms-panel').style.display = 'none';
        document.getElementById('roomDetail').style.display = 'block';

        await refreshRoomDetail();
    }

    async function refreshRoomDetail() {
        if (!currentRoomId) return;
        try {
            const resp = await fetch(`${API}/${currentRoomId}`);
            if (!resp.ok) return;
            const room = await resp.json();
            renderRoomDetail(room);

            // If game already launched, navigate
            const gameId = room.launchedGameId || room.LaunchedGameId;
            if (gameId) {
                navigateToGame(gameId);
            }
        } catch (_) { /* ignore */ }
    }

    function renderRoomDetail(room) {
        const id = room.roomId || room.RoomId;
        const status = room.status ?? room.Status ?? 0;
        const current = room.currentPlayerCount ?? room.CurrentPlayerCount ?? 0;
        const max = room.maxPlayers ?? room.MaxPlayers ?? 0;
        const teams = room.teams || room.Teams || [];

        document.getElementById('roomTitle').textContent = `Room ${id.substring(0, 8)}…`;

        const statusLabels = ['Waiting for players', 'Launching…', 'Game started', 'Closed'];
        document.getElementById('roomStatus').textContent = `${statusLabels[status] || 'Unknown'} — ${current}/${max} players`;

        const grid = document.getElementById('teamsGrid');
        grid.innerHTML = teams.map(t => {
            const tIdx = t.teamIndex ?? t.TeamIndex ?? 0;
            const tName = t.name || t.Name || `Team ${tIdx + 1}`;
            const players = t.players || t.Players || [];
            const tMax = t.maxSize ?? t.MaxSize ?? 0;

            const playersHtml = players.length > 0
                ? players.map(p => `<div class="player-slot filled">${p.substring(0, 8)}…</div>`).join('')
                : '<div class="player-slot empty">—</div>';
            const emptySlots = Math.max(0, tMax - players.length);
            const emptySlotsHtml = Array(emptySlots).fill('<div class="player-slot empty">—</div>').join('');

            return `<div class="team-card team-${tIdx}">
                        <h3>${tName} <span class="team-count">(${players.length}/${tMax})</span></h3>
                        ${playersHtml}${emptySlotsHtml}
                    </div>`;
        }).join('');
    }

    async function leaveCurrentRoom(skipApi) {
        if (currentRoomId && !skipApi) {
            try {
                await fetch(`${API}/${currentRoomId}/leave`, { method: 'POST' });
            } catch (_) { /* ignore */ }

            try {
                await roomHub.invoke('UnsubscribeFromRoom', currentRoomId);
            } catch (_) { /* ignore */ }
        }

        currentRoomId = null;
        document.getElementById('roomDetail').style.display = 'none';
        document.querySelector('.create-panel').style.display = '';
        document.querySelector('.rooms-panel').style.display = '';
        await loadRooms();
    }
});
