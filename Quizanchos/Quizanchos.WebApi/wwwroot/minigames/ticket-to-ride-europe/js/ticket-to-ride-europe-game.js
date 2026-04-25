// Ticket to Ride: Europe — Game page
// Renders the SVG map and player UI; talks to the server via SignalR + HTTP.

// Map data must mirror the C# TicketToRideEuropeMap class.
const TTR_MAP = {
    width: 920,
    height: 620,
    cities: [
        { id: 'edinburgh', name: 'Edinburgh', x: 200, y: 130 },
        { id: 'london', name: 'London', x: 215, y: 215 },
        { id: 'amsterdam', name: 'Amsterdam', x: 285, y: 215 },
        { id: 'essen', name: 'Essen', x: 340, y: 230 },
        { id: 'hamburg', name: 'Hamburg', x: 370, y: 195 },
        { id: 'kobenhavn', name: 'Kobenhavn', x: 400, y: 155 },
        { id: 'stockholm', name: 'Stockholm', x: 470, y: 95 },
        { id: 'petrograd', name: 'Petrograd', x: 620, y: 85 },
        { id: 'moskva', name: 'Moskva', x: 760, y: 175 },
        { id: 'smolensk', name: 'Smolensk', x: 685, y: 215 },
        { id: 'wilno', name: 'Wilno', x: 580, y: 235 },
        { id: 'riga', name: 'Riga', x: 540, y: 175 },
        { id: 'danzig', name: 'Danzig', x: 460, y: 245 },
        { id: 'berlin', name: 'Berlin', x: 405, y: 260 },
        { id: 'warszawa', name: 'Warszawa', x: 495, y: 275 },
        { id: 'kyiv', name: 'Kyiv', x: 645, y: 305 },
        { id: 'kharkov', name: 'Kharkov', x: 745, y: 305 },
        { id: 'rostov', name: 'Rostov', x: 800, y: 365 },
        { id: 'sevastopol', name: 'Sevastopol', x: 720, y: 405 },
        { id: 'bucuresti', name: 'Bucuresti', x: 580, y: 380 },
        { id: 'budapest', name: 'Budapest', x: 480, y: 355 },
        { id: 'wien', name: 'Wien', x: 425, y: 330 },
        { id: 'frankfurt', name: 'Frankfurt', x: 340, y: 285 },
        { id: 'munchen', name: 'Munchen', x: 385, y: 320 },
        { id: 'zurich', name: 'Zurich', x: 340, y: 340 },
        { id: 'venezia', name: 'Venezia', x: 390, y: 380 },
        { id: 'zagrab', name: 'Zagrab', x: 425, y: 385 },
        { id: 'sarajevo', name: 'Sarajevo', x: 480, y: 410 },
        { id: 'sofia', name: 'Sofia', x: 555, y: 430 },
        { id: 'athina', name: 'Athina', x: 540, y: 510 },
        { id: 'constantinople', name: 'Constantinople', x: 645, y: 455 },
        { id: 'smyrna', name: 'Smyrna', x: 645, y: 510 },
        { id: 'angora', name: 'Angora', x: 720, y: 470 },
        { id: 'erzurum', name: 'Erzurum', x: 815, y: 455 },
        { id: 'roma', name: 'Roma', x: 380, y: 445 },
        { id: 'brindisi', name: 'Brindisi', x: 450, y: 470 },
        { id: 'palermo', name: 'Palermo', x: 400, y: 525 },
        { id: 'marseille', name: 'Marseille', x: 280, y: 380 },
        { id: 'barcelona', name: 'Barcelona', x: 215, y: 425 },
        { id: 'madrid', name: 'Madrid', x: 130, y: 445 },
        { id: 'lisboa', name: 'Lisboa', x: 50, y: 460 },
        { id: 'cadiz', name: 'Cadiz', x: 70, y: 510 },
        { id: 'pamplona', name: 'Pamplona', x: 195, y: 380 },
        { id: 'brest', name: 'Brest', x: 175, y: 290 },
        { id: 'paris', name: 'Paris', x: 260, y: 295 },
        { id: 'dieppe', name: 'Dieppe', x: 240, y: 250 },
        { id: 'bruxelles', name: 'Bruxelles', x: 290, y: 255 }
    ],
    routes: [
        r('edinburgh', 'london', 4, 'BLACK'),
        r('london', 'dieppe', 2, 'GRAY', { ferry: 1 }),
        r('london', 'amsterdam', 2, 'GRAY', { ferry: 2 }),
        r('dieppe', 'brest', 2, 'ORANGE'),
        r('dieppe', 'paris', 1, 'PURPLE'),
        r('dieppe', 'bruxelles', 2, 'GREEN'),
        r('brest', 'paris', 3, 'BLACK'),
        r('brest', 'pamplona', 4, 'PURPLE'),
        r('paris', 'bruxelles', 2, 'YELLOW'),
        r('paris', 'frankfurt', 3, 'WHITE'),
        r('paris', 'zurich', 3, 'GRAY', { tunnel: true }),
        r('paris', 'marseille', 4, 'GRAY'),
        r('paris', 'pamplona', 4, 'BLUE'),
        r('marseille', 'zurich', 2, 'PURPLE'),
        r('marseille', 'roma', 4, 'GRAY', { tunnel: true }),
        r('marseille', 'barcelona', 4, 'GRAY'),
        r('barcelona', 'pamplona', 2, 'GRAY'),
        r('barcelona', 'madrid', 2, 'YELLOW'),
        r('madrid', 'pamplona', 3, 'BLACK'),
        r('madrid', 'lisboa', 3, 'PURPLE'),
        r('madrid', 'cadiz', 3, 'ORANGE'),
        r('lisboa', 'cadiz', 2, 'BLUE'),
        r('bruxelles', 'amsterdam', 1, 'BLACK'),
        r('bruxelles', 'frankfurt', 2, 'BLUE'),
        r('amsterdam', 'essen', 3, 'YELLOW'),
        r('amsterdam', 'frankfurt', 2, 'WHITE'),
        r('essen', 'frankfurt', 2, 'GREEN'),
        r('essen', 'berlin', 2, 'BLUE'),
        r('essen', 'kobenhavn', 3, 'GRAY', { ferry: 1 }),
        r('frankfurt', 'berlin', 3, 'BLACK'),
        r('frankfurt', 'munchen', 2, 'PURPLE'),
        r('munchen', 'berlin', 2, 'GREEN'),
        r('munchen', 'zurich', 2, 'YELLOW', { tunnel: true }),
        r('munchen', 'venezia', 2, 'BLUE', { tunnel: true }),
        r('munchen', 'wien', 3, 'ORANGE'),
        r('zurich', 'venezia', 2, 'GREEN', { tunnel: true }),
        r('berlin', 'hamburg', 1, 'BLUE'),
        r('berlin', 'danzig', 4, 'GRAY'),
        r('berlin', 'warszawa', 4, 'YELLOW'),
        r('hamburg', 'essen', 1, 'RED'),
        r('hamburg', 'kobenhavn', 2, 'GRAY', { ferry: 1 }),
        r('hamburg', 'danzig', 3, 'GRAY'),
        r('kobenhavn', 'stockholm', 3, 'YELLOW', { ferry: 1 }),
        r('stockholm', 'petrograd', 8, 'GRAY'),
        r('danzig', 'warszawa', 2, 'GRAY'),
        r('danzig', 'riga', 3, 'BLACK'),
        r('riga', 'petrograd', 4, 'GRAY'),
        r('riga', 'wilno', 4, 'GREEN'),
        r('petrograd', 'wilno', 4, 'BLUE'),
        r('petrograd', 'moskva', 4, 'WHITE'),
        r('wilno', 'warszawa', 3, 'RED'),
        r('wilno', 'smolensk', 3, 'YELLOW'),
        r('wilno', 'kyiv', 2, 'GRAY'),
        r('smolensk', 'moskva', 2, 'ORANGE'),
        r('smolensk', 'kyiv', 3, 'RED'),
        r('moskva', 'kharkov', 4, 'GRAY'),
        r('kharkov', 'kyiv', 4, 'GRAY'),
        r('kharkov', 'rostov', 2, 'GREEN'),
        r('rostov', 'sevastopol', 4, 'GRAY'),
        r('rostov', 'kharkov', 2, 'GREEN'),
        r('warszawa', 'wien', 4, 'BLUE'),
        r('warszawa', 'kyiv', 4, 'GRAY'),
        r('wien', 'budapest', 1, 'WHITE'),
        r('wien', 'zagrab', 2, 'GRAY'),
        r('budapest', 'kyiv', 6, 'GRAY', { tunnel: true }),
        r('budapest', 'bucuresti', 4, 'GRAY'),
        r('budapest', 'sarajevo', 3, 'PURPLE'),
        r('budapest', 'zagrab', 2, 'ORANGE'),
        r('kyiv', 'bucuresti', 4, 'GRAY', { tunnel: true }),
        r('bucuresti', 'sofia', 2, 'GRAY'),
        r('bucuresti', 'constantinople', 3, 'YELLOW'),
        r('bucuresti', 'sevastopol', 4, 'WHITE', { ferry: 1 }),
        r('zagrab', 'sarajevo', 3, 'RED'),
        r('zagrab', 'venezia', 2, 'GRAY', { tunnel: true }),
        r('sarajevo', 'sofia', 2, 'GRAY', { tunnel: true }),
        r('sarajevo', 'athina', 4, 'GREEN'),
        r('sofia', 'athina', 3, 'PURPLE'),
        r('sofia', 'constantinople', 3, 'BLUE'),
        r('venezia', 'roma', 2, 'BLACK', { tunnel: true }),
        r('roma', 'brindisi', 2, 'WHITE'),
        r('roma', 'palermo', 4, 'GRAY', { ferry: 1 }),
        r('brindisi', 'palermo', 3, 'GRAY', { ferry: 1 }),
        r('brindisi', 'athina', 4, 'GRAY', { ferry: 1 }),
        r('athina', 'smyrna', 2, 'GRAY', { ferry: 1 }),
        r('smyrna', 'constantinople', 2, 'GRAY', { tunnel: true }),
        r('smyrna', 'angora', 3, 'ORANGE'),
        r('smyrna', 'palermo', 6, 'GRAY', { ferry: 2 }),
        r('constantinople', 'angora', 2, 'GRAY'),
        r('angora', 'erzurum', 3, 'BLACK'),
        r('erzurum', 'sevastopol', 4, 'GRAY', { ferry: 2 }),
        r('constantinople', 'sevastopol', 4, 'GRAY', { ferry: 2 })
    ]
};

