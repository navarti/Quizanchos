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

function setupPasswordToggles(scope = document) {
    scope.querySelectorAll('[data-toggle-target]').forEach(btn => {
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

async function performSignin(event) {
    event.preventDefault();

    const form = document.getElementById('signinForm');
    const submitBtn = document.getElementById('signinButton');
    const email = document.getElementById('signinEmail');
    const password = document.getElementById('signinPassword');

    clearErrors();

    let isValid = true;
    if (!validateEmail(email.value.trim())) {
        showError('signinEmailError', 'Please enter a valid email address.');
        addErrorClass(email);
        isValid = false;
    }
    if (!password.value.trim()) {
        showError('signinPasswordError', 'Password cannot be empty.');
        addErrorClass(password);
        isValid = false;
    }
    if (!isValid) return;

    setButtonBusy(submitBtn, true, 'Signing in…');

    try {
        const response = await fetch('/Authorization/SignIn', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                email: email.value.trim(),
                password: password.value.trim(),
            }),
        });

        if (response.ok) {
            window.location.href = getRedirectTarget(form);
            return;
        }

        let message = 'Invalid email or password.';
        try { const data = await response.json(); message = data?.Message || data?.message || message; } catch {}
        showError('signinPasswordError', message);
        addErrorClass(password);
    } catch (err) {
        console.error(err);
        showToast('Network error. Please try again.', 'error');
    } finally {
        setButtonBusy(submitBtn, false);
    }
}

/* ------------------------------------------------------------------------
   Forgot password — opens the verify modal and uses the email-confirmation
   flow already wired into /Authorization/RequestPasswordReset and
   /Authorization/ConfirmPasswordReset.
   ------------------------------------------------------------------------ */
let resetState = { email: '', newPassword: '' };

function setupForgotPassword() {
    const btn = document.getElementById('forgotPasswordBtn');
    const sendCodeBtn = document.getElementById('resetSendCodeBtn');
    const verifyBtn = document.getElementById('verifyButton');
    const resendBtn = document.getElementById('resendButton');
    const resetEmailInput = document.getElementById('resetEmail');
    const newPasswordInput = document.getElementById('resetNewPassword');
    const codeInput = document.getElementById('codeInput');
    const messageContainer = document.getElementById('messageContainer');
    const errorContainer = document.getElementById('errorContainer');
    const emailStep = document.getElementById('resetEmailStep');
    const codeStep = document.getElementById('resetCodeStep');
    const intro = document.getElementById('resetIntro');

    if (!btn || !verifyBtn) return;

    btn.addEventListener('click', () => {
        const signinEmail = document.getElementById('signinEmail')?.value?.trim();
        if (signinEmail && resetEmailInput) resetEmailInput.value = signinEmail;
        emailStep.style.display = '';
        codeStep.style.display = 'none';
        if (intro) intro.textContent = "Enter your email — we'll send you a 6-digit code.";
        if (messageContainer) messageContainer.innerHTML = '';
        if (errorContainer) errorContainer.textContent = '';
        openVerifyModal();
        resetEmailInput?.focus();
    });

    async function sendCode() {
        const email = resetEmailInput?.value?.trim() || '';
        const newPassword = newPasswordInput?.value || '';
        if (!validateEmail(email)) {
            if (messageContainer) messageContainer.innerHTML = '<span class="error-message" style="display:block">Please enter a valid email.</span>';
            return;
        }
        if (newPassword.length < 8) {
            if (messageContainer) messageContainer.innerHTML = '<span class="error-message" style="display:block">Password must be at least 8 characters.</span>';
            return;
        }

        setButtonBusy(sendCodeBtn, true, 'Sending…');
        try {
            const response = await fetch('/Authorization/RequestPasswordReset', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ Email: email }),
            });
            if (!response.ok) {
                let message = 'Failed to send code. Try again later.';
                try { const data = await response.json(); message = data?.message || data?.Message || message; } catch {}
                if (messageContainer) messageContainer.innerHTML = `<span class="error-message" style="display:block">${message}</span>`;
                return;
            }
            resetState = { email, newPassword };
            emailStep.style.display = 'none';
            codeStep.style.display = '';
            if (intro) intro.textContent = `Code sent to ${email}. It may take a moment.`;
            if (messageContainer) messageContainer.innerHTML = '';
            codeInput?.focus();
        } catch (err) {
            console.error(err);
            if (messageContainer) messageContainer.innerHTML = '<span class="error-message" style="display:block">Network error. Try again.</span>';
        } finally {
            setButtonBusy(sendCodeBtn, false);
        }
    }

    sendCodeBtn?.addEventListener('click', sendCode);

    verifyBtn.addEventListener('click', async () => {
        if (errorContainer) errorContainer.textContent = '';
        codeInput?.classList.remove('error');

        const code = codeInput?.value?.trim() || '';
        if (code.length !== 6) {
            displayError('Code must be exactly 6 characters long.');
            return;
        }

        setButtonBusy(verifyBtn, true, 'Verifying…');
        try {
            const response = await fetch('/Authorization/ConfirmPasswordReset', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    Email: resetState.email,
                    Code: code,
                    NewPassword: resetState.newPassword,
                }),
            });
            if (!response.ok) {
                let message = 'Invalid code. Please try again.';
                try { const data = await response.json(); message = data?.message || data?.Message || message; } catch {}
                displayError(message);
                return;
            }
            displaySuccess('Password updated. You can sign in now.');
            disableInput();
            setTimeout(() => {
                closeVerifyModal();
                showToast('Password updated. Please sign in.', 'success');
                const emailField = document.getElementById('signinEmail');
                if (emailField && resetState.email) emailField.value = resetState.email;
                document.getElementById('signinPassword')?.focus();
            }, 1200);
        } catch (err) {
            displayError('Network error. Please try again.');
            console.error(err);
        } finally {
            setButtonBusy(verifyBtn, false);
        }
    });

    resendBtn?.addEventListener('click', sendCode);
}

document.addEventListener('DOMContentLoaded', () => {
    setupPasswordToggles();
    setupForgotPassword();
    document.getElementById('signinForm')?.addEventListener('submit', performSignin);
});
