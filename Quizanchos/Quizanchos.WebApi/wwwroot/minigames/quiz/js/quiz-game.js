// Quiz Game Page - API-driven
document.addEventListener('DOMContentLoaded', async () => {
    console.log('[QUIZ-GAME] Page loaded, initializing...');
    
    const container = document.getElementById('quiz-container');
    const gameId = container.getAttribute('data-game-id');
    const userId = document.body.getAttribute('data-user-id');
    const loadingContainer = document.getElementById('loading-container');

    console.log('[QUIZ-GAME] GameId:', gameId, 'UserId:', userId);

    if (!gameId || !userId) {
        console.error('[QUIZ-GAME] Missing game or user information');
        alert('Missing game or user information');
        window.location.href = window.minigameConfig?.lobbyUrl ?? window.quizLobbyUrl;
        return;
    }

    try {
        console.log('[QUIZ-GAME] Loading game state from API...');
        // Load game state
        const gameState = await quizClient.getQuizState(gameId);
        
        console.log('[QUIZ-GAME] Game state received:', JSON.stringify(gameState, null, 2));
        
        if (!gameState) {
            throw new Error('Failed to load game state');
        }

        // Hide loading, show game
        loadingContainer.style.display = 'none';
        container.style.display = 'block';

        // Initialize the quiz UI
        await initializeQuizGame(gameState, gameId, userId);
    } catch (error) {
        console.error('[QUIZ-GAME] Error loading quiz game:', error);
        loadingContainer.style.display = 'none';
        showError('Failed to load quiz game. Please try again.');
    }
});

async function initializeQuizGame(gameState, gameId, userId) {
    console.log('[QUIZ-GAME] initializeQuizGame called');
    
    // Update UI with game state
    updateQuizUI(gameState);

    // Setup event listeners
    setupQuizEventListeners(gameState, gameId, userId);

    // Setup finish button
    setupFinishButton(gameId);

    // Start timeline if there's a current card
    const state = gameState.state || gameState;
    const currentCardIndex = state.currentCardIndex ?? state.CurrentCardIndex ?? 0;
    const cards = state.cards || state.Cards || [];
    
    if (currentCardIndex >= 0 && currentCardIndex < cards.length) {
        const currentCard = cards[currentCardIndex];
        const creationTime = currentCard.creationTime || currentCard.CreationTime;
        const secondsPerCard = gameState.secondsPerCard || state.secondsPerCard || state.SecondsPerCard || 30;
        startTimeline(creationTime, secondsPerCard);
    }
}

function updateQuizUI(gameState) {
    console.log('[QUIZ-GAME] updateQuizUI called with:', gameState);
    
    // Get the state object which contains quiz-specific data
    const state = gameState.state || gameState;
    
    // Update question number and score (handle both camelCase and PascalCase)
    const questionNumber = document.querySelector('[data-current]');
    const totalQuestions = document.querySelector('[data-total]');
    const scoreDisplay = document.querySelector('[data-score]');

    const currentCardIndex = state.currentCardIndex ?? state.CurrentCardIndex ?? 0;
    const totalCards = state.totalCards ?? state.TotalCards ?? 10;
    const playerScores = state.playerScores || state.PlayerScores || {};

    console.log('[QUIZ-GAME] Current card index:', currentCardIndex, 'Total cards:', totalCards);
    console.log('[QUIZ-GAME] Player scores:', playerScores);

    questionNumber.textContent = currentCardIndex + 1;
    totalQuestions.textContent = totalCards;
    
    // Get the first player's score
    const firstPlayerId = Object.keys(playerScores)[0];
    const score = firstPlayerId ? playerScores[firstPlayerId] : 0;
    scoreDisplay.textContent = score;

    // Load category info and render options
    loadCategoryAndRenderOptions(gameState);
}

