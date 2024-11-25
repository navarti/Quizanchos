document.addEventListener("DOMContentLoaded", function () {
    const burgerMenu = document.querySelector(".burger-menu");
    const navList = document.querySelector(".nav-list");
    
    burgerMenu?.addEventListener("click", function () {
        navList?.classList.toggle("active");
    });
    
    const form = document.getElementById('signupForm');
    const submitButton = document.getElementById('submitButton');
    
    form.addEventListener('submit', function (event) {
        event.preventDefault();
    });
    
    submitButton.removeEventListener('click', handleSubmit); 
    submitButton.addEventListener('click', handleSubmit);
});

async function handleSubmit(event) {
    event.preventDefault(); 

    const email = document.getElementById('email');
    const password = document.getElementById('password');
    const repeatPassword = document.getElementById('repeatPassword');

    clearErrors();

    let isValid = true;

    if (!validateEmail(email.value.trim())) {
        showError('emailError', 'Please enter a valid email address.');
        addErrorClass(email);
        isValid = false;
    }

    if (password.value.trim().length < 8) {
        showError('passwordError', 'Password must be at least 8 characters long.');
        addErrorClass(password);
        isValid = false;
    }

    if (password.value.trim() !== repeatPassword.value.trim()) {
        showError('repeatPasswordError', 'Passwords do not match.');
        addErrorClass(repeatPassword);
        isValid = false;
    }

    if (!isValid) return;

    const data = {
        email: email.value.trim(),
        password: password.value.trim(),
    };

    try {
        const response = await fetch('/Authorization/SignUp', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(data),
        });

        if (response.ok) {
            showModal('Registration successful! Welcome to Quizanchos!', true);
            setTimeout(() => {
                window.location.href = "/";
            }, 2000);
            return;
        }
        
        const responseData = await response.json(); 
        const errorMessage = responseData.Message || 'An unexpected error occurred.';
        showModal(errorMessage);
    } catch (error) {
        console.error('Network error:', error);
        showModal('A network error occurred. Please try again later.');
    }
}

// document.addEventListener("DOMContentLoaded", function () {
//     document.querySelectorAll(".quiz-link").forEach((link) => {
//         link.addEventListener("click", async function (event) {
//             event.preventDefault(); // Prevent default link behavior
//
//             const categoryId = this.getAttribute("data-category-id");
//
//             if (!categoryId) {
//                 alert("Category ID is missing.");
//                 return;
//             }
//            
//             this.setAttribute("disabled", "true");
//
//             try {
//                 const data = {
//                     quizCategoryId: categoryId,
//                 };
//
//                 const response = await fetch("/Quiz/Setup/", {
//                     method: "POST",
//                     headers: {
//                         "Content-Type": "application/json",
//                     },
//                     body: JSON.stringify(data),
//                 });
//
//                 if (response.ok) {
//                     const responseData = await response.json();
//                     if (responseData && responseData.id) {
//                         window.location.href = `/Quiz/${responseData.id}`;
//                     } else {
//                         alert("Unexpected response format. Please try again.");
//                     }
//                 } else {
//                     const errorData = await response.json();
//                     alert(errorData.message || "An error occurred. Please try again.");
//                 }
//             } catch (error) {
//                 console.error("Error:", error);
//                 alert("A network error occurred. Please try again later.");
//             } finally {
//                 // Re-enable the link after the operation is complete
//                 this.removeAttribute("disabled");
//             }
//         });
//     });
// });


