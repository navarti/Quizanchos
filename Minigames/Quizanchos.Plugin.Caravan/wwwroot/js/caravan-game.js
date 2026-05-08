(function () {
    const root = document.getElementById('minigame-root');
    if (!root) return;

    const cfg = window.minigameConfig || {};
    const gameId = cfg.gameId;
    const minigameTypeId = cfg.minigameTypeId;
    const playerId = document.body.getAttribute('data-user-id') || '';

    let state = null;
    let selectedHandIdx = -1;

    const SUIT_GLYPH = { 0: '♥', 1: '♦', 2: '♣', 3: '♠' };
    const RANK_LABEL = {
        1: 'A', 2: '2', 3: '3', 4: '4', 5: '5', 6: '6', 7: '7', 8: '8',
        9: '9', 10: '10', 11: 'J', 12: 'Q', 13: 'K', 14: 'JK',
    };

    function isRedSuit(s) { return s === 0 || s === 1; }

    async function loadState() {
        try {
            const resp = await fetch(`/api/Game/${gameId}/state?minigameType=${minigameTypeId}`, {
                credentials: 'include',
            });
            if (!resp.ok) throw new Error(`State HTTP ${resp.status}`);
            const wrapper = await resp.json();
            // Server returns { gameId, minigameType, players, isFinished, winner, state: {...} }
            state = wrapper.state || wrapper.State || wrapper;
            // Hoist top-level fields onto state for convenience
            state.isFinished = wrapper.isFinished ?? wrapper.IsFinished ?? state.isFinished;
            state.winner = wrapper.winner ?? wrapper.Winner ?? state.winner;
            state.players = wrapper.players ?? wrapper.Players ?? state.players;
            render();
        } catch (err) {
            root.innerHTML = `<div class="caravan caravan__lobby"><p>Could not load game state.</p><p>${err.message}</p></div>`;
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
                    move: { gameType: 'caravan', ...move },
                }),
            });
            if (!resp.ok) {
                const err = await resp.json().catch(() => ({}));
                alert('Move rejected: ' + (err.message || resp.status));
                return;
            }
            await loadState();
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

    function renderCard(card, opts) {
        opts = opts || {};
        const klass = ['caravan__card'];
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
                <div class="caravan__slot" data-col="${idx}" data-slot="${sIdx}">
                    ${renderCard(slot.card)}
                    <div class="caravan__attached">${attached}</div>
                </div>
            `;
        }).join('');
        return `
            <div class="caravan__column" data-col="${idx}">
                <div class="caravan__column-head">
                    <span>Caravan ${columnLabel(idx)}</span>
                    <span class="caravan__column-value ${valueClass}">${col.value}</span>
                </div>
                ${slotsHtml || '<div class="caravan__slot" data-col="' + idx + '" data-slot="-1" style="opacity:.6">empty</div>'}
            </div>
        `;
    }

    function render() {
        if (!state || !state.playerStates || state.playerStates.length < 2) {
            root.innerHTML = '<div class="caravan"><h2>Caravan</h2><p>Loading...</p></div>';
            return;
        }

        const myIdx = getMyIndex();
        const oppIdx = 1 - myIdx;
        const myState = state.playerStates[myIdx];
        const oppState = state.playerStates[oppIdx];

        const myColumns = state.columns.filter(c => c.ownerId === myState.playerId);
        const oppColumns = state.columns.filter(c => c.ownerId === oppState.playerId);

        const myTurn = state.currentTurnIndex === myIdx && !state.isFinished;
        const finishedHtml = state.isFinished
            ? `<div class="caravan__finished">${state.winner === playerId ? 'You won the caravans!' : (state.winner ? 'AI won the caravans.' : 'Caravans ended in a draw.')}</div>`
            : '';

        root.innerHTML = `
            <div class="caravan">
                <h2>Caravan</h2>
                <div class="caravan__status">${state.lastMoveDescription || (myTurn ? 'Your turn.' : 'Opponent thinking...')}</div>
                <div class="caravan__board">
                    <div class="caravan__side-label">Opponent caravans</div>
                    ${oppColumns.map((c) => renderColumn(c, state.columns.indexOf(c))).join('')}
                    <div class="caravan__side-label">Your caravans</div>
                    ${myColumns.map((c) => renderColumn(c, state.columns.indexOf(c))).join('')}
                </div>
                <h3>Hand</h3>
                <div class="caravan__hand">
                    ${myState.hand.map((card, idx) => `<div class="caravan__hand-card" data-handidx="${idx}">${renderCard(card, { selected: idx === selectedHandIdx, handIdx: idx })}</div>`).join('')}
                </div>
                <div class="caravan__actions">
                    <button class="caravan__btn" data-action="discard-card" ${selectedHandIdx < 0 || !myTurn ? 'disabled' : ''}>Discard selected card</button>
                    <button class="caravan__btn" data-action="discard-caravan" ${!myTurn ? 'disabled' : ''}>Discard a caravan...</button>
                </div>
                ${finishedHtml}
            </div>
        `;

        wireEvents(myTurn, myIdx);
    }

    function wireEvents(myTurn, myIdx) {
        root.querySelectorAll('.caravan__hand-card').forEach(el => {
            el.addEventListener('click', () => {
                const idx = parseInt(el.dataset.handidx, 10);
                selectedHandIdx = (selectedHandIdx === idx) ? -1 : idx;
                render();
            });
        });

        if (!myTurn) return;

        root.querySelectorAll('.caravan__column').forEach(el => {
            el.addEventListener('click', evt => {
                if (evt.target.closest('.caravan__slot')) return;
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

        root.querySelectorAll('.caravan__slot').forEach(el => {
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

        root.querySelector('[data-action="discard-card"]').addEventListener('click', () => {
            if (selectedHandIdx < 0) return;
            submitMove({ type: 2, handIndex: selectedHandIdx });
            selectedHandIdx = -1;
        });
        root.querySelector('[data-action="discard-caravan"]').addEventListener('click', () => {
            const colIdxStr = prompt('Discard which of your caravans? Enter A, B or C.');
            if (!colIdxStr) return;
            const idxOffset = colIdxStr.toUpperCase().charCodeAt(0) - 'A'.charCodeAt(0);
            const myStartIdx = state.columns.findIndex(c => c.ownerId === playerId);
            const target = myStartIdx + idxOffset;
            if (target < myStartIdx || target >= myStartIdx + 3) {
                alert('Invalid caravan letter.');
                return;
            }
            submitMove({ type: 3, targetColumnIndex: target });
        });
    }

    setInterval(loadState, 2000);
    loadState();
})();
