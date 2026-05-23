(function () {
    const root = document.getElementById('minigame-root');
    if (!root) return;

    const cfg = window.minigameConfig || {};
    const gameId = cfg.gameId;
    const minigameTypeId = cfg.minigameTypeId;
    const playerId = document.body.getAttribute('data-user-id') || '';
    const moveDiscriminator = 'caravanMp';

    let state = null;
    let selectedHandIdx = -1;
    let signalRConnected = false;

    const SUIT_GLYPH = { 0: '♥', 1: '♦', 2: '♣', 3: '♠' };
    const RANK_LABEL = {
        1: 'A', 2: '2', 3: '3', 4: '4', 5: '5', 6: '6', 7: '7', 8: '8',
        9: '9', 10: '10', 11: 'J', 12: 'Q', 13: 'K', 14: 'JK',
    };

    function isRedSuit(s) { return s === 0 || s === 1; }

    function normalize(payload) {
        const inner = payload.state || payload.State || payload;
        const merged = {
            ...inner,
            gameId: payload.gameId || payload.GameId || inner.gameId || inner.GameId,
            players: payload.players || payload.Players || inner.players || inner.Players || [],
            isFinished: payload.isFinished ?? payload.IsFinished ?? inner.isFinished ?? inner.IsFinished ?? false,
            winner: payload.winner || payload.Winner || inner.winner || inner.Winner || null,
        };
        return merged;
    }

    async function loadState() {
        try {
            const resp = await fetch(`/api/Game/${gameId}/state?minigameType=${minigameTypeId}`, {
                credentials: 'include',
            });
            if (!resp.ok) throw new Error('HTTP ' + resp.status);
            const wrapper = await resp.json();
            state = normalize(wrapper);
            render();
        } catch (err) {
            console.error('[CaravanMp] loadState failed', err);
            root.innerHTML = `
                <div class="caravan-mp minigame-card">
                    <h2 class="minigame-title">Caravan Multiplayer</h2>
                    <p class="minigame-prose">Could not load game state.</p>
                    <p class="minigame-prose">${err.message}</p>
                </div>`;
        }
    }

    async function submitMove(move) {
        try {
            const resp = await fetch('/api/Game/move', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                credentials: 'include',
                body: JSON.stringify({
                    gameId,
                    playerId,
                    move: { gameType: moveDiscriminator, ...move },
                }),
            });
            if (!resp.ok) {
                const err = await resp.json().catch(() => ({}));
                alert('Move rejected: ' + (err.message || resp.status));
                return;
            }
            const result = await resp.json().catch(() => null);
            if (result) {
                state = normalize(result);
                render();
            } else {
                await loadState();
            }
        } catch (err) {
            console.error(err);
            alert('Failed to submit move: ' + err.message);
        }
    }

    function getMyIndex() {
        if (!state || !state.playerStates) return 0;
        for (let i = 0; i < state.playerStates.length; i++) {
            if (state.playerStates[i].playerId === playerId) return i;
        }
        return 0;
    }

    function nicknameFor(id, fallback) {
        const nicks = (state && state.playerNicknames) || {};
        return nicks[id] || fallback || (id ? id.substring(0, 8) : '?');
    }

    function renderCard(card, opts) {
        opts = opts || {};
        const klass = ['caravan-mp__card'];
        if (isRedSuit(card.suit)) klass.push('red');
        if (card.rank > 10) klass.push('face');
        if (opts.selected) klass.push('selected');
        const rank = RANK_LABEL[card.rank] || '?';
        const suit = SUIT_GLYPH[card.suit] || '?';
        return `
            <div class="${klass.join(' ')}" data-selected="${!!opts.selected}" data-handidx="${opts.handIdx ?? ''}">
                <span class="rank-top">${rank}</span>
                <span class="suit">${suit}</span>
                <span class="rank-bot">${rank}</span>
            </div>
        `;
    }

    function columnLabel(idx) {
        return String.fromCharCode('A'.charCodeAt(0) + idx);
    }

    function renderColumn(col, idx) {
        const valueClass = col.value >= 21 && col.value <= 26 ? 'sold'
                          : col.value > 26 ? 'busted' : '';
        const slotsHtml = col.slots.map((slot, sIdx) => {
            const attached = (slot.attached || []).map(c => `<span>${RANK_LABEL[c.rank]}${SUIT_GLYPH[c.suit]}</span>`).join('');
            return `
                <div class="caravan-mp__slot" data-col="${idx}" data-slot="${sIdx}">
                    ${renderCard(slot.card)}
                    <div class="caravan-mp__attached">${attached}</div>
                </div>
            `;
        }).join('');
        return `
            <div class="caravan-mp__column" data-col="${idx}">
                <div class="caravan-mp__column-head">
                    <span>Caravan ${columnLabel(idx)}</span>
                    <span class="caravan-mp__column-value ${valueClass}">${col.value}</span>
                </div>
                ${slotsHtml || '<div class="caravan-mp__slot" data-col="' + idx + '" data-slot="-1" style="opacity:.55;justify-content:center;">empty</div>'}
            </div>
        `;
    }

    function renderFinishedBanner() {
        if (!state.isFinished) return '';
        const isWin = state.winner === playerId;
        const isDraw = !state.winner;
        const cls = isDraw ? 'minigame-finished--draw'
            : isWin ? 'minigame-finished--win' : 'minigame-finished--loss';
        const opp = state.players && state.players.find(p => p !== playerId);
        const oppName = nicknameFor(opp, 'opponent');
        const surrenderedId = state.surrenderedPlayerId || state.SurrenderedPlayerId;
        let msg;
        if (surrenderedId) {
            const iSurrendered = surrenderedId === playerId;
            msg = iSurrendered
                ? `You surrendered. ${oppName} wins.`
                : `${oppName} surrendered. You win!`;
        } else {
            msg = isDraw ? 'Caravans ended in a draw.'
                : isWin ? 'You won the caravans!'
                : `${oppName} won the caravans.`;
        }
        return `<div class="minigame-finished ${cls}">${msg}</div>`;
    }

    function turnStatus(myTurn, oppName) {
        if (state.isFinished) return state.lastMoveDescription || 'Game finished.';
        if (myTurn) {
            return state.lastMoveDescription
                ? `${state.lastMoveDescription} — your turn.`
                : 'Your turn.';
        }
        return state.lastMoveDescription
            ? `${state.lastMoveDescription} — waiting on ${oppName}...`
            : `Waiting on ${oppName}...`;
    }

    function render() {
        if (!state || !state.playerStates || state.playerStates.length < 2) {
            root.innerHTML = `
                <div class="caravan-mp minigame-card minigame-card--lobby">
                    <h2 class="minigame-title">Caravan Multiplayer</h2>
                    <p class="minigame-prose">Loading...</p>
                </div>`;
            return;
        }

        const myIdx = getMyIndex();
        const oppIdx = 1 - myIdx;
        const myState = state.playerStates[myIdx];
        const oppState = state.playerStates[oppIdx];

        const myColumns = state.columns.filter(c => c.ownerId === myState.playerId);
        const oppColumns = state.columns.filter(c => c.ownerId === oppState.playerId);

        const myTurn = state.currentTurnIndex === myIdx && !state.isFinished;
        const oppName = nicknameFor(oppState.playerId, 'Opponent');
        const myName = nicknameFor(myState.playerId, 'You');

        const turnPill = state.isFinished
            ? '<span class="minigame-pill">Finished</span>'
            : myTurn
                ? '<span class="minigame-pill caravan-mp-pill--me">Your turn</span>'
                : `<span class="minigame-pill caravan-mp-pill--opp">${oppName}'s turn</span>`;

        root.innerHTML = `
            <div class="caravan-mp minigame-card">
                <div class="minigame-head">
                    <h2 class="minigame-title">Caravan — vs ${oppName}</h2>
                    <div class="minigame-head-meta">${turnPill}</div>
                </div>
                <div class="minigame-status">${turnStatus(myTurn, oppName)}</div>
                <div class="caravan-mp__board">
                    <div class="caravan-mp__side-label">${oppName}'s caravans (${oppState.hand.length} cards in hand)</div>
                    ${oppColumns.map((c) => renderColumn(c, state.columns.indexOf(c))).join('')}
                    <div class="caravan-mp__side-label">${myName === 'You' ? 'Your' : myName + "'s"} caravans</div>
                    ${myColumns.map((c) => renderColumn(c, state.columns.indexOf(c))).join('')}
                </div>
                <h3 class="minigame-section-title">Hand</h3>
                <div class="caravan-mp__hand">
                    ${myState.hand.map((card, idx) => `<div class="caravan-mp__hand-card" data-handidx="${idx}">${renderCard(card, { selected: idx === selectedHandIdx, handIdx: idx })}</div>`).join('')}
                </div>
                <div class="minigame-actions">
                    <button class="minigame-btn minigame-btn--danger" data-action="discard-card" ${selectedHandIdx < 0 || !myTurn ? 'disabled' : ''}>Discard selected card</button>
                    <button class="minigame-btn minigame-btn--secondary" data-action="discard-caravan" ${!myTurn ? 'disabled' : ''}>Discard a caravan...</button>
                    <button class="minigame-btn caravan-mp-btn--surrender" data-action="surrender" ${!myTurn || state.isFinished ? 'disabled' : ''} title="${myTurn ? 'Concede the match to your opponent' : 'You can only surrender on your turn'}">🏳️ Surrender</button>
                </div>
                ${renderFinishedBanner()}
            </div>
        `;

        wireEvents(myTurn, myIdx);
    }

    function wireEvents(myTurn, myIdx) {
        root.querySelectorAll('.caravan-mp__hand-card').forEach(el => {
            el.addEventListener('click', () => {
                const idx = parseInt(el.dataset.handidx, 10);
                selectedHandIdx = (selectedHandIdx === idx) ? -1 : idx;
                render();
            });
        });

        if (!myTurn) return;

        root.querySelectorAll('.caravan-mp__column').forEach(el => {
            el.addEventListener('click', evt => {
                if (evt.target.closest('.caravan-mp__slot')) return;
                if (selectedHandIdx < 0) return;
                const colIdx = parseInt(el.dataset.col, 10);
                const myState = state.playerStates[myIdx];
                const card = myState.hand[selectedHandIdx];
                const isMyCol = state.columns[colIdx].ownerId === myState.playerId;
                if ((card.rank >= 1 && card.rank <= 10) && isMyCol) {
                    submitMove({ type: 0, handIndex: selectedHandIdx, targetColumnIndex: colIdx });
                    selectedHandIdx = -1;
                } else {
                    alert('Number cards must go on your own caravan; faces must attach to a slot.');
                }
            });
        });

        root.querySelectorAll('.caravan-mp__slot').forEach(el => {
            el.addEventListener('click', evt => {
                evt.stopPropagation();
                if (selectedHandIdx < 0) return;
                const colIdx = parseInt(el.dataset.col, 10);
                const slotIdx = parseInt(el.dataset.slot, 10);
                if (slotIdx < 0) return;
                const myState = state.playerStates[myIdx];
                const card = myState.hand[selectedHandIdx];
                if (card.rank > 10) {
                    submitMove({ type: 1, handIndex: selectedHandIdx, targetColumnIndex: colIdx, targetSlotIndex: slotIdx });
                    selectedHandIdx = -1;
                } else {
                    alert('Pick a face card to attach to a slot.');
                }
            });
        });

        root.querySelector('[data-action="discard-card"]')?.addEventListener('click', () => {
            if (selectedHandIdx < 0) return;
            submitMove({ type: 2, handIndex: selectedHandIdx });
            selectedHandIdx = -1;
        });
        root.querySelector('[data-action="discard-caravan"]')?.addEventListener('click', () => {
            const colIdxStr = prompt('Discard which of your caravans? Enter A, B or C.');
            if (!colIdxStr) return;
            const idxOffset = colIdxStr.toUpperCase().charCodeAt(0) - 'A'.charCodeAt(0);
            const myStartIdx = state.columns.findIndex(c => c.ownerId === playerId);
            const target = myStartIdx + idxOffset;
            if (target < myStartIdx || target >= myStartIdx + 3) {
                alert('Invalid caravan letter (use the letter shown next to YOUR caravans).');
                return;
            }
            submitMove({ type: 3, targetColumnIndex: target });
        });

        root.querySelector('[data-action="surrender"]')?.addEventListener('click', () => {
            if (state.isFinished) return;
            if (!confirm('Surrender this match? Your opponent will be declared the winner.')) return;
            submitMove({ type: 4 });
            selectedHandIdx = -1;
        });
    }

    async function startSignalR() {
        if (typeof signalR === 'undefined') {
            console.warn('[CaravanMp] signalR global missing — falling back to polling.');
            return;
        }
        const connection = new signalR.HubConnectionBuilder()
            .withUrl('/hubs/game')
            .withAutomaticReconnect()
            .build();

        const onUpdate = async (payload) => {
            if (payload && (payload.State || payload.state)) {
                state = normalize(payload);
                render();
            } else {
                await loadState();
            }
        };

        connection.on('MoveMade', onUpdate);
        connection.on('GameStateChanged', onUpdate);
        connection.on('GameFinished', onUpdate);
        connection.on('PlayerJoined', () => loadState());
        connection.on('PlayerLeft', () => loadState());

        try {
            await connection.start();
            await connection.invoke('JoinGame', gameId);
            signalRConnected = true;
            console.log('[CaravanMp] SignalR connected, joined game group.');
        } catch (err) {
            console.error('[CaravanMp] SignalR connection error', err);
        }

        window.addEventListener('beforeunload', () => {
            try { connection.invoke('LeaveGame', gameId); } catch (_) { }
        });
    }

    startSignalR();
    setInterval(() => { if (!signalRConnected) loadState(); }, 4000);
    loadState();
})();