function r(a, b, length, color, opts) {
    return {
        id: `${a}__${b}`,
        cityA: a,
        cityB: b,
        length,
        color,
        tunnel: !!(opts && opts.tunnel),
        ferry: (opts && opts.ferry) || 0
    };
}

const COLOR_HEX = {
    PURPLE: '#9b59b6',
    BLUE: '#3498db',
    ORANGE: '#e67e22',
    WHITE: '#ecf0f1',
    GREEN: '#27ae60',
    YELLOW: '#f1c40f',
    BLACK: '#1c1c1c',
    RED: '#e74c3c',
    GRAY: '#7f8c8d',
    LOCOMOTIVE: '#e91e63'
};

const PLAYER_HEX = {
    red: '#d8403a',
    blue: '#2e6dbb',
    green: '#3a8c4a',
    yellow: '#e9b832',
    black: '#222222'
};

const ROUTE_LENGTH_POINTS = { 1: 1, 2: 2, 3: 4, 4: 7, 6: 15, 8: 21 };

const CITIES_BY_ID = Object.fromEntries(TTR_MAP.cities.map(c => [c.id, c]));
const ROUTES_BY_ID = Object.fromEntries(TTR_MAP.routes.map(r => [r.id, r]));

let currentState = null;
let currentUserId = null;
let currentGameId = null;

