document.addEventListener('DOMContentLoaded', () => {
    const filterBtns = document.querySelectorAll('.filter-btn');
    const leaderboardContainer = document.querySelector('.leaderboard');
    const currentUserName = leaderboardContainer?.getAttribute('data-current-user') || '';

    function renderRows(users) {
        if (!leaderboardContainer) return;
        if (!users || users.length === 0) {
            leaderboardContainer.innerHTML = `
                <div class="leaderboard-empty" role="status">
                    <p>No players to show yet for this minigame.</p>
                    <p>Be the first — <a href="/Minigames">play a game</a> to claim the top spot.</p>
                </div>`;
            return;
        }
        leaderboardContainer.innerHTML = users.map(u => {
            const position = u.position ?? u.Position;
            const userName = u.userName ?? u.UserName;
            const avatarUrl = u.avatarUrl ?? u.AvatarUrl ?? '';
            const score = u.score ?? u.Score;
            const numericPosition = Number(position);
            const isCurrent = userName === currentUserName;
            const trophyHtml = numericPosition === 1 ? '<span class="trophy" aria-hidden="true">🏆</span>' : '';
            return `
                <div class="leaderboard-item${isCurrent ? ' current-user' : ''}" aria-label="Rank ${escapeHtml(String(position))}, ${escapeHtml(userName)}, score ${escapeHtml(String(score))}">
                    <div class="rank">${escapeHtml(String(position))}</div>
                    <div class="player-info">
                        <img src="${escapeHtml(avatarUrl)}" alt="" class="player-avatar" data-fallback="1">
                        <span class="player-name">${escapeHtml(userName)}${isCurrent ? ' <span class="leaderboard-you">(you)</span>' : ''}</span>
                    </div>
                    <div class="score">
                        ${trophyHtml}
                        <span>${escapeHtml(String(score))}</span>
                    </div>
                </div>`;
        }).join('');

        leaderboardContainer.querySelectorAll('img[data-fallback="1"]').forEach(applyDefaultAvatarFallback);
    }

    function showSkeleton(count = 6) {
        if (!leaderboardContainer) return;
        leaderboardContainer.innerHTML = Array.from({ length: count }).map(() =>
            `<div class="leaderboard-item leaderboard-item--skeleton" aria-hidden="true">
                <div class="skeleton skeleton--rank"></div>
                <div class="player-info">
                    <div class="skeleton skeleton--avatar"></div>
                    <div class="skeleton skeleton--name"></div>
                </div>
                <div class="skeleton skeleton--score"></div>
            </div>`).join('');
    }

    async function loadLeaderboard(minigameType) {
        showSkeleton();
        const take = 100;
        const skip = 0;
        const url = `/LeaderBoard/GetLeaderBoard?take=${take}&skip=${skip}${minigameType ? `&minigameType=${encodeURIComponent(minigameType)}` : ''}`;
        try {
            const resp = await fetch(url);
            if (!resp.ok) {
                renderRows([]);
                return;
            }
            const users = await resp.json();
            renderRows(users);
        } catch {
            renderRows([]);
        }
    }

    filterBtns.forEach(btn => {
        btn.addEventListener('click', () => {
            filterBtns.forEach(b => {
                b.classList.remove('active');
                b.setAttribute('aria-pressed', 'false');
            });
            btn.classList.add('active');
            btn.setAttribute('aria-pressed', 'true');
            loadLeaderboard(btn.dataset.minigame);
        });
    });

    // Prize modal
    const prizeBtn = document.querySelector('.prize-btn');
    const modal = document.getElementById('prizeModal');
    const closeModal = document.querySelector('.close-modal');

    if (prizeBtn && modal) {
        prizeBtn.addEventListener('click', () => {
            modal.classList.add('active');
            document.body.style.overflow = 'hidden';
            modal.setAttribute('aria-hidden', 'false');
        });
        const close = () => {
            modal.classList.remove('active');
            document.body.style.overflow = '';
            modal.setAttribute('aria-hidden', 'true');
        };
        closeModal?.addEventListener('click', close);
        modal.addEventListener('click', (e) => { if (e.target === modal) close(); });
        document.addEventListener('keydown', (e) => { if (e.key === 'Escape' && modal.classList.contains('active')) close(); });
    }

    // Apply fallback to server-rendered rows on first paint
    document.querySelectorAll('.leaderboard-item img.player-avatar').forEach(applyDefaultAvatarFallback);

    document.querySelectorAll('.page-btn').forEach(button => {
        button.addEventListener('click', async () => {
            const page = button.dataset.page;
            try {
                const response = await fetch(`/Home/GetPage?page=${encodeURIComponent(page)}`);
                if (!response.ok) return;
                const data = await response.json();
                document.querySelectorAll('.page-btn').forEach(btn => btn.classList.remove('active'));
                button.classList.add('active');
                renderRows(data.Users || data.users || []);
            } catch { /* ignore */ }
        });
    });
});

function escapeHtml(value) {
    return String(value)
        .replaceAll('&', '&amp;')
        .replaceAll('<', '&lt;')
        .replaceAll('>', '&gt;')
        .replaceAll('"', '&quot;')
        .replaceAll("'", '&#39;');
}
