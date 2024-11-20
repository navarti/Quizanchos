// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.addEventListener("DOMContentLoaded", function () {
    const burgerMenu = document.querySelector(".burger-menu");
    const navList = document.querySelector(".nav-list");

    burgerMenu.addEventListener("click", function () {
        navList.classList.toggle("active");
    });
});

document.getElementById('submitButton').addEventListener('click', async function () {
    const email = document.getElementById('email').value.trim();
    const password = document.getElementById('password').value.trim();
    const repeatPassword = document.getElementById('repeatPassword').value.trim();
    
    if (!validateEmail(email)) {
        alert('Please enter a valid email address.');
        return;
    }
    
    if (password.length < 8) {
        alert('Password must be at least 8 characters long.');
        return;
    }

    if (password !== repeatPassword) {
        alert('Passwords do not match. Please try again.');
        return;
    }
    
    const data = {
        email: email,
        password: password
    };

    try {
        const response = await fetch('/Authorization/SignUp', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(data)
        });

        if (response.ok) {
            alert('Registration successful');
        } else {
            const errorData = await response.json();
            alert(`Error: ${errorData.Message || 'Something went wrong'}`);
        }
    } catch (error) {
        console.error('Error:', error);
        alert('An unexpected error occurred. Please try again later.');
    }
});

function validateEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
}