document.addEventListener('DOMContentLoaded', async () => {
    ensureGameLayout();

    const container = document.getElementById('ttr-container');
    currentGameId = container.getAttribute('data-game-id');
    currentUserId = document.body.getAttribute('data-user-id');

    if (!currentGameId || !currentUserId) {
        alert('Missing game or user information');
        window.location.href = window.minigameConfig.lobbyUrl;
        return;
    }

    bindUiHandlers();

    // SignalR
    const connection = new signalR.HubConnectionBuilder()
        .withUrl('/hubs/game')
        .withAutomaticReconnect()
        .build();

    const refresh = async () => { await refreshState(); };
    connection.on('MoveMade', refresh);
    connection.on('GameStateChanged', refresh);
    connection.on('GameFinished', refresh);

    try {
        await connection.start();
        await connection.invoke('JoinGame', currentGameId);
    } catch (err) {
        console.error('[TTR] SignalR error', err);
    }

    await refreshState();
});

function ensureGameLayout() {
    if (document.getElementById('ttr-container')) return;
    const root = document.getElementById('minigame-root');
    if (!root) return;
    root.innerHTML = `
<div id="ttr-container" class="ttr-game" data-game-id="${window.minigameConfig?.gameId ?? ''}">
    <header class="ttr-topbar">
        <div class="ttr-title">Ticket to Ride: Europe</div>
        <div id="ttr-turn-info" class="ttr-turn-info"></div>
    </header>
    <div class="ttr-main">
        <div class="ttr-map-wrap">
            <svg id="ttr-map" viewBox="0 0 ${TTR_MAP.width} ${TTR_MAP.height}" preserveAspectRatio="xMidYMid meet"></svg>
            <div id="ttr-log" class="ttr-log"></div>
        </div>
        <aside class="ttr-side">
            <section id="ttr-players" class="ttr-section"></section>
            <section id="ttr-actions" class="ttr-section"></section>
            <section id="ttr-faceup" class="ttr-section"></section>
            <section id="ttr-hand" class="ttr-section"></section>
            <section id="ttr-tickets" class="ttr-section"></section>
        </aside>
    </div>
    <div id="ttr-modal-root" class="ttr-modal-root" style="display:none;"></div>
</div>`;
}

function bindUiHandlers() {
    document.getElementById('ttr-modal-root').addEventListener('click', (e) => {
        if (e.target.classList.contains('ttr-modal-root')) closeModal();
    });
}

async function refreshState() {
    try {
        const wrapper = await ttrClient.getTtrState(currentGameId);
        currentState = wrapper.state;
        if (!currentState) {
            console.warn('[TTR] No state in response', wrapper);
            return;
        }
        renderAll();
    } catch (e) {
        console.error('[TTR] refreshState failed', e);
    }
}

function findPlayer(state, id) {
    return (state.playerStates || state.PlayerStates || []).find(p => (p.playerId || p.PlayerId) === id);
}

function getPlayers(state) {
    return (state.playerStates || state.PlayerStates || []);
}

function getCurrentTurnPlayerId(state) {
    const players = getPlayers(state);
    const idx = state.currentPlayerIndex ?? state.CurrentPlayerIndex ?? 0;
    return players[idx] ? (players[idx].playerId || players[idx].PlayerId) : null;
}

function getPhase(state) { return state.phase || state.Phase || 'init'; }
function getPendingAction(state) { return state.pendingAction || state.PendingAction || null; }

function isMyTurn(state) {
    return getCurrentTurnPlayerId(state) === currentUserId;
}

function renderAll() {
    renderTopBar();
    renderMap();
    renderPlayers();
    renderActions();
    renderFaceUp();
    renderHand();
    renderTickets();
    renderLog();

    const phase = getPhase(currentState);
    const me = findPlayer(currentState, currentUserId);
    if (phase === 'init' && me && !(me.hasPickedInitialTickets ?? me.HasPickedInitialTickets)) {
        showKeepTicketsModal(me, true);
    } else if (getPendingAction(currentState) === 'keepDrawnTickets' && isMyTurn(currentState)) {
        const me2 = findPlayer(currentState, currentUserId);
        if (me2 && (me2.pendingTickets || me2.PendingTickets || []).length > 0) {
            showKeepTicketsModal(me2, false);
        }
    } else if (getPendingAction(currentState) === 'tunnelDecision' && isMyTurn(currentState)) {
        showTunnelModal();
    } else if (currentState.isFinished || (getPhase(currentState) === 'ended')) {
        showFinalModal();
    }
}

