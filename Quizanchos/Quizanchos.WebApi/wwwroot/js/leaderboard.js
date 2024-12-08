document.addEventListener('DOMContentLoaded', () => {
    // Filter functionality
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

            // Always show current user's position
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

    // Pagination functionality
    const pageButtons = document.querySelectorAll('.page-btn');

    pageButtons.forEach(button => {
        if (!button.disabled) {
            button.addEventListener('click', () => {
                pageButtons.forEach(btn => btn.classList.remove('active'));
                button.classList.add('active');

                // Simulate page change with animation
                const leaderboard = document.querySelector('.leaderboard');
                leaderboard.style.opacity = '0';
                leaderboard.style.transform = 'translateY(20px)';

                setTimeout(() => {
                    leaderboard.style.opacity = '1';
                    leaderboard.style.transform = 'translateY(0)';
                }, 300);
            });
        }
    });
});

