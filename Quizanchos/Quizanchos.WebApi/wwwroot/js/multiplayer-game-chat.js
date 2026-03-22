document.addEventListener('DOMContentLoaded', async () => {
    const root = document.getElementById('minigame-root');
    const gameId = root?.dataset?.gameId;
    const viewMode = root?.dataset?.viewMode;
    const minigameTypeId = root?.dataset?.minigameTypeId;
    const userId = document.body.getAttribute('data-user-id');

    if (!root || viewMode !== 'game' || !gameId || !minigameTypeId || !userId || typeof signalR === 'undefined') {
        return;
    }

    if (!await isMultiplayerGame(gameId, minigameTypeId)) {
        return;
    }

    renderChatLayout();

    const messagesContainer = document.getElementById('multiplayer-chat-messages');
    const messageInput = document.getElementById('multiplayer-chat-input');
    const sendButton = document.getElementById('multiplayer-chat-send');

    const connection = new signalR.HubConnectionBuilder()
        .withUrl('/hubs/game')
        .withAutomaticReconnect()
        .build();

    connection.on('ChatMessageReceived', payload => {
        appendMessage(messagesContainer, payload, userId);
    });

    connection.onreconnecting(() => {
        appendSystemMessage(messagesContainer, 'Reconnecting...');
    });

    connection.onreconnected(async () => {
        appendSystemMessage(messagesContainer, 'Connected');
        await safeJoin();
    });

    sendButton?.addEventListener('click', sendMessage);
    messageInput?.addEventListener('keydown', event => {
        if (event.key === 'Enter' && !event.shiftKey) {
            event.preventDefault();
            sendMessage();
        }
    });

    window.addEventListener('beforeunload', async () => {
        try {
            await connection.invoke('LeaveGame', gameId);
        } catch (_) {
        }
    });

    try {
        await connection.start();
        await safeJoin();
    } catch (_) {
        appendSystemMessage(messagesContainer, 'Chat unavailable');
    }

    async function safeJoin() {
        try {
            await connection.invoke('JoinGame', gameId);
        } catch (_) {
        }
    }

    async function sendMessage() {
        if (!messageInput) {
            return;
        }

        const text = messageInput.value.trim();
        if (!text) {
            return;
        }

        try {
            await connection.invoke('SendChatMessage', gameId, text);
            messageInput.value = '';
        } catch (error) {
            appendSystemMessage(messagesContainer, error?.message || 'Failed to send message');
        }
    }
});

async function isMultiplayerGame(gameId, minigameTypeId) {
    try {
        const response = await fetch(`/api/Game/${gameId}/state?minigameType=${minigameTypeId}`);
        if (!response.ok) {
            return false;
        }

        const payload = await response.json();
        const players = payload.players || payload.Players || [];
        return Array.isArray(players) && players.length > 1;
    } catch (_) {
        return false;
    }
}

function renderChatLayout() {
    if (document.getElementById('multiplayer-chat-widget')) {
        return;
    }

    const widget = document.createElement('aside');
    widget.id = 'multiplayer-chat-widget';
    widget.className = 'multiplayer-chat-widget';
    widget.innerHTML = `
<div class="multiplayer-chat-header">Game chat</div>
<div id="multiplayer-chat-messages" class="multiplayer-chat-messages"></div>
<div class="multiplayer-chat-controls">
    <input id="multiplayer-chat-input" maxlength="300" type="text" placeholder="Type a message..." />
    <button id="multiplayer-chat-send" type="button">Send</button>
</div>`;

    document.body.appendChild(widget);
}

function appendMessage(container, payload, currentUserId) {
    if (!container) {
        return;
    }

    const senderId = payload.senderId || payload.SenderId;
    const senderName = payload.senderName || payload.SenderName || senderId;
    const message = payload.message || payload.Message || '';

    const item = document.createElement('div');
    item.className = `multiplayer-chat-message ${senderId === currentUserId ? 'mine' : ''}`;

    const sender = document.createElement('div');
    sender.className = 'sender';
    sender.textContent = senderId === currentUserId ? 'You' : senderName;

    const text = document.createElement('div');
    text.className = 'text';
    text.textContent = message;

    item.appendChild(sender);
    item.appendChild(text);
    container.appendChild(item);
    container.scrollTop = container.scrollHeight;
}

function appendSystemMessage(container, text) {
    if (!container) {
        return;
    }

    const item = document.createElement('div');
    item.className = 'multiplayer-chat-system';
    item.textContent = text;
    container.appendChild(item);
    container.scrollTop = container.scrollHeight;
}