function renderTopBar() {
    const turnInfo = document.getElementById('ttr-turn-info');
    const phase = getPhase(currentState);

    if (phase === 'init') {
        turnInfo.textContent = 'Setup — choose your starting destination tickets';
        return;
    }
    if (phase === 'ended') {
        turnInfo.textContent = 'Game over';
        return;
    }
    const turnPlayerId = getCurrentTurnPlayerId(currentState);
    const turnPlayer = findPlayer(currentState, turnPlayerId);
    const youTag = turnPlayerId === currentUserId ? ' (your turn)' : '';
    const colorTag = turnPlayer ? `<span class="player-chip" style="background:${PLAYER_HEX[turnPlayer.color || turnPlayer.Color]}"></span>` : '';
    const lastRound = currentState.lastRoundTriggered || currentState.LastRoundTriggered;
    const lastTag = lastRound ? ' <span class="last-round-tag">final round</span>' : '';
    turnInfo.innerHTML = `${colorTag} Turn ${currentState.turnNumber || currentState.TurnNumber || 1}: ${shortId(turnPlayerId)}${youTag}${lastTag}`;
}

function renderMap() {
    const svg = document.getElementById('ttr-map');
    let html = '';

    // Background tint
    html += `<rect x="0" y="0" width="${TTR_MAP.width}" height="${TTR_MAP.height}" fill="#cdb892"/>`;

    // Routes
    const claimed = currentState.claimedRoutes || currentState.ClaimedRoutes || [];
    const claimedById = Object.fromEntries(claimed.map(c => [(c.routeId || c.RouteId), c]));

    for (const route of TTR_MAP.routes) {
        html += renderRouteSvg(route, claimedById[route.id]);
    }

    // Stations
    const stations = currentState.stations || currentState.Stations || [];
    for (const s of stations) {
        const cityId = s.cityId || s.CityId;
        const playerId = s.playerId || s.PlayerId;
        const c = CITIES_BY_ID[cityId];
        if (!c) continue;
        const owner = findPlayer(currentState, playerId);
        const fill = owner ? PLAYER_HEX[owner.color || owner.Color] : '#999';
        html += `<rect x="${c.x - 8}" y="${c.y - 16}" width="16" height="6" fill="${fill}" stroke="#000"/>`;
    }

    // Cities
    for (const city of TTR_MAP.cities) {
        const isClickable = canBuildStationHere(city.id);
        const cls = isClickable ? 'city clickable' : 'city';
        html += `<g class="${cls}" data-city="${city.id}">
            <circle cx="${city.x}" cy="${city.y}" r="6" fill="#fff" stroke="#000" stroke-width="1.4"/>
            <text x="${city.x + 8}" y="${city.y - 8}" font-size="10" font-family="sans-serif" fill="#222" stroke="#fff" stroke-width="2.5" paint-order="stroke">${city.name}</text>
            <text x="${city.x + 8}" y="${city.y - 8}" font-size="10" font-family="sans-serif" fill="#222">${city.name}</text>
        </g>`;
    }

    svg.innerHTML = html;

    // Click handlers
    svg.querySelectorAll('g.route.clickable').forEach(g => {
        g.addEventListener('click', () => {
            const id = g.getAttribute('data-route');
            openClaimRouteModal(ROUTES_BY_ID[id]);
        });
    });
    svg.querySelectorAll('g.city.clickable').forEach(g => {
        g.addEventListener('click', () => {
            openBuildStationModal(g.getAttribute('data-city'));
        });
    });
}

function renderRouteSvg(route, claim) {
    const a = CITIES_BY_ID[route.cityA];
    const b = CITIES_BY_ID[route.cityB];
    if (!a || !b) return '';

    const dx = b.x - a.x, dy = b.y - a.y;
    const len = Math.sqrt(dx * dx + dy * dy);
    const nx = dx / len, ny = dy / len;
    // Move endpoints inward to avoid covering the city circle
    const ax = a.x + nx * 10, ay = a.y + ny * 10;
    const bx = b.x - nx * 10, by = b.y - ny * 10;

    const segLen = (Math.sqrt((bx - ax) ** 2 + (by - ay) ** 2)) / route.length;
    const px = -ny, py = nx; // perpendicular

    const baseColor = COLOR_HEX[route.color] || '#7f8c8d';
    const claimedColor = claim ? PLAYER_HEX[(findPlayer(currentState, claim.playerId || claim.PlayerId) || {}).color || (findPlayer(currentState, claim.playerId || claim.PlayerId) || {}).Color] : null;

    const isClickable = !claim && canClaimRoute(route);
    let g = `<g class="route ${isClickable ? 'clickable' : ''}" data-route="${route.id}">`;

    for (let i = 0; i < route.length; i++) {
        const cx = ax + nx * segLen * (i + 0.5);
        const cy = ay + ny * segLen * (i + 0.5);
        const w = Math.max(8, segLen * 0.7), h = 7;
        const cardColor = claimedColor || baseColor;
        const stroke = route.tunnel ? '#000' : '#222';
        const dash = route.tunnel ? 'stroke-dasharray="2 2"' : '';
        // Rotate around center to align rectangle with route direction
        const angle = Math.atan2(ny, nx) * 180 / Math.PI;
        g += `<rect x="${cx - w / 2}" y="${cy - h / 2}" width="${w}" height="${h}" fill="${cardColor}" stroke="${stroke}" stroke-width="0.8" ${dash} transform="rotate(${angle} ${cx} ${cy})"/>`;
    }

    // Ferry locomotive markers
    if (route.ferry > 0) {
        for (let i = 0; i < route.ferry; i++) {
            const cx = ax + nx * segLen * (i + 0.5);
            const cy = ay + ny * segLen * (i + 0.5);
            g += `<circle cx="${cx}" cy="${cy}" r="3" fill="#e91e63" stroke="#fff" stroke-width="0.5"/>`;
        }
    }

    g += `</g>`;
    return g;
}

function canClaimRoute(route) {
    if (!isMyTurn(currentState)) return false;
    if (getPendingAction(currentState)) return false;
    const me = findPlayer(currentState, currentUserId);
    if (!me) return false;
    const trains = me.trainsRemaining ?? me.TrainsRemaining ?? 0;
    if (trains < route.length) return false;
    return findValidPaymentOptions(me, route).length > 0;
}

