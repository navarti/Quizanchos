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
        console.log('Failed to upload avatar. Please try again.');
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

function deleteCookie(name) {
    document.cookie = `${name}=; Max-Age=0; path=/; domain=${window.location.hostname}`;
}

function logout() {
    showModal(
        'Confirm Logout',
        'Are you sure you want to log out?',
        false,
        [
            {
                text: 'Yes',
                class: 'btn-yes',
                onClick: () => {
                    deleteCookie('Identity.External');
                    deleteCookie('QAuth');
                },
            },
            {
                text: 'No',
                class: 'btn-no',
                onClick: () => {
                    document.getElementById('errorModal').style.display = 'none';
                },
            },
        ]
    );
}
let elements;
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

    elements.closeModalButton?.addEventListener('click', closeVerifyModal);
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

function changePassword(email, newPassword) {
    const resendButton = document.getElementById('resendButton');
    const verifyButton = document.getElementById('verifyButton');
    const messageContainer = document.getElementById('messageContainer');
    const codeInput = document.getElementById('codeInput');
    const errorContainer = document.getElementById('errorContainer');

    fetch('/Authorization/UpdatePassword', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({ Email: email, NewPassword: newPassword }),
    })
        .then((response) => {
            if (response.ok) {
                openVerifyModal();

                if (verifyButton) {
                    verifyButton.addEventListener("click", function () {
                        if (errorContainer) errorContainer.innerText = "";
                        if (messageContainer) messageContainer.innerHTML = "";
                        if (codeInput) codeInput.classList.remove("error");

                        const code = codeInput ? codeInput.value.trim() : "";
                        if (code.length !== 6) {
                            if (errorContainer) errorContainer.innerText = "Code must be exactly 6 characters long.";
                            if (codeInput) codeInput.classList.add("error");
                            return;
                        }

                        fetch("/EmailConfirmation/ConfirmPassword", {
                            method: "POST",
                            headers: {
                                "Content-Type": "application/x-www-form-urlencoded",
                            },
                            body: new URLSearchParams({ code: code }),
                        })
                            .then((response) => {
                                if (response.ok) {
                                    if (messageContainer) {
                                        messageContainer.innerHTML = `<span class="success-message">Password is changed! Code verified successfully!</span>`;
                                    }
                                    if (codeInput) {
                                        codeInput.value = "";
                                        codeInput.disabled = true;
                                        codeInput.classList.add("disabled");
                                    }
                                    setTimeout(() => {
                                        window.location.href = '/Account/Profile';
                                    }, 2000);
                                } else {
                                    if (errorContainer) {
                                        errorContainer.innerText = "Invalid code. Please try again.";
                                    }
                                }
                            })
                            .catch((error) => {
                                if (errorContainer) {
                                    errorContainer.innerText = "An error occurred. Please try again later.";
                                }
                                console.error("Error during verification:", error);
                            });
                    });
                }
            } else {
                response.json().then((errorData) => {
                    throw new Error(errorData.message || 'Failed to change password');
                });
            }
        })
        .catch((error) => {
            console.error('Error changing password:', error);
            if (errorContainer) {
                errorContainer.innerText = 'An unexpected error occurred. Please try again later.';
            }
        });
}
