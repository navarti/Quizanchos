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
    const emojiWheel = document.getElementById('multiplayer-emoji-wheel');
    const emojiShop = document.getElementById('multiplayer-emoji-shop');
    const emojiToggle = document.getElementById('multiplayer-emoji-toggle');
    const shopToggle = document.getElementById('multiplayer-shop-toggle');

    const emojiState = {
        catalog: [],
        ownedIds: new Set(),
        lastSentAt: 0,
        sendCooldownMs: 1000
    };

    const connection = new signalR.HubConnectionBuilder()
        .withUrl('/hubs/game')
        .withAutomaticReconnect()
        .build();

    connection.on('ChatMessageReceived', payload => {
        appendMessage(messagesContainer, payload, userId);
    });

    connection.on('EmojiReceived', payload => {
        showEmojiOverlay(payload, userId);
    });

    connection.onreconnecting(() => {
        appendSystemMessage(messagesContainer, 'Reconnecting...');
    });

    connection.onreconnected(async () => {
        appendSystemMessage(messagesContainer, 'Connected');
        await safeJoin();
    });

    sendButton?.addEventListener('click', sendMessage);

    emojiToggle?.addEventListener('click', () => {
        if (!emojiWheel || !emojiShop) {
            return;
        }

        emojiWheel.classList.toggle('open');
        emojiShop.classList.remove('open');
    });

    shopToggle?.addEventListener('click', () => {
        if (!emojiWheel || !emojiShop) {
            return;
        }

        emojiShop.classList.toggle('open');
        emojiWheel.classList.remove('open');
    });

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
        await loadEmojiUi();
    } catch (_) {
        appendSystemMessage(messagesContainer, 'Chat unavailable');
    }

    async function loadEmojiUi() {
        try {
            const [catalogRaw, ownershipRaw] = await Promise.all([
                fetchJson('/api/market/catalog?type=emoji'),
                fetchJson('/api/market/ownership?type=emoji')
            ]);

            const ownershipIds = new Set((ownershipRaw || []).map(x => getValue(x, 'itemId', 'ItemId')));
            const catalog = (catalogRaw || []).map(item => {
                const id = getValue(item, 'id', 'Id');
                const isFree = !!getValue(item, 'isFree', 'IsFree');
                const isOwnedFlag = !!getValue(item, 'isOwned', 'IsOwned');
                const isOwned = isFree || isOwnedFlag || ownershipIds.has(id);
                return {
                    id,
                    type: getValue(item, 'type', 'Type'),
                    name: getValue(item, 'name', 'Name') || 'Emoji',
                    imageUrl: getValue(item, 'imageUrl', 'ImageUrl') || '',
                    priceCoins: getValue(item, 'priceCoins', 'PriceCoins') ?? 0,
                    isFree,
                    isOwned,
                    isLocked: !isFree && !isOwned
                };
            });

            emojiState.catalog = catalog;
            emojiState.ownedIds = new Set(catalog.filter(x => !x.isLocked).map(x => x.id));

            renderEmojiWheel(emojiWheel, emojiState.catalog, sendEmoji);
            renderEmojiShop(emojiShop, emojiState.catalog, purchaseEmoji);
        } catch (error) {
            appendSystemMessage(messagesContainer, error?.message || 'Failed to load emoji catalog');
        }
    }

    async function purchaseEmoji(itemId) {
        try {
            const payload = await fetchJson('/api/market/purchase', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ marketItemId: itemId })
            });

            const remainingCoins = getValue(payload, 'remainingCoins', 'RemainingCoins');
            updateUserCoins(remainingCoins);
            await loadEmojiUi();
            appendSystemMessage(messagesContainer, 'Emoji purchased');
        } catch (error) {
            appendSystemMessage(messagesContainer, error?.message || 'Purchase failed');
        }
    }

    async function sendEmoji(itemId) {
        if (!itemId) {
            return;
        }

        const now = Date.now();
        if (now - emojiState.lastSentAt < emojiState.sendCooldownMs) {
            appendSystemMessage(messagesContainer, 'Please wait before sending another emoji');
            return;
        }

        try {
            await connection.invoke('SendEmoji', gameId, itemId);
            emojiState.lastSentAt = now;
        } catch (error) {
            appendSystemMessage(messagesContainer, error?.message || 'Failed to send emoji');
        }
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

function renderEmojiWheel(container, catalog, onSend) {
    if (!container) {
        return;
    }

    const usable = (catalog || []).filter(x => !x.isLocked);
    if (!usable.length) {
        container.innerHTML = '<div class="emoji-empty-state">No emojis available</div>';
        return;
    }

    container.innerHTML = usable.map(item => `
<button class="emoji-wheel-item" type="button" data-item-id="${item.id}" title="${escapeHtml(item.name)}">
    <img src="${escapeHtml(item.imageUrl)}" alt="${escapeHtml(item.name)}" />
</button>`).join('');

    container.querySelectorAll('.emoji-wheel-item').forEach(button => {
        button.addEventListener('click', () => {
            const itemId = button.getAttribute('data-item-id');
            onSend(itemId);
        });
    });
}

function renderEmojiShop(container, catalog, onPurchase) {
    if (!container) {
        return;
    }

    if (!catalog || !catalog.length) {
        container.innerHTML = '<div class="emoji-empty-state">Catalog is empty</div>';
        return;
    }

    container.innerHTML = catalog.map(item => {
        const stateText = item.isLocked ? 'Locked' : 'Unlocked';
        const isPurchasable = item.isLocked && !item.isFree;
        const priceText = item.isFree ? 'Free' : `${item.priceCoins} coins`;

        return `
<div class="emoji-shop-item ${item.isLocked ? 'locked' : 'unlocked'}">
    <img src="${escapeHtml(item.imageUrl)}" alt="${escapeHtml(item.name)}" class="emoji-shop-image" />
    <div class="emoji-shop-meta">
        <div class="emoji-shop-name">${escapeHtml(item.name)}</div>
        <div class="emoji-shop-price">${priceText}</div>
        <div class="emoji-shop-state">${stateText}</div>
    </div>
    ${isPurchasable
            ? `<button class="emoji-buy-button" type="button" data-item-id="${item.id}">Buy</button>`
            : '<button class="emoji-buy-button" type="button" disabled>Owned</button>'}
</div>`;
    }).join('');

    container.querySelectorAll('.emoji-buy-button[data-item-id]').forEach(button => {
        button.addEventListener('click', () => {
            const itemId = button.getAttribute('data-item-id');
            onPurchase(itemId);
        });
    });
}

function showEmojiOverlay(payload, currentUserId) {
    const queueState = getEmojiOverlayQueueState();
    if (queueState.queue.length >= queueState.maxQueue) {
        queueState.queue.shift();
    }

    queueState.queue.push({ payload, currentUserId });
    processEmojiOverlayQueue();
}

function processEmojiOverlayQueue() {
    const queueState = getEmojiOverlayQueueState();
    if (queueState.active >= queueState.maxConcurrent || queueState.queue.length === 0) {
        return;
    }

    const next = queueState.queue.shift();
    if (!next) {
        return;
    }

    renderEmojiOverlay(next.payload, next.currentUserId);
}

function renderEmojiOverlay(payload, currentUserId) {
    const queueState = getEmojiOverlayQueueState();
    const layer = getOrCreateEmojiOverlayLayer();
    const senderId = getValue(payload, 'senderId', 'SenderId');
    const senderName = getValue(payload, 'senderName', 'SenderName') || senderId || 'Player';
    const imageUrl = getValue(payload, 'imageUrl', 'ImageUrl');

    if (!imageUrl) {
        return;
    }

    const baseShift = senderId === currentUserId ? 95 : -95;
    const jitter = Math.floor(Math.random() * 70) - 35;
    const shift = baseShift + jitter;

    const node = document.createElement('div');
    node.className = `emoji-overlay-item ${senderId === currentUserId ? 'mine' : 'from-other'}`;
    node.style.setProperty('--emoji-shift', `${shift}px`);
    node.innerHTML = `
<div class="emoji-overlay-burst"></div>
<img src="${escapeHtml(imageUrl)}" alt="emoji" class="emoji-overlay-image" />
<div class="emoji-overlay-sender">${senderId === currentUserId ? 'You' : escapeHtml(senderName)}</div>`;

    layer.appendChild(node);
    queueState.active += 1;

    setTimeout(() => {
        node.remove();
        queueState.active = Math.max(0, queueState.active - 1);
        processEmojiOverlayQueue();
    }, queueState.animationMs);

    setTimeout(() => {
        processEmojiOverlayQueue();
    }, queueState.gapMs);
}

function getEmojiOverlayQueueState() {
    if (!window.__emojiOverlayQueueState) {
        window.__emojiOverlayQueueState = {
            queue: [],
            active: 0,
            maxConcurrent: 2,
            maxQueue: 24,
            animationMs: 2200,
            gapMs: 180
        };
    }

    return window.__emojiOverlayQueueState;
}

function getOrCreateEmojiOverlayLayer() {
    let layer = document.getElementById('emoji-overlay-layer');
    if (!layer) {
        layer = document.createElement('div');
        layer.id = 'emoji-overlay-layer';
        layer.className = 'emoji-overlay-layer';
        document.body.appendChild(layer);
    }

    return layer;
}

function updateUserCoins(coins) {
    if (coins === null || coins === undefined) {
        return;
    }

    const balanceValue = document.getElementById('user-balance-value');
    if (balanceValue) {
        balanceValue.textContent = String(coins);
    }
}

async function fetchJson(url, options) {
    const response = await fetch(url, options);

    let payload = null;
    try {
        payload = await response.json();
    } catch (_) {
    }

    if (!response.ok) {
        const message = getValue(payload, 'message', 'Message') || 'Request failed';
        throw new Error(message);
    }

    return payload;
}

function getValue(obj, camelName, pascalName) {
    if (!obj) {
        return undefined;
    }

    if (obj[camelName] !== undefined) {
        return obj[camelName];
    }

    return obj[pascalName];
}

function escapeHtml(value) {
    return String(value)
        .replaceAll('&', '&amp;')
        .replaceAll('<', '&lt;')
        .replaceAll('>', '&gt;')
        .replaceAll('"', '&quot;')
        .replaceAll("'", '&#39;');
}

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
<div class="multiplayer-chat-header">
    <span>Game chat</span>
    <div class="multiplayer-chat-header-actions">
        <button id="multiplayer-emoji-toggle" type="button" class="multiplayer-header-button" title="Open emoji wheel">😊</button>
        <button id="multiplayer-shop-toggle" type="button" class="multiplayer-header-button" title="Open emoji shop">Shop</button>
    </div>
</div>
<div id="multiplayer-chat-messages" class="multiplayer-chat-messages"></div>
<div id="multiplayer-emoji-wheel" class="multiplayer-emoji-wheel"></div>
<div id="multiplayer-emoji-shop" class="multiplayer-emoji-shop"></div>
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
