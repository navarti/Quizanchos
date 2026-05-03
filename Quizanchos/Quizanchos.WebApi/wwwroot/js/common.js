function validateEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
}

function showError(elementId, message) {
    const errorElement = document.getElementById(elementId);
    if (!errorElement) return;
    errorElement.textContent = message;
    errorElement.style.display = 'block';
    const inputId = elementId.replace(/Error$/, '');
    const input = document.getElementById(inputId);
    if (input) input.setAttribute('aria-invalid', 'true');
}

function hideError(elementId) {
    const errorElement = document.getElementById(elementId);
    if (!errorElement) return;
    errorElement.textContent = '';
    errorElement.style.display = 'none';
    const inputId = elementId.replace(/Error$/, '');
    const input = document.getElementById(inputId);
    if (input) input.removeAttribute('aria-invalid');
}

function addErrorClass(inputElement) {
    if (!inputElement) return;
    inputElement.classList.add('input-error');
    inputElement.setAttribute('aria-invalid', 'true');
}

function removeErrorClass(inputElement) {
    if (!inputElement) return;
    inputElement.classList.remove('input-error');
    inputElement.removeAttribute('aria-invalid');
}

function clearErrors() {
    document.querySelectorAll('.error-message').forEach((el) => {
        el.textContent = '';
        el.style.display = 'none';
    });
    document.querySelectorAll('.input-error').forEach((el) => {
        el.classList.remove('input-error');
        el.removeAttribute('aria-invalid');
    });
}

/* ------------------------------------------------------------------------
   Toast notifications — replaces native alert() and overuse of errorModal
   for short status updates. Errors that demand acknowledgement still use
   showModal().
   ------------------------------------------------------------------------ */

function ensureToastContainer() {
    let container = document.getElementById('toast-container');
    if (!container) {
        container = document.createElement('div');
        container.id = 'toast-container';
        container.className = 'toast-container';
        container.setAttribute('aria-live', 'polite');
        container.setAttribute('aria-atomic', 'false');
        document.body.appendChild(container);
    }
    return container;
}

function showToast(message, variant = 'info', durationMs = 4000) {
    if (!message) return;
    const container = ensureToastContainer();
    const toast = document.createElement('div');
    toast.className = `toast toast--${variant}`;
    toast.setAttribute('role', variant === 'error' ? 'alert' : 'status');

    const icon = document.createElement('span');
    icon.className = 'toast__icon';
    icon.setAttribute('aria-hidden', 'true');
    icon.textContent = variant === 'success' ? '✓'
        : variant === 'error' ? '!'
        : variant === 'warning' ? '!'
        : 'i';

    const text = document.createElement('span');
    text.className = 'toast__text';
    text.textContent = message;

    const close = document.createElement('button');
    close.type = 'button';
    close.className = 'toast__close';
    close.setAttribute('aria-label', 'Dismiss notification');
    close.textContent = '×';
    close.addEventListener('click', () => dismissToast(toast));

    toast.append(icon, text, close);
    container.appendChild(toast);

    requestAnimationFrame(() => toast.classList.add('toast--visible'));

    if (durationMs > 0) {
        setTimeout(() => dismissToast(toast), durationMs);
    }
    return toast;
}

function dismissToast(toast) {
    if (!toast || !toast.parentNode) return;
    toast.classList.remove('toast--visible');
    toast.classList.add('toast--leaving');
    setTimeout(() => toast.remove(), 200);
}

/* ------------------------------------------------------------------------
   Modal dialogs — for messages requiring acknowledgement / a choice.
   For transient feedback prefer showToast().
   ------------------------------------------------------------------------ */

function showModal(headerText = 'Notification', bodyText, isSuccess = false, buttons = []) {
    const modal = document.getElementById('errorModal');
    const modalHeader = document.getElementById('modalHeader');
    const modalErrorText = document.getElementById('modalErrorText');
    const modalButtons = document.getElementById('modalButtons');

    if (!modal || !modalHeader || !modalErrorText || !modalButtons) {
        console.error('showModal: required modal elements missing.');
        return;
    }

    modalHeader.textContent = headerText;
    modalErrorText.textContent = bodyText || '';

    modalButtons.innerHTML = '';
    buttons.forEach(button => {
        const btn = document.createElement('button');
        btn.type = 'button';
        btn.textContent = button.text;
        btn.className = `btn ${button.class || ''}`;
        btn.addEventListener('click', (e) => {
            try { button.onClick && button.onClick(e); } finally {
                if (button.closeOnClick !== false) hideModal();
            }
        });
        modalButtons.appendChild(btn);
    });

    modal.classList.toggle('success', !!isSuccess);
    modal.classList.toggle('error', !isSuccess);
    modal.style.display = 'flex';
    modal.setAttribute('aria-hidden', 'false');

    const focusTarget = modalButtons.querySelector('button') || modal.querySelector('.close-btn');
    focusTarget?.focus();
}

function hideModal() {
    const modal = document.getElementById('errorModal');
    if (!modal) return;
    modal.style.display = 'none';
    modal.setAttribute('aria-hidden', 'true');
}

