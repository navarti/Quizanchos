document.addEventListener('DOMContentLoaded', async () => {
    const root = document.getElementById('minigame-root');
    if (!root) return;

    ensureLayout(root);

    const gameId = window.minigameConfig?.gameId;
    if (!gameId) {
        root.innerHTML = '<p>Game ID missing.</p>';
        return;
    }

    const userId = document.body.getAttribute('data-user-id');
    if (!userId) {
        root.innerHTML = '<p>Please sign in to play.</p>';
        return;
    }

    await refreshState(gameId, userId);

    document.getElementById('refreshTtrState')?.addEventListener('click', () => refreshState(gameId, userId));
    document.getElementById('finishSessionBtn')?.addEventListener('click', () => finishSession(gameId, userId));
    document.getElementById('drawDeckCard')?.addEventListener('click', () => makeMove(gameId, userId, 0, { drawSources: ['deck', 'deck'] }));
    document.getElementById('drawTickets')?.addEventListener('click', () => makeMove(gameId, userId, 2, {}));
    document.getElementById('claimRouteBtn')?.addEventListener('click', () => {
        const routeId = document.getElementById('routeSelect').value;
        const color = document.getElementById('claimColor').value;
        makeMove(gameId, userId, 1, { routeId, color });
    });
    document.getElementById('placeStationBtn')?.addEventListener('click', () => {
        const stationCity = document.getElementById('stationCity').value.trim();
        const color = document.getElementById('stationColor').value;
        makeMove(gameId, userId, 4, { stationCity, color });
    });
    document.getElementById('keepTicketsBtn')?.addEventListener('click', () => {
        const selected = [...document.querySelectorAll('input[name="ticketKeep"]:checked')].map(x => parseInt(x.value, 10));
        makeMove(gameId, userId, 3, { keepTicketIndexes: selected });
    });
});

function ensureLayout(root) {
    root.innerHTML = `
<div class="ttr-game">
  <h1>Ticket to Ride: Europe</h1>
  <div class="ttr-toolbar">
    <button id="refreshTtrState">Refresh</button>
    <button id="finishSessionBtn">Finish session</button>
    <a href="${window.minigameConfig.lobbyUrl}" class="btn-back">Back to Lobby</a>
  </div>

  <div id="ttrStatus" class="ttr-status"></div>

  <section>
    <h2>Your hand</h2>
    <div id="handCards"></div>
  </section>

  <section>
    <h2>Face-up cards</h2>
    <div id="faceUpCards"></div>
    <button id="drawDeckCard">Draw 2 from deck</button>
  </section>

  <section>
    <h2>Claim route</h2>
    <label>Route:</label>
    <select id="routeSelect"></select>
    <label>Color:</label>
    <select id="claimColor"></select>
    <button id="claimRouteBtn">Claim route</button>
  </section>

  <section>
    <h2>Destination tickets</h2>
    <button id="drawTickets">Draw destination tickets</button>
    <div id="myTickets"></div>
    <div id="pendingTickets"></div>
    <button id="keepTicketsBtn">Keep selected tickets</button>
  </section>

  <section>
    <h2>Stations</h2>
    <input id="stationCity" placeholder="City name" />
    <select id="stationColor"></select>
    <button id="placeStationBtn">Place station</button>
  </section>

  <section>
    <h2>Routes overview</h2>
    <div id="routesTable"></div>
  </section>
</div>`;

    fillColorSelect('claimColor');
    fillColorSelect('stationColor');
}

async function finishSession(gameId, userId) {
    try {
        await ticketToRideEuropeClient.finishGame(gameId);
        await refreshState(gameId, userId);
    } catch (error) {
        alert(error.message || 'Failed to finish session');
    }
}

async function makeMove(gameId, userId, action, payload) {
    try {
        const move = ticketToRideEuropeClient.createMoveObject(action, payload);
        await ticketToRideEuropeClient.makeMove(gameId, userId, move);
        await refreshState(gameId, userId);
    } catch (error) {
        alert(error.message || 'Move failed');
    }
}

async function drawFaceUp(gameId, userId, index) {
    try {
        const btn = document.querySelectorAll('#faceUpCards button')[index];
        const isLocomotive = btn?.textContent?.toLowerCase() === 'locomotive';
        const sources = isLocomotive ? [`faceup:${index}`] : [`faceup:${index}`, 'deck'];
        await makeMove(gameId, userId, 0, { drawSources: sources });
    } catch (error) {
        alert(error.message || 'Draw failed');
    }
}

