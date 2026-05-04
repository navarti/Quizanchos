(function () {
    const root = document.getElementById('minigame-root');
    if (!root) return;

    const cfg = window.minigameConfig || {};

    root.innerHTML = `
        <div class="click-counter">
            <h2 class="click-counter__title">${cfg.displayName || 'Click Counter'}</h2>
            <p class="click-counter__note">
                Sample third-party plugin loaded from
                <code>plugins/ClickCounter/</code>.
                The lobby is rendered from plugin assets served via
                <code>${window.location.pathname}</code>.
            </p>
            <p class="click-counter__note">
                Full gameplay wiring (SignalR move submission) is intentionally out of
                scope for the loader smoke test.
            </p>
        </div>
    `;
})();
