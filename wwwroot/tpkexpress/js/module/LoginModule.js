function setupFormValidation(form, messages, onValid) {
    if (!form) return;

    const setError = (input, message) => {
        const field = input.closest(".login-field");
        if (!field) return;
        const error = field.querySelector(".login-error");
        if (message) {
            field.classList.add("is-invalid");
            input.setAttribute("aria-invalid", "true");
            if (error) error.textContent = message;
        } else {
            field.classList.remove("is-invalid");
            input.removeAttribute("aria-invalid");
        }
    };

    const inputs = form.querySelectorAll("input[required]");

    form.addEventListener("submit", (e) => {
        e.preventDefault();
        let firstInvalid = null;
        inputs.forEach((input) => {
            if (!input.value.trim()) {
                setError(input, messages[input.id] || "Vui lòng nhập thông tin");
                if (!firstInvalid) firstInvalid = input;
            }
        });
        if (firstInvalid) {
            firstInvalid.focus();
            return;
        }
        if (typeof onValid === "function") onValid();
    });

    inputs.forEach((input) => {
        input.addEventListener("input", () => setError(input, ""));
    });
}

function createModal(el) {
    if (!el || typeof bootstrap === "undefined") return null;
    return new bootstrap.Modal(el, { backdrop: "static", keyboard: true });
}

function signIn() {
    let userName = $('#login-account').val();
    if (userName.trim() === "") {
        showToast("Username là bắt buộc", 3, false);
        return;
    }
    let password = $('#login-password').val();
    if (password.trim() === "") {
        showToast("Password là bắt buộc", 3, false);
        return;
    }
    disableButton();
    // Gửi yêu cầu AJAX
    $.ajax({
        url: '/Home/SignIn', // URL tới action
        type: 'POST',
        data: { username: userName, password: password },
        success: function (response) {
            if (response.Data == 2) {
                window.location.href = "/home";
            } else {
                window.location.href = "/dashboard";
            }
        },
        error: function (response) {
            showToast(response.responseJSON.Message, response.responseJSON.Type, false);
        },
        complete: function () {
            enableButton();
        }
    });
}

function forgotPassword() {
    let email = $('#forgot-password-email').val();
    if (email.trim() === "") {
        showToast("Email là bắt buộc", 3, false);
        return;
    }
    disableButton();
    // Gửi yêu cầu AJAX
    $.ajax({
        url: '/Home/ForgotPassword', // URL tới action
        type: 'POST',
        data: { email: email },
        success: function (response) {
            showToast("Vui lòng kiểm tra Email", 4, false);
        },
        error: function (response) {
            showToast(response.responseJSON.Message, response.responseJSON.Type, false);
        },
        complete: function () {
            enableButton();
        }
    });
}

function signUp() {
    let username = $('#signup-username').val().trim();
    let email = $('#signup-email').val().trim();
    let phone = $('#signup-phone').val().trim();
    let address = $('#signup-address').val().trim();
    let fullName = $('#signup-fullname').val().trim();
    let password = $('#signup-password').val().trim();
    let confirmPassword = $('#signup-confirm-password').val().trim();

    if (username === "" || email === "" || phone === "" || address === "" || fullName === "" || password === "" || confirmPassword === "") {
        showToast("Vui lòng điền thông tin còn thiếu", 2, false);
        return;
    }
    if (password !== confirmPassword) {
        showToast("Mật khẩu nhập lại không khớp", 2, false);
        return;
    }

    const createData = {
        Username: username,
        Password: password,
        Email: email,
        Phone: phone,
        Address: address,
        FullName: fullName
    }
    disableButton();
    $.ajax({
        url: '/Home/SignUp',
        type: 'POST',
        data: { Request: createData },
        success: function (response) {
            showToast("Đăng ký thành công, vui lòng đăng nhập", 4, true);

            const signupEl = document.querySelector('#modal-signup');
            const loginEl = document.querySelector('#modal-login');
            if (signupEl && loginEl && typeof bootstrap !== "undefined") {
                const signupModal = bootstrap.Modal.getOrCreateInstance(signupEl);
                const loginModal = bootstrap.Modal.getOrCreateInstance(loginEl);
                signupEl.addEventListener("hidden.bs.modal", () => loginModal.show(), { once: true });
                signupModal.hide();
            }
        },
        error: function (response) {
            showToast(response.responseJSON.Message, response.responseJSON.Type, false);
        },
        complete: function () {
            enableButton();
        }
    });
}