async function loadCategoryAndRenderOptions(gameState) {
    console.log('[QUIZ-GAME] loadCategoryAndRenderOptions called');
    
    try {
        // Get the state object which contains quiz-specific data
        const state = gameState.state || gameState;
        
        // Try both camelCase and PascalCase for compatibility
        const categoryId = state.quizCategoryId || state.QuizCategoryId;
        
        console.log('[QUIZ-GAME] Category ID:', categoryId);
        
        if (!categoryId) {
            console.error('[QUIZ-GAME] Category ID not found in game state:', gameState);
            document.getElementById('quiz-question').textContent = 'Which one is the biggest?';
            return;
        }

        // Load category information
        console.log('[QUIZ-GAME] Loading category from /QuizCategory/GetById?id=' + categoryId);
        const response = await fetch(`/QuizCategory/GetById?id=${categoryId}`);
        if (response.ok) {
            const category = await response.json();
            console.log('[QUIZ-GAME] Category loaded:', category);
            document.getElementById('quiz-question').textContent = category.questionToDisplay || 'Which one is the biggest?';
        } else {
            console.error('[QUIZ-GAME] Failed to load category, status:', response.status);
        }

        // Render current card options
        const currentCardIndex = state.currentCardIndex || state.CurrentCardIndex || 0;
        const cards = state.cards || state.Cards || [];
        
        console.log('[QUIZ-GAME] Current card index:', currentCardIndex, 'Cards array:', cards);
        console.log('[QUIZ-GAME] Cards count:', cards.length);
        
        if (currentCardIndex >= 0 && currentCardIndex < cards.length) {
            const currentCard = cards[currentCardIndex];
            console.log('[QUIZ-GAME] Rendering current card:', currentCard);
            renderOptions(currentCard);
        } else {
            console.warn('[QUIZ-GAME] No current card to render. CurrentCardIndex:', currentCardIndex, 'Cards.length:', cards.length);
            console.warn('[QUIZ-GAME] ISSUE: Cards array is empty or index is out of bounds!');
        }
    } catch (error) {
        console.error('[QUIZ-GAME] Error loading category:', error);
        document.getElementById('quiz-question').textContent = 'Which one is the biggest?';
    }
}

function renderOptions(card) {
    console.log('[QUIZ-GAME] renderOptions called with card:', card);
    
    const optionsContainer = document.getElementById('quiz-options');
    optionsContainer.innerHTML = '';

    // Handle both camelCase and PascalCase
    const entityNames = card.entityNames || card.EntityNames || [];
    
    console.log('[QUIZ-GAME] Entity names to render:', entityNames);
    
    if (entityNames.length === 0) {
        console.error('[QUIZ-GAME] No entity names to render!');
        return;
    }
    
    entityNames.forEach((name, index) => {
        console.log('[QUIZ-GAME] Creating option', index, ':', name);
        const option = document.createElement('div');
        option.className = 'quiz-option';
        option.innerHTML = `<p class="quiz-option-text">${name}</p>`;
        option.dataset.index = index;
        optionsContainer.appendChild(option);
    });
    
    console.log('[QUIZ-GAME] Rendered', entityNames.length, 'options');
}

function setupQuizEventListeners(gameState, gameId, userId) {
    const optionsContainer = document.getElementById('quiz-options');
    let isAnswerSubmitted = false;

    optionsContainer.addEventListener('click', async (event) => {
        const optionElement = event.target.closest('.quiz-option');
        if (!optionElement || isAnswerSubmitted) return;

        isAnswerSubmitted = true;
        stopTimeline();

        const selectedIndex = parseInt(optionElement.dataset.index);

        // Get the current card index BEFORE submitting (this is the card being answered)
        const state = gameState.state || gameState;
        const answeredCardIndex = state.currentCardIndex ?? state.CurrentCardIndex ?? 0;
        const cards = state.cards || state.Cards || [];
        const answeredCard = cards[answeredCardIndex];

        console.log('[QUIZ-GAME] Answer selected:', selectedIndex, 'for card index:', answeredCardIndex);

        try {
            // Submit answer via API
            const result = await quizClient.pickAnswer(gameId, userId, selectedIndex);

            console.log('[QUIZ-GAME] Answer submitted, result:', result);

            if (result && result.state) {
                // Show correct/incorrect for the card we just answered (not the new current card)
                showAnswerFeedback(answeredCard, selectedIndex);

                // Update score
                const scoreDisplay = document.querySelector('[data-score]');
                const resultState = result.state;
                const playerScores = resultState.playerScores || resultState.PlayerScores || {};
                const playerScore = playerScores[userId] || 0;
                scoreDisplay.textContent = playerScore;

                // Check if game is finished or if there are more cards
                const newCurrentCardIndex = resultState.currentCardIndex ?? resultState.CurrentCardIndex ?? 0;
                const totalCards = resultState.totalCards || resultState.TotalCards || 0;

                console.log('[QUIZ-GAME] After answer - Current card index:', newCurrentCardIndex, 'Total cards:', totalCards, 'Is finished:', result.isFinished);

                if (result.isFinished || newCurrentCardIndex >= totalCards) {
                    // Game finished
                    console.log('[QUIZ-GAME] Game finished!');
                    setTimeout(() => {
                        showFinalStats(playerScore, totalCards);
                    }, 2000);
                } else {
                    // More cards to answer - reload to show next card
                    console.log('[QUIZ-GAME] Moving to next card (index', newCurrentCardIndex, ') after delay');
                    setTimeout(() => {
                        window.location.reload();
                    }, 2000);
                }
            }
        } catch (error) {
            console.error('[QUIZ-GAME] Error submitting answer:', error);
            alert('Failed to submit answer. Please try again.');
            isAnswerSubmitted = false;
        }
    });
}

