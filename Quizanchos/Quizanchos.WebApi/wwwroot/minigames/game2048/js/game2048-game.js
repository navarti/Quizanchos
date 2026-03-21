// 2048 Game Page - API-driven
document.addEventListener('DOMContentLoaded', async () => {
    const gameUrlTemplate = window.minigameConfig.gameUrlTemplate;
    const lobbyUrl = window.minigameConfig.lobbyUrl;
    ensureGame2048Layout();

    const wrapper = document.getElementById('game2048-wrapper');
    const gameId = wrapper.getAttribute('data-game-id');
    const userId = document.body.getAttribute('data-user-id');
    const loadingContainer = document.getElementById('loading-container');

    if (!gameId || !userId) {
        alert('Missing game or user information');
        window.location.href = lobbyUrl;
        return;
    }

    try {
        const gameState = await game2048Client.getGame2048State(gameId);

        if (!gameState) {
            throw new Error('Failed to load game state');
        }

        loadingContainer.style.display = 'none';
        wrapper.style.display = 'block';

        initializeGame(gameState, gameId, userId);
    } catch (error) {
        console.error('[2048] Error loading game:', error);
        loadingContainer.innerHTML = `<p>Failed to load game. <a href="${lobbyUrl}">Try again</a></p>`;
    }
});

function ensureGame2048Layout() {
    if (document.getElementById('game2048-wrapper')) return;

    const root = document.getElementById('minigame-root');
    if (!root) return;

    root.innerHTML = `
<div class="game2048-wrapper" data-game-id="${window.minigameConfig?.gameId ?? ''}" id="game2048-wrapper" style="display:none;">
    <div class="game2048-header">
        <h1>2048</h1>
        <div class="game2048-scores">
            <div class="score-box"><span class="score-label">Score</span><span class="score-value" id="current-score">0</span></div>
            <div class="score-box"><span class="score-label">Best Tile</span><span class="score-value" id="best-tile">0</span></div>
            <div class="score-box"><span class="score-label">Moves</span><span class="score-value" id="move-count">0</span></div>
        </div>
    </div>
    <div class="game2048-board" id="game2048-board"></div>
    <p class="game2048-hint">Use <strong>arrow keys</strong> or <strong>swipe</strong> to move tiles.</p>
    <div class="game2048-actions">
        <button id="finishGameBtn" class="btn-finish-game">Finish Game</button>
        <button id="newGameBtn" class="btn-new-game">New Game</button>
        <a href="${window.minigameConfig.lobbyUrl}" class="btn-back">Back</a>
    </div>
</div>
<div id="loading-container" style="display: flex; justify-content: center; align-items: center; height: 60vh;"><p>Loading game...</p></div>
<div id="gameOverModal" class="modal" style="display: none;">
    <div class="modal-content">
        <h2>Game Over!</h2>
        <p>Your Score: <strong id="finalScoreDisplay">0</strong></p>
        <p>Best Tile: <strong id="finalBestTile">0</strong></p>
        <p>Total Moves: <strong id="finalMoveCount">0</strong></p>
        <div class="modal-buttons">
            <button id="playAgainBtn">Play Again</button>
            <button id="returnHomeBtn">Return to Home</button>
        </div>
    </div>
</div>`;
}

let currentBoard = [];
let boardSize = 4;
let isProcessing = false;

function initializeGame(gameState, gameId, userId) {
    boardSize = gameState.size || 4;
    currentBoard = gameState.board || [];
    updateUI(gameState);
    renderBoard(currentBoard);
    setupControls(gameId, userId);
    setupButtons(gameId, userId);
}

function updateUI(gameState) {
    document.getElementById('current-score').textContent = gameState.score || 0;
    document.getElementById('best-tile').textContent = gameState.bestTile || 0;
    document.getElementById('move-count').textContent = gameState.moveCount || 0;
}

function renderBoard(board) {
    const boardEl = document.getElementById('game2048-board');
    boardEl.innerHTML = '';
    boardEl.style.gridTemplateColumns = `repeat(${boardSize}, 1fr)`;
    boardEl.style.gridTemplateRows = `repeat(${boardSize}, 1fr)`;

    for (let r = 0; r < boardSize; r++) {
        for (let c = 0; c < boardSize; c++) {
            const value = board[r] ? board[r][c] : 0;
            const cell = document.createElement('div');
            cell.className = 'game2048-cell';
            if (value > 0) {
                cell.classList.add('tile-' + Math.min(value, 8192));
                cell.textContent = value;
            }
            boardEl.appendChild(cell);
        }
    }
}

