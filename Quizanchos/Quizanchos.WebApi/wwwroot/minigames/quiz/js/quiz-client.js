// Quiz Game Client Implementation
// Extends the universal GameClient for Quiz-specific functionality

class QuizGameClient extends GameClient {
    constructor() {
        super(window.minigameConfig.minigameTypeId);
    }

    /**
     * Create a Quiz move object
     * @param {number} optionPicked - The index of the selected option
     * @returns {Object} - Quiz move object
     */
    createMoveObject(optionPicked) {
        return {
            gameType: "quiz",
            optionPicked: optionPicked
        };
    }

    /**
     * Get default Quiz game parameters
     * @returns {Object} - Default parameters
     */
    getDefaultParameters() {
        return {
            totalCards: 10
        };
    }

    /**
     * Create Quiz game with custom settings
     * @param {string} playerId - Player GUID
     * @param {number} totalCards - Number of cards
     * @param {Object} additionalParams - Additional game parameters
     * @returns {Promise<Object>} - Game creation response
     */
    async createQuizGame(playerId, totalCards, additionalParams = {}) {
        const parameters = {
            totalCards: totalCards,
            ...additionalParams
        };

        return await this.createGame([playerId], parameters);
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
     * Get Quiz-specific game state
     * @param {string} gameId - Game session ID
     * @returns {Promise<Object>} - Quiz game state with currentCardIndex, playerScores, etc.
     */
    async getQuizState(gameId) {
        const response = await this.getGameState(gameId);
        
        // Return the full response to include all state data
        return {
            gameId: response.gameId || response.GameId,
            players: response.players || response.Players,
            isFinished: response.isFinished ?? response.IsFinished,
            winner: response.winner || response.Winner,
            state: response.state || response.State, // Keep the full state object
            // Also expose commonly used properties at top level for convenience
            currentCardIndex: response.state?.currentCardIndex ?? response.state?.CurrentCardIndex ?? response.State?.CurrentCardIndex,
            playerScores: response.state?.playerScores || response.state?.PlayerScores || response.State?.PlayerScores,
            totalCards: response.state?.totalCards ?? response.state?.TotalCards ?? response.State?.TotalCards,
            cards: response.state?.cards || response.state?.Cards || response.State?.Cards,
            quizCategoryId: response.state?.quizCategoryId || response.state?.QuizCategoryId || response.State?.QuizCategoryId,
            secondsPerCard: response.state?.secondsPerCard ?? response.state?.SecondsPerCard ?? response.State?.SecondsPerCard
        };
    }
}

// Create a global instance for easy access
const quizClient = new QuizGameClient();
