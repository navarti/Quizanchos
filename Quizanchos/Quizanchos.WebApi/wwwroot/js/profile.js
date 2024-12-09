const avatarInput = document.getElementById('avatar-input');
const avatarImg = document.getElementById('avatar');
const uploadImage = async (file) => {
    const formData = new FormData();
    formData.append('formFile', file);

    try {
        const response = await fetch('/UserProfile/UpdateAvatar', {
            method: 'POST',
            body: formData,
        });

        if (response.ok) {
            const data = await response.json();
            alert('Avatar updated successfully!');
            return data;
        } else {
            const errorMessage = await response.text();
            alert(`Error: ${errorMessage}`);
        }
    } catch (error) {
        console.error('Error uploading image:', error);
        alert('Failed to upload avatar. Please try again.');
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

function toggleEdit(fieldId) {
    const field = document.getElementById(fieldId);
    const editButton = document.getElementById('edit-username-btn');

    if (field.readOnly) {
        field.readOnly = false; // Make field editable
        field.focus(); // Focus the input field
        editButton.textContent = "Save"; // Change button text to "Save"
    } else {
        field.readOnly = true; // Make field read-only
        console.log('Updated username:', field.value); // Log updated value
        editButton.textContent = "Edit"; // Revert button text to "Edit"
    }
}

function toggleEdit(fieldId) {
    const field = document.getElementById(fieldId);
    const editButton = document.getElementById('edit-username-btn');

    if (editButton.textContent === "Edit") {
        field.readOnly = false;
        field.focus();
        editButton.textContent = "Save";
    } else {
        field.readOnly = true;
        editButton.textContent = "Edit";
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
document.addEventListener('DOMContentLoaded', function () {
    const avatarInput = document.getElementById('avatar-input');
    const avatarImg = document.getElementById('avatar');
    const editUsernameBtn = document.getElementById('edit-username-btn');
    const usernameField = document.getElementById('username');
    const saveChangesBtn = document.querySelector('.save-button');
    const logoutBtn = document.getElementById('logout-btn');
    const defaultView = 'profile';
    const defaultButton = document.querySelector(`.nav-button[onclick*="${defaultView}"]`);
    if (defaultButton) {
        defaultButton.classList.add('active');
    }
    switchView(defaultView, defaultButton);

    // Logout Functionality
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
            .then(() => {
                window.location.reload();
            })
            .catch((error) => {
                console.error('Error during logout:', error);
                alert('Logout failed. Please try again.');
            });
    }
});

function switchView(view, button) {
    const buttons = document.querySelectorAll('.nav-button');
    buttons.forEach(btn => btn.classList.remove('active'));
    if (button) {
        button.classList.add('active');
    }
    
    const views = document.querySelectorAll('.view-section');
    views.forEach(v => v.style.display = 'none');
    
    const selectedView = document.getElementById(`${view}-view`);
    if (selectedView) {
        selectedView.style.display = 'block';
    }
    
    history.pushState(null, '', `#${view}`);
}

function handlePasswordSubmit(event) {
    event.preventDefault();
    const newPassword = document.getElementById('new-password').value;
    const confirmPassword = document.getElementById('confirm-password').value;
    const emailInput = document.getElementById('email');
    const email = emailInput.value.trim();
    if (newPassword !== confirmPassword) {
        alert('Passwords do not match');
        return;
    }

    if (newPassword.length < 8) {
        alert('Password must be at least 8 characters long');
        return;
    }
    
    changePassword(email,newPassword)
        .then(() => {
            alert('Password changed successfully');
            document.getElementById('password-form').reset();
            switchView('profile');
        })
        .catch(error => {
            alert('Failed to change password: ' + error.message);
        });
}

async function changePassword(email, newPassword) {
    try {
        const response = await fetch('/Authorization/UpdatePassword', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ Email: email, NewPassword: newPassword }),
        });

        if (!response.ok) {
            const errorData = await response.json();
            throw new Error(errorData.message || 'Failed to change password');
        }

        return await response.json(); 
    } catch (error) {
        console.error('Error changing password:', error);
        throw error; 
    }
}