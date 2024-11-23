function validateEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
}

function showError(elementId, message) {
    const errorElement = document.getElementById(elementId);
    errorElement.textContent = message;
    errorElement.style.display = 'block';
}

function hideError(elementId) {
    const errorElement = document.getElementById(elementId);
    errorElement.textContent = '';
    errorElement.style.display = 'none';
}

function addErrorClass(inputElement) {
    inputElement.classList.add('input-error');
}

function removeErrorClass(inputElement) {
    inputElement.classList.remove('input-error');
}

function clearErrors() {
    document.querySelectorAll('.error-message').forEach((el) => (el.textContent = ''));
    document.querySelectorAll('.input-error').forEach((el) => el.classList.remove('input-error'));
}

function showModal(message, isSuccess = false) {
    const modal = document.getElementById('errorModal');
    const modalText = document.getElementById('modalErrorText');
    modalText.innerHTML = message;

    if (isSuccess) {
        modal.classList.add('success');
        modal.classList.remove('error');
    } else {
        modal.classList.add('error');
        modal.classList.remove('success');
    }

    modal.style.display = 'flex';
}


// Скрытие модального окна
function hideModal() {
    const modal = document.getElementById('errorModal');
    modal.style.display = 'none';
}

// Закрытие модального окна при нажатии на кнопку
document.getElementById('closeModal').addEventListener('click', hideModal);

// Закрытие модального окна при клике вне области окна
window.addEventListener('click', function (event) {
    const modal = document.getElementById('errorModal');
    if (event.target === modal) {
        hideModal();
    }
});
