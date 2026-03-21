// Quiz Multiplayer Game Page — real-time via SignalR
document.addEventListener('DOMContentLoaded', async () => {
    console.log('[QM] Page loaded, initializing...');

    ensureQuizMultiplayerGameLayout();

    const container = document.getElementById('qm-container');
    const gameId = container.getAttribute('data-game-id');
    const userId = document.body.getAttribute('data-user-id');
    const loadingContainer = document.getElementById('loading-container');

    console.log('[QM] GameId:', gameId, 'UserId:', userId);

    if (!gameId || !userId) {
        console.error('[QM] Missing game or user information');
        alert('Missing game or user information');
        window.location.href = window.minigameConfig.lobbyUrl;
        return;
    }

    // ?? SignalR connection ???????????????????????????????????????????????
    const connection = new signalR.HubConnectionBuilder()
        .withUrl('/hubs/game')
        .withAutomaticReconnect()
        .build();

    // When any player's move is processed the server broadcasts the updated
    // state to every member of the game group.  We re-fetch state and
    // re-render the UI so everyone stays in sync.
    connection.on('MoveMade', async (_payload) => {
        console.log('[QM] MoveMade received — refreshing state');
        await refreshGameState(gameId, userId);
    });

function ensureQuizMultiplayerGameLayout() {
    if (document.getElementById('qm-container')) return;

    const root = document.getElementById('minigame-root');
    if (!root) return;

    root.innerHTML = `
<div class="qm-container" data-game-id="${window.minigameConfig?.gameId ?? ''}" id="qm-container" style="display:none;">
    <div class="qm-header" id="qm-header">
        <h1 id="qm-question">Loading...</h1>
        <div class="question-info"><span id="question-number">Question <span data-current>1</span> of <span data-total>10</span></span></div>
    </div>
    <div class="qm-scoreboard" id="qm-scoreboard"></div>
    <div class="qm-waiting" id="qm-waiting" style="display:none;"><p>Waiting for other players... <span id="qm-answered-count">0</span>/<span id="qm-total-players">0</span></p></div>
    <div class="timeline-container" id="timeline-container"><div class="timeline" id="timeline"></div></div>
    <div class="quiz-options" id="qm-options"></div>
</div>
<div id="loading-container" style="display: flex; justify-content: center; align-items: center; height: 100vh;"><div id="loader"></div></div>
<div id="errorModal" class="modal" style="display: none;"><div class="modal-content"><span class="close-btn" id="closeModal">&times;</span><h2 id="modalHeader">Error</h2><p id="modalErrorText"></p><div id="modalButtons" class="modal-buttons"><button id="returnToMenu">Return to Menu</button></div></div></div>
<div id="roundResultModal" class="modal" style="display: none;"><div class="modal-content"><h2>Round Results</h2><div id="roundResultBody"></div></div></div>
<div id="finalStatsModal" class="modal" style="display: none;"><div class="modal-content"><h2 id="finalTitle">Game Over!</h2><div id="finalScoreboard"></div><p id="winnerText"></p><button id="goToResults">Return to Main Menu</button></div></div>
<div id="preloader"><div id="loader"></div></div>`;
}

    connection.on('GameStateChanged', async (_payload) => {
        console.log('[QM] GameStateChanged received — refreshing state');
        await refreshGameState(gameId, userId);
    });

    connection.on('GameFinished', async (payload) => {
        console.log('[QM] GameFinished received', payload);
        await refreshGameState(gameId, userId);
    });

    // The hub acknowledges a move submitted through SignalR with MoveReceived.
    connection.on('MoveReceived', (payload) => {
        console.log('[QM] MoveReceived ack', payload);
    });

    try {
        await connection.start();
        console.log('[QM] SignalR connected');
        await connection.invoke('JoinGame', gameId);
        console.log('[QM] Joined game group');
    } catch (err) {
        console.error('[QM] SignalR connection error', err);
    }

    // ?? Initial load ????????????????????????????????????????????????????
    try {
        console.log('[QM] Loading initial game state...');
        const gameState = await qmClient.getQuizMultiplayerState(gameId);
        console.log('[QM] Initial state:', JSON.stringify(gameState, null, 2));

        if (!gameState) throw new Error('Failed to load game state');

        loadingContainer.style.display = 'none';
        container.style.display = 'block';

        window._qmState = gameState;
        window._qmAnswered = false;
        renderAll(gameState, userId);
    } catch (error) {
        console.error('[QM] Error loading game:', error);
        loadingContainer.style.display = 'none';
        showErrorModal('Failed to load game. Please try again.');
    }

    // ?? Helpers ??????????????????????????????????????????????????????????

    async function refreshGameState(gid, uid) {
        try {
            const gs = await qmClient.getQuizMultiplayerState(gid);

            // If the card advanced, the player hasn't answered the new card yet
            const prevIdx = window._qmState?.currentCardIndex ?? 0;
            const newIdx = gs.currentCardIndex ?? 0;
            if (newIdx !== prevIdx) {
                window._qmAnswered = false;
            }

            window._qmState = gs;
            renderAll(gs, uid);
        } catch (e) {
            console.error('[QM] Error refreshing state', e);
        }
    }

    // ?? Event delegation for option clicks ??????????????????????????????
    document.getElementById('qm-options').addEventListener('click', async (event) => {
        const optionEl = event.target.closest('.quiz-option');
        if (!optionEl || window._qmAnswered) return;

        window._qmAnswered = true;
        const selectedIndex = parseInt(optionEl.dataset.index);
        console.log('[QM] Option clicked:', selectedIndex);

        // Visual feedback — highlight selected option
        optionEl.classList.add('selected-option');

        try {
            const result = await qmClient.pickAnswer(gameId, userId, selectedIndex);
            console.log('[QM] Move result:', result);

            if (result && result.state) {
                const updatedState = normalizeState(result);
                const prevIdx = window._qmState?.currentCardIndex ?? 0;
                const newIdx = updatedState.currentCardIndex ?? 0;
                if (newIdx !== prevIdx) {
                    window._qmAnswered = false;
                }
                window._qmState = updatedState;
                renderAll(updatedState, userId);
            }
        } catch (error) {
            console.error('[QM] Error submitting answer:', error);
            window._qmAnswered = false;
            optionEl.classList.remove('selected-option');
            showErrorModal('Failed to submit answer: ' + error.message);
        }
    });

    // Return to menu from modals
    document.getElementById('returnToMenu')?.addEventListener('click', () => {
        window.location.href = window.minigameConfig.lobbyUrl;
    });
    document.getElementById('goToResults')?.addEventListener('click', () => {
        window.location.href = window.minigameConfig.lobbyUrl;
    });
});

