document.addEventListener('DOMContentLoaded', () => {
    const pendingTbody = document.getElementById('pending-tbody');
    const pendingTable = document.getElementById('pending-table');
    const pendingEmpty = document.getElementById('pending-empty');
    const historyTbody = document.getElementById('history-tbody');
    const historyTable = document.getElementById('history-table');
    const historyEmpty = document.getElementById('history-empty');
    const statusEl = document.getElementById('topup-status');

    const confirmModal = document.getElementById('confirm-modal');
    const confirmCoins = document.getElementById('confirm-coins');
    const confirmUser = document.getElementById('confirm-user');
    const confirmAmount = document.getElementById('confirm-amount');
    const confirmNetwork = document.getElementById('confirm-network');
    const confirmTxId = document.getElementById('confirm-txid');
    const confirmYes = document.getElementById('confirm-yes');
    const confirmNo = document.getElementById('confirm-no');
    const confirmClose = document.getElementById('confirm-close');

    let confirmOrderId = null;

    // ---- Tabs ----
    document.querySelectorAll('.topup-tab').forEach(tab => {
        tab.addEventListener('click', () => {
            document.querySelectorAll('.topup-tab').forEach(t => t.classList.remove('active'));
            tab.classList.add('active');
            const target = tab.dataset.tab;
            document.getElementById('tab-pending').style.display = target === 'pending' ? '' : 'none';
            document.getElementById('tab-history').style.display = target === 'history' ? '' : 'none';

            if (target === 'history') loadHistory();
        });
    });

    // ---- Load pending ----
    loadPending();

    async function loadPending() {
        try {
            const res = await fetch('/api/admin/topup/pending');
            if (!res.ok) return;
            const orders = await res.json();

            if (orders.length === 0) {
                pendingTable.style.display = 'none';
                pendingEmpty.style.display = '';
                return;
            }

            pendingEmpty.style.display = 'none';
            pendingTable.style.display = '';
            pendingTbody.innerHTML = '';

            orders.forEach(o => {
                const tr = document.createElement('tr');
                tr.innerHTML =
                    `<td>${esc(o.userName)}</td>` +
                    `<td>${o.amountUSDT} USDT</td>` +
                    `<td>${o.coinsToCredit}</td>` +
                    `<td>${o.network}</td>` +
                    `<td>${formatDate(o.createdAtUtc)}</td>` +
                    `<td><button class="topup-btn-sm" data-id="${o.orderId}" data-coins="${o.coinsToCredit}" data-user="${esc(o.userName)}" data-amount="${o.amountUSDT}" data-network="${o.network}">Confirm</button></td>`;
                pendingTbody.appendChild(tr);
            });

            pendingTbody.querySelectorAll('.topup-btn-sm').forEach(btn => {
                btn.addEventListener('click', () => openConfirm(btn));
            });
        } catch {
            showStatus('Failed to load pending orders.', false);
        }
    }

    // ---- Load history ----
    async function loadHistory() {
        try {
            const res = await fetch('/api/admin/topup/history');
            if (!res.ok) return;
            const orders = await res.json();

            if (orders.length === 0) {
                historyTable.style.display = 'none';
                historyEmpty.style.display = '';
                return;
            }

            historyEmpty.style.display = 'none';
            historyTable.style.display = '';
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
        } catch {
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
        confirmYes.textContent = 'Processing...';

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
            showStatus('Order confirmed! Coins credited.', true);
            loadPending();
        } catch {
            showStatus('Error confirming order.', false);
        } finally {
            confirmYes.disabled = false;
            confirmYes.textContent = 'Confirm & Credit';
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
        statusEl.textContent = msg;
        statusEl.className = 'topup-status-msg ' + (success ? 'success' : 'error');
        statusEl.style.display = '';
        setTimeout(() => { statusEl.style.display = 'none'; }, 5000);
    }
});
