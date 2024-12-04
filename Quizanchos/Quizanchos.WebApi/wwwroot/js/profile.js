const avatarInput = document.getElementById('avatar-input');
const avatarImg = document.getElementById('avatar');

const uploadImage = async (file) => {
    const cloudName = "dpsdhr3fy"; 
    const uploadPreset = "avatar_upload"; 

    const formData = new FormData();
    formData.append("file", file);
    formData.append("upload_preset", uploadPreset);
    console.log(file);
    try {
        const response = await fetch(`https://api.cloudinary.com/v1_1/${cloudName}/image/upload`, {
            method: "POST",
            body: formData
        });

        if (response.ok) {
            const data = await response.json();
            console.log("Uploaded Image URL:", data.secure_url); 
            return data.secure_url;
        } else {
            throw new Error(`Image upload failed: ${response.status}`);
        }
    } catch (error) {
        console.error("Error uploading image:", error);
    }
};


function toggleEdit(fieldId) {
    const field = document.getElementById(fieldId);
    field.readOnly = !field.readOnly;
    if (!field.readOnly) {
        field.focus();
    }
}

async function handleSubmit(event) {
    event.preventDefault();

    const formData = {
        username: document.getElementById('username').value,
        email: document.getElementById('email').value,
        avatarUrl: document.getElementById('avatar').src,
    };

    console.log('Submitting profile update:', formData);
    
    try {
        const response = await fetch('/UserProfile/UpdateAvatarUrl', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ avatarUrl: formData.avatarUrl }),
        });

        if (response.ok) {
            alert('Profile updated successfully!');
        } else {
            console.error('Failed to update profile:', response.statusText);
        }
    } catch (error) {
        console.error('Error:', error);
    }

    // Reset edit states
    document.getElementById('username').readOnly = true;
}