// ?? Rendering functions ?????????????????????????????????????????????????

function renderAll(gs, userId) {
    renderScoreboard(gs);
    renderQuestionInfo(gs);

    if (gs.isFinished) {
        showFinalStats(gs);
        return;
    }

    const cards = gs.cards || [];
    const idx = gs.currentCardIndex ?? 0;
    if (idx >= 0 && idx < cards.length) {
        const card = cards[idx];
        const answered = hasPlayerAnswered(card, userId);

        renderOptions(card, answered, userId);
        renderWaiting(card, gs, answered);
        startTimeline(card, gs.secondsPerCard);

        // If everyone answered this card, show round result briefly
        const totalPlayers = (gs.players || []).length;
        const answeredCount = Object.keys(card.playerAnswers || card.PlayerAnswers || {}).length;
        if (answeredCount >= totalPlayers && totalPlayers > 0) {
            showRoundResult(card, gs);
        }
    } else if (idx >= (gs.totalCards ?? 0)) {
        showFinalStats(gs);
    }

    loadCategoryQuestion(gs);
}

function normalizeState(result) {
    const s = result.state || result.State;
    return {
        gameId: result.gameId || result.GameId,
        players: s?.players || s?.Players || [],
        isFinished: result.isFinished ?? result.IsFinished ?? false,
        winner: result.winner || result.Winner,
        state: s,
        currentCardIndex: s?.currentCardIndex ?? s?.CurrentCardIndex ?? 0,
        totalCards: s?.totalCards ?? s?.TotalCards ?? 0,
        cards: s?.cards || s?.Cards || [],
        teams: s?.teams || s?.Teams || [],
        teamScores: s?.teamScores || s?.TeamScores || {},
        quizCategoryId: s?.quizCategoryId || s?.QuizCategoryId,
        secondsPerCard: s?.secondsPerCard ?? s?.SecondsPerCard ?? 30,
        optionCount: s?.optionCount ?? s?.OptionCount ?? 4
    };
}

