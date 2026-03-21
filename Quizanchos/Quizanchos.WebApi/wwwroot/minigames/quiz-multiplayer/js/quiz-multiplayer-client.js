// Quiz Multiplayer Game Client Implementation
// Extends the universal GameClient for QuizMultiplayer-specific functionality

class QuizMultiplayerClient extends GameClient {
    constructor() {
        super(window.quizMultiplayerMinigameTypeId);
    }

    /**
     * Create a QuizMultiplayer move object
     * @param {number} optionPicked - The index of the selected option
     * @returns {Object} - QuizMultiplayer move object
     */
    createMoveObject(optionPicked) {
        return {
            gameType: "quizMultiplayer",
            optionPicked: optionPicked
        };
    }

    /**
     * Get default game parameters
     * @returns {Object} - Default parameters
     */
    getDefaultParameters() {
        return {
            totalCards: 10
        };
    }

    /**
     * Pick an answer for the current quiz question
     * @param {string} gameId - Game session ID
     * @param {string} playerId - Player ID
     * @param {number} optionPicked - Selected option index
     * @returns {Promise<Object>} - Move response
     */
    async pickAnswer(gameId, playerId, optionPicked) {
        const move = this.createMoveObject(optionPicked);
        return await this.makeMove(gameId, playerId, move);
    }

    /**
     * Get QuizMultiplayer-specific game state
     * @param {string} gameId - Game session ID
     * @returns {Promise<Object>} - Game state
     */
    async getQuizMultiplayerState(gameId) {
        const response = await this.getGameState(gameId);
        const state = response.state || response.State;
        return {
            gameId: response.gameId || response.GameId,
            players: response.players || response.Players,
            isFinished: response.isFinished ?? response.IsFinished,
            winner: response.winner || response.Winner,
            state: state,
            currentCardIndex: state?.currentCardIndex ?? state?.CurrentCardIndex,
            totalCards: state?.totalCards ?? state?.TotalCards,
            cards: state?.cards || state?.Cards || [],
            teams: state?.teams || state?.Teams || [],
            teamScores: state?.teamScores || state?.TeamScores || {},
            quizCategoryId: state?.quizCategoryId || state?.QuizCategoryId,
            secondsPerCard: state?.secondsPerCard ?? state?.SecondsPerCard ?? 30,
            optionCount: state?.optionCount ?? state?.OptionCount ?? 4
        };
    }
}

// Create a global instance for easy access
const qmClient = new QuizMultiplayerClient();