async function moveToNextCard(gameId, nextCardIndex) {
    console.log('[QUIZ-GAME] Moving to card index:', nextCardIndex);
    window.location.reload();
}

function showAnswerFeedback(card, selectedIndex) {
    const options = document.querySelectorAll('.quiz-option');
    
    // Handle both camelCase and PascalCase
    const optionValues = card.optionValues || card.OptionValues || [];
    const correctOption = card.correctOption ?? card.CorrectOption ?? 0;
    
    options.forEach((option, index) => {
        const valueText = document.createElement('p');
        valueText.className = 'quiz-value-text';
        valueText.textContent = optionValues[index] || '';
        option.appendChild(valueText);

        if (index === correctOption) {
            option.classList.add('correct-option');
        } else {
            option.classList.add('incorrect-option');
        }
    });
}

let activeTimeline = null;

function startTimeline(creationTime, secondsPerCard) {
    const timeline = document.getElementById('timeline');
    
    // Parse the creation time ensuring it's treated as UTC
    // Backend sends DateTime.UtcNow, so we need to parse it correctly
    let creationDate;
    if (typeof creationTime === 'string') {
        // Ensure the date string is parsed as UTC
        creationDate = new Date(creationTime.endsWith('Z') ? creationTime : creationTime + 'Z');
    } else {
        creationDate = new Date(creationTime);
    }
    
    console.log('[QUIZ-GAME] Timeline started - Creation time:', creationTime, 'Parsed as:', creationDate.toISOString(), 'Seconds per card:', secondsPerCard);
    
    function updateTimeline() {
        const now = new Date();
        const elapsed = (now - creationDate) / 1000;
        const progress = Math.min((elapsed / secondsPerCard) * 100, 100);
        
        timeline.style.width = `${progress}%`;
        
        if (progress < 100) {
            activeTimeline = requestAnimationFrame(updateTimeline);
        } else {
            console.log('[QUIZ-GAME] Timeline expired - elapsed:', elapsed, 'seconds');
            showTimeUpModal();
        }
    }
    
    updateTimeline();
}

function stopTimeline() {
    if (activeTimeline) {
        cancelAnimationFrame(activeTimeline);
        activeTimeline = null;
    }
}

function showTimeUpModal() {
    const modal = document.getElementById('errorModal');
    const modalText = document.getElementById('modalErrorText');
    modalText.textContent = "Time's up! What's next? Your call!";
    modal.style.display = 'flex';
}

function showFinalStats(score, total) {
    const modal = document.getElementById('finalStatsModal');
    document.getElementById('scoreDisplay').textContent = score;
    document.getElementById('totalCardsDisplay').textContent = total;
    modal.style.display = 'flex';
    
    document.getElementById('goToResults').addEventListener('click', () => {
        window.location.href = window.minigameConfig?.lobbyUrl ?? window.quizLobbyUrl;
    });
}

function showError(message) {
    const modal = document.getElementById('errorModal');
    const modalText = document.getElementById('modalErrorText');
    modalText.textContent = message;
    modal.style.display = 'flex';
    
    document.getElementById('returnToMenu').addEventListener('click', () => {
        window.location.href = window.minigameConfig?.lobbyUrl ?? window.quizLobbyUrl;
    });
}

function setupFinishButton(gameId) {
    const finishBtn = document.getElementById('finishQuizBtn');
    if (!finishBtn) return;

    finishBtn.addEventListener('click', async () => {
        try {
            stopTimeline();
            const result = await quizClient.finishGame(gameId);
            if (result) {
                const state = result.state || result.State || {};
                const playerScores = state.playerScores || state.PlayerScores || {};
                const firstPlayerId = Object.keys(playerScores)[0];
                const score = firstPlayerId ? playerScores[firstPlayerId] : 0;
                const totalCards = state.totalCards ?? state.TotalCards ?? 0;
                showFinalStats(score, totalCards);
            }
        } catch (error) {
            console.error('[QUIZ-GAME] Error finishing game:', error);
            alert('Failed to finish game: ' + error.message);
        }
    });
}
