(function () {
    const root = document.getElementById('minigame-root');
    if (!root) return;

    const cfg = window.minigameConfig || {};
    const gameId = cfg.gameId;
    const minigameTypeId = cfg.minigameTypeId;
    const playerId = document.body.getAttribute('data-user-id') || '';
    const moveDiscriminator = 'countryGuesser';

    let state = null;
    let map = null;
    let highlightLayer = null;
    let pickedThisRound = false;
    let lastCardIndex = -1;

    function ensureMap() {
        if (map) return;
        if (typeof L === 'undefined') return;
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

    function highlightCountry(card) {
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

    function renderShell() {
        if (root.dataset.cgInitialized === '1') return;
        root.dataset.cgInitialized = '1';
        root.innerHTML = `
            <div class="country-guesser">
                <div class="country-guesser__head">
                    <h2>Country Guesser</h2>
                    <div>
                        <span class="country-guesser__progress" data-progress></span>
                        <span class="country-guesser__timer" data-timer>--</span>
                    </div>
                </div>
                <div class="country-guesser__map" data-map></div>
                <div class="country-guesser__options" data-options></div>
                <div class="country-guesser__scores" data-scores></div>
                <div data-finished></div>
            </div>
        `;
    }

    function paintOptions(card, currentAnswer) {
        const opts = root.querySelector('[data-options]');
        if (!opts) return;
        opts.innerHTML = card.optionNames.map((name, i) => {
            let stateAttr = '';
            if (currentAnswer != null && i === currentAnswer) {
                stateAttr = i === card.correctOption ? 'correct' : 'wrong';
            } else if (currentAnswer != null && i === card.correctOption) {
                stateAttr = 'correct';
            }
            return `<button class="country-guesser__option" data-state="${stateAttr}" data-idx="${i}" ${pickedThisRound || currentAnswer != null ? 'disabled' : ''}>${name}</button>`;
        }).join('');
        if (currentAnswer == null && !pickedThisRound) {
            opts.querySelectorAll('.country-guesser__option').forEach(btn => {
                btn.addEventListener('click', () => {
                    if (pickedThisRound) return;
                    const idx = parseInt(btn.dataset.idx, 10);
                    pickedThisRound = true;
                    btn.dataset.state = 'picked';
                    submitMove(idx);
                });
            });
        }
    }

    function paintScores() {
        const el = root.querySelector('[data-scores]');
        if (!el) return;
        const scores = state?.scores || {};
        const ids = Object.keys(scores);
        if (ids.length === 0) {
            el.innerHTML = '';
            return;
        }
        el.innerHTML = ids.map(id => {
            const label = id === playerId ? 'You' : id.substring(0, 8);
            return `<div class="country-guesser__score">${label}: ${scores[id]}</div>`;
        }).join('');
    }

    function paintFinished() {
        const el = root.querySelector('[data-finished]');
        if (!el || !state) return;
        if (!state.isFinished) {
            el.innerHTML = '';
            return;
        }
        const myScore = (state.scores || {})[playerId] || 0;
        const winnerLabel = state.winner === playerId ? 'You won!'
            : state.winner ? `Winner: ${state.winner.substring(0, 8)}`
            : 'Draw / no clear winner';
        el.innerHTML = `<div class="country-guesser__finished">${winnerLabel} — final score ${myScore}/${state.totalCards}</div>`;
    }

    function paintTimerAndProgress() {
        const prog = root.querySelector('[data-progress]');
        const tim = root.querySelector('[data-timer]');
        if (!state) return;
        const idx = Math.min(state.currentCardIndex + 1, state.totalCards);
        if (prog) prog.textContent = `Round ${idx} of ${state.totalCards}`;
        if (state.currentCardIndex >= 0 && state.currentCardIndex < state.cards.length) {
            const card = state.cards[state.currentCardIndex];
            const elapsed = (Date.now() - new Date(card.creationTime).getTime()) / 1000;
            const remaining = Math.max(0, Math.round(state.secondsPerCard - elapsed));
            if (tim) tim.textContent = remaining + 's';
        } else if (tim) {
            tim.textContent = '--';
        }
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

    function applyState() {
        renderShell();
        ensureMap();

        if (state.currentCardIndex !== lastCardIndex) {
            pickedThisRound = false;
            lastCardIndex = state.currentCardIndex;
        }

        if (state.cards && state.currentCardIndex >= 0 && state.currentCardIndex < state.cards.length) {
            const card = state.cards[state.currentCardIndex];
            highlightCountry(card);
            const myAnswer = (card.playerAnswers || {})[playerId];
            paintOptions(card, myAnswer);
            if (myAnswer != null) pickedThisRound = true;
        } else if (state.cards && state.cards.length > 0) {
            const last = state.cards[state.cards.length - 1];
            highlightCountry(last);
            paintOptions(last, (last.playerAnswers || {})[playerId]);
        }

        paintScores();
        paintFinished();
        paintTimerAndProgress();
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
                return;
            }
            await loadState();
        } catch (err) {
            console.error(err);
        }
    }

    setInterval(loadState, 1500);
    setInterval(paintTimerAndProgress, 500);
    loadState();
})();
