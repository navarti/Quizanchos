document.addEventListener('DOMContentLoaded', function () {
    setupBurgerMenu();
    setupPasswordToggles();
    setupPasswordStrengthMeter();
    setupSignupForm();
    setupHeaderUserChips();
    loadUserBalance();
});

function setupBurgerMenu() {
    const burgerMenu = document.querySelector('.burger-menu');
    const navList = document.querySelector('.nav-list');
    if (!burgerMenu || !navList) return;

    function setOpen(willOpen) {
        navList.classList.toggle('active', willOpen);
        burgerMenu.setAttribute('aria-expanded', String(willOpen));
        burgerMenu.classList.toggle('is-open', willOpen);
    }

    burgerMenu.addEventListener('click', () => {
        setOpen(!navList.classList.contains('active'));
    });

    // Auto-close when a nav link is tapped (mobile UX)
    navList.querySelectorAll('a').forEach(link => {
        link.addEventListener('click', () => {
            if (window.matchMedia('(max-width: 860px)').matches) {
                setOpen(false);
            }
        });
    });

    // Close on Escape
    document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape' && navList.classList.contains('active')) {
            setOpen(false);
            burgerMenu.focus();
        }
    });

    // Reset state when crossing the breakpoint
    window.addEventListener('resize', () => {
        if (!window.matchMedia('(max-width: 860px)').matches) {
            setOpen(false);
        }
    });
}

function setupPasswordToggles(scope = document) {
    scope.querySelectorAll('[data-toggle-target]').forEach(btn => {
        if (btn.dataset.bound === 'true') return;
        btn.dataset.bound = 'true';
        btn.addEventListener('click', () => {
            const targetId = btn.getAttribute('data-toggle-target');
            const input = document.getElementById(targetId);
            if (!input) return;
            const next = input.type === 'password' ? 'text' : 'password';
            input.type = next;
            const isShown = next === 'text';
            btn.textContent = isShown ? 'Hide' : 'Show';
            btn.setAttribute('aria-label', isShown ? 'Hide password' : 'Show password');
            btn.setAttribute('aria-pressed', String(isShown));
        });
    });
}

function setupPasswordStrengthMeter() {
    const passwordInput = document.getElementById('password');
    const meter = document.getElementById('passwordStrength');
    if (!passwordInput || !meter) return;

    const bar = meter.querySelector('.password-strength__bar span');
    const hint = meter.querySelector('.password-strength__hint');

    passwordInput.addEventListener('input', () => {
        const value = passwordInput.value;
        const score = scorePassword(value);
        const labels = ['Too short', 'Weak', 'Okay', 'Strong', 'Excellent'];
        const colors = ['var(--color-danger-500)', 'var(--color-danger-500)', 'var(--color-warning-500)', 'var(--color-success-500)', 'var(--color-success-600)'];

        bar.style.width = `${(score / 4) * 100}%`;
        bar.style.background = colors[score];
        if (hint) hint.textContent = value ? labels[score] : 'Use 8+ characters with letters, numbers, and a symbol.';
    });
}

function scorePassword(value) {
    if (!value || value.length < 8) return 0;
    let score = 1;
    if (/[A-Z]/.test(value) && /[a-z]/.test(value)) score++;
    if (/\d/.test(value)) score++;
    if (/[^A-Za-z0-9]/.test(value) && value.length >= 12) score++;
    return Math.min(score, 4);
}

function setupSignupForm() {
    const form = document.getElementById('signupForm');
    if (!form) return;
    form.addEventListener('submit', handleSubmit);
}

function setupHeaderUserChips() {
    document.querySelectorAll('img.profile-icon').forEach(applyDefaultAvatarFallback);
}

function isSafeReturnUrl(url) {
    if (!url) return false;
    try {
        const parsed = new URL(url, window.location.origin);
        return parsed.origin === window.location.origin && parsed.pathname.startsWith('/');
    } catch {
        return typeof url === 'string' && url.startsWith('/') && !url.startsWith('//');
    }
}

function getRedirectTarget(form) {
    const rawReturn = form?.dataset.returnUrl
        || new URLSearchParams(window.location.search).get('returnUrl');
    return isSafeReturnUrl(rawReturn) ? rawReturn : '/';
}

async function loadUserBalance() {
    const balanceElement = document.getElementById('user-balance-value');
    const statusElement = document.getElementById('user-status-value');
    const statusChipElement = document.getElementById('user-status-chip');

    if (!balanceElement && !statusElement) return;

    try {
        const response = await fetch('/UserProfile/GetUserInfo');
        if (!response.ok) return;

        const userInfo = await response.json();
        if (balanceElement) {
            balanceElement.textContent = `${userInfo.coins ?? 0}`;
        }

        if (statusElement) {
            const isPremiumActive = !!userInfo.premiumUntilUtc && new Date(userInfo.premiumUntilUtc) > new Date();
            const normalizedStatus = normalizeUserStatus(userInfo.userStatus);
            const displayStatus = isPremiumActive ? 'Premium' : normalizedStatus;
            statusElement.textContent = displayStatus;
            if (statusChipElement) {
                statusChipElement.classList.toggle('premium', displayStatus === 'Premium');
            }
        }

        broadcastUserInfo({
            coins: userInfo.coins ?? 0,
            premiumUntilUtc: userInfo.premiumUntilUtc,
        });
    } catch { /* offline / not signed in */ }
}

