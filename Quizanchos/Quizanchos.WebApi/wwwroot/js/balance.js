document.addEventListener('DOMContentLoaded', () => {
    const addCoinsButton = document.getElementById('add-coins-button');
    const coinsInput = document.getElementById('coins-to-add');
    const balanceAmount = document.getElementById('balance-amount');

    if (!addCoinsButton || !coinsInput || !balanceAmount) {
        return;
    }

    addCoinsButton.addEventListener('click', async () => {
        const coinsToAdd = Number(coinsInput.value);
        if (!Number.isInteger(coinsToAdd) || coinsToAdd <= 0) {
            showModal('Notification', 'Please enter a valid positive number of coins.');
            return;
        }

        try {
            const response = await fetch('/UserProfile/AddCoins', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                },
                body: new URLSearchParams({ coinsToAdd: `${coinsToAdd}` }),
            });

            if (!response.ok) {
                showModal('Notification', 'Failed to add coins. Please try again.');
                return;
            }

            const userInfoResponse = await fetch('/UserProfile/GetUserInfo');
            if (!userInfoResponse.ok) {
                showModal('Notification', 'Coins were added, but balance refresh failed.');
                return;
            }

            const userInfo = await userInfoResponse.json();
            const coins = userInfo.coins ?? 0;
            balanceAmount.textContent = `${coins} coins`;

            const headerBalance = document.getElementById('user-balance-value');
            if (headerBalance) {
                headerBalance.textContent = `${coins}`;
            }

            showModal('Success', 'Coins were added successfully!', true);
        } catch {
            showModal('Notification', 'Unexpected error. Please try again later.');
        }
    });
});
