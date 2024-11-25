document.getElementById('signinButton').addEventListener('click', async function (event) {
    event.preventDefault(); 

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

    const data = {
        email: email.value.trim(),
        password: password.value.trim(),
    };

    try {
        const response = await fetch('/Authorization/SignIn', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(data),
        });

        if (response.ok) {
            const responseData = await response.json();
            if (responseData.redirectUrl) {
                showModal('Sign in successful! Welcome back!', true);
                setTimeout(() => {
                    window.location.href = responseData.redirectUrl;
                }, 2000); // 2 секунды до редиректа
            } else {
                showModal('Sign in successful! Welcome back!', true);
            }
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
