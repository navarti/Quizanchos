(() => {
    const list = document.getElementById('minigame-list');
    if (!list) return;

    const searchInput = document.getElementById('minigame-filter-search');
    const emptyState = document.getElementById('minigame-filter-empty');
    const resetBtn = document.getElementById('minigame-filter-reset');
    const cards = Array.from(list.querySelectorAll('[data-minigame-card]'));

    const state = { query: '', mode: 'all', tier: 'all' };

    const setActiveChip = (group, target) => {
        group.querySelectorAll('.minigame-chip').forEach(chip => {
            const active = chip === target;
            chip.classList.toggle('minigame-chip--active', active);
            chip.setAttribute('aria-checked', active ? 'true' : 'false');
        });
    };

    const apply = () => {
        const q = state.query.trim().toLowerCase();
        let visible = 0;
        cards.forEach(card => {
            const name = card.dataset.name || '';
            const mode = card.dataset.mode || '';
            const tier = card.dataset.tier || '';
            const matchesQuery = !q || name.includes(q);
            const matchesMode = state.mode === 'all' || state.mode === mode;
            const matchesTier = state.tier === 'all' || state.tier === tier;
            const show = matchesQuery && matchesMode && matchesTier;
            card.hidden = !show;
            if (show) visible++;
        });
        if (emptyState) emptyState.hidden = visible !== 0;
    };

    searchInput?.addEventListener('input', e => {
        state.query = e.target.value;
        apply();
    });

    document.querySelectorAll('[data-filter-mode]').forEach(btn => {
        btn.addEventListener('click', () => {
            state.mode = btn.dataset.filterMode;
            setActiveChip(btn.parentElement, btn);
            apply();
        });
    });

    document.querySelectorAll('[data-filter-tier]').forEach(btn => {
        btn.addEventListener('click', () => {
            state.tier = btn.dataset.filterTier;
            setActiveChip(btn.parentElement, btn);
            apply();
        });
    });

    resetBtn?.addEventListener('click', () => {
        state.query = '';
        state.mode = 'all';
        state.tier = 'all';
        if (searchInput) searchInput.value = '';
        document.querySelectorAll('[data-filter-mode], [data-filter-tier]').forEach(btn => {
            const isAllChip = btn.dataset.filterMode === 'all' || btn.dataset.filterTier === 'all';
            btn.classList.toggle('minigame-chip--active', isAllChip);
            btn.setAttribute('aria-checked', isAllChip ? 'true' : 'false');
        });
        apply();
    });
})();
