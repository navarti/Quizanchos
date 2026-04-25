document.addEventListener('DOMContentLoaded', () => {
    const packagesGrid = document.getElementById('packages-grid');
    const balanceStatus = document.getElementById('balance-status');
    const balanceAmountVal = document.getElementById('balance-amount-value');
    const pendingSection = document.getElementById('pending-section');
    const pendingList = document.getElementById('pending-orders-list');

    const modal = document.getElementById('payment-modal');
    const modalCloseBtn = document.getElementById('modal-close-btn');
    const stepNetwork = document.getElementById('step-network');
    const stepPayment = document.getElementById('step-payment');
    const payAmount = document.getElementById('pay-amount');
    const payAddress = document.getElementById('pay-address');
    const payNetwork = document.getElementById('pay-network');
    const payTimer = document.getElementById('pay-timer');
    const checkStatusBtn = document.getElementById('check-status-btn');
    const backBtn = document.getElementById('back-btn');
    const copyAmountBtn = document.getElementById('copy-amount-btn');
    const copyAddressBtn = document.getElementById('copy-address-btn');

    let currentOrderId = null;
    let timerInterval = null;
    let pollInterval = null;
    let selectedPackageId = null;

    loadPackages();
    loadPendingOrders();

    // ---- Packages ----

    async function loadPackages() {
        try {
            const res = await fetch('/api/topup/packages');
            if (!res.ok) return;
            renderPackages(await res.json());
        } catch {
            packagesGrid.innerHTML = '<p style="color:#fee2e2;text-align:center;">Failed to load packages.</p>';
        }
    }

    function renderPackages(packages) {
        packagesGrid.innerHTML = '';
        packages.forEach(pkg => {
            const card = document.createElement('div');
            card.className = 'balance-pkg-card';
            card.innerHTML =
                `<div class="balance-pkg-coins">${pkg.coins} coins</div>` +
                `<div class="balance-pkg-name">${pkg.name}</div>` +
                `<div class="balance-pkg-price">${pkg.priceUSDT} USDT</div>` +
                `<button class="balance-pkg-buy" data-id="${pkg.id}">Buy</button>`;
            packagesGrid.appendChild(card);
        });

        packagesGrid.querySelectorAll('.balance-pkg-buy').forEach(btn => {
            btn.addEventListener('click', () => {
                selectedPackageId = parseInt(btn.dataset.id);
                openModal();
            });
        });
    }

    // ---- Modal ----

    function openModal() {
        stepNetwork.style.display = '';
        stepPayment.style.display = 'none';
        modal.classList.add('active');
    }

    function closeModal() {
        modal.classList.remove('active');
        stopTimers();
        currentOrderId = null;
    }

    modalCloseBtn.addEventListener('click', closeModal);
    modal.addEventListener('click', e => { if (e.target === modal) closeModal(); });

    backBtn.addEventListener('click', () => {
        stopTimers();
        stepPayment.style.display = 'none';
        stepNetwork.style.display = '';
    });

    document.querySelectorAll('.balance-network-opt').forEach(btn => {
        btn.addEventListener('click', () => createOrder(btn.dataset.network));
    });

    async function createOrder(network) {
        try {
            const res = await fetch('/api/topup/create-order', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ packageId: selectedPackageId, network })
            });

            if (!res.ok) {
                const err = await res.json().catch(() => null);
                showStatus(err?.message || 'Failed to create order.', false);
                closeModal();
                return;
            }

            const data = await res.json();
            currentOrderId = data.orderId;

            payAmount.textContent = data.amountUSDT + ' USDT';
            payAddress.textContent = data.walletAddress;
            payNetwork.textContent = data.network;

            stepNetwork.style.display = 'none';
            stepPayment.style.display = '';

            startTimer(new Date(data.expiresAtUtc));
            startPolling();
            loadPendingOrders();
        } catch {
            showStatus('Unexpected error. Try again.', false);
            closeModal();
        }
    }

    // ---- Timer & polling ----

    function startTimer(expiresAt) {
        clearInterval(timerInterval);
        updateTimerDisplay(expiresAt);
        timerInterval = setInterval(() => updateTimerDisplay(expiresAt), 1000);
    }

    function updateTimerDisplay(expiresAt) {
        const diff = expiresAt - new Date();
        if (diff <= 0) {
            payTimer.textContent = 'Expired';
            stopTimers();
            return;
        }
        const m = Math.floor(diff / 60000);
        const s = Math.floor((diff % 60000) / 1000);
        payTimer.textContent = m + ':' + s.toString().padStart(2, '0');
    }

    function startPolling() {
        clearInterval(pollInterval);
        pollInterval = setInterval(async () => {
            if (!currentOrderId) return;
            try {
                const res = await fetch('/api/topup/order/' + currentOrderId + '/status');
                if (!res.ok) return;
                const data = await res.json();

                if (data.status === 'Completed' || data.status === 'ManuallyConfirmed') {
                    closeModal();
                    refreshBalance();
                    loadPendingOrders();
                    showStatus('Payment received! Coins credited.', true);
                } else if (data.status === 'Expired') {
                    stopTimers();
                    payTimer.textContent = 'Expired';
                    loadPendingOrders();
                }
            } catch { /* ignore */ }
        }, 5000);
    }

    function stopTimers() {
        clearInterval(timerInterval);
        clearInterval(pollInterval);
    }

    // ---- Check status (manual) ----

    checkStatusBtn.addEventListener('click', async () => {
        if (!currentOrderId) return;
        checkStatusBtn.textContent = 'Checking...';
        checkStatusBtn.disabled = true;

        try {
            const res = await fetch('/api/topup/order/' + currentOrderId + '/status');
            if (!res.ok) { showStatus('Failed to check status.', false); return; }
            const data = await res.json();

            if (data.status === 'Completed' || data.status === 'ManuallyConfirmed') {
                closeModal();
                refreshBalance();
                loadPendingOrders();
                showStatus('Payment received! Coins credited.', true);
            } else if (data.status === 'Expired') {
                closeModal();
                loadPendingOrders();
                showStatus('Order expired. Please create a new one.', false);
            } else {
                showStatus('Payment not yet detected. It may take 1–2 minutes.', false);
            }
        } catch {
            showStatus('Error checking status.', false);
        } finally {
            checkStatusBtn.textContent = 'Check Status';
            checkStatusBtn.disabled = false;
        }
    });

    // ---- Copy ----

    copyAmountBtn.addEventListener('click', () => copyText(payAmount.textContent.replace(' USDT', ''), copyAmountBtn));
    copyAddressBtn.addEventListener('click', () => copyText(payAddress.textContent, copyAddressBtn));

    function copyText(text, btn) {
        navigator.clipboard.writeText(text);
        const orig = btn.textContent;
        btn.textContent = 'Copied!';
        setTimeout(() => { btn.textContent = orig; }, 1500);
    }

    // ---- Balance refresh ----

    async function refreshBalance() {
        try {
            const res = await fetch('/UserProfile/GetUserInfo');
            if (!res.ok) return;
            const info = await res.json();
            const coins = info.coins ?? 0;
            balanceAmountVal.textContent = coins;
            const header = document.getElementById('user-balance-value');
            if (header) header.textContent = coins;
        } catch { /* ignore */ }
    }

    // ---- Pending orders ----

    async function loadPendingOrders() {
        try {
            const res = await fetch('/api/topup/pending');
            if (!res.ok) return;
            const orders = await res.json();

            if (orders.length === 0) { pendingSection.style.display = 'none'; return; }

            pendingSection.style.display = '';
            pendingList.innerHTML = '';

            orders.forEach(o => {
                const expired = new Date(o.expiresAtUtc) < new Date();
                const row = document.createElement('div');
                row.className = 'balance-pending-row' + (expired ? ' is-expired' : '');
                row.innerHTML =
                    `<span class="balance-pending-coins">${o.coinsToCredit} coins</span>` +
                    `<span class="balance-pending-amount">${o.amountUSDT} USDT</span>` +
                    `<span class="balance-pending-network">${o.network}</span>` +
                    `<span class="balance-pending-badge">${expired ? 'Expiring...' : 'Waiting...'}</span>`;
                pendingList.appendChild(row);
            });
        } catch { /* ignore */ }
    }

    // ---- Status banner ----

    function showStatus(msg, success) {
        balanceStatus.textContent = msg;
        balanceStatus.className = 'balance-status-banner ' + (success ? 'success' : 'error');
        balanceStatus.style.display = '';
        setTimeout(() => { balanceStatus.style.display = 'none'; }, 5000);
    }
});