async function refreshState(gameId, userId) {
    const data = await ticketToRideEuropeClient.getState(gameId);
    const state = data.state || {};

    const me = state.playerStates?.[userId] || state.PlayerStates?.[userId];
    const routes = state.routes || state.Routes || [];
    const faceUp = state.faceUpCards || state.FaceUpCards || [];
    const currentPlayerId = state.currentPlayerId || state.CurrentPlayerId;
    const pendingChoices = state.pendingDestinationChoices || state.PendingDestinationChoices || [];

    document.getElementById('ttrStatus').textContent =
        `Turn: ${state.turnNumber ?? state.TurnNumber} | Current: ${currentPlayerId} | Last action: ${state.lastActionSummary ?? state.LastActionSummary ?? ''}`;

    renderHand(me);
    renderFaceUp(faceUp, gameId, userId);
    renderRoutes(routes);
    renderRouteSelect(routes);
    renderTickets(me, pendingChoices);

    const isMyTurn = currentPlayerId === userId && !data.isFinished;
    setControlsState(!isMyTurn);

    if (data.isFinished) {
        document.getElementById('ttrStatus').textContent += ` | Game finished. Winner: ${data.winner}`;
        setControlsState(true);
    }

    const finishSessionBtn = document.getElementById('finishSessionBtn');
    if (finishSessionBtn) {
        finishSessionBtn.disabled = !!data.isFinished;
    }
}

function setControlsState(disabled) {
    [
        'drawDeckCard',
        'drawTickets',
        'claimRouteBtn',
        'placeStationBtn',
        'keepTicketsBtn'
    ].forEach(id => {
        const el = document.getElementById(id);
        if (el) el.disabled = disabled;
    });
}

function renderHand(me) {
    const hand = me?.hand || me?.Hand || [];
    const trains = me?.trainsRemaining ?? me?.TrainsRemaining ?? 0;
    const stations = me?.stationsRemaining ?? me?.StationsRemaining ?? 0;
    const score = me?.score ?? me?.Score ?? 0;

    const counts = {};
    for (const card of hand) {
        const key = colorName(card);
        counts[key] = (counts[key] || 0) + 1;
    }

    const info = Object.entries(counts).map(([k, v]) => `${k}: ${v}`).join(' | ') || 'No cards';
    document.getElementById('handCards').textContent = `Score: ${score} | Trains: ${trains} | Stations: ${stations} | ${info}`;
}

function renderFaceUp(cards, gameId, userId) {
    const container = document.getElementById('faceUpCards');
    container.innerHTML = '';

    cards.forEach((card, index) => {
        const btn = document.createElement('button');
        btn.type = 'button';
        btn.textContent = colorName(card);
        btn.addEventListener('click', () => drawFaceUp(gameId, userId, index));
        container.appendChild(btn);
    });
}

function renderRoutes(routes) {
    const container = document.getElementById('routesTable');
    const rows = routes
        .map(r => {
            const owner = r.claimedBy || r.ClaimedBy || 'Free';
            const color = colorName(r.color ?? r.Color);
            const tunnel = (r.isTunnel ?? r.IsTunnel) ? 'Tunnel' : '';
            const ferry = (r.ferryLocomotivesRequired ?? r.FerryLocomotivesRequired ?? 0) > 0
                ? `Ferry:${r.ferryLocomotivesRequired ?? r.FerryLocomotivesRequired}`
                : '';

            return `<tr><td>${r.cityA ?? r.CityA}</td><td>${r.cityB ?? r.CityB}</td><td>${r.length ?? r.Length}</td><td>${color}</td><td>${tunnel} ${ferry}</td><td>${owner}</td></tr>`;
        })
        .join('');

    container.innerHTML = `<table><thead><tr><th>From</th><th>To</th><th>Len</th><th>Color</th><th>Type</th><th>Owner</th></tr></thead><tbody>${rows}</tbody></table>`;
}

function renderRouteSelect(routes) {
    const select = document.getElementById('routeSelect');
    const freeRoutes = routes.filter(r => !(r.claimedBy || r.ClaimedBy));
    select.innerHTML = freeRoutes
        .map(r => `<option value="${r.id ?? r.Id}">${r.cityA ?? r.CityA} - ${r.cityB ?? r.CityB} (${r.length ?? r.Length})</option>`)
        .join('');
}

function renderTickets(me, pendingChoices) {
    const myTickets = me?.destinationTickets || me?.DestinationTickets || [];
    document.getElementById('myTickets').innerHTML = myTickets
        .map(t => `<div>${t.cityA ?? t.CityA} → ${t.cityB ?? t.CityB} (${t.points ?? t.Points})</div>`)
        .join('');

    const pendingHtml = pendingChoices
        .map((t, i) => `<label><input type="checkbox" name="ticketKeep" value="${i}" /> ${t.cityA ?? t.CityA} → ${t.cityB ?? t.CityB} (${t.points ?? t.Points})</label>`)
        .join('<br/>');

    document.getElementById('pendingTickets').innerHTML = pendingChoices.length
        ? `<h4>Pending choices (keep at least 1)</h4>${pendingHtml}`
        : '';
}

function fillColorSelect(id) {
    const select = document.getElementById(id);
    select.innerHTML = ['Red', 'Blue', 'Green', 'Yellow', 'Black', 'White', 'Orange', 'Purple']
        .map(c => `<option value="${c}">${c}</option>`)
        .join('');
}

function colorName(value) {
    const map = {
        0: 'Red',
        1: 'Blue',
        2: 'Green',
        3: 'Yellow',
        4: 'Black',
        5: 'White',
        6: 'Orange',
        7: 'Purple',
        8: 'Locomotive'
    };

    if (value === null || value === undefined || value === '') return 'Any';
    if (typeof value === 'number') return map[value] || value;
    return value;
}