function findValidPaymentOptions(player, route) {
    const hand = player.hand || player.Hand || {};
    const locos = hand['LOCOMOTIVE'] || 0;
    const options = [];
    const ferry = route.ferry || 0;
    // For each color choice
    const candidateColors = route.color === 'GRAY'
        ? ['PURPLE','BLUE','ORANGE','WHITE','GREEN','YELLOW','BLACK','RED']
        : [route.color];

    for (const color of candidateColors) {
        const colorCount = hand[color] || 0;
        // For each locomotive count from ferry to route.length
        for (let l = ferry; l <= Math.min(locos, route.length); l++) {
            const need = route.length - l;
            if (need <= colorCount) {
                options.push({ color, locomotives: l });
            }
        }
        // All-locomotive option
        if (route.length <= locos && route.length >= ferry) {
            options.push({ color: '', locomotives: route.length });
        }
    }
    // De-duplicate
    const seen = new Set();
    return options.filter(o => {
        const k = `${o.color}-${o.locomotives}`;
        if (seen.has(k)) return false;
        seen.add(k);
        return true;
    });
}

function canBuildStationHere(cityId) {
    if (!isMyTurn(currentState)) return false;
    if (getPendingAction(currentState)) return false;
    const stations = currentState.stations || currentState.Stations || [];
    if (stations.some(s => (s.cityId || s.CityId) === cityId)) return false;
    const me = findPlayer(currentState, currentUserId);
    if (!me) return false;
    const built = me.stationsBuilt ?? me.StationsBuilt ?? 0;
    if (built >= 3) return false;
    return findStationPaymentOptions(me).length > 0;
}

function findStationPaymentOptions(player) {
    const hand = player.hand || player.Hand || {};
    const cost = (player.stationsBuilt ?? player.StationsBuilt ?? 0) + 1;
    const locos = hand['LOCOMOTIVE'] || 0;
    const options = [];
    const colors = ['PURPLE','BLUE','ORANGE','WHITE','GREEN','YELLOW','BLACK','RED'];
    for (const color of colors) {
        for (let l = 0; l <= Math.min(locos, cost); l++) {
            const need = cost - l;
            if (need >= 0 && (hand[color] || 0) >= need) {
                options.push({ color, locomotives: l });
            }
        }
    }
    if (cost <= locos) {
        options.push({ color: '', locomotives: cost });
    }
    const seen = new Set();
    return options.filter(o => {
        const k = `${o.color}-${o.locomotives}`;
        if (seen.has(k)) return false;
        seen.add(k);
        return true;
    });
}

function renderPlayers() {
    const root = document.getElementById('ttr-players');
    const players = getPlayers(currentState);
    let html = '<h3>Players</h3>';
    for (const p of players) {
        const id = p.playerId || p.PlayerId;
        const isTurn = id === getCurrentTurnPlayerId(currentState);
        const color = p.color || p.Color;
        const isMe = id === currentUserId;
        html += `<div class="player-row ${isTurn ? 'turn' : ''}">
            <span class="player-chip" style="background:${PLAYER_HEX[color]}"></span>
            <span class="player-name">${shortId(id)}${isMe ? ' (you)' : ''}</span>
            <div class="player-stats">
                <span title="Trains">🚂 ${p.trainsRemaining ?? p.TrainsRemaining}</span>
                <span title="Stations used">🏛 ${p.stationsBuilt ?? p.StationsBuilt}/3</span>
                <span title="Score">⭐ ${p.score ?? p.Score}</span>
                <span title="Tickets">🎫 ${(p.tickets || p.Tickets || []).length}</span>
                <span title="Hand">🃏 ${handTotal(p)}</span>
            </div>
        </div>`;
    }
    root.innerHTML = html;
}

function handTotal(p) {
    const hand = p.hand || p.Hand || {};
    return Object.values(hand).reduce((a, b) => a + b, 0);
}

function renderActions() {
    const root = document.getElementById('ttr-actions');
    const phase = getPhase(currentState);
    const pending = getPendingAction(currentState);
    const myTurn = isMyTurn(currentState);

    if (phase !== 'play') {
        root.innerHTML = '<h3>Actions</h3><p class="muted">Waiting...</p>';
        return;
    }

    if (!myTurn) {
        root.innerHTML = `<h3>Actions</h3><p class="muted">Waiting for ${shortId(getCurrentTurnPlayerId(currentState))}...</p>`;
        return;
    }

    if (pending === 'tunnelDecision') {
        root.innerHTML = '<h3>Actions</h3><p class="muted">Resolve the tunnel attempt</p>';
        return;
    }

    if (pending === 'keepDrawnTickets') {
        root.innerHTML = '<h3>Actions</h3><p class="muted">Choose tickets to keep</p>';
        return;
    }

    if (pending === 'drawSecondCard') {
        root.innerHTML = `<h3>Actions</h3>
            <p class="muted">Draw your second train card from the deck or market.</p>
            <button id="ttr-draw-deck" class="btn-primary">Draw from deck</button>`;
        document.getElementById('ttr-draw-deck').onclick = () => doDrawDeck();
        return;
    }

    root.innerHTML = `<h3>Actions</h3>
        <button id="ttr-draw-deck" class="btn-primary">Draw 2 train cards</button>
        <button id="ttr-draw-tickets" class="btn-primary">Draw destination tickets</button>
        <p class="muted">Click a route on the map to claim it. Click a city to build a station.</p>`;
    document.getElementById('ttr-draw-deck').onclick = () => doDrawDeck();
    document.getElementById('ttr-draw-tickets').onclick = () => doDrawTickets();
}

