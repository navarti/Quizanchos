document.addEventListener('DOMContentLoaded', () => {
    const filterBtns = document.querySelectorAll('.filter-btn');
    const leaderboardItems = document.querySelectorAll('.leaderboard-item');
    const currentUserItem = document.querySelector('.leaderboard-item.current-user');

    filterBtns.forEach(btn => {
        btn.addEventListener('click', () => {
            // Remove active class from all buttons
            filterBtns.forEach(b => b.classList.remove('active'));
            // Add active class to clicked button
            btn.classList.add('active');

            const category = btn.dataset.category;

            // Filter leaderboard items
            leaderboardItems.forEach(item => {
                if (category === 'all' || item.dataset.category === category) {
                    item.style.display = 'grid';
                    setTimeout(() => {
                        item.style.opacity = '1';
                        item.style.transform = 'translateY(0)';
                    }, 10);
                } else {
                    item.style.opacity = '0';
                    item.style.transform = 'translateY(20px)';
                    setTimeout(() => {
                        item.style.display = 'none';
                    }, 300);
                }
            });
            
            if (category !== 'all' && currentUserItem.dataset.category !== category) {
                currentUserItem.style.display = 'grid';
                setTimeout(() => {
                    currentUserItem.style.opacity = '1';
                    currentUserItem.style.transform = 'translateY(0)';
                }, 10);
            }
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

