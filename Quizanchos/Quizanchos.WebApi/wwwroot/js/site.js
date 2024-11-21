// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.addEventListener("DOMContentLoaded", function () {
    const burgerMenu = document.querySelector(".burger-menu");
    const navList = document.querySelector(".nav-list");

    burgerMenu.addEventListener("click", function () {
        navList.classList.toggle("active");
    });
});

document.getElementById('submitButton').addEventListener('click', async function () {
    const email = document.getElementById('email');
    const password = document.getElementById('password');
    const repeatPassword = document.getElementById('repeatPassword');

    // Очистка ошибок перед валидацией
    clearErrors();

    let isValid = true;

    // Валидация email
    if (!validateEmail(email.value.trim())) {
        showError('emailError', 'Please enter a valid email address.');
        addErrorClass(email);
        isValid = false;
    }

    // Валидация длины пароля
    if (password.value.trim().length < 8) {
        showError('passwordError', 'Password must be at least 8 characters long.');
        addErrorClass(password);
        isValid = false;
    }

    // Проверка совпадения паролей
    if (password.value.trim() !== repeatPassword.value.trim()) {
        showError('repeatPasswordError', 'Passwords do not match.');
        addErrorClass(repeatPassword);
        isValid = false;
    }

    // Если валидация не прошла, прекращаем выполнение
    if (!isValid) return;

    // Отправка данных на сервер
    const data = {
        email: email.value.trim(),
        password: password.value.trim(),
    };

    try {
        const response = await fetch('/Authorization/SignUp', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(data),
        });

        if (response.ok) {
            const responseData = await response.json();
            if (responseData.redirectUrl) {
                showModal('Registration successful! Welcome to Quizanchos!', true);
                setTimeout(() => {
                    window.location.href = responseData.redirectUrl;
                }, 2000); 
            } else {
                showModal('Registration successful! Welcome to Quizanchos!', true);
            }
        } else {
            const errorData = await response.json();
            if (errorData.Message) {
                showModal(errorData.Message); 
            } else {
                showModal('An unexpected error occurred. Please try again.');
            }
        }
    } catch (error) {
        console.error('Error:', error);
        showModal('A network error occurred. Please try again later.');
    }
});

// Валидация email
function validateEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
}

// Показываем ошибку под полем
function showError(elementId, message) {
    const errorElement = document.getElementById(elementId);
    errorElement.textContent = message;
}

// Добавляем красный контур для поля с ошибкой
function addErrorClass(inputElement) {
    inputElement.classList.add('input-error');
}

// Убираем красный контур и ошибки
function clearErrors() {
    document.querySelectorAll('.error-message').forEach((el) => (el.textContent = ''));
    document.querySelectorAll('.input-error').forEach((el) => el.classList.remove('input-error'));
}

// Показ модального окна
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

