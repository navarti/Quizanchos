console.log("Script Loaded");

const timelineContainer = document.querySelector('.timeline-container');
const timeline = timelineContainer.querySelector('.timeline');
const creationTime = new Date(timelineContainer.dataset.creationTime);
const secondsPerCard = parseInt(timelineContainer.dataset.secondsPerCard, 10);

function updateTimeline() {
    const currentTime = new Date();
    const elapsedTime = Math.floor((currentTime - creationTime) / 1000); // Время с начала (в секундах)
    const remainingTime = Math.max(secondsPerCard - elapsedTime, 0); // Оставшееся время

    // Рассчитать процент заполнения
    const progressPercentage = ((secondsPerCard - remainingTime) / secondsPerCard) * 100;
    console.log(secondsPerCard);
    // Обновить градиент на основе прогресса
    timeline.style.background = `linear-gradient(
        to right,
        rgba(255, 223, 0, 1) ${progressPercentage}%, /* Жёлтая часть */
        rgba(255, 223, 0, 0) ${progressPercentage}% /* Прозрачная часть */
    )`;

    // Остановить таймер, когда время истечет
    if (remainingTime === 0) {
        clearInterval(timerInterval);
    }
}

// Таймер обновления каждые 100ms для плавности
const timerInterval = setInterval(updateTimeline, 100);

// Инициализация
updateTimeline();console.log("Script Loaded");