function renderFaceUp() {
    const root = document.getElementById('ttr-faceup');
    const faceUp = currentState.faceUp || currentState.FaceUp || [];
    const myTurn = isMyTurn(currentState);
    const pending = getPendingAction(currentState);
    const canDraw = myTurn && (pending == null || pending === 'drawSecondCard');
    const isSecond = pending === 'drawSecondCard';

    let html = '<h3>Market</h3><div class="faceup-row">';
    faceUp.forEach((card, i) => {
        const c = card || 'empty';
        const hex = card ? COLOR_HEX[card] : '#444';
        const blocked = isSecond && card === 'LOCOMOTIVE';
        const klass = canDraw && card && !blocked ? 'face-card clickable' : 'face-card';
        const label = card === 'LOCOMOTIVE' ? '★' : (card ? card[0] : '');
        const txtColor = (card === 'WHITE' || card === 'YELLOW') ? '#000' : '#fff';
        html += `<div class="${klass}" data-idx="${i}" style="background:${hex}; color:${txtColor};" title="${c}">${label}</div>`;
    });
    html += '</div>';
    const deckCount = (currentState.trainDeck || currentState.TrainDeck || []).length;
    const discardCount = (currentState.trainDiscard || currentState.TrainDiscard || []).length;
    html += `<p class="muted">Deck ${deckCount} • Discard ${discardCount}</p>`;
    root.innerHTML = html;

    if (canDraw) {
        root.querySelectorAll('.face-card.clickable').forEach(el => {
            el.onclick = () => doDrawFace(parseInt(el.dataset.idx, 10));
        });
    }
}

function renderHand() {
    const root = document.getElementById('ttr-hand');
    const me = findPlayer(currentState, currentUserId);
    if (!me) {
        root.innerHTML = '';
        return;
    }
    const hand = me.hand || me.Hand || {};
    let html = '<h3>Your Hand</h3><div class="hand-row">';
    const order = ['PURPLE','BLUE','ORANGE','WHITE','GREEN','YELLOW','BLACK','RED','LOCOMOTIVE'];
    for (const color of order) {
        const count = hand[color] || 0;
        if (count === 0) continue;
        const hex = COLOR_HEX[color];
        const txtColor = (color === 'WHITE' || color === 'YELLOW') ? '#000' : '#fff';
        const label = color === 'LOCOMOTIVE' ? '★' : color[0];
        html += `<div class="hand-card" style="background:${hex}; color:${txtColor};" title="${color}">${label}<span class="card-count">${count}</span></div>`;
    }
    html += '</div>';
    root.innerHTML = html;
}

function renderTickets() {
    const root = document.getElementById('ttr-tickets');
    const me = findPlayer(currentState, currentUserId);
    if (!me) {
        root.innerHTML = '';
        return;
    }
    const tickets = me.tickets || me.Tickets || [];
    let html = '<h3>Your Tickets</h3>';
    if (tickets.length === 0) {
        html += '<p class="muted">No tickets yet.</p>';
    } else {
        const playerCities = collectPlayerCities(currentState, currentUserId);
        const adj = buildPlayerAdj(currentState, currentUserId);
        for (const t of tickets) {
            const a = t.cityA || t.CityA, b = t.cityB || t.CityB, pts = t.points || t.Points, isLong = t.isLong || t.IsLong;
            const completed = isPathConnected(adj, a, b);
            html += `<div class="ticket ${completed ? 'completed' : ''} ${isLong ? 'long' : ''}">
                <span class="ticket-route">${cityName(a)} → ${cityName(b)}</span>
                <span class="ticket-points">${pts}</span>
                ${completed ? '<span class="ticket-check">✓</span>' : ''}
            </div>`;
        }
    }
    root.innerHTML = html;
}

function collectPlayerCities(state, playerId) {
    const set = new Set();
    const claimed = state.claimedRoutes || state.ClaimedRoutes || [];
    for (const c of claimed) {
        if ((c.playerId || c.PlayerId) === playerId) {
            const route = ROUTES_BY_ID[c.routeId || c.RouteId];
            if (route) {
                set.add(route.cityA);
                set.add(route.cityB);
            }
        }
    }
    return set;
}

function buildPlayerAdj(state, playerId) {
    const adj = {};
    const claimed = state.claimedRoutes || state.ClaimedRoutes || [];
    for (const c of claimed) {
        if ((c.playerId || c.PlayerId) === playerId) {
            const route = ROUTES_BY_ID[c.routeId || c.RouteId];
            if (!route) continue;
            (adj[route.cityA] = adj[route.cityA] || []).push(route.cityB);
            (adj[route.cityB] = adj[route.cityB] || []).push(route.cityA);
        }
    }
    return adj;
}

function isPathConnected(adj, a, b) {
    if (a === b) return true;
    if (!adj[a]) return false;
    const seen = new Set([a]);
    const stack = [a];
    while (stack.length) {
        const cur = stack.pop();
        if (cur === b) return true;
        for (const n of (adj[cur] || [])) {
            if (!seen.has(n)) { seen.add(n); stack.push(n); }
        }
    }
    return false;
}

function renderLog() {
    const log = currentState.recentLog || currentState.RecentLog || [];
    const root = document.getElementById('ttr-log');
    root.innerHTML = '<h4>Activity</h4>' + log.slice(-10).map(e => {
        const who = e.playerId || e.PlayerId;
        const msg = e.message || e.Message;
        return `<div class="log-line"><strong>${who === 'system' ? 'System' : shortId(who)}:</strong> ${msg}</div>`;
    }).reverse().join('');
}

// ---- Action handlers -----------------------------------------------------

