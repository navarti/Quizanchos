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
                const url = `/LeaderBoard/GetLeaderBoardAsync?take=${take}&skip=${skip}${minigame ? `&minigameType=${minigame}` : ''}`;
                const resp = await fetch(url);
                const users = await resp.json();

                // Update leaderboard DOM
                const leaderboard = leaderboardContainer;
                leaderboard.innerHTML = '';
                users.forEach(u => {
                    const div = document.createElement('div');
                    div.className = 'leaderboard-item' + (u.UserName === leaderboard.getAttribute('data-current-user') ? ' current-user' : '');
                    div.innerHTML = `
                        <div class="rank">${u.Position}</div>
                        <div class="player-info">
                            <img src="${u.AvatarUrl}" alt="${u.UserName}" class="player-avatar">
                            <span class="player-name">${u.UserName}</span>
                        </div>
                        <div class="score">
                            <span class="trophy">${u.Position === 1 ? '??' : ''}</span>
                            ${u.Score}
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
                const userItem = document.createElement('div');
                userItem.className = 'leaderboard-item';
                userItem.innerHTML = `
                <div class="rank">${user.Position}</div>
                <div class="player-info">
                    <img src="${user.AvatarUrl}" alt="${user.UserName}" class="player-avatar">
                    <span class="player-name">${user.UserName}</span>
                </div>
                <div class="score">${user.Score}</div>`;
                leaderboard.appendChild(userItem);
            });
        });
    });
});

