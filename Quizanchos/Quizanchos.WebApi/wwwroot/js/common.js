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
    modalText.textContent = message; 

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

document.getElementById('closeModal').addEventListener('click', hideModal);

window.addEventListener('click', function (event) {
    const modal = document.getElementById('errorModal');
    if (event.target === modal) {
        hideModal();
    }
});

function deleteCookie(name) {
    document.cookie = `${name}=; Max-Age=0; path=/; domain=${window.location.hostname}`;
}

function logout() {
    deleteCookie('Identity.External');
    deleteCookie('QAuth');

    fetch('/Account/Logout', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        credentials: 'include',
    })
        .then(response => {
            window.location.reload();
        })
        .catch(error => {
            console.error('Error during logout:', error);
            alert('An error occurred. Please try again.');
        });
}


