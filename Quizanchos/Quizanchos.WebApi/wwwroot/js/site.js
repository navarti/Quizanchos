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
            const responseData = await response.json();

            if (responseData === 1) {
                openVerifyModal();
                const verifyForm = document.getElementById('verify-form');
                if (verifyForm) {
                    verifyForm.addEventListener('submit', function(e) {
                        e.preventDefault();
                        const codeInput = verifyForm.querySelector('input[type="text"]');
                        if (codeInput && codeInput.value.length === 6) {
                            const code = codeInput.value;

                            fetch('/EmailConfirmation/ConfirmEmail', {
                                method: 'POST',
                                headers: {
                                    'Content-Type': 'application/json',
                                },
                                body: JSON.stringify({ code: code })
                            })
                                .then(response => {
                                    if (!response.ok) {
                                        throw new Error('Network response was not ok');
                                    }
                                    return response.json();
                                })
                                .then(data => {
                                    if (data.success) {
                                        alert('Code verified successfully!');
                                        closeVerifyModal();
                                    } else {
                                        alert('Invalid code. Please try again.');
                                    }
                                })
                                .catch((error) => {
                                    console.error('Error:', error);
                                    alert('An error occurred. Please try again later.');
                                });
                        } else {
                            alert('Please enter a valid 6-digit code');
                        }
                    });
                }

                const resendBtn = document.getElementById('verify-resend-btn');
                if (resendBtn) {
                    resendBtn.addEventListener('click', function() {
                        fetch('/api/resend-code', {
                            method: 'POST',
                            headers: {
                                'Content-Type': 'application/json',
                            },
                            body: JSON.stringify({ userId: 'user_id_here' }) 
                        })
                            .then(response => {
                                if (!response.ok) {
                                    throw new Error('Network response was not ok');
                                }
                                return response.json();
                            })
                            .then(data => {
                                if (data.success) {
                                    alert('A new verification code has been sent to your email.');
                                } else {
                                    alert('Failed to resend code. Please try again.');
                                }
                            })
                            .catch((error) => {
                                console.error('Error:', error);
                                alert('An error occurred. Please try again later.');
                            });
                    });
                }
            } else if (responseData === 0) { 
                showModal('Registration successful! Welcome to Quizanchos!', true);
                setTimeout(() => {
                    window.location.href = "/";
                }, 2000);
            } else { 
                showModal('Unexpected response from the server. Please try again.');
            }
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
function redirectToCategory(category) {
    window.location.href = `/QuizCategories?filter=${category}`;
}

document.getElementById('verifyModal').addEventListener('click', function(e) {
    if (e.target === this) {
        closeVerifyModal();
    }
});

const modalContent = document.querySelector('#verifyModal .verify-modal-content');
if (modalContent) {
    modalContent.addEventListener('click', function(e) {
        e.stopPropagation();
    });
}
document.addEventListener("DOMContentLoaded", function () {
    const startQuestButton = document.getElementById("startQuestButton");

    startQuestButton.addEventListener("click", async function (event) {
        event.preventDefault();

        const categoryId = this.getAttribute("data-category-id");

        if (!categoryId) {
            alert("Quiz Category ID is missing.");
            return;
        }

        const gameLevel = document.getElementById("gameLevel").value;
        const cardsCount = document.getElementById("cardsCount").value;
        const secondPerCard = document.getElementById("secondsPerCard").value;
        const optionCount = document.getElementById("optionCount").value;

        const data = {
            quizCategoryId: categoryId,
            gameLevel: parseInt(gameLevel, 10),
            cardsCount: parseInt(cardsCount, 10),
            SecondPerCard: parseInt(secondPerCard, 10),
            optionCount: parseInt(optionCount, 10),
        };

        try {
            const response = await fetch("/SingleGameSession/Create", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify(data),
            });

            if (response.ok) {
                const responseData = await response.json();
                if (responseData && responseData.id) {
                    window.location.href = `/Quiz/${responseData.id}`;
                } else {
                    alert("Unexpected response format. Please try again.");
                }
            } else {
                const errorData = await response.json();
                alert(errorData.message || "An error occurred. Please try again.");
            }
        } catch (error) {
            console.error("Error:", error);
            alert("A network error occurred. Please try again later.");
        }
    });
});





