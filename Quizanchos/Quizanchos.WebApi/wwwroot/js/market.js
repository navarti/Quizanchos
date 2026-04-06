document.addEventListener('DOMContentLoaded', async () => {
    const premiumGrid = document.getElementById('market-grid-premium');
    const emojiGrid = document.getElementById('market-grid-emoji');
    const status = document.getElementById('market-status');
    const balance = document.getElementById('market-balance');

    if (!premiumGrid || !emojiGrid || !status || !balance) {
        return;
    }

    await loadMarket();

    async function loadMarket() {
        try {
            const [premiumCatalog, emojiCatalog] = await Promise.all([
                fetchJson('/api/market/catalog?type=PremiumSubscription'),
                fetchJson('/api/market/catalog?type=Emoji')
            ]);

            renderCatalog(premiumCatalog, premiumGrid);
            renderCatalog(emojiCatalog, emojiGrid);
            await refreshBalance();
        } catch (error) {
            showStatus(error?.message || 'Failed to load market.', true);
        }
    }

    function renderCatalog(items, targetGrid) {
        if (!Array.isArray(items) || items.length === 0) {
            targetGrid.innerHTML = '<p>No items available.</p>';
            return;
        }

        targetGrid.innerHTML = items.map(item => {
            const id = getValue(item, 'id', 'Id');
            const name = getValue(item, 'name', 'Name') || 'Emoji';
            const imageUrl = getValue(item, 'imageUrl', 'ImageUrl') || '';
            const price = getValue(item, 'priceCoins', 'PriceCoins') ?? 0;
            const isFree = !!getValue(item, 'isFree', 'IsFree');
            const isLocked = !!getValue(item, 'isLocked', 'IsLocked');
            const durationMonths = getValue(item, 'durationMonths', 'DurationMonths');
            const isPremiumPlan = durationMonths !== undefined && durationMonths !== null;

            const subtitle = isPremiumPlan
                ? `${durationMonths} month${durationMonths === 1 ? '' : 's'} premium`
                : (isFree ? 'Free' : `${price} coins`);

            const actionText = isPremiumPlan ? 'Purchase' : 'Buy';

            return `<div class="market-item ${isLocked ? 'locked' : 'unlocked'}">
    <img class="market-item-image" src="${escapeHtml(imageUrl)}" alt="${escapeHtml(name)}" />
    <div class="market-item-name">${escapeHtml(name)}</div>
    <div class="market-item-meta">${escapeHtml(subtitle)}</div>
    <div class="market-item-meta">${isPremiumPlan ? `${price} coins` : (isFree ? 'Free' : `${price} coins`)}</div>
    <div class="market-item-meta">${isLocked ? 'Locked' : 'Unlocked'}</div>
    ${isLocked
        ? `<button type="button" class="market-item-action" data-item-id="${id}">${actionText}</button>`
        : '<button type="button" class="market-item-action" disabled>Owned</button>'}
</div>`;
        }).join('');

        targetGrid.querySelectorAll('[data-item-id]').forEach(button => {
            button.addEventListener('click', async () => {
                const itemId = button.getAttribute('data-item-id');
                await purchase(itemId);
            });
        });
    }

    async function purchase(itemId) {
        if (!itemId) {
            return;
        }

        try {
            const result = await fetchJson('/api/market/purchase', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ marketItemId: itemId })
            });

            const remainingCoins = getValue(result, 'remainingCoins', 'RemainingCoins');
            const premiumUntilUtc = getValue(result, 'premiumUntilUtc', 'PremiumUntilUtc');
            if (remainingCoins !== undefined) {
                balance.textContent = `${remainingCoins}`;
                const headerBalance = document.getElementById('user-balance-value');
                if (headerBalance) {
                    headerBalance.textContent = `${remainingCoins}`;
                }
            }

            const successMessage = premiumUntilUtc
                ? `Purchase successful. Premium active until ${new Date(premiumUntilUtc).toLocaleString()}.`
                : 'Purchase successful.';

            showStatus(successMessage, false);
            await loadMarket();
        } catch (error) {
            showStatus(error?.message || 'Purchase failed.', true);
        }
    }

    async function refreshBalance() {
        try {
            const userInfo = await fetchJson('/UserProfile/GetUserInfo');
            const coins = getValue(userInfo, 'coins', 'Coins') ?? 0;
            balance.textContent = `${coins}`;
        } catch {
        }
    }

    function showStatus(message, isError) {
        status.className = `market-status ${isError ? 'error' : 'success'}`;
        status.style.display = 'block';
        status.textContent = message;
    }
});

async function fetchJson(url, options) {
    const response = await fetch(url, options);

    let payload = null;
    try {
        payload = await response.json();
    } catch {
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
