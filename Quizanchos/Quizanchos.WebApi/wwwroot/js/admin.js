document.getElementById('registerAdminButton').addEventListener('click', async function (event) {

    const email = document.getElementById('registerAdminEmail');
    const password = document.getElementById('registerAdminPassword');
    const confirmPassword = document.getElementById('registerAdminConfirmPassword');

    clearErrors();

    let isValid = true;
    
    if (!validateEmail(email.value.trim())) {
        showError('registerAdminEmailError', 'Please enter a valid email address.');
        addErrorClass(email);
        isValid = false;
    }
    
    if (!password.value.trim()) {
        showError('registerAdminPasswordError', 'Password cannot be empty.');
        addErrorClass(password);
        isValid = false;
    }
    
    if (password.value.trim() !== confirmPassword.value.trim()) {
        showError('registerAdminConfirmPasswordError', 'Passwords do not match.');
        addErrorClass(confirmPassword);
        isValid = false;
    }

    if (!isValid) return;

    const data = {
        email: email.value.trim(),
        password: password.value.trim(),
    };

    try {
        const response = await fetch('/Authorization/RegisterAdmin', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(data),
        });

        if (response.ok) {
            showModal('Admin registration successful! Redirecting...', true);
            setTimeout(() => {
                window.location.href = "/Admin";
            }, 2000);
        } else {
            const errorData = await response.json();
            if (errorData.Message) {
                showModal(errorData.Message);
            } else {
                showModal('An unexpected error occurred. Please try again.');
            }
        }
    } catch (error) {
        console.error('Error:', error);
        showModal('A network error occurred. Please try again later.');
    }
});