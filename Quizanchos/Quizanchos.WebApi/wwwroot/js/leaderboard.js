document.addEventListener('DOMContentLoaded', () => {
    const filterBtns = document.querySelectorAll('.filter-btn');
    const leaderboardItems = document.querySelectorAll('.leaderboard-item');
    const currentUserItem = document.querySelector('.leaderboard-item.current-user');
    const leaderboardContainer = document.querySelector('.leaderboard');

    filterBtns.forEach(btn => {
        btn.addEventListener('click', () => {
            // Remove active class from all buttons
            filterBtns.forEach(b => b.classList.remove('active'));
            // Add active class to clicked button
            btn.classList.add('active');

            const minigame = btn.dataset.minigame;

            // fetch leaderboard for selected minigame
            (async () => {
                const take = 100; // adjust as needed
                const skip = 0;
                const url = `/LeaderBoard/GetLeaderBoard?take=${take}&skip=${skip}${minigame ? `&minigameType=${minigame}` : ''}`;
                const resp = await fetch(url);
                if (!resp.ok) {
                    return;
                }
                const users = await resp.json();

                // Update leaderboard DOM
                const leaderboard = leaderboardContainer;
                leaderboard.innerHTML = '';
                users.forEach(u => {
                    const position = u.position ?? u.Position;
                    const numericPosition = Number(position);
                    const userName = u.userName ?? u.UserName;
                    const avatarUrl = u.avatarUrl ?? u.AvatarUrl ?? 'data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///ywAAAAAAQABAAACAUwAOw==';
                    const score = u.score ?? u.Score;
                    const div = document.createElement('div');
                    div.className = 'leaderboard-item' + (userName === leaderboard.getAttribute('data-current-user') ? ' current-user' : '');
                    div.innerHTML = `
                        <div class="rank">${position}</div>
                        <div class="player-info">
                            <img src="${avatarUrl}" alt="${userName}" class="player-avatar">
                            <span class="player-name">${userName}</span>
                        </div>
                        <div class="score">
                            ${numericPosition === 1 ? '<span class="trophy">&#x1F3C6;</span>' : ''}
                            ${score}
                        </div>`;
                    leaderboard.appendChild(div);
                });
            })();
        });
    });

    // Prize modal functionality
    const prizeBtn = document.querySelector('.prize-btn');
    const modal = document.querySelector('.modal');
    const closeModal = document.querySelector('.close-modal');

    prizeBtn.addEventListener('click', () => {
        modal.classList.add('active');
        document.body.style.overflow = 'hidden';
    });

    closeModal.addEventListener('click', () => {
        modal.classList.remove('active');
        document.body.style.overflow = '';
    });

    modal.addEventListener('click', (e) => {
        if (e.target === modal) {
            modal.classList.remove('active');
            document.body.style.overflow = '';
        }
    });

    document.querySelectorAll('.page-btn').forEach(button => {
        button.addEventListener('click', async () => {
            const page = button.dataset.page;
            const response = await fetch(`/Home/GetPage?page=${page}`);
            const data = await response.json();

            // Update pagination
            document.querySelectorAll('.page-btn').forEach(btn => btn.classList.remove('active'));
            button.classList.add('active');

            // Update leaderboard
            const leaderboard = document.querySelector('.leaderboard');
            leaderboard.innerHTML = ''; // Clear current leaderboard
            data.Users.forEach(user => {
                const position = user.position ?? user.Position;
                const userName = user.userName ?? user.UserName;
                const avatarUrl = user.avatarUrl ?? user.AvatarUrl ?? 'data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///ywAAAAAAQABAAACAUwAOw==';
                const score = user.score ?? user.Score;
                const userItem = document.createElement('div');
                userItem.className = 'leaderboard-item';
                userItem.innerHTML = `
                <div class="rank">${position}</div>
                <div class="player-info">
                    <img src="${avatarUrl}" alt="${userName}" class="player-avatar">
                    <span class="player-name">${userName}</span>
                </div>
                <div class="score">${score}</div>`;
                leaderboard.appendChild(userItem);
            });
        });
    });
});

