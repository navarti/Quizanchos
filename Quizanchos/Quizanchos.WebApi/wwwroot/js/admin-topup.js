// =============================================================================
// Admin top-up orders page — pending and history tabs.
// =============================================================================

document.addEventListener('DOMContentLoaded', () => {
    const pendingTbody   = document.getElementById('pending-tbody');
    const pendingTable   = document.getElementById('pending-table');
    const pendingEmpty   = document.getElementById('pending-empty');
    const pendingLoading = document.getElementById('pending-loading');
    const pendingError   = document.getElementById('pending-error');

    const historyTbody   = document.getElementById('history-tbody');
    const historyTable   = document.getElementById('history-table');
    const historyEmpty   = document.getElementById('history-empty');
    const historyLoading = document.getElementById('history-loading');
    const historyError   = document.getElementById('history-error');

    const statusEl = document.getElementById('topup-status');

    const confirmModal   = document.getElementById('confirm-modal');
    const confirmCoins   = document.getElementById('confirm-coins');
    const confirmUser    = document.getElementById('confirm-user');
    const confirmAmount  = document.getElementById('confirm-amount');
    const confirmNetwork = document.getElementById('confirm-network');
    const confirmTxId    = document.getElementById('confirm-txid');
    const confirmYes     = document.getElementById('confirm-yes');
    const confirmNo      = document.getElementById('confirm-no');
    const confirmClose   = document.getElementById('confirm-close');

    let confirmOrderId = null;

    // ---- Tabs ----
    document.querySelectorAll('.topup-tab').forEach(tab => {
        tab.addEventListener('click', () => {
            document.querySelectorAll('.topup-tab').forEach(t => {
                t.classList.remove('active');
                t.setAttribute('aria-selected', 'false');
            });
            tab.classList.add('active');
            tab.setAttribute('aria-selected', 'true');
            const target = tab.dataset.tab;
            document.getElementById('tab-pending').style.display = target === 'pending' ? '' : 'none';
            document.getElementById('tab-history').style.display = target === 'history' ? '' : 'none';

            if (target === 'history') loadHistory();
        });
    });

    // ---- View-state helpers ----
    function setPendingState(state, message) {
        if (pendingLoading) pendingLoading.hidden = state !== 'loading';
        if (pendingTable)   pendingTable.style.display = state === 'ready' ? '' : 'none';
        if (pendingEmpty)   pendingEmpty.style.display = state === 'empty' ? '' : 'none';
        if (pendingError)   {
            pendingError.hidden = state !== 'error';
            if (state === 'error') pendingError.textContent = message || 'Could not load pending orders.';
        }
    }
    function setHistoryState(state, message) {
        if (historyLoading) historyLoading.hidden = state !== 'loading';
        if (historyTable)   historyTable.style.display = state === 'ready' ? '' : 'none';
        if (historyEmpty)   historyEmpty.style.display = state === 'empty' ? '' : 'none';
        if (historyError)   {
            historyError.hidden = state !== 'error';
            if (state === 'error') historyError.textContent = message || 'Could not load history.';
        }
    }

    // ---- Load pending ----
    loadPending();

    async function loadPending() {
        setPendingState('loading');
        try {
            const res = await fetch('/api/admin/topup/pending');
            if (!res.ok) {
                setPendingState('error', 'Server returned ' + res.status);
                return;
            }
            const orders = await res.json();

            if (!orders || orders.length === 0) {
                setPendingState('empty');
                return;
            }

            pendingTbody.innerHTML = '';
            orders.forEach(o => {
                const tr = document.createElement('tr');
                tr.innerHTML =
                    `<td>${esc(o.userName)}</td>` +
                    `<td>${o.amountUSDT} USDT</td>` +
                    `<td>${o.coinsToCredit}</td>` +
                    `<td>${o.network}</td>` +
                    `<td>${formatDate(o.createdAtUtc)}</td>` +
                    `<td><button type="button" class="topup-btn-sm" data-id="${o.orderId}" data-coins="${o.coinsToCredit}" data-user="${esc(o.userName)}" data-amount="${o.amountUSDT}" data-network="${o.network}">Confirm</button></td>`;
                pendingTbody.appendChild(tr);
            });

            pendingTbody.querySelectorAll('.topup-btn-sm').forEach(btn => {
                btn.addEventListener('click', () => openConfirm(btn));
            });
            setPendingState('ready');
        } catch {
            setPendingState('error', 'Failed to load pending orders.');
            showStatus('Failed to load pending orders.', false);
        }
    }

    // ---- Load history ----
    async function loadHistory() {
        setHistoryState('loading');
        try {
            const res = await fetch('/api/admin/topup/history');
            if (!res.ok) {
                setHistoryState('error', 'Server returned ' + res.status);
                return;
            }
            const orders = await res.json();

            if (!orders || orders.length === 0) {
                setHistoryState('empty');
                return;
            }

            historyTbody.innerHTML = '';
            orders.forEach(o => {
                const tr = document.createElement('tr');
                tr.innerHTML =
                    `<td>${esc(o.userName)}</td>` +
                    `<td>${o.amountUSDT} USDT</td>` +
                    `<td>${o.coinsToCredit}</td>` +
                    `<td>${o.network}</td>` +
                    `<td>${badgeHtml(o.status)}</td>` +
                    `<td>${o.completedAtUtc ? formatDate(o.completedAtUtc) : '—'}</td>`;
                historyTbody.appendChild(tr);
            });
            setHistoryState('ready');
        } catch {
            setHistoryState('error', 'Failed to load history.');
            showStatus('Failed to load history.', false);
        }
    }

    // ---- Confirm modal ----
    function openConfirm(btn) {
        confirmOrderId = btn.dataset.id;
        confirmCoins.textContent = btn.dataset.coins;
        confirmUser.textContent = btn.dataset.user;
        confirmAmount.textContent = btn.dataset.amount;
        confirmNetwork.textContent = btn.dataset.network;
        confirmTxId.value = '';
        confirmModal.style.display = 'block';
    }

    function closeConfirm() {
        confirmModal.style.display = 'none';
        confirmOrderId = null;
    }

    confirmClose.addEventListener('click', closeConfirm);
    confirmNo.addEventListener('click', closeConfirm);

    confirmYes.addEventListener('click', async () => {
        if (!confirmOrderId) return;
        confirmYes.disabled = true;
        const originalLabel = confirmYes.textContent;
        confirmYes.textContent = 'Processing…';

        try {
            const txId = confirmTxId.value.trim();
            const url = `/api/admin/topup/${confirmOrderId}/confirm` + (txId ? `?txId=${encodeURIComponent(txId)}` : '');
            const res = await fetch(url, { method: 'POST' });

            if (!res.ok) {
                const err = await res.json().catch(() => null);
                showStatus(err?.message || 'Failed to confirm order.', false);
                return;
            }

            closeConfirm();
            showStatus('Order confirmed. Coins credited.', true);
            loadPending();
        } catch {
            showStatus('Error confirming order.', false);
        } finally {
            confirmYes.disabled = false;
            confirmYes.textContent = originalLabel;
        }
    });

    // ---- Helpers ----
    function badgeHtml(status) {
        const cls = status === 'Completed' ? 'completed'
            : status === 'ManuallyConfirmed' ? 'manual'
            : 'expired';
        const label = status === 'ManuallyConfirmed' ? 'Manual' : status;
        return `<span class="topup-badge ${cls}">${label}</span>`;
    }

    function formatDate(iso) {
        if (!iso) return '—';
        const d = new Date(iso);
        return d.toLocaleDateString() + ' ' + d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    }

    function esc(str) {
        const d = document.createElement('div');
        d.textContent = str || '';
        return d.innerHTML;
    }

    function showStatus(msg, success) {
        if (!statusEl) return;
        statusEl.textContent = msg;
        statusEl.className = 'topup-status-msg ' + (success ? 'success' : 'error');
        statusEl.style.display = '';
        setTimeout(() => { statusEl.style.display = 'none'; }, 5000);
    }
});
