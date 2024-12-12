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
function showModal(headerText = 'Notification', bodyText, isSuccess = false,buttons = []) {
    const modal = document.getElementById('errorModal');
    const modalHeader = document.getElementById('modalHeader');
    const modalErrorText = document.getElementById('modalErrorText');
    const modalButtons = document.getElementById('modalButtons');

    if (!modal) {
        console.error('Error: Modal element not found.');
        return;
    }
    if (!modalHeader) {
        console.error('Error: Modal header element not found.');
        return;
    }
    if (!modalErrorText) {
        console.error('Error: Modal error text element not found.');
        return;
    }
    if (!modalButtons) {
        console.error('Error: Modal buttons container not found.');
        return;
    }
    
    modalHeader.textContent = headerText;
    modalErrorText.textContent = bodyText;
    
    modalButtons.innerHTML = '';
    
    buttons.forEach(button => {
        const btn = document.createElement('button');
        btn.textContent = button.text;
        btn.className = `btn ${button.class || ''}`;
        btn.addEventListener('click', button.onClick);
        modalButtons.appendChild(btn);
    });

    if (isSuccess) {
        modal.classList.add('success');
        modal.classList.remove('error');
    } else {
        modal.classList.add('error');
        modal.classList.remove('success');
    }

    modal.style.display = 'flex';
}

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

function openVerifyModal() {
    const verifyModal = document.getElementById('verifyModal');
    const closeModalButton = document.querySelector('.modal-close');
    verifyModal.style.display = 'flex'; 
    verifyModal.offsetHeight;
    verifyModal.classList.add('active');
}

function closeVerifyModal() {
    const verifyModal = document.getElementById('verifyModal');
    const closeModalButton = document.querySelector('.modal-close');
    verifyModal.classList.remove('active'); 
    setTimeout(() => {
        verifyModal.style.display = 'none'; 
    }, 300);
}
function disableInput() {
    codeInput.disabled = true; 
    codeInput.classList.add("disabled"); 
}

function displayError(message) {
    errorContainer.innerText = message;
    codeInput.classList.add("error"); 
}

function displaySuccess(message) {
    messageContainer.innerHTML = `<span class="success-message">${message}</span>`;
    errorContainer.innerText = ""; 
}


