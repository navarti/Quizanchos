document.addEventListener('DOMContentLoaded', function() {
    const avatarInput = document.getElementById('avatar-input');
    const avatarImg = document.getElementById('avatar');
    const editUsernameBtn = document.getElementById('edit-username-btn');
    const usernameField = document.getElementById('username');
    const saveChangesBtn = document.querySelector('.save-button');
    const logoutBtn = document.getElementById('logout-btn');
    const changePasswordBtn = document.querySelector('.change-password-button');
    const changePasswordModal = document.getElementById('change-password-modal');
    const closeModalBtn = changePasswordModal.querySelector('.close');
    const changePasswordForm = document.getElementById('change-password-form');

    // Existing avatar upload logic
    const uploadImage = async (file) => {
        const formData = new FormData();
        formData.append("formFile", file);

        try {
            const response = await fetch('/UserProfile/UpdateAvatar', {
                method: 'POST',
                body: formData,
            });

            if (response.ok) {
                const data = await response.json();
                console.log("Image uploaded successfully:", data);
                return data;
            } else {
                console.error(`Image upload failed with status ${response.status}`);
                const errorMessage = await response.text();
                console.error("Error message:", errorMessage);
            }
        } catch (error) {
            console.error("Error uploading image:", error);
        }
        return null;
    };

    avatarInput.addEventListener('change', async (event) => {
        const file = event.target.files[0];
        if (file) {
            avatarImg.src = URL.createObjectURL(file);
            const uploadResult = await uploadImage(file);
            if (uploadResult && uploadResult.newAvatarUrl) {
                avatarImg.src = uploadResult.newAvatarUrl;
                console.log("Avatar updated to:", uploadResult.newAvatarUrl);
            } else {
                console.error("Failed to update avatar on the server.");
            }
        }
    });

    // Existing username edit logic
    function toggleEdit() {
        if (editUsernameBtn.textContent === "Edit") {
            usernameField.readOnly = false;
            usernameField.focus();
            editUsernameBtn.textContent = "Save";
        } else {
            usernameField.readOnly = true;
            editUsernameBtn.textContent = "Edit";
            saveChanges('username');
        }
    }

    async function saveChanges(fieldId) {
        const field = document.getElementById(fieldId);
        const value = field.value;

        console.log("Saving value:", value);

        try {
            const response = await fetch('/UserProfile/UpdateNickname', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                },
                body: new URLSearchParams({ nickname: value }),
            });

            if (response.ok) {
                alert('Changes saved successfully!');
            } else {
                alert('Failed to save changes.');
            }
        } catch (error) {
            console.error("Error saving changes:", error);
        }
    }

    // Existing logout logic
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

    // New password change logic
    function openChangePasswordModal() {
        changePasswordModal.style.display = 'block';
    }

    function closeChangePasswordModal() {
        changePasswordModal.style.display = 'none';
        changePasswordForm.reset();
    }

    async function changePassword(newPassword, emailCode) {
        try {
            const response = await fetch('/UserProfile/ChangePassword', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ newPassword, emailCode }),
            });

            if (response.ok) {
                alert('Password changed successfully!');
                closeChangePasswordModal();
            } else {
                const errorData = await response.json();
                alert(errorData.message || 'Failed to change password. Please try again.');
            }
        } catch (error) {
            console.error("Error changing password:", error);
            alert('An error occurred. Please try again.');
        }
    }

    // Event listeners
    editUsernameBtn.addEventListener('click', toggleEdit);
    saveChangesBtn.addEventListener('click', () => saveChanges('username'));
    logoutBtn.addEventListener('click', logout);
    changePasswordBtn.addEventListener('click', openChangePasswordModal);
    closeModalBtn.addEventListener('click', closeChangePasswordModal);

    changePasswordForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        const newPassword = document.getElementById('new-password').value;
        const confirmPassword = document.getElementById('confirm-password').value;
        const emailCode = document.getElementById('email-code').value;

        if (newPassword !== confirmPassword) {
            alert('Passwords do not match');
            return;
        }

        if (newPassword.length < 8) {
            alert('Password must be at least 8 characters long');
            return;
        }

        if (!/^\d{6}$/.test(emailCode)) {
            alert('Email code must be 6 digits');
            return;
        }

        await changePassword(newPassword, emailCode);
    });
    
    window.addEventListener('click', (event) => {
        if (event.target === changePasswordModal) {
            closeChangePasswordModal();
        }
    });
});