function showConfirm({ title = 'Confirm', message = '', confirmText = 'Confirm', cancelText = 'Cancel', danger = false } = {}) {
    return new Promise(resolve => {
        showModal(title, message, !danger, [
            {
                text: cancelText,
                class: 'btn-ghost',
                onClick: () => resolve(false),
            },
            {
                text: confirmText,
                class: danger ? 'btn-danger' : 'btn-primary',
                onClick: () => resolve(true),
            },
        ]);
    });
}

document.addEventListener('DOMContentLoaded', () => {
    document.getElementById('closeModal')?.addEventListener('click', hideModal);

    document.addEventListener('keydown', (e) => {
        if (e.key !== 'Escape') return;
        const modal = document.getElementById('errorModal');
        if (modal && modal.style.display === 'flex') hideModal();
        const verifyModal = document.getElementById('verifyModal');
        if (verifyModal && verifyModal.classList.contains('active')) closeVerifyModal();
    });
});

window.addEventListener('click', function (event) {
    const modal = document.getElementById('errorModal');
    if (modal && event.target === modal) {
        hideModal();
    }
});

function deleteCookie(name) {
    document.cookie = `${name}=; Max-Age=0; path=/; domain=${window.location.hostname}`;
}

function openVerifyModal() {
    const verifyModal = document.getElementById('verifyModal');
    if (!verifyModal) return;
    verifyModal.style.display = 'flex';
    verifyModal.offsetHeight;
    verifyModal.classList.add('active');
    verifyModal.setAttribute('aria-hidden', 'false');
    document.getElementById('codeInput')?.focus();
}

function closeVerifyModal() {
    const verifyModal = document.getElementById('verifyModal');
    if (!verifyModal) return;
    verifyModal.classList.remove('active');
    verifyModal.setAttribute('aria-hidden', 'true');
    setTimeout(() => {
        verifyModal.style.display = 'none';
    }, 300);
}

function disableInput() {
    const codeInput = document.getElementById('codeInput');
    if (!codeInput) return;
    codeInput.disabled = true;
    codeInput.classList.add('disabled');
}

function displayError(message) {
    const errorContainer = document.getElementById('errorContainer');
    const codeInput = document.getElementById('codeInput');
    if (errorContainer) errorContainer.textContent = message;
    if (codeInput) codeInput.classList.add('error');
}

function displaySuccess(message) {
    const messageContainer = document.getElementById('messageContainer');
    const errorContainer = document.getElementById('errorContainer');
    if (messageContainer) messageContainer.innerHTML = `<span class="success-message">${message}</span>`;
    if (errorContainer) errorContainer.textContent = '';
}

/* ------------------------------------------------------------------------
   Submit-button busy state
   ------------------------------------------------------------------------ */

function setButtonBusy(button, busy, busyText) {
    if (!button) return;
    if (busy) {
        if (!button.dataset.originalText) {
            button.dataset.originalText = button.textContent;
        }
        button.disabled = true;
        button.classList.add('is-busy');
        button.setAttribute('aria-busy', 'true');
        if (busyText) button.textContent = busyText;
    } else {
        button.disabled = false;
        button.classList.remove('is-busy');
        button.removeAttribute('aria-busy');
        if (button.dataset.originalText) {
            button.textContent = button.dataset.originalText;
            delete button.dataset.originalText;
        }
    }
}

/* ------------------------------------------------------------------------
   Default avatar — small inline SVG so we never depend on external CDNs
   for a placeholder image.
   ------------------------------------------------------------------------ */

const DEFAULT_AVATAR_DATA_URI =
    'data:image/svg+xml;charset=utf-8,' + encodeURIComponent(
        '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 144 144">'
        + '<defs><linearGradient id="g" x1="0" y1="0" x2="1" y2="1">'
        + '<stop offset="0%" stop-color="#a78bfa"/>'
        + '<stop offset="100%" stop-color="#ec4899"/>'
        + '</linearGradient></defs>'
        + '<rect width="144" height="144" fill="url(#g)"/>'
        + '<circle cx="72" cy="58" r="26" fill="#ffffff" opacity="0.9"/>'
        + '<path d="M28 132c4-26 22-40 44-40s40 14 44 40z" fill="#ffffff" opacity="0.9"/>'
        + '</svg>');

function applyDefaultAvatarFallback(img) {
    if (!img || img.dataset.fallbackBound === 'true') return;
    img.dataset.fallbackBound = 'true';
    img.addEventListener('error', () => {
        if (img.src !== DEFAULT_AVATAR_DATA_URI) img.src = DEFAULT_AVATAR_DATA_URI;
    });
    if (!img.getAttribute('src') || /via\.placeholder\.com/.test(img.src)) {
        img.src = DEFAULT_AVATAR_DATA_URI;
    }
}

/* ------------------------------------------------------------------------
   Cross-tab balance / status sync
   ------------------------------------------------------------------------ */

function broadcastUserInfo(userInfo) {
    try {
        localStorage.setItem('quizanchos:user-info', JSON.stringify({
            coins: userInfo?.coins ?? null,
            premiumUntilUtc: userInfo?.premiumUntilUtc ?? null,
            updatedAt: Date.now(),
        }));
    } catch { /* private mode etc. */ }
}
