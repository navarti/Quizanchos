const avatarInput = document.getElementById('avatar-input');
const avatarImg = document.getElementById('avatar');

// Function to upload an image
const uploadImage = async (file) => {
    const formData = new FormData();
    formData.append("formFile", file); // Ensure this matches the server-side parameter name

    try {
        const response = await fetch('/UserProfile/UpdateAvatar', {
            method: 'POST',
            body: formData, // Automatically sets multipart/form-data
        });

        if (response.ok) {
            const data = await response.json();
            console.log("Image uploaded successfully:", data);
            return data; // Return the server's response (e.g., new avatar URL)
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

// Event listener for avatar file input change
avatarInput.addEventListener('change', async (event) => {
    const file = event.target.files[0];
    if (file) {
        // Update avatar preview immediately
        avatarImg.src = URL.createObjectURL(file);

        // Upload the image to the server
        const uploadResult = await uploadImage(file);
        if (uploadResult && uploadResult.newAvatarUrl) {
            avatarImg.src = uploadResult.newAvatarUrl; // Update avatar with the server URL
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