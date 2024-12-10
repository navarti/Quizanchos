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