function normalizeUserStatus(userStatus) {
    if (typeof userStatus === 'string') {
        const normalized = userStatus.trim();
        if (!normalized) return 'Ordinary';
        return normalized.charAt(0).toUpperCase() + normalized.slice(1).toLowerCase();
    }
    if (typeof userStatus === 'number') return userStatus === 1 ? 'Premium' : 'Ordinary';
    return 'Ordinary';
}

window.addEventListener('storage', (event) => {
    if (event.key !== 'quizanchos:user-info' || !event.newValue) return;
    try {
        const info = JSON.parse(event.newValue);
        const balanceEl = document.getElementById('user-balance-value');
        if (balanceEl && info.coins !== null && info.coins !== undefined) {
            balanceEl.textContent = `${info.coins}`;
        }
        const statusEl = document.getElementById('user-status-value');
        const statusChip = document.getElementById('user-status-chip');
        if (statusEl) {
            const isPremium = info.premiumUntilUtc && new Date(info.premiumUntilUtc) > new Date();
            statusEl.textContent = isPremium ? 'Premium' : 'Ordinary';
            if (statusChip) statusChip.classList.toggle('premium', !!isPremium);
        }
    } catch { /* ignore */ }
});

async function handleSubmit(event) {
    event.preventDefault();

    const form = event.currentTarget?.tagName === 'FORM' ? event.currentTarget : document.getElementById('signupForm');
    const submitButton = document.getElementById('submitButton');
    const email = document.getElementById('email');
    const password = document.getElementById('password');
    const repeatPassword = document.getElementById('repeatPassword');

    clearErrors();

    let isValid = true;
    if (!validateEmail(email.value.trim())) {
        showError('emailError', 'Please enter a valid email address.');
        addErrorClass(email);
        isValid = false;
    }
    if (password.value.trim().length < 8) {
        showError('passwordError', 'Password must be at least 8 characters long.');
        addErrorClass(password);
        isValid = false;
    }
    if (password.value.trim() !== repeatPassword.value.trim()) {
        showError('repeatPasswordError', 'Passwords do not match.');
        addErrorClass(repeatPassword);
        isValid = false;
    }
    if (!isValid) return;

    setButtonBusy(submitButton, true, 'Creating account…');

    try {
        const response = await fetch('/Authorization/SignUp', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                email: email.value.trim(),
                password: password.value.trim(),
            }),
        });

        if (!response.ok) {
            let message = 'An unexpected error occurred.';
            try { const data = await response.json(); message = data?.Message || data?.message || message; } catch {}
            showError('emailError', message);
            addErrorClass(email);
            return;
        }

        const responseData = await response.json();
        if (responseData === 1) {
            // Email confirmation required
            openVerifyModal();
            bindEmailVerifyHandler(form);
        } else if (responseData === 0) {
            showToast('Account created. Welcome!', 'success');
            window.location.href = getRedirectTarget(form);
        } else {
            showToast('Unexpected response from the server.', 'error');
        }
    } catch (err) {
        console.error('Network error:', err);
        showToast('Network error. Please try again.', 'error');
    } finally {
        setButtonBusy(submitButton, false);
    }
}

let signupVerifyHandler = null;
function bindEmailVerifyHandler(form) {
    const verifyButton = document.getElementById('verifyButton');
    const codeInput = document.getElementById('codeInput');
    const errorContainer = document.getElementById('errorContainer');
    const messageContainer = document.getElementById('messageContainer');
    if (!verifyButton || !codeInput) return;

    if (signupVerifyHandler) {
        verifyButton.removeEventListener('click', signupVerifyHandler);
    }

    signupVerifyHandler = async () => {
        if (errorContainer) errorContainer.textContent = '';
        if (messageContainer) messageContainer.innerHTML = '';
        codeInput.classList.remove('error');

        const code = codeInput.value.trim();
        if (code.length !== 6) {
            displayError('Code must be exactly 6 characters long.');
            return;
        }

        setButtonBusy(verifyButton, true, 'Verifying…');
        try {
            const response = await fetch('/EmailConfirmation/ConfirmEmail', {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                body: new URLSearchParams({ code: code }),
            });
            if (!response.ok) {
                displayError('Invalid code. Please try again.');
                return;
            }
            displaySuccess('Verified! Redirecting…');
            disableInput();
            const target = isSafeReturnUrl(form?.dataset.returnUrl) ? form.dataset.returnUrl : '/Account/Profile';
            setTimeout(() => { window.location.href = target; }, 800);
        } catch (err) {
            displayError('Network error. Please try again.');
            console.error('Verification error:', err);
        } finally {
            setButtonBusy(verifyButton, false);
        }
    };
    verifyButton.addEventListener('click', signupVerifyHandler);
}

function redirectToCategory(category) {
    window.location.href = `/Minigame/Quiz?filter=${encodeURIComponent(category)}`;
}

const verifyModal = document.getElementById('verifyModal');
if (verifyModal) {
    verifyModal.addEventListener('click', function (e) {
        if (e.target === this) closeVerifyModal();
    });
}

const modalContent = document.querySelector('#verifyModal .verify-modal-content');
if (modalContent) {
    modalContent.addEventListener('click', (e) => e.stopPropagation());
}
