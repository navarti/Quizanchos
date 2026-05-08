(function () {
    const root = document.getElementById('minigame-root');
    if (!root) return;

    const cfg = window.minigameConfig || {};
    const gameId = cfg.gameId;
    const minigameTypeId = cfg.minigameTypeId;
    const playerId = document.body.getAttribute('data-user-id') || '';
    const moveDiscriminator = 'countryGuesserMp';

    let state = null;
    let map = null;
    let highlightLayer = null;
    let lastCardIndex = -1;
    let pickedThisRound = false;

    function ensureShell() {
        if (root.dataset.cgmpInitialized === '1') return;
        root.dataset.cgmpInitialized = '1';
        root.innerHTML = `
            <div class="country-guesser-mp">
                <div class="country-guesser-mp__head">
                    <h2>Country Guesser — Multiplayer</h2>
                    <div>
                        <span class="country-guesser-mp__progress" data-progress></span>
                        <span class="country-guesser-mp__timer" data-timer>--</span>
                    </div>
                </div>
                <div class="country-guesser-mp__map" data-map></div>
                <div class="country-guesser-mp__options" data-options></div>
                <div class="country-guesser-mp__scores" data-scores></div>
                <div data-finished></div>
            </div>
        `;
    }

    function ensureMap() {
        if (map || typeof L === 'undefined') return;
        const mapEl = root.querySelector('[data-map]');
        if (!mapEl) return;
        map = L.map(mapEl, {
            zoomControl: false,
            worldCopyJump: true,
        }).setView([20, 0], 2);
        L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '© OpenStreetMap',
            maxZoom: 7,
            minZoom: 2,
        }).addTo(map);
    }

    function highlight(card) {
        if (!map || !card) return;
        if (highlightLayer) {
            map.removeLayer(highlightLayer);
            highlightLayer = null;
        }
        const target = [card.targetLat, card.targetLon];
        highlightLayer = L.circle(target, {
            radius: 700000,
            color: '#ffd166',
            weight: 3,
            fillColor: '#ffd166',
            fillOpacity: 0.25,
        }).addTo(map);
        map.flyTo(target, 4, { duration: 0.6 });
    }

    function paintOptions(card) {
        const opts = root.querySelector('[data-options]');
        if (!opts) return;
        const myAnswer = (card.playerAnswers || {})[playerId];
        const everyoneAnswered = Object.keys(card.playerAnswers || {}).length >= (state.players?.length || 1);
        const reveal = everyoneAnswered || state.isFinished;

        opts.innerHTML = card.optionNames.map((name, i) => {
            let stateAttr = '';
            if (myAnswer != null && i === myAnswer) {
                stateAttr = reveal
                    ? (i === card.correctOption ? 'correct' : 'wrong')
                    : 'picked';
            } else if (reveal && i === card.correctOption) {
                stateAttr = 'correct';
            }
            const disabled = pickedThisRound || myAnswer != null ? 'disabled' : '';
            return `<button class="country-guesser-mp__option" data-state="${stateAttr}" data-idx="${i}" ${disabled}>${name}</button>`;
        }).join('');

        if (myAnswer == null && !pickedThisRound) {
            opts.querySelectorAll('.country-guesser-mp__option').forEach(btn => {
                btn.addEventListener('click', () => {
                    if (pickedThisRound) return;
                    pickedThisRound = true;
                    const idx = parseInt(btn.dataset.idx, 10);
                    btn.dataset.state = 'picked';
                    submitMove(idx);
                });
            });
        }
    }

    function paintScores() {
        const el = root.querySelector('[data-scores]');
        if (!el || !state) return;
        const scores = state.scores || {};
        const players = state.players || [];
        el.innerHTML = players.map(id => {
            const label = id === playerId ? 'You' : id.substring(0, 8);
            const cls = id === playerId ? 'country-guesser-mp__score me' : 'country-guesser-mp__score';
            return `<div class="${cls}"><span>${label}</span><strong>${scores[id] || 0}</strong></div>`;
        }).join('');
    }

    function paintFinished() {
        const el = root.querySelector('[data-finished]');
        if (!el || !state) return;
        if (!state.isFinished) {
            el.innerHTML = '';
            return;
        }
        const winnerLabel = state.winner === playerId ? 'You won!'
            : state.winner ? `Winner: ${state.winner.substring(0, 8)}`
            : 'Draw — no clear winner';
        const myScore = (state.scores || {})[playerId] || 0;
        el.innerHTML = `<div class="country-guesser-mp__finished">${winnerLabel} — your score ${myScore}/${state.totalCards}</div>`;
    }

    function paintProgress() {
        const prog = root.querySelector('[data-progress]');
        const tim = root.querySelector('[data-timer]');
        if (!state) return;
        const idx = Math.min(state.currentCardIndex + 1, state.totalCards);
        if (prog) prog.textContent = `Round ${idx} of ${state.totalCards}`;
        if (state.cards && state.currentCardIndex >= 0 && state.currentCardIndex < state.cards.length) {
            const card = state.cards[state.currentCardIndex];
            const elapsed = (Date.now() - new Date(card.creationTime).getTime()) / 1000;
            const remaining = Math.max(0, Math.round(state.secondsPerCard - elapsed));
            if (tim) tim.textContent = remaining + 's';
        } else if (tim) {
            tim.textContent = '--';
        }
    }

    function applyState() {
        ensureShell();
        ensureMap();

        if (state.currentCardIndex !== lastCardIndex) {
            pickedThisRound = false;
            lastCardIndex = state.currentCardIndex;
        }

        if (state.cards && state.currentCardIndex >= 0 && state.currentCardIndex < state.cards.length) {
            const card = state.cards[state.currentCardIndex];
            highlight(card);
            paintOptions(card);
        } else if (state.cards && state.cards.length > 0) {
            const last = state.cards[state.cards.length - 1];
            highlight(last);
            paintOptions(last);
        }

        paintScores();
        paintFinished();
        paintProgress();
    }

    async function loadState() {
        try {
            const resp = await fetch(`/api/Game/${gameId}/state?minigameType=${minigameTypeId}`, {
                credentials: 'include',
            });
            if (!resp.ok) throw new Error('HTTP ' + resp.status);
            const wrapper = await resp.json();
            state = wrapper.state || wrapper.State || wrapper;
            state.isFinished = wrapper.isFinished ?? wrapper.IsFinished ?? state.isFinished;
            state.winner = wrapper.winner ?? wrapper.Winner ?? state.winner;
            state.players = wrapper.players ?? wrapper.Players ?? state.players;
            applyState();
        } catch (err) {
            console.error(err);
        }
    }

    async function submitMove(optionPicked) {
        try {
            const resp = await fetch('/api/Game/move', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                credentials: 'include',
                body: JSON.stringify({
                    gameId,
                    playerId,
                    move: { gameType: moveDiscriminator, optionPicked },
                }),
            });
            if (!resp.ok) {
                const err = await resp.json().catch(() => ({}));
                console.error('Move rejected:', err);
                pickedThisRound = false;
            }
            await loadState();
        } catch (err) {
            console.error(err);
        }
    }

    setInterval(loadState, 1500);
    setInterval(paintProgress, 500);
    loadState();
})();
