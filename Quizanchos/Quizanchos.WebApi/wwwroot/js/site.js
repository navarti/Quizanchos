document.addEventListener("DOMContentLoaded", function () {
    const burgerMenu = document.querySelector(".burger-menu");
    const navList = document.querySelector(".nav-list");
    
    burgerMenu?.addEventListener("click", function () {
        navList?.classList.toggle("active");
    });
    
    const form = document.getElementById('signupForm');
    const submitButton = document.getElementById('submitButton');
    
    if (form && submitButton) {
        form.addEventListener('submit', function (event) {
            event.preventDefault();
        });
        
        submitButton.removeEventListener('click', handleSubmit); 
        submitButton.addEventListener('click', handleSubmit);
    }
    
    const startQuestButton = document.getElementById("startQuestButton");
    
    if (startQuestButton) {
        startQuestButton.addEventListener("click", async function (event) {
            event.preventDefault();

            const form = document.getElementById('gameSettingsForm');
            const categoryId = form.getAttribute('data-category-id');

            if (!categoryId) {
                alert("Quiz Category ID is missing.");
                return;
            }

            const gameLevel = document.getElementById("gameLevel").value;
            const cardsCount = document.getElementById("cardsCount").value;
            const secondPerCard = document.getElementById("secondsPerCard").value;
            const optionCount = document.getElementById("optionCount").value;

            try {
                // Get current user ID from the body element
                const userId = document.body.getAttribute('data-user-id');
                if (!userId) {
                    alert("User information is missing. Please log in again.");
                    return;
                }

                // Create game using the new universal GameController
                const gameResponse = await quizClient.createQuizGame(
                    userId,
                    parseInt(cardsCount, 10),
                    {
                        gameLevel: parseInt(gameLevel, 10),
                        secondsPerCard: parseInt(secondPerCard, 10),
                        optionCount: parseInt(optionCount, 10),
                        categoryId: categoryId
                    }
                );

                if (gameResponse && gameResponse.gameId) {
                    // Navigate to the quiz game page
                    window.location.href = `/Quiz/${gameResponse.gameId}`;
                } else {
                    alert("Unexpected response format. Please try again.");
                }
            } catch (error) {
                console.error("Error:", error);
                alert(error.message || "An error occurred. Please try again later.");
            }
        });
    }
});

async function handleSubmit(event) {
    event.preventDefault();
    const verifyButton = document.getElementById("verifyButton");
    const codeInput = document.getElementById("codeInput");
    const errorContainer = document.getElementById("errorContainer");
    const messageContainer = document.getElementById("messageContainer");
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
                if (verifyButton) {
                    verifyButton.addEventListener("click", function () {
                        errorContainer.innerText = "";
                        messageContainer.innerHTML = "";
                        codeInput.classList.remove("error");

                        const code = codeInput.value.trim();
                        if (code.length !== 6) {
                            displayError("Code must be exactly 6 characters long.");
                            return;
                        }

                        fetch("/EmailConfirmation/ConfirmEmail", {
                            method: "POST",
                            headers: {
                                "Content-Type": "application/x-www-form-urlencoded",
                            },
                            body: new URLSearchParams({ code: code }),
                        })
                            .then((response) => {
                                if (response.ok) {
                                    displaySuccess("Code verified successfully!");
                                    codeInput.value = "";
                                    disableInput();
                                    setTimeout(() => {
                                        window.location.href = '/Account/Profile';
                                    }, 2000);
                                } else {
                                    displayError("Invalid code. Please try again.");
                                }
                            })
                            .catch((error) => {
                                displayError("An error occurred. Please try again later.");
                                console.error("Error during verification:", error);
                            });
                    });
                }
            } else if (responseData === 0) { 
                showModal('Sucssesful Message','Registration successful! Welcome to Quizanchos!', true);
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
        showModal('Notification', errorMessage);
    } catch (error) {
        console.error('Network error:', error);
        showModal('Notification','A network error occurred. Please try again later.');
    }
}
function redirectToCategory(category) {
    window.location.href = `/QuizCategories?filter=${category}`;
}

const verifyModal = document.getElementById('verifyModal');
if (verifyModal) {
    verifyModal.addEventListener('click', function(e) {
        if (e.target === this) {
            closeVerifyModal();
        }
    });
}

const modalContent = document.querySelector('#verifyModal .verify-modal-content');
if (modalContent) {
    modalContent.addEventListener('click', function(e) {
        e.stopPropagation();
    });
}