export default function LoginModule() {
    if (typeof bootstrap === "undefined") return;

    const loginEl = document.querySelector("#modal-login");
    if (!loginEl) return;

    const loginModal = createModal(loginEl);
    const forgotEl = document.querySelector("#modal-forgot-password");
    const forgotModal = createModal(forgotEl);
    const signupEl = document.querySelector("#modal-signup");
    const signupModal = createModal(signupEl);

    // Chỉ mở popup đăng nhập khi click #login-btn
    const loginBtn = document.querySelector("#login-btn");
    if (loginBtn) {
        loginBtn.addEventListener("click", (e) => {
            e.preventDefault();
            loginModal.show();
        });
    }

    // Đóng popup đăng nhập thì chuyển về trang chỉ định, trừ khi đang chuyển sang modal khác
    let suppressLoginRedirect = false;
    loginEl.addEventListener("hidden.bs.modal", () => {
        if (suppressLoginRedirect) {
            suppressLoginRedirect = false;
            return;
        }
        if (loginEl.dataset.closeRedirect) {
            window.location.href = loginEl.dataset.closeRedirect;
        }
    });

    const switchModal = (fromEl, fromModal, toModal) => {
        if (!fromModal || !toModal) return;
        if (fromEl === loginEl) suppressLoginRedirect = true;
        fromEl.addEventListener("hidden.bs.modal", () => toModal.show(), { once: true });
        fromModal.hide();
    };

    setupFormValidation(loginEl.querySelector("#login-form"), {
        "login-account": "Vui lòng nhập tài khoản",
        "login-password": "Vui lòng nhập mật khẩu",
    }, signIn);

    // Modal quên mật khẩu
    if (forgotEl && forgotModal) {
        const forgotLink = loginEl.querySelector("#login-forgot-link");
        if (forgotLink) {
            forgotLink.addEventListener("click", (e) => {
                e.preventDefault();
                switchModal(loginEl, loginModal, forgotModal);
            });
        }

        const backBtn = forgotEl.querySelector("#forgot-password-back");
        if (backBtn) {
            backBtn.addEventListener("click", () => switchModal(forgotEl, forgotModal, loginModal));
        }

        setupFormValidation(forgotEl.querySelector("#forgot-password-form"), {
            "forgot-password-email": "Vui lòng nhập email",
        }, forgotPassword);
    }

    // Modal đăng ký
    if (signupEl && signupModal) {
        document.querySelectorAll(".js-open-signup").forEach((trigger) => {
            trigger.addEventListener("click", (e) => {
                e.preventDefault();
                if (forgotEl && forgotEl.classList.contains("show")) {
                    switchModal(forgotEl, forgotModal, signupModal);
                } else if (loginEl.classList.contains("show")) {
                    switchModal(loginEl, loginModal, signupModal);
                } else {
                    signupModal.show();
                }
            });
        });

        signupEl.querySelectorAll(".js-open-login").forEach((trigger) => {
            trigger.addEventListener("click", (e) => {
                e.preventDefault();
                switchModal(signupEl, signupModal, loginModal);
            });
        });

        setupFormValidation(signupEl.querySelector("#signup-form"), {
            "signup-fullname": "Vui lòng nhập họ và tên",
            "signup-email": "Vui lòng nhập email",
            "signup-phone": "Vui lòng nhập số điện thoại",
            "signup-address": "Vui lòng nhập địa chỉ",
            "signup-username": "Vui lòng nhập tên đăng nhập",
            "signup-password": "Vui lòng nhập mật khẩu",
            "signup-confirm-password": "Vui lòng nhập lại mật khẩu",
        }, signUp);
    }
}
