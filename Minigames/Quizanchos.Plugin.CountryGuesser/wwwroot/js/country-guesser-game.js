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
    let answerLayer = null;
    let pickedThisRound = false;
    let lastCardIndex = -1;

    function ensureMap() {
        if (map) return;
        if (typeof L === 'undefined') return;
        const mapEl = root.querySelector('[data-map]');
        if (!mapEl) return;
        map = L.map(mapEl, {
            zoomControl: true,
            worldCopyJump: true,
        }).setView([20, 0], 2);
        L.tileLayer('https://{s}.basemaps.cartocdn.com/light_nolabels/{z}/{x}/{y}{r}.png', {
            attribution: '© OpenStreetMap © CARTO',
            subdomains: 'abcd',
            maxZoom: 7,
            minZoom: 2,
        }).addTo(map);
        map.on('click', onMapClick);
    }

    function onMapClick(e) {
        if (pickedThisRound) return;
        if (!state || state.isFinished) return;
        if (state.currentCardIndex < 0 || state.currentCardIndex >= (state.cards?.length || 0)) return;
        const card = state.cards[state.currentCardIndex];
        if ((card.playerAnswers || {})[playerId]) return;

        pickedThisRound = true;
        // Wrap longitude to [-180, 180] for worldCopyJump-style clicks across copies.
        const lat = e.latlng.lat;
        let lon = ((e.latlng.lng + 540) % 360) - 180;
        showPendingMarker(lat, lon);
        submitMove(lat, lon);
    }

    function clearAnswerLayer() {
        if (!map) return;
        if (answerLayer) {
            map.removeLayer(answerLayer);
            answerLayer = null;
        }
    }

    function showPendingMarker(lat, lon) {
        if (!map) return;
        clearAnswerLayer();
        answerLayer = L.layerGroup([
            L.circleMarker([lat, lon], {
                radius: 8,
                color: '#fcd34d',
                weight: 3,
                fillColor: '#fcd34d',
                fillOpacity: 0.9,
            }),
        ]).addTo(map);
    }

    function showAnswerOverlay(card, answer) {
        if (!map || !card || !answer) return;
        clearAnswerLayer();

        const click = [answer.lat, answer.lon];
        const target = [card.targetLat, card.targetLon];
        const correct = answer.correct;
        const clickColor = correct ? '#16a34a' : '#dc2626';

        const layers = [
            L.circleMarker(click, {
                radius: 8,
                color: clickColor,
                weight: 3,
                fillColor: clickColor,
                fillOpacity: 0.9,
            }).bindTooltip(`Your pick (${Math.round(answer.distanceKm)} km off)`, { permanent: false }),
            L.circle(target, {
                radius: (state.maxDistanceKm || 600) * 1000,
                color: '#16a34a',
                weight: 1,
                fillColor: '#16a34a',
                fillOpacity: 0.10,
            }),
            L.circleMarker(target, {
                radius: 6,
                color: '#16a34a',
                weight: 3,
                fillColor: '#ffffff',
                fillOpacity: 1,
            }).bindTooltip(card.targetName, { permanent: false }),
            L.polyline([click, target], {
                color: clickColor,
                weight: 2,
                dashArray: '6 6',
                opacity: 0.8,
            }),
        ];
        answerLayer = L.layerGroup(layers).addTo(map);
        try {
            map.fitBounds(L.latLngBounds(click, target).pad(0.4), { maxZoom: 5, animate: true });
        } catch (_) { /* identical points */ }
    }

    function renderShell() {
        if (root.dataset.cgInitialized === '1') return;
        root.dataset.cgInitialized = '1';
        root.innerHTML = `
            <div class="minigame-card">
                <div class="minigame-head">
                    <h2 class="minigame-title">Country Guesser</h2>
                    <div class="minigame-head-meta">
                        <span class="minigame-pill" data-progress></span>
                        <span class="minigame-pill minigame-pill--timer" data-timer>--</span>
                    </div>
                </div>
                <div class="country-guesser__prompt">
                    <span class="country-guesser__prompt-label">Find this country</span>
                    <span class="country-guesser__prompt-name" data-prompt>—</span>
                    <span class="country-guesser__prompt-hint" data-hint>Click on the world map.</span>
                </div>
                <div class="country-guesser__map" data-map></div>
                <h3 class="minigame-section-title">Score</h3>
                <div class="minigame-scores" data-scores></div>
                <div data-finished></div>
            </div>
        `;
    }

    function paintPrompt(card, answer) {
        const promptEl = root.querySelector('[data-prompt]');
        const hintEl = root.querySelector('[data-hint]');
        if (promptEl) promptEl.textContent = card?.targetName || '—';
        if (!hintEl) return;
        if (!card) {
            hintEl.textContent = '';
            hintEl.dataset.state = '';
            return;
        }
        if (answer) {
            const km = Math.round(answer.distanceKm);
            if (answer.correct) {
                hintEl.textContent = `Correct! ${km} km from the centroid.`;
                hintEl.dataset.state = 'correct';
            } else {
                hintEl.textContent = `Off by ${km} km — see the actual location.`;
                hintEl.dataset.state = 'wrong';
            }
        } else if (pickedThisRound) {
            hintEl.textContent = 'Locked in — waiting for results...';
            hintEl.dataset.state = 'picked';
        } else {
            hintEl.textContent = 'Click on the world map.';
            hintEl.dataset.state = '';
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
            const cls = id === playerId ? 'minigame-score is-me' : 'minigame-score';
            return `<div class="${cls}"><span>${label}</span><strong>${scores[id]}</strong></div>`;
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
        const isWin = state.winner === playerId;
        const isDraw = !state.winner;
        const cls = isDraw ? 'minigame-finished--draw'
            : isWin ? 'minigame-finished--win' : 'minigame-finished--loss';
        const winnerLabel = isWin ? 'You won!'
            : state.winner ? `Winner: ${state.winner.substring(0, 8)}`
            : 'Draw — no clear winner';
        el.innerHTML = `<div class="minigame-finished ${cls}">${winnerLabel} — final score ${myScore}/${state.totalCards}</div>`;
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
            clearAnswerLayer();
            lastCardIndex = state.currentCardIndex;
        }

        let card = null;
        if (state.cards && state.currentCardIndex >= 0 && state.currentCardIndex < state.cards.length) {
            card = state.cards[state.currentCardIndex];
        } else if (state.cards && state.cards.length > 0) {
            card = state.cards[state.cards.length - 1];
        }

        const myAnswer = card ? (card.playerAnswers || {})[playerId] : null;
        if (myAnswer) {
            pickedThisRound = true;
            showAnswerOverlay(card, myAnswer);
        }

        paintPrompt(card, myAnswer);
        paintScores();
        paintFinished();
        paintTimerAndProgress();
    }

    async function submitMove(lat, lon) {
        try {
            const resp = await fetch('/api/Game/move', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                credentials: 'include',
                body: JSON.stringify({
                    gameId,
                    playerId,
                    move: { gameType: moveDiscriminator, lat, lon },
                }),
            });
            if (!resp.ok) {
                const err = await resp.json().catch(() => ({}));
                console.error('Move rejected:', err);
                pickedThisRound = false;
                clearAnswerLayer();
                return;
            }
            await loadState();
        } catch (err) {
            console.error(err);
            pickedThisRound = false;
            clearAnswerLayer();
        }
    }

    setInterval(loadState, 1500);
    setInterval(paintTimerAndProgress, 500);
    loadState();
})();
