// Universal Game Client Interface
// All minigame clients should implement this interface

class GameClient {
    constructor(minigameType) {
        if (new.target === GameClient) {
            throw new TypeError("Cannot construct GameClient instances directly - use a specific implementation");
        }
        this.minigameType = minigameType;
        this.baseUrl = '/api/Game';
    }

    /**
     * Create a new game session
     * @param {Array<string>} playerIds - Array of player GUIDs
     * @param {Object} parameters - Game-specific parameters
     * @returns {Promise<Object>} - Game creation response
     */
    async createGame(playerIds, parameters) {
        const response = await fetch(`${this.baseUrl}/create`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                minigameType: this.minigameType,
                playerIds: playerIds,
                parameters: parameters
            })
        });

        if (!response.ok) {
            const error = await response.json();
            throw new Error(error.message || 'Failed to create game');
        }

        return await response.json();
    }

    /**
     * Make a move in the game
     * @param {string} gameId - Game session ID
     * @param {string} playerId - Player ID making the move
     * @param {Object} move - The move object specific to the game type
     * @returns {Promise<Object>} - Move response with updated state
     */
    async makeMove(gameId, playerId, move) {
        const response = await fetch(`${this.baseUrl}/move`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                gameId: gameId,
                playerId: playerId,
                move: move
            })
        });

        if (!response.ok) {
            const error = await response.json();
            throw new Error(error.message || 'Failed to make move');
        }

        return await response.json();
    }

    /**
     * Get current game state
     * @param {string} gameId - Game session ID
     * @returns {Promise<Object>} - Current game state
     */
    async getGameState(gameId) {
        const response = await fetch(`${this.baseUrl}/${gameId}/state?minigameType=${this.minigameType}`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
            }
        });

        if (!response.ok) {
            const error = await response.json();
            throw new Error(error.message || 'Failed to get game state');
        }

        return await response.json();
    }

    /**
     * Get player's active game
     * @returns {Promise<Object>} - Active game info
     */
    async getMyActiveGame() {
        const response = await fetch(`${this.baseUrl}/my-active`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
            }
        });

        if (!response.ok) {
            if (response.status === 404) {
                return null;
            }
            const error = await response.json();
            throw new Error(error.message || 'Failed to get active game');
        }

        return await response.json();
    }

    /**
     * Finish a game session
     * @param {string} gameId - Game session ID
     * @returns {Promise<Object>} - Finish response with final state
     */
    async finishGame(gameId) {
        const response = await fetch(`${this.baseUrl}/${gameId}/finish?minigameType=${this.minigameType}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            }
        });

        if (!response.ok) {
            const error = await response.json();
            throw new Error(error.message || 'Failed to finish game');
        }

        return await response.json();
    }

    /**
     * Create a move object specific to the game type
     * @abstract
     * @param {*} moveData - Raw move data
     * @returns {Object} - Formatted move object
     */
    createMoveObject(moveData) {
        throw new Error("Method 'createMoveObject' must be implemented by subclass");
    }

    /**
     * Get default game parameters
     * @abstract
     * @returns {Object} - Default parameters for game creation
     */
    getDefaultParameters() {
        throw new Error("Method 'getDefaultParameters' must be implemented by subclass");
    }
}
