let activeTimeline = null;

function hideModalOnStart() {
    const modal = document.getElementById('errorModal');
    modal.style.display = 'none';
}

function initializeTimeline(creationTime, secondsPerCard, timeline, currentCardIndex, totalCards) {
    if (activeTimeline) {
        cancelAnimationFrame(activeTimeline);
        activeTimeline = null;
    }

    console.log("Initializing timeline with:", { creationTime, secondsPerCard });

    hideModalOnStart();

    timeline.style.width = "0%";

    const currentTime = new Date();
    const elapsedTime = (currentTime - creationTime) / 1000;
    const remainingTime = Math.max(secondsPerCard - elapsedTime, 0);

    if (remainingTime > 0) {
        activeTimeline = requestAnimationFrame(() =>
            updateTimeline(creationTime, secondsPerCard, timeline, currentCardIndex, totalCards)
        );
    } else {
        console.warn("Timeline started with 0 remaining time.");
        showEndOfTimeModal(currentCardIndex, totalCards); 
    }
}

function updateTimeline(creationTime, secondsPerCard, timeline, currentCardIndex, totalCards) {
    const currentTime = new Date();
    const elapsedTime = (currentTime - creationTime) / 1000;
    const remainingTime = Math.max(secondsPerCard - elapsedTime, 0);
    const progressPercentage = (elapsedTime / secondsPerCard) * 100;

    timeline.style.width = `${progressPercentage}%`;

    if (remainingTime > 0) {
        activeTimeline = requestAnimationFrame(() =>
            updateTimeline(creationTime, secondsPerCard, timeline, currentCardIndex, totalCards)
        );
    } else {
        console.warn("Time's up, showing modal.");
        showEndOfTimeModal(currentCardIndex, totalCards); 
    }
}


function showFinalStatsModal() {
    const modal = document.getElementById('finalStatsModal');
    
    const score = modal.getAttribute('data-score');
    const totalCards = modal.getAttribute('data-total');

    const scoreElement = document.getElementById('scoreDisplay');
    const totalCardsElement = document.getElementById('totalCardsDisplay');
    
    scoreElement.textContent = score;
    totalCardsElement.textContent = totalCards;
    
    modal.style.display = 'flex';
    
    document.getElementById('goToResults').addEventListener('click', () => {
        window.location.href = "/";
    });
}
function stopTimeline() {
    if (activeTimeline) {
        cancelAnimationFrame(activeTimeline); 
        activeTimeline = null; 
        console.log('Timeline stopped');
    }
}
function showEndOfTimeModal(currentCardIndex, totalCards) {
    if (currentCardIndex === totalCards) {
        console.log("Time's up on the last question. Showing results...");
        stopTimeline();
        showFinalStatsModal(); 
        return;
    }

    const modal = document.getElementById('errorModal');
    const modalText = document.getElementById('modalErrorText');
    modalText.innerHTML = "Oops! Time's Up!<br>What’s next? Your call!";
    modal.style.display = 'flex';

    document.getElementById('restartQuiz').addEventListener('click', () => {
        const quizContainer = document.getElementById('quiz-header');
        if (!quizContainer) {
            console.error('Quiz container not found!');
            return;
        }

        const categoryId = quizContainer.getAttribute('data-category-id');
        if (!categoryId) {
            console.error('Category ID is missing or invalid');
            return;
        }

        redirectwithPreloader(`/Quiz/Setup/${categoryId}`);
    });

    document.getElementById('returnToMenu').addEventListener('click', () => {
        window.location.href = "/";
    });
}

function showPreloader() {
    const preloader = document.getElementById('preloader');
    if (preloader) {
        preloader.style.display = 'flex';
    }
}

function hidePreloader() {
    const preloader = document.getElementById('preloader');
    if (preloader) {
        preloader.style.display = 'none';
    }
}

function redirectwithPreloader(url) {
    showPreloader();
    setTimeout(() => {
        window.location.href = url;
    }, 200);
};



document.addEventListener('DOMContentLoaded', () => {
    let activeTimeline = null;
    const timelineContainer = document.querySelector('.timeline-container');
    const timeline = timelineContainer.querySelector('.timeline');
    const mainContainer = document.querySelector('.quiz-container');
    const questionInfoElement = document.querySelector('.question-info');

    if (!timelineContainer || !mainContainer) {
        console.error("Timeline or quiz container not found.");
        return;
    }

    const sessionId = mainContainer.getAttribute('data-session-id');
    const creationTime = new Date(timelineContainer.getAttribute('data-creation-time'));
    const secondsPerCard = parseInt(timelineContainer.getAttribute('data-seconds-per-card'), 10);
    const currentCardIndex = parseInt(questionInfoElement.dataset.current, 10);
    const totalCards = parseInt(questionInfoElement.dataset.total, 10);

    console.log('Current Card Index:', currentCardIndex);
    console.log('Total Cards:', totalCards);

    initializeTimeline(creationTime, secondsPerCard, timeline,currentCardIndex,totalCards);

    const options = document.querySelectorAll('.quiz-option');
    let isAnswerSubmitted = false;

    options.forEach((option, index) => {
        option.addEventListener('click', async () => {
            if (isAnswerSubmitted) return; 
            isAnswerSubmitted = true;

            stopTimeline(); 

            const selectedOption = index;
            const url = '/SingleGameSession/PickAnswerForSession';

            if (!sessionId) {
                console.error('Session ID is missing');
                return;
            }

            const data = {
                SessionId: sessionId,
                OptionPicked: selectedOption,
            };

            try {
                const response = await fetch(url, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify(data),
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

                    // Проверяем, последний ли вопрос
                    if (currentCardIndex === totalCards) {
                        setTimeout(() => {
                            showFinalStatsModal(); 
                        }, 2000); 
                        return;
                    }
                    
                    const createNextCardUrl = `/SingleGameSession/CreateNextCardForSession?sessionId=${sessionId}`;

                    try {
                        const nextCardResponse = await fetch(createNextCardUrl, {
                            method: 'POST',
                            headers: {
                                'Content-Type': 'application/json',
                            },
                        });

                        if (nextCardResponse.ok) {
                            const nextResult = await nextCardResponse.json();
                            console.log('Next card created:', nextResult);
                            redirectwithPreloader(`/Quiz/${sessionId}`);
                        } else {
                            console.error('Error creating next card:', nextCardResponse.status, nextCardResponse.statusText);
                        }
                    } catch (error) {
                        console.error('Fetch error for next card:', error);
                    }
                } else {
                    console.error('Error:', response.status, response.statusText);
                }
            } catch (error) {
                console.error('Fetch error:', error);
            }
        });
    });
});