async function doDrawDeck() {
    try { await ttrClient.drawDeck(currentGameId, currentUserId); await refreshState(); }
    catch (e) { showError(e.message); }
}
async function doDrawFace(i) {
    try { await ttrClient.drawFace(currentGameId, currentUserId, i); await refreshState(); }
    catch (e) { showError(e.message); }
}
async function doDrawTickets() {
    try { await ttrClient.drawTickets(currentGameId, currentUserId); await refreshState(); }
    catch (e) { showError(e.message); }
}

function openClaimRouteModal(route) {
    if (!canClaimRoute(route)) return;
    const me = findPlayer(currentState, currentUserId);
    const options = findValidPaymentOptions(me, route);

    const a = CITIES_BY_ID[route.cityA].name, b = CITIES_BY_ID[route.cityB].name;
    const pts = ROUTE_LENGTH_POINTS[route.length] || route.length;
    const tunnelTag = route.tunnel ? '<span class="badge tunnel">tunnel</span>' : '';
    const ferryTag = route.ferry > 0 ? `<span class="badge ferry">ferry ×${route.ferry}</span>` : '';

    let html = `<div class="ttr-modal">
        <h2>Claim ${a} → ${b}</h2>
        <p>${route.length} cars · ${pts} points · ${route.color} ${tunnelTag}${ferryTag}</p>
        <div class="claim-options">`;
    options.forEach((o, i) => {
        const colorTag = o.color ? `<span class="card-pip" style="background:${COLOR_HEX[o.color]}"></span> ${o.color}` : '';
        const locoTag = o.locomotives > 0 ? ` <span class="card-pip" style="background:${COLOR_HEX.LOCOMOTIVE}"></span> ×${o.locomotives}` : '';
        const colorCount = route.length - o.locomotives;
        html += `<button class="claim-option" data-i="${i}">${colorCount > 0 ? `${colorCount} × ${colorTag}` : ''}${locoTag || (colorCount === 0 ? '' : '')}</button>`;
    });
    html += `</div><div class="modal-actions"><button id="ttr-cancel" class="btn-secondary">Cancel</button></div></div>`;
    showModal(html);

    document.getElementById('ttr-cancel').onclick = () => closeModal();
    document.querySelectorAll('.claim-option').forEach(el => {
        el.onclick = async () => {
            const o = options[parseInt(el.dataset.i, 10)];
            closeModal();
            try {
                await ttrClient.claimRoute(currentGameId, currentUserId, route.id, o.color || null, o.locomotives);
                await refreshState();
            } catch (e) { showError(e.message); }
        };
    });
}

function openBuildStationModal(cityId) {
    if (!canBuildStationHere(cityId)) return;
    const me = findPlayer(currentState, currentUserId);
    const options = findStationPaymentOptions(me);
    const cost = (me.stationsBuilt ?? me.StationsBuilt ?? 0) + 1;

    let html = `<div class="ttr-modal">
        <h2>Build station at ${cityName(cityId)}</h2>
        <p>Cost: ${cost} card${cost > 1 ? 's' : ''} of the same colour (or locomotives).</p>
        <div class="claim-options">`;
    options.forEach((o, i) => {
        const colorTag = o.color ? `<span class="card-pip" style="background:${COLOR_HEX[o.color]}"></span> ${o.color}` : '';
        const locoTag = o.locomotives > 0 ? ` <span class="card-pip" style="background:${COLOR_HEX.LOCOMOTIVE}"></span> ×${o.locomotives}` : '';
        const colorCount = cost - o.locomotives;
        html += `<button class="claim-option" data-i="${i}">${colorCount > 0 ? `${colorCount} × ${colorTag}` : ''}${locoTag || (colorCount === 0 ? '' : '')}</button>`;
    });
    html += `</div><div class="modal-actions"><button id="ttr-cancel" class="btn-secondary">Cancel</button></div></div>`;
    showModal(html);

    document.getElementById('ttr-cancel').onclick = () => closeModal();
    document.querySelectorAll('.claim-option').forEach(el => {
        el.onclick = async () => {
            const o = options[parseInt(el.dataset.i, 10)];
            closeModal();
            try {
                await ttrClient.buildStation(currentGameId, currentUserId, cityId, o.color || null, o.locomotives);
                await refreshState();
            } catch (e) { showError(e.message); }
        };
    });
}

function showKeepTicketsModal(player, isInitial) {
    const tickets = player.pendingTickets || player.PendingTickets || [];
    const minKeep = player.pendingMinKeep ?? player.PendingMinKeep ?? (isInitial ? 2 : 1);
    if (tickets.length === 0) return;

    let html = `<div class="ttr-modal">
        <h2>${isInitial ? 'Choose your starting tickets' : 'Keep at least one ticket'}</h2>
        <p>Tick the tickets you want to keep. You must keep at least ${minKeep}.</p>
        <div class="ticket-choices">`;
    tickets.forEach((t, i) => {
        const a = t.cityA || t.CityA, b = t.cityB || t.CityB, pts = t.points || t.Points, isLong = t.isLong || t.IsLong;
        html += `<label class="ticket-choice ${isLong ? 'long' : ''}">
            <input type="checkbox" data-i="${i}" ${isInitial ? 'checked' : ''}/>
            <span>${cityName(a)} → ${cityName(b)} <strong>(${pts}${isLong ? ' long' : ''})</strong></span>
        </label>`;
    });
    html += `</div><div class="modal-actions">
        <button id="ttr-confirm-tickets" class="btn-primary">Confirm</button>
    </div></div>`;
    showModal(html);

    document.getElementById('ttr-confirm-tickets').onclick = async () => {
        const flags = Array.from(document.querySelectorAll('.ticket-choice input')).map(c => c.checked);
        const kept = flags.filter(Boolean).length;
        if (kept < minKeep) {
            alert(`You must keep at least ${minKeep} ticket(s)`);
            return;
        }
        closeModal();
        try {
            await ttrClient.keepTickets(currentGameId, currentUserId, flags);
            await refreshState();
        } catch (e) { showError(e.message); }
    };
}