function renderScoreboard(gs) {
    const sb = document.getElementById('qm-scoreboard');
    const teams = gs.teams || [];
    const scores = gs.teamScores || {};

    sb.innerHTML = teams.map(t => {
        const idx = t.teamIndex ?? t.TeamIndex ?? 0;
        const name = t.name || t.Name || `Team ${idx + 1}`;
        const score = scores[idx] ?? scores[String(idx)] ?? 0;
        const playerCount = (t.playerIds || t.PlayerIds || []).length;
        return `<div class="team-score team-${idx}">
                    <span class="team-name">${name}</span>
                    <span class="team-score-value">${score}</span>
                    <span class="team-player-count">${playerCount} players</span>
                </div>`;
    }).join('');
}

function renderQuestionInfo(gs) {
    const currentEl = document.querySelector('[data-current]');
    const totalEl = document.querySelector('[data-total]');
    if (currentEl) currentEl.textContent = (gs.currentCardIndex ?? 0) + 1;
    if (totalEl) totalEl.textContent = gs.totalCards ?? 0;
}

function renderOptions(card, answered, userId) {
    const container = document.getElementById('qm-options');
    const entityNames = card.entityNames || card.EntityNames || [];
    const optionCount = entityNames.length;

    container.setAttribute('data-option-count', optionCount);
    container.innerHTML = '';

    entityNames.forEach((name, index) => {
        const opt = document.createElement('div');
        opt.className = 'quiz-option';
        opt.dataset.index = index;

        if (answered) {
            opt.classList.add('disabled-option');
            const playerAnswer = getPlayerAnswer(card, userId);
            if (playerAnswer === index) {
                opt.classList.add('selected-option');
            }
        }

        opt.innerHTML = `<p class="quiz-option-text">${name}</p>`;
        container.appendChild(opt);
    });
}

function renderWaiting(card, gs, answered) {
    const waitDiv = document.getElementById('qm-waiting');
    const answeredCountEl = document.getElementById('qm-answered-count');
    const totalPlayersEl = document.getElementById('qm-total-players');

    const playerAnswers = card.playerAnswers || card.PlayerAnswers || {};
    const answeredCount = Object.keys(playerAnswers).length;
    const totalPlayers = (gs.players || []).length;

    if (answered && answeredCount < totalPlayers) {
        waitDiv.style.display = 'block';
        answeredCountEl.textContent = answeredCount;
        totalPlayersEl.textContent = totalPlayers;
    } else {
        waitDiv.style.display = 'none';
    }
}

function hasPlayerAnswered(card, userId) {
    const answers = card.playerAnswers || card.PlayerAnswers || {};
    return userId in answers;
}

function getPlayerAnswer(card, userId) {
    const answers = card.playerAnswers || card.PlayerAnswers || {};
    return answers[userId] ?? null;
}

async function loadCategoryQuestion(gs) {
    const categoryId = gs.quizCategoryId;
    if (!categoryId) {
        document.getElementById('qm-question').textContent = 'Which one is the biggest?';
        return;
    }
    try {
        const resp = await fetch(`/QuizCategory/GetById?id=${categoryId}`);
        if (resp.ok) {
            const cat = await resp.json();
            document.getElementById('qm-question').textContent = cat.questionToDisplay || 'Which one is the biggest?';
        }
    } catch (_) {
        document.getElementById('qm-question').textContent = 'Which one is the biggest?';
    }
}

