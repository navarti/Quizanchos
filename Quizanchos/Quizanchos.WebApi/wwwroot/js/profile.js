/* ------------------------------------------------------------------------
   Avatar upload
   ------------------------------------------------------------------------ */
async function uploadAvatar(file) {
    const formData = new FormData();
    formData.append('formFile', file);

    const response = await fetch('/UserProfile/UpdateAvatar', {
        method: 'POST',
        body: formData,
    });

    if (!response.ok) {
        let message = 'Failed to upload avatar.';
        try {
            const data = await response.json();
            message = data?.message || data?.Message || message;
        } catch { /* keep default */ }
        throw new Error(message);
    }

    let payload = null;
    try { payload = await response.json(); } catch { /* server may still return empty */ }
    return payload;
}

function initAvatarUpload() {
    const avatarInput = document.getElementById('avatar-input');
    const avatarImg = document.getElementById('avatar');
    if (!avatarInput || !avatarImg) return;

    applyDefaultAvatarFallback(avatarImg);

    avatarInput.addEventListener('change', async (event) => {
        const file = event.target.files[0];
        if (!file) return;

        const previewUrl = URL.createObjectURL(file);
        const previousSrc = avatarImg.src;
        avatarImg.src = previewUrl;

        try {
            const result = await uploadAvatar(file);
            const newUrl = result?.newAvatarUrl ?? result?.NewAvatarUrl ?? result?.avatarUrl ?? result?.AvatarUrl;
            if (newUrl) avatarImg.src = newUrl;
            showToast('Avatar updated.', 'success');
        } catch (err) {
            avatarImg.src = previousSrc;
            showToast(err.message || 'Failed to upload avatar.', 'error');
        } finally {
            URL.revokeObjectURL(previewUrl);
            avatarInput.value = '';
        }
    });
}

/* ------------------------------------------------------------------------
   Username editing
   ------------------------------------------------------------------------ */
function toggleEdit(fieldId) {
    const field = document.getElementById(fieldId);
    const editButton = document.getElementById(`edit-${fieldId}-btn`) || document.getElementById('edit-username-btn');
    if (!field || !editButton) return;

    if (field.readOnly) {
        field.readOnly = false;
        field.focus();
        field.select?.();
        editButton.textContent = 'Cancel';
        field.dataset.originalValue = field.value;
    } else {
        field.readOnly = true;
        editButton.textContent = 'Edit';
        if (field.dataset.originalValue !== undefined) {
            field.value = field.dataset.originalValue;
            delete field.dataset.originalValue;
        }
    }
}

async function saveChanges(fieldId) {
    const field = document.getElementById(fieldId);
    const saveBtn = document.querySelector('#profile-form .save-button');
    if (!field) return;

    const value = field.value.trim();
    if (!value) {
        showToast('Username cannot be empty.', 'error');
        field.focus();
        return;
    }

    setButtonBusy(saveBtn, true, 'Saving…');
    try {
        const response = await fetch('/UserProfile/UpdateNickname', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: new URLSearchParams({ nickname: value }),
        });

        if (!response.ok) {
            let message = 'Failed to save changes.';
            try { const data = await response.json(); message = data?.message || data?.Message || message; } catch {}
            throw new Error(message);
        }

        field.readOnly = true;
        delete field.dataset.originalValue;
        const editButton = document.getElementById('edit-username-btn');
        if (editButton) editButton.textContent = 'Edit';
        showToast('Profile updated.', 'success');
    } catch (err) {
        showToast(err.message || 'Failed to save changes.', 'error');
    } finally {
        setButtonBusy(saveBtn, false);
    }
}

/* ------------------------------------------------------------------------
   Logout
   ------------------------------------------------------------------------ */
async function logout() {
    const confirmed = await showConfirm({
        title: 'Sign out?',
        message: 'You\'ll need to sign in again to play.',
        confirmText: 'Sign out',
        cancelText: 'Stay signed in',
        danger: true,
    });
    if (!confirmed) return;

    try {
        deleteCookie('Identity.External');
        deleteCookie('QAuth');
        await fetch('/Account/Logout', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            credentials: 'include',
        });
    } catch (err) {
        console.error('Logout error:', err);
    } finally {
        window.location.href = '/';
    }
}

/* ------------------------------------------------------------------------
   View switching (Profile / Change Password tabs)
   ------------------------------------------------------------------------ */
function switchView(view, button, replaceHistory = false) {
    document.querySelectorAll('.nav-button').forEach(btn => {
        btn.classList.remove('active');
        btn.setAttribute('aria-selected', 'false');
    });
    if (button) {
        button.classList.add('active');
        button.setAttribute('aria-selected', 'true');
    }

    document.querySelectorAll('.view-section').forEach(v => v.style.display = 'none');
    const selectedView = document.getElementById(`${view}-view`);
    if (selectedView) selectedView.style.display = 'block';

    const targetHash = `#${view}`;
    if (window.location.hash !== targetHash) {
        if (replaceHistory) history.replaceState(null, '', targetHash);
        else                history.pushState(null, '', targetHash);
    }
}

