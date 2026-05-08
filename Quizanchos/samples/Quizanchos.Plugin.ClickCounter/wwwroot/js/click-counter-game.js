(function () {
    const root = document.getElementById('minigame-root');
    if (!root) return;

    const cfg = window.minigameConfig || {};
    let count = 0;

    function render() {
        root.innerHTML = `
            <div class="click-counter">
                <h2 class="click-counter__title">${cfg.displayName || 'Click Counter'}</h2>
                <div class="click-counter__count" data-count>${count}</div>
                <button class="click-counter__btn" type="button" data-click>Click me</button>
                <p class="click-counter__note">
                    Local-only counter. Real gameplay would post moves to the
                    <code>/hubs/game</code> SignalR hub.
                </p>
            </div>
        `;
        root.querySelector('[data-click]').addEventListener('click', () => {
            count++;
            root.querySelector('[data-count]').textContent = count;
        });
    }

    render();
})();
