class TicketToRideEuropeClient extends GameClient {
    constructor() {
        super(window.minigameConfig.minigameTypeId);
    }

    createMoveObject(action, data = {}) {
        return {
            gameType: "ticketToRideEurope",
            action,
            drawSources: data.drawSources,
            routeId: data.routeId,
            color: data.color,
            keepTicketIndexes: data.keepTicketIndexes,
            stationCity: data.stationCity
        };
    }

    getDefaultParameters() {
        return {};
    }

    async getState(gameId) {
        const response = await this.getGameState(gameId);
        const state = response.state || response.State || {};

        return {
            gameId: response.gameId || response.GameId,
            isFinished: response.isFinished ?? response.IsFinished,
            winner: response.winner || response.Winner,
            state
        };
    }
}

const ticketToRideEuropeClient = new TicketToRideEuropeClient();