/* ------------------------------------------------------------------------
   Password change (uses email-confirmation flow)
   ------------------------------------------------------------------------ */
function handlePasswordSubmit(event) {
    event.preventDefault();

    const newPasswordInput = document.getElementById('new-password');
    const confirmPasswordInput = document.getElementById('confirm-password');
    const emailInput = document.getElementById('email');

    const newPassword = newPasswordInput?.value || '';
    const confirmPassword = confirmPasswordInput?.value || '';
    const email = emailInput?.value?.trim() || '';

    if (newPassword.length < 8) {
        showToast('Password must be at least 8 characters.', 'error');
        newPasswordInput?.focus();
        return;
    }
    if (newPassword !== confirmPassword) {
        showToast('Passwords do not match.', 'error');
        confirmPasswordInput?.focus();
        return;
    }

    requestPasswordReset(email, newPassword);
}

async function requestPasswordReset(email, newPassword) {
    const submitBtn = document.querySelector('#password-form .save-button');
    setButtonBusy(submitBtn, true, 'Sending code…');

    try {
        const response = await fetch('/Authorization/RequestPasswordReset', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ Email: email }),
        });
        if (!response.ok) {
            let message = 'Failed to start password reset.';
            try { const data = await response.json(); message = data?.message || data?.Message || message; } catch {}
            throw new Error(message);
        }
        openVerifyModal();
        bindPasswordResetVerify(email, newPassword);
    } catch (err) {
        showToast(err.message || 'Could not request password reset.', 'error');
    } finally {
        setButtonBusy(submitBtn, false);
    }
}

let passwordResetVerifyHandler = null;
function bindPasswordResetVerify(email, newPassword) {
    const verifyButton = document.getElementById('verifyButton');
    const codeInput = document.getElementById('codeInput');
    const errorContainer = document.getElementById('errorContainer');
    const messageContainer = document.getElementById('messageContainer');
    if (!verifyButton || !codeInput) return;

    if (passwordResetVerifyHandler) {
        verifyButton.removeEventListener('click', passwordResetVerifyHandler);
    }

    passwordResetVerifyHandler = async () => {
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
            const response = await fetch('/Authorization/ConfirmPasswordReset', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ Email: email, Code: code, NewPassword: newPassword }),
            });
            if (!response.ok) {
                let message = 'Invalid code. Please try again.';
                try { const data = await response.json(); message = data?.message || data?.Message || message; } catch {}
                displayError(message);
                return;
            }
            displaySuccess('Password updated. Redirecting…');
            disableInput();
            setTimeout(() => { window.location.href = '/Account/Profile'; }, 1200);
        } catch (err) {
            displayError('Network error. Please try again.');
            console.error(err);
        } finally {
            setButtonBusy(verifyButton, false);
        }
    };

    verifyButton.addEventListener('click', passwordResetVerifyHandler);
}

/* ------------------------------------------------------------------------
   Cross-tab balance / status sync — keep header chip in sync after another
   tab buys, tops up, or signs out.
   ------------------------------------------------------------------------ */
function applyUserInfoToHeader(userInfo) {
    if (!userInfo) return;
    const balanceEl = document.getElementById('user-balance-value');
    if (balanceEl && userInfo.coins !== null && userInfo.coins !== undefined) {
        balanceEl.textContent = `${userInfo.coins}`;
    }

    const statusEl = document.getElementById('user-status-value');
    const statusChip = document.getElementById('user-status-chip');
    if (statusEl) {
        const isPremium = userInfo.premiumUntilUtc && new Date(userInfo.premiumUntilUtc) > new Date();
        statusEl.textContent = isPremium ? 'Premium' : 'Ordinary';
        if (statusChip) statusChip.classList.toggle('premium', !!isPremium);
    }
}

window.addEventListener('storage', (event) => {
    if (event.key !== 'quizanchos:user-info' || !event.newValue) return;
    try { applyUserInfoToHeader(JSON.parse(event.newValue)); } catch { /* ignore */ }
});

document.addEventListener('DOMContentLoaded', function () {
    initAvatarUpload();

    document.querySelectorAll('img.avatar, img.player-avatar, img.profile-icon').forEach(applyDefaultAvatarFallback);

    const navButtons = document.querySelectorAll('.nav-button');
    const viewSections = document.querySelectorAll('.view-section');

    if (navButtons.length > 0 && viewSections.length > 0) {
        const hashView = window.location.hash ? window.location.hash.substring(1) : '';
        const fallbackView = 'profile';
        const defaultView = document.getElementById(`${hashView}-view`) ? hashView : fallbackView;
        const defaultButton = document.querySelector(`.nav-button[onclick*="'${defaultView}'"]`)
            || document.querySelector(`.nav-button[onclick*="\"${defaultView}\""]`);
        switchView(defaultView, defaultButton, true);
    }

    document.getElementById('closeModal')?.addEventListener('click', closeVerifyModal);
});
