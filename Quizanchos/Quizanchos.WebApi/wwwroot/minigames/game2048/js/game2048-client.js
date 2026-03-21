// 2048 Game Client Implementation
// Extends the universal GameClient for 2048-specific functionality

class Game2048Client extends GameClient {
    constructor() {
        super(window.game2048MinigameTypeId);
    }

    /**
     * Create a 2048 move object
     * @param {number} direction - 0=Up, 1=Down, 2=Left, 3=Right
     * @returns {Object} - 2048 move object
     */
    createMoveObject(direction) {
        return {
            gameType: "game2048",
            direction: direction
        };
    }

    /**
     * Get default 2048 game parameters
     * @returns {Object} - Default parameters
     */
    getDefaultParameters() {
        return {
            size: 4
        };
    }

    /**
     * Submit a direction move
     * @param {string} gameId - Game session ID
     * @param {string} playerId - Player ID
     * @param {number} direction - Direction enum value
     * @returns {Promise<Object>} - Move response
     */
    async submitMove(gameId, playerId, direction) {
        const move = this.createMoveObject(direction);
        return await this.makeMove(gameId, playerId, move);
    }

    /**
     * Get 2048-specific game state
     * @param {string} gameId - Game session ID
     * @returns {Promise<Object>} - 2048 game state
     */
    async getGame2048State(gameId) {
        const response = await this.getGameState(gameId);

        const state = response.state || response.State || {};
        return {
            gameId: response.gameId || response.GameId,
            players: response.players || response.Players,
            isFinished: response.isFinished ?? response.IsFinished,
            winner: response.winner || response.Winner,
            state: state,
            board: state.board || state.Board || [],
            size: state.size ?? state.Size ?? 4,
            score: state.score ?? state.Score ?? 0,
            bestTile: state.bestTile ?? state.BestTile ?? 0,
            moveCount: state.moveCount ?? state.MoveCount ?? 0
        };
    }
}

// Create a global instance
const game2048Client = new Game2048Client();