// Direction enum: Up=0, Down=1, Left=2, Right=3
const DIRECTIONS = { ArrowUp: 0, ArrowDown: 1, ArrowLeft: 2, ArrowRight: 3 };

function setupControls(gameId, userId) {
    document.addEventListener('keydown', async (e) => {
        if (!(e.key in DIRECTIONS) || isProcessing) return;
        e.preventDefault();
        await submitDirection(gameId, userId, DIRECTIONS[e.key]);
    });

    // Touch/swipe support
    let touchStartX = 0, touchStartY = 0;
    const boardEl = document.getElementById('game2048-board');

    boardEl.addEventListener('touchstart', (e) => {
        touchStartX = e.touches[0].clientX;
        touchStartY = e.touches[0].clientY;
    }, { passive: true });

    boardEl.addEventListener('touchend', async (e) => {
        if (isProcessing) return;
        const dx = e.changedTouches[0].clientX - touchStartX;
        const dy = e.changedTouches[0].clientY - touchStartY;
        const absDx = Math.abs(dx);
        const absDy = Math.abs(dy);

        if (Math.max(absDx, absDy) < 30) return; // too short

        let direction;
        if (absDx > absDy) {
            direction = dx > 0 ? 3 : 2; // Right : Left
        } else {
            direction = dy > 0 ? 1 : 0; // Down : Up
        }
        await submitDirection(gameId, userId, direction);
    }, { passive: true });
}

async function submitDirection(gameId, userId, direction) {
    isProcessing = true;
    try {
        const result = await game2048Client.submitMove(gameId, userId, direction);

        if (result && result.state) {
            const state = result.state;
            currentBoard = state.board || state.Board || currentBoard;
            const score = state.score ?? state.Score ?? 0;
            const bestTile = state.bestTile ?? state.BestTile ?? 0;
            const moveCount = state.moveCount ?? state.MoveCount ?? 0;

            document.getElementById('current-score').textContent = score;
            document.getElementById('best-tile').textContent = bestTile;
            document.getElementById('move-count').textContent = moveCount;

            renderBoard(currentBoard);

            if (result.isFinished) {
                showGameOver(score, bestTile, moveCount);
            }
        }
    } catch (error) {
        // "No tiles can be moved" is expected, not a real error
        if (!error.message.includes('No tiles')) {
            console.error('[2048] Move error:', error);
        }
    } finally {
        isProcessing = false;
    }
}

function showGameOver(score, bestTile, moveCount) {
    document.getElementById('finalScoreDisplay').textContent = score;
    document.getElementById('finalBestTile').textContent = bestTile;
    document.getElementById('finalMoveCount').textContent = moveCount;
    document.getElementById('gameOverModal').style.display = 'flex';
}

function setupButtons(gameId, userId) {
    const newGameBtn = document.getElementById('newGameBtn');
    const playAgainBtn = document.getElementById('playAgainBtn');
    const returnHomeBtn = document.getElementById('returnHomeBtn');
    const finishGameBtn = document.getElementById('finishGameBtn');

    async function finishAndStartNew() {
        try {
            await game2048Client.finishGame(gameId);
        } catch (_) { /* ignore */ }

        try {
            const result = await game2048Client.createGame([userId], { size: boardSize });
            if (result && result.gameId) {
                window.location.href = window.minigameConfig.gameUrlTemplate.replace('{gameId}', result.gameId);
            }
        } catch (error) {
            alert('Failed to create new game: ' + error.message);
        }
    }

    async function finishCurrentGame() {
        try {
            const result = await game2048Client.finishGame(gameId);
            if (result) {
                const state = result.state || {};
                const score = state.score ?? state.Score ?? 0;
                const bestTile = state.bestTile ?? state.BestTile ?? 0;
                const moveCount = state.moveCount ?? state.MoveCount ?? 0;
                showGameOver(score, bestTile, moveCount);
            }
        } catch (error) {
            alert('Failed to finish game: ' + error.message);
        }
    }

    if (finishGameBtn) finishGameBtn.addEventListener('click', finishCurrentGame);
    if (newGameBtn) newGameBtn.addEventListener('click', finishAndStartNew);
    if (playAgainBtn) playAgainBtn.addEventListener('click', finishAndStartNew);
    if (returnHomeBtn) returnHomeBtn.addEventListener('click', () => {
        window.location.href = window.minigameConfig.lobbyUrl;
    });
}
