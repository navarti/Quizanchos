let activeTimeline = null;

function hideModalOnStart() {
    const modal = document.getElementById('errorModal');
    modal.style.display = 'none';
}

function initializeTimeline(creationTime, secondsPerCard, timeline) {
    if (activeTimeline) {
        cancelAnimationFrame(activeTimeline); // Прерываем предыдущий таймер
        activeTimeline = null;
    }

    console.log("Initializing timeline with:", { creationTime, secondsPerCard });

    hideModalOnStart();

    // Сбрасываем ширину линии
    timeline.style.width = "0%";

    const currentTime = new Date();
    const elapsedTime = (currentTime - creationTime) / 1000;
    const remainingTime = Math.max(secondsPerCard - elapsedTime, 0);

    if (remainingTime > 0) {
        activeTimeline = requestAnimationFrame(() =>
            updateTimeline(creationTime, secondsPerCard, timeline)
        );
    } else {
        console.warn("Timeline started with 0 remaining time.");
        showEndOfTimeModal();
    }
}

function updateTimeline(creationTime, secondsPerCard, timeline) {
    const currentTime = new Date();
    const elapsedTime = (currentTime - creationTime) / 1000;
    const remainingTime = Math.max(secondsPerCard - elapsedTime, 0);
    const progressPercentage = (elapsedTime / secondsPerCard) * 100;

    timeline.style.width = `${progressPercentage}%`;

    if (remainingTime > 0) {
        activeTimeline = requestAnimationFrame(() =>
            updateTimeline(creationTime, secondsPerCard, timeline)
        );
    } else {
        console.warn("Time's up, showing modal.");
        showEndOfTimeModal();
    }
}

function showFinalStatsModal(score) {
    const modal = document.getElementById('finalStatsModal');
    const scoreElement = modal.querySelector('strong');
    scoreElement.textContent = score; 
    modal.style.display = 'flex';

    document.getElementById('goToResults').addEventListener('click', () => {
        window.location.href = "/"; 
    });
}


function showEndOfTimeModal() {
    const modal = document.getElementById('errorModal');
    const modalText = document.getElementById('modalErrorText');
    modalText.innerHTML = "Oops! Time's Up!<br>What’s next? Your call!";
    modal.style.display = 'flex';

    document.getElementById('continueQuiz').addEventListener('click', () => {
        modal.style.display = 'none';
        console.log("Continuing the quiz...");
    });

    document.getElementById('returnToMenu').addEventListener('click', () => {
        window.location.href = "/";
    });
}


document.addEventListener('DOMContentLoaded', () => {
    let activeTimeline = null;
    const timelineContainer = document.querySelector('.timeline-container');
    const timeline = timelineContainer.querySelector('.timeline');
    const mainContainer = document.querySelector('.quiz-container');

    if (!timelineContainer || !mainContainer) {
        console.error("Timeline or quiz container not found.");
        return;
    }

    const sessionId = mainContainer.getAttribute('data-session-id');
    const creationTime = new Date(timelineContainer.getAttribute('data-creation-time'));
    const secondsPerCard = parseInt(timelineContainer.getAttribute('data-seconds-per-card'), 10);
    const currentCardIndex = parseInt(timelineContainer.getAttribute('data-current-card-index'), 10);
    const totalCards = parseInt(timelineContainer.getAttribute('data-total-cards'), 10);

    initializeTimeline(creationTime, secondsPerCard, timeline);

    const options = document.querySelectorAll('.quiz-option');
    let isAnswerSubmitted = false;

    options.forEach((option, index) => {
        option.addEventListener('click', async () => {
            if (isAnswerSubmitted) return;
            isAnswerSubmitted = true;

            const selectedOption = index;
            const url = '/SingleGameSession/PickAnswerForSession';

            if (!sessionId) {
                console.error('Session ID is missing');
                return;
            }

            const data = {
                SessionId: sessionId,
                OptionPicked: selectedOption
            };

            try {
                const response = await fetch(url, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(data)
                });

                if (response.ok) {
                    const result = await response.json();
                    console.log('Response:', result);
                    const correctOption = result.correctOption;

                    options.forEach((opt, idx) => {
                        if (idx === correctOption) {
                            opt.style.background = "linear-gradient(to bottom, rgba(46, 204, 113, 0.8), rgba(46, 204, 113, 0.6))";
                            opt.style.boxShadow = "0 0 10px rgba(46, 204, 113, 0.8)";
                            opt.style.border = "2px solid rgba(46, 204, 113, 1)";
                        } else {
                            opt.style.background = "linear-gradient(to bottom, rgba(231, 76, 60, 0.8), rgba(231, 76, 60, 0.6))";
                            opt.style.boxShadow = "0 0 10px rgba(231, 76, 60, 0.8)";
                            opt.style.border = "2px solid rgba(231, 76, 60, 1)";
                        }
                    });

                    setTimeout(async () => {
                        if (currentCardIndex === totalCards - 1 ) {
                            showFinalStatsModal(result.score); 
                            return;
                        }

                        const createNextCardUrl = `/SingleGameSession/CreateNextCardForSession?sessionId=${sessionId}`;

                        try {
                            const nextCardResponse = await fetch(createNextCardUrl, {
                                method: 'POST',
                                headers: {
                                    'Content-Type': 'application/json'
                                }
                            });

                            if (nextCardResponse.ok) {
                                const nextResult = await nextCardResponse.json();
                                console.log('Next card created:', nextResult);

                                window.location.href = `/Quiz/${sessionId}`;
                            } else {
                                console.error('Error creating next card:', nextCardResponse.status, nextCardResponse.statusText);
                            }
                        } catch (error) {
                            console.error('Fetch error for next card:', error);
                        }
                    }, 3000);
                } else {
                    console.error('Error:', response.status, response.statusText);
                }
            } catch (error) {
                console.error('Fetch error:', error);
            }
        });
    });
});
