// Ticket to Ride: Europe — game client
// Extends GameClient with TtR-Europe specific helpers.

class TicketToRideEuropeClient extends GameClient {
    constructor() {
        super(window.minigameConfig.minigameTypeId);
    }

    getDefaultParameters() {
        return {};
    }

    createMoveObject(kind, extras) {
        return Object.assign({ gameType: 'ticketToRideEurope', kind }, extras || {});
    }

    drawDeck(gameId, playerId) {
        return this.makeMove(gameId, playerId, this.createMoveObject('drawDeck'));
    }

    drawFace(gameId, playerId, faceIndex) {
        return this.makeMove(gameId, playerId, this.createMoveObject('drawFace', { faceIndex }));
    }

    drawTickets(gameId, playerId) {
        return this.makeMove(gameId, playerId, this.createMoveObject('drawTickets'));
    }

    keepTickets(gameId, playerId, keepFlags) {
        return this.makeMove(gameId, playerId, this.createMoveObject('keepTickets', { keepFlags }));
    }

    claimRoute(gameId, playerId, routeId, color, locomotives) {
        return this.makeMove(gameId, playerId, this.createMoveObject('claimRoute', {
            routeId,
            color,
            locomotives
        }));
    }

    buildStation(gameId, playerId, cityId, color, locomotives) {
        return this.makeMove(gameId, playerId, this.createMoveObject('buildStation', {
            cityId,
            color,
            locomotives
        }));
    }

    tunnelPay(gameId, playerId, extraLocomotives) {
        return this.makeMove(gameId, playerId, this.createMoveObject('tunnelPay', { extraLocomotives }));
    }

    tunnelSkip(gameId, playerId) {
        return this.makeMove(gameId, playerId, this.createMoveObject('tunnelSkip'));
    }

    async getTtrState(gameId) {
        const response = await this.getGameState(gameId);
        const state = response.state || response.State;
        return {
            gameId: response.gameId || response.GameId,
            players: response.players || response.Players || [],
            isFinished: response.isFinished ?? response.IsFinished ?? false,
            winner: response.winner || response.Winner,
            state: state
        };
    }
}

const ttrClient = new TicketToRideEuropeClient();