function showTunnelModal() {
    const pt = currentState.pendingTunnel || currentState.PendingTunnel;
    if (!pt) return;
    const route = ROUTES_BY_ID[pt.routeId || pt.RouteId];
    const need = (pt.extraColorCardsRequired || pt.ExtraColorCardsRequired || 0) + (pt.extraLocomotivesRequired || pt.ExtraLocomotivesRequired || 0);
    const minLoco = pt.extraLocomotivesRequired || pt.ExtraLocomotivesRequired || 0;
    const colorUsed = pt.colorUsed || pt.ColorUsed || '';
    const revealed = pt.revealedCards || pt.RevealedCards || [];

    const me = findPlayer(currentState, currentUserId);
    const handLoco = (me?.hand || me?.Hand || {})['LOCOMOTIVE'] || 0;
    const handColor = colorUsed ? ((me?.hand || me?.Hand || {})[colorUsed] || 0) : 0;

    let revealedHtml = revealed.map(c => {
        const hex = COLOR_HEX[c] || '#888';
        const txt = (c === 'WHITE' || c === 'YELLOW') ? '#000' : '#fff';
        const label = c === 'LOCOMOTIVE' ? '★' : c[0];
        return `<span class="reveal-card" style="background:${hex}; color:${txt}">${label}</span>`;
    }).join('');

    let optionsHtml = '';
    for (let l = minLoco; l <= need; l++) {
        const colorNeeded = need - l;
        if (l > handLoco) continue;
        if (colorUsed && colorNeeded > handColor) continue;
        if (!colorUsed && colorNeeded > 0) continue;
        optionsHtml += `<button class="claim-option" data-loco="${l}">Pay ${l} ★ + ${colorNeeded} ${colorUsed || ''}</button>`;
    }
    if (optionsHtml === '') {
        optionsHtml = '<p class="muted">You cannot afford the extra cost.</p>';
    }

    const html = `<div class="ttr-modal">
        <h2>Tunnel attempt: ${cityName(route.cityA)} → ${cityName(route.cityB)}</h2>
        <p>Revealed cards: <span class="reveal-row">${revealedHtml}</span></p>
        <p>You must pay <strong>${need}</strong> additional card(s)${minLoco > 0 ? ` (at least ${minLoco} locomotive)` : ''}.</p>
        <div class="claim-options">${optionsHtml}</div>
        <div class="modal-actions">
            <button id="ttr-tunnel-skip" class="btn-secondary">Back out (return cards, end turn)</button>
        </div>
    </div>`;
    showModal(html);

    document.getElementById('ttr-tunnel-skip').onclick = async () => {
        closeModal();
        try { await ttrClient.tunnelSkip(currentGameId, currentUserId); await refreshState(); }
        catch (e) { showError(e.message); }
    };
    document.querySelectorAll('.claim-option').forEach(el => {
        el.onclick = async () => {
            const l = parseInt(el.dataset.loco, 10);
            closeModal();
            try { await ttrClient.tunnelPay(currentGameId, currentUserId, l); await refreshState(); }
            catch (e) { showError(e.message); }
        };
    });
}

function showFinalModal() {
    if (document.querySelector('.ttr-modal.final-modal')) return;
    const players = getPlayers(currentState);
    const ranked = [...players].sort((a, b) => (b.score ?? b.Score) - (a.score ?? a.Score));
    let html = `<div class="ttr-modal final-modal"><h2>Game Over</h2>
        <table class="final-table">
            <thead><tr><th>#</th><th>Player</th><th>Score</th><th>Tickets</th><th>Stations used</th></tr></thead><tbody>`;
    ranked.forEach((p, i) => {
        html += `<tr>
            <td>${i + 1}</td>
            <td><span class="player-chip" style="background:${PLAYER_HEX[p.color || p.Color]}"></span> ${shortId(p.playerId || p.PlayerId)}${(p.playerId || p.PlayerId) === currentUserId ? ' (you)' : ''}</td>
            <td><strong>${p.score ?? p.Score}</strong></td>
            <td>${(p.tickets || p.Tickets || []).length}</td>
            <td>${p.stationsBuilt ?? p.StationsBuilt}</td>
        </tr>`;
    });
    html += `</tbody></table>
        <div class="modal-actions">
            <button id="ttr-back-lobby" class="btn-primary">Back to lobby</button>
        </div></div>`;
    showModal(html);
    document.getElementById('ttr-back-lobby').onclick = () => {
        window.location.href = window.minigameConfig.lobbyUrl;
    };
}

// ---- Modal helpers -------------------------------------------------------

function showModal(html) {
    const root = document.getElementById('ttr-modal-root');
    root.innerHTML = html;
    root.style.display = 'flex';
}
function closeModal() {
    const root = document.getElementById('ttr-modal-root');
    root.innerHTML = '';
    root.style.display = 'none';
}
function showError(message) {
    showModal(`<div class="ttr-modal">
        <h2>Action failed</h2>
        <p class="error-text">${escapeHtml(message)}</p>
        <div class="modal-actions"><button id="ttr-err-ok" class="btn-primary">OK</button></div>
    </div>`);
    document.getElementById('ttr-err-ok').onclick = () => closeModal();
}

function shortId(id) {
    if (!id) return '?';
    return id.substring(0, 8);
}
function cityName(id) {
    return CITIES_BY_ID[id] ? CITIES_BY_ID[id].name : id;
}
function escapeHtml(s) {
    return String(s).replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
}
