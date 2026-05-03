document.addEventListener('DOMContentLoaded', async () => {
    const premiumGrid = document.getElementById('market-grid-premium');
    const emojiGrid = document.getElementById('market-grid-emoji');
    const status = document.getElementById('market-status');
    const balance = document.getElementById('market-balance');

    if (!premiumGrid || !emojiGrid || !status || !balance) return;

    const state = {
        currentBalance: parseInt(balance.textContent, 10) || 0,
        items: new Map(),
    };

    showSkeleton(premiumGrid, 2);
    showSkeleton(emojiGrid, 4);
    await loadMarket();

    async function loadMarket() {
        try {
            const [premiumCatalog, emojiCatalog] = await Promise.all([
                fetchJson('/api/market/catalog?type=PremiumSubscription'),
                fetchJson('/api/market/catalog?type=Emoji'),
            ]);
            renderCatalog(premiumCatalog, premiumGrid, true);
            renderCatalog(emojiCatalog, emojiGrid, false);
            await refreshBalance();
        } catch (error) {
            premiumGrid.innerHTML = '';
            emojiGrid.innerHTML = '';
            showStatus(error?.message || 'Failed to load market.', true);
        }
    }

    function showSkeleton(grid, count) {
        grid.innerHTML = Array.from({ length: count }).map(() =>
            `<div class="market-item market-item--skeleton" aria-hidden="true">
                <div class="skeleton skeleton--circle"></div>
                <div class="skeleton skeleton--line"></div>
                <div class="skeleton skeleton--line skeleton--short"></div>
                <div class="skeleton skeleton--button"></div>
            </div>`).join('');
    }

    function renderCatalog(items, targetGrid, isPremiumSection) {
        if (!Array.isArray(items) || items.length === 0) {
            targetGrid.innerHTML = `<p class="market-empty">No ${isPremiumSection ? 'premium plans' : 'emoji'} available right now.</p>`;
            return;
        }

        targetGrid.innerHTML = items.map(item => {
            const id = getValue(item, 'id', 'Id');
            const name = getValue(item, 'name', 'Name') || (isPremiumSection ? 'Premium plan' : 'Emoji');
            const imageUrl = getValue(item, 'imageUrl', 'ImageUrl') || '';
            const price = getValue(item, 'priceCoins', 'PriceCoins') ?? 0;
            const isFree = !!getValue(item, 'isFree', 'IsFree');
            const isLocked = !!getValue(item, 'isLocked', 'IsLocked');
            const durationMonths = getValue(item, 'durationMonths', 'DurationMonths');
            const isPremiumPlan = durationMonths !== undefined && durationMonths !== null;

            state.items.set(String(id), { name, price, isFree, isLocked, isPremiumPlan, durationMonths });

            const subtitle = isPremiumPlan
                ? `${durationMonths} month${durationMonths === 1 ? '' : 's'} of premium`
                : (isFree ? 'Free' : `${price} coins`);

            const priceText = isFree ? 'Free' : `${price} coins`;
            const canAfford = state.currentBalance >= price || isFree;

            let actionHtml;
            if (!isLocked) {
                actionHtml = '<button type="button" class="market-item-action" disabled>Owned</button>';
            } else if (!canAfford) {
                actionHtml = `<button type="button" class="market-item-action market-item-action--insufficient" data-item-id="${id}" data-price="${price}">Need ${price - state.currentBalance} more</button>`;
            } else {
                actionHtml = `<button type="button" class="market-item-action" data-item-id="${id}" data-price="${price}">${isPremiumPlan ? 'Purchase' : 'Buy'}</button>`;
            }

            return `<div class="market-item ${isLocked ? 'locked' : 'unlocked'}">
    <img class="market-item-image" src="${escapeHtml(imageUrl)}" alt="" />
    <div class="market-item-name">${escapeHtml(name)}</div>
    <div class="market-item-meta">${escapeHtml(subtitle)}</div>
    <div class="market-item-price">${escapeHtml(priceText)}</div>
    ${actionHtml}
</div>`;
        }).join('');

        targetGrid.querySelectorAll('[data-item-id]').forEach(button => {
            button.addEventListener('click', async () => {
                const itemId = button.getAttribute('data-item-id');
                const item = state.items.get(String(itemId));
                if (!item) return;
                if (state.currentBalance < item.price && !item.isFree) {
                    showToast('Not enough coins. Top up first.', 'warning');
                    return;
                }
                const confirmed = await showConfirm({
                    title: item.isPremiumPlan ? 'Activate premium?' : 'Confirm purchase',
                    message: item.isFree
                        ? `Add "${item.name}" to your account?`
                        : `Spend ${item.price} coins to ${item.isPremiumPlan ? 'activate' : 'buy'} "${item.name}"?`,
                    confirmText: item.isPremiumPlan ? 'Activate' : 'Buy',
                    cancelText: 'Cancel',
                });
                if (!confirmed) return;
                await purchase(itemId, button);
            });
        });
    }

    async function purchase(itemId, button) {
        if (!itemId) return;
        setButtonBusy(button, true, 'Purchasing…');
        try {
            const result = await fetchJson('/api/market/purchase', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ marketItemId: itemId }),
            });

            const remainingCoins = getValue(result, 'remainingCoins', 'RemainingCoins');
            const premiumUntilUtc = getValue(result, 'premiumUntilUtc', 'PremiumUntilUtc');

            if (remainingCoins !== undefined && remainingCoins !== null) {
                state.currentBalance = remainingCoins;
                balance.textContent = `${remainingCoins}`;
                const headerBalance = document.getElementById('user-balance-value');
                if (headerBalance) headerBalance.textContent = `${remainingCoins}`;
                broadcastUserInfo({ coins: remainingCoins, premiumUntilUtc });
            }

            if (premiumUntilUtc) {
                const headerStatus = document.getElementById('user-status-value');
                const statusChip = document.getElementById('user-status-chip');
                if (headerStatus) headerStatus.textContent = 'Premium';
                if (statusChip) statusChip.classList.add('premium');
                showToast(`Premium active until ${new Date(premiumUntilUtc).toLocaleString()}.`, 'success');
            } else {
                showToast('Purchase complete.', 'success');
            }

            await loadMarket();
        } catch (error) {
            showStatus(error?.message || 'Purchase failed.', true);
        } finally {
            setButtonBusy(button, false);
        }
    }

    async function refreshBalance() {
        try {
            const userInfo = await fetchJson('/UserProfile/GetUserInfo');
            const coins = getValue(userInfo, 'coins', 'Coins') ?? 0;
            state.currentBalance = coins;
            balance.textContent = `${coins}`;
            broadcastUserInfo({ coins, premiumUntilUtc: getValue(userInfo, 'premiumUntilUtc', 'PremiumUntilUtc') });
        } catch { /* ignore */ }
    }

    function showStatus(message, isError) {
        if (isError) {
            showToast(message, 'error');
        } else {
            showToast(message, 'success');
        }
    }
});

async function fetchJson(url, options) {
    const response = await fetch(url, options);
    let payload = null;
    try { payload = await response.json(); } catch { /* may be empty */ }
    if (!response.ok) {
        const message = getValue(payload, 'message', 'Message') || 'Request failed';
        throw new Error(message);
    }
    return payload;
}

function getValue(obj, camelName, pascalName) {
    if (!obj) return undefined;
    if (obj[camelName] !== undefined) return obj[camelName];
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
