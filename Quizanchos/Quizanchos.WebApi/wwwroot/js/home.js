document.addEventListener('DOMContentLoaded', () => {
    // Add hover effect to quiz cards
    const quizCards = document.querySelectorAll('.quiz-card');
    quizCards.forEach(card => {
        card.addEventListener('mouseenter', () => {
            card.style.transform = 'translateY(-5px)';
        });
        card.addEventListener('mouseleave', () => {
            card.style.transform = 'translateY(0)';
        });
    });
    
    
});

document.getElementById("leaderboard-card").addEventListener("click", function () {
    window.location.href = "/leaderboard";
});
