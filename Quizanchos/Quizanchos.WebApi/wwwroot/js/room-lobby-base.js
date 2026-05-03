window.RoomLobbyBase = (function () {
    async function initialize(options) {
        const minigameType = options.minigameType;
        const gameUrlTemplate = options.gameUrlTemplate;
        const api = options.apiBase || '/api/GameRoom';
        let currentRoomId = null;

        options.ensureLayout();

        const userId = document.body.getAttribute('data-user-id');
        if (!userId) {
            alert('Please log in to play.');
            return;
        }

        if (options.onAfterLayout) {
            await options.onAfterLayout();
        }

        const roomHub = new signalR.HubConnectionBuilder()
            .withUrl('/hubs/room')
            .withAutomaticReconnect()
            .build();

        roomHub.on('RoomUpdated', (room) => {
            if (currentRoomId && getRoomId(room) === currentRoomId) {
                renderRoomDetail(room);
            }
        });

        roomHub.on('PlayerJoinedRoom', () => refreshRoomDetail());
        roomHub.on('PlayerLeftRoom', () => refreshRoomDetail());
        roomHub.on('RoomClosed', () => leaveCurrentRoom(true));
        roomHub.on('GameStarting', (payload) => {
            const gameId = payload.gameId || payload.GameId;
            if (gameId) {
                window.location.href = gameUrlTemplate.replace('{gameId}', gameId);
            }
        });

        try {
            await roomHub.start();
        } catch (error) {
            console.error('SignalR room hub error', error);
        }

        document.getElementById('refreshBtn')?.addEventListener('click', loadRooms);
        document.getElementById('createRoomForm')?.addEventListener('submit', createRoom);
        document.getElementById('leaveRoomBtn')?.addEventListener('click', () => leaveCurrentRoom(false));

        await loadRooms();

        setInterval(tickExpiry, 1000);

        function tickExpiry() {
            const elements = document.querySelectorAll('[data-expires-at]');
            let shouldRefreshList = false;

            elements.forEach(el => {
                const secsLeft = secondsRemaining(el.dataset.expiresAt);
                el.textContent = formatTimeRemaining(secsLeft);
                el.classList.toggle('expiring-soon', secsLeft > 0 && secsLeft < 60);

                if (secsLeft <= 0 && el.closest('#roomsList') && !el.dataset.expiredHandled) {
                    el.dataset.expiredHandled = '1';
                    shouldRefreshList = true;
                }
            });

            if (shouldRefreshList) {
                loadRooms();
            }
        }

        async function createRoom(event) {
            event.preventDefault();

            const body = options.buildCreateRoomRequest(minigameType);
            const response = await fetch(api, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(body)
            });

            if (!response.ok) {
                alert(await tryReadError(response));
                return;
            }

            const room = await response.json();
            await enterRoom(getRoomId(room));
        }

        async function loadRooms() {
            const list = document.getElementById('roomsList');
            const response = await fetch(`${api}?minigameType=${minigameType}`);

            if (!response.ok) {
                list.innerHTML = '<p class="empty-msg">Failed to load rooms.</p>';
                return;
            }

            const rooms = await response.json();
            if (!rooms.length) {
                list.innerHTML = '<p class="empty-msg">No rooms available. Create one!</p>';
                return;
            }

            list.innerHTML = rooms.map(roomCardHtml).join('');

            list.querySelectorAll('.btn-join').forEach(btn => {
                btn.addEventListener('click', async () => {
                    const roomId = btn.dataset.roomId;
                    const teamIndex = parseInt(btn.dataset.teamIndex, 10);

                    const response = await fetch(`${api}/${roomId}/join`, {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify({ teamIndex })
                    });

                    if (!response.ok) {
                        alert(await tryReadError(response));
                        return;
                    }

                    await enterRoom(roomId);
                });
            });
        }

        async function enterRoom(roomId) {
            currentRoomId = roomId;

            try {
                await roomHub.invoke('SubscribeToRoom', roomId);
            } catch (_) {
            }

            document.querySelector('.create-panel').style.display = 'none';
            document.querySelector('.rooms-panel').style.display = 'none';
            document.getElementById('roomDetail').style.display = 'block';

            await refreshRoomDetail();
        }

        async function refreshRoomDetail() {
            if (!currentRoomId) {
                return;
            }

            const response = await fetch(`${api}/${currentRoomId}`);
            if (!response.ok) {
                return;
            }

            const room = await response.json();
            renderRoomDetail(room);

            const launchedGameId = room.launchedGameId || room.LaunchedGameId;
            if (launchedGameId) {
                window.location.href = gameUrlTemplate.replace('{gameId}', launchedGameId);
            }
        }

        async function leaveCurrentRoom(skipApi) {
            if (currentRoomId && !skipApi) {
                await fetch(`${api}/${currentRoomId}/leave`, { method: 'POST' });
            }

            if (currentRoomId) {
                try {
                    await roomHub.invoke('UnsubscribeFromRoom', currentRoomId);
                } catch (_) {
                }
            }

            currentRoomId = null;
            document.getElementById('roomDetail').style.display = 'none';
            document.querySelector('.create-panel').style.display = '';
            document.querySelector('.rooms-panel').style.display = '';
            await loadRooms();
        }

        function roomCardHtml(room) {
            const roomId = getRoomId(room);
            const teams = room.teams || room.Teams || [];
            const current = room.currentPlayerCount ?? room.CurrentPlayerCount ?? 0;
            const max = room.maxPlayers ?? room.MaxPlayers ?? 0;
            const expiresAt = room.expiresAtUtc ?? room.ExpiresAtUtc;
            const secsLeft = secondsRemaining(expiresAt);
            const expiresClass = secsLeft > 0 && secsLeft < 60 ? ' expiring-soon' : '';

            const teamsHtml = teams.map(team => {
                const teamIndex = team.teamIndex ?? team.TeamIndex;
                const teamName = team.name || team.Name || `Team ${teamIndex + 1}`;
                const players = team.players || team.Players || [];
                const teamMax = team.maxSize ?? team.MaxSize ?? 0;
                const full = team.isFull ?? team.IsFull;

                return `<div class="room-team">
                            <span>${teamName} (${players.length}/${teamMax})</span>
                            ${full
                                ? '<span class="tag-full">Full</span>'
                                : `<button class="btn-join" data-room-id="${roomId}" data-team-index="${teamIndex}">Join</button>`}
                        </div>`;
            }).join('');

            return `<div class="room-card">
                        <div class="room-card-header">
                            <span class="room-id">${roomId.substring(0, 8)}...</span>
                            <span class="room-expires${expiresClass}" data-expires-at="${expiresAt}" title="Time left to join">${formatTimeRemaining(secsLeft)}</span>
                            <span class="room-players">${current}/${max} players</span>
                        </div>
                        <div class="room-teams">${teamsHtml}</div>
                    </div>`;
        }

        function renderRoomDetail(room) {
            const roomId = getRoomId(room);
            const status = room.status ?? room.Status ?? 0;
            const current = room.currentPlayerCount ?? room.CurrentPlayerCount ?? 0;
            const max = room.maxPlayers ?? room.MaxPlayers ?? 0;
            const teams = room.teams || room.Teams || [];
            const nicknames = room.playerNicknames || room.PlayerNicknames || {};
            const expiresAt = room.expiresAtUtc ?? room.ExpiresAtUtc;
            const statusLabels = ['Waiting for players', 'Launching', 'Game started', 'Closed'];

            document.getElementById('roomTitle').textContent = `Room ${roomId.substring(0, 8)}...`;

            const statusText = `${statusLabels[status] || 'Unknown'} • ${current}/${max} players`;
            const statusEl = document.getElementById('roomStatus');
            const showExpiry = status === 0 && expiresAt;
            if (showExpiry) {
                const secsLeft = secondsRemaining(expiresAt);
                const expiresClass = secsLeft > 0 && secsLeft < 60 ? ' expiring-soon' : '';
                statusEl.innerHTML = `${statusText} • <span class="room-expires${expiresClass}" data-expires-at="${expiresAt}" title="Time left to join">${formatTimeRemaining(secsLeft)}</span>`;
            } else {
                statusEl.textContent = statusText;
            }

            const grid = document.getElementById('teamsGrid');
            grid.innerHTML = teams.map(team => {
                const teamIndex = team.teamIndex ?? team.TeamIndex ?? 0;
                const teamName = team.name || team.Name || `Team ${teamIndex + 1}`;
                const players = team.players || team.Players || [];
                const maxSize = team.maxSize ?? team.MaxSize ?? 0;

                const playersHtml = players.map(p => {
                    const display = nicknames[p] || `${p.substring(0, 8)}...`;
                    return `<div class="player-slot filled">${escapeText(display)}</div>`;
                }).join('');
                const emptyHtml = Array(Math.max(0, maxSize - players.length)).fill('<div class="player-slot empty">...</div>').join('');

                return `<div class="team-card team-${teamIndex}">
                            <h3>${teamName} <span class="team-count">(${players.length}/${maxSize})</span></h3>
                            ${playersHtml}${emptyHtml}
                        </div>`;
            }).join('');
        }
    }

    function escapeText(s) {
        return String(s).replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
    }

    function getRoomId(room) {
        return room.roomId || room.RoomId;
    }

    function secondsRemaining(expiresAt) {
        if (!expiresAt) return 0;
        return Math.max(0, Math.floor((new Date(expiresAt) - new Date()) / 1000));
    }

    function formatTimeRemaining(secsLeft) {
        if (secsLeft <= 0) return 'Expired';
        const m = Math.floor(secsLeft / 60);
        const s = secsLeft % 60;
        return `${m}m ${s.toString().padStart(2, '0')}s`;
    }

    async function tryReadError(response) {
        try {
            const payload = await response.json();
            return payload.message || payload.Message || 'Request failed';
        } catch (_) {
            return 'Request failed';
        }
    }

    return { initialize };
})();
