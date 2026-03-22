document.addEventListener('DOMContentLoaded', async () => {
    const grid = document.getElementById('market-grid');
    const status = document.getElementById('market-status');
    const balance = document.getElementById('market-balance');

    if (!grid || !status || !balance) {
        return;
    }

    await loadMarket();

    async function loadMarket() {
        try {
            const catalog = await fetchJson('/api/market/catalog?type=emoji');
            renderCatalog(catalog);
            await refreshBalance();
        } catch (error) {
            showStatus(error?.message || 'Failed to load market.', true);
        }
    }

    function renderCatalog(items) {
        if (!Array.isArray(items) || items.length === 0) {
            grid.innerHTML = '<p>No items available.</p>';
            return;
        }

        grid.innerHTML = items.map(item => {
            const id = getValue(item, 'id', 'Id');
            const name = getValue(item, 'name', 'Name') || 'Emoji';
            const imageUrl = getValue(item, 'imageUrl', 'ImageUrl') || '';
            const price = getValue(item, 'priceCoins', 'PriceCoins') ?? 0;
            const isFree = !!getValue(item, 'isFree', 'IsFree');
            const isLocked = !!getValue(item, 'isLocked', 'IsLocked');

            return `<div class="market-item ${isLocked ? 'locked' : 'unlocked'}">
    <img class="market-item-image" src="${escapeHtml(imageUrl)}" alt="${escapeHtml(name)}" />
    <div class="market-item-name">${escapeHtml(name)}</div>
    <div class="market-item-meta">${isFree ? 'Free' : `${price} coins`}</div>
    <div class="market-item-meta">${isLocked ? 'Locked' : 'Unlocked'}</div>
    ${isLocked
        ? `<button type="button" class="market-item-action" data-item-id="${id}">Buy</button>`
        : '<button type="button" class="market-item-action" disabled>Owned</button>'}
</div>`;
        }).join('');

        grid.querySelectorAll('[data-item-id]').forEach(button => {
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
            if (remainingCoins !== undefined) {
                balance.textContent = `${remainingCoins}`;
                const headerBalance = document.getElementById('user-balance-value');
                if (headerBalance) {
                    headerBalance.textContent = `${remainingCoins}`;
                }
            }

            showStatus('Purchase successful.', false);
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