function showRoundResult(card, gs) {
    const teamVotes = card.teamVotes || card.TeamVotes || {};
    const correctOption = card.correctOption ?? card.CorrectOption ?? 0;
    const entityNames = card.entityNames || card.EntityNames || [];
    const optionValues = card.optionValues || card.OptionValues || [];
    const teams = gs.teams || [];

    // Show correct / incorrect on options
    document.querySelectorAll('#qm-options .quiz-option').forEach((opt, idx) => {
        // Add value text
        const valText = document.createElement('p');
        valText.className = 'quiz-value-text';
        valText.textContent = optionValues[idx] ?? '';
        opt.appendChild(valText);

        if (idx === correctOption) {
            opt.classList.add('correct-option');
        } else {
            opt.classList.add('incorrect-option');
        }
    });

    // Build round result body
    const body = document.getElementById('roundResultBody');
    body.innerHTML = teams.map(t => {
        const tIdx = t.teamIndex ?? t.TeamIndex ?? 0;
        const tName = t.name || t.Name || `Team ${tIdx + 1}`;
        const vote = teamVotes[tIdx] ?? teamVotes[String(tIdx)];
        const correct = vote === correctOption;
        const voteName = vote != null ? (entityNames[vote] || `Option ${vote}`) : 'No vote';
        return `<p><strong>${tName}</strong>: ${voteName} ${correct ? '?' : '?'}</p>`;
    }).join('');

    const modal = document.getElementById('roundResultModal');
    modal.style.display = 'flex';

    // Auto-dismiss after 3 seconds
    setTimeout(() => {
        modal.style.display = 'none';
    }, 3000);
}

function showFinalStats(gs) {
    const teams = gs.teams || [];
    const scores = gs.teamScores || {};

    const sb = document.getElementById('finalScoreboard');
    sb.innerHTML = '<h3>Team Scores</h3>' + teams.map(t => {
        const idx = t.teamIndex ?? t.TeamIndex ?? 0;
        const name = t.name || t.Name || `Team ${idx + 1}`;
        const score = scores[idx] ?? scores[String(idx)] ?? 0;
        return `<p class="final-team-row"><strong>${name}</strong>: ${score} / ${gs.totalCards ?? 0}</p>`;
    }).join('');

    const winnerText = document.getElementById('winnerText');
    if (gs.winner) {
        winnerText.textContent = `?? Winner: ${gs.winner}`;
    } else {
        winnerText.textContent = "It's a draw!";
    }

    document.getElementById('finalStatsModal').style.display = 'flex';
}

function showErrorModal(message) {
    const modal = document.getElementById('errorModal');
    document.getElementById('modalErrorText').textContent = message;
    modal.style.display = 'flex';
}

// ?? Timeline ????????????????????????????????????????????????????????????
let activeTimeline = null;

function startTimeline(card, secondsPerCard) {
    stopTimeline();
    const timeline = document.getElementById('timeline');
    const creationTime = card.creationTime || card.CreationTime;
    if (!creationTime) return;

    let creationDate;
    if (typeof creationTime === 'string') {
        creationDate = new Date(creationTime.endsWith('Z') ? creationTime : creationTime + 'Z');
    } else {
        creationDate = new Date(creationTime);
    }

    function update() {
        const now = new Date();
        const elapsed = (now - creationDate) / 1000;
        const progress = Math.min((elapsed / secondsPerCard) * 100, 100);
        timeline.style.width = `${progress}%`;

        if (progress < 100) {
            activeTimeline = requestAnimationFrame(update);
        }
    }

    update();
}

function stopTimeline() {
    if (activeTimeline) {
        cancelAnimationFrame(activeTimeline);
        activeTimeline = null;
    }
}
