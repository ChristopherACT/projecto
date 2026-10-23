// =====================================================
// auth.js — logica de la pantalla de login/registro
// =====================================================

document.addEventListener("DOMContentLoaded", () => {
    // Si ya hay sesion activa, no tiene caso quedarse en esta pantalla
    if (API.isLoggedIn()) {
        window.location.href = "feed.html";
        return;
    }

    const tabLogin = document.getElementById("tab-login");
    const tabRegister = document.getElementById("tab-register");
    const panelLogin = document.getElementById("panel-login");
    const panelRegister = document.getElementById("panel-register");

    function showTab(tab) {
        const isLogin = tab === "login";
        tabLogin.classList.toggle("active", isLogin);
        tabRegister.classList.toggle("active", !isLogin);
        panelLogin.classList.toggle("active", isLogin);
        panelRegister.classList.toggle("active", !isLogin);
        clearAlerts();
    }

    tabLogin.addEventListener("click", () => showTab("login"));
    tabRegister.addEventListener("click", () => showTab("register"));

    function clearAlerts() {
        document.querySelectorAll(".alert").forEach(el => {
            el.className = "alert";
            el.textContent = "";
        });
    }

    function showError(alertEl, message) {
        // message viene de nuestro propio codigo o de la API como texto plano,
        // usamos textContent (nunca innerHTML) asi que es seguro por diseno.
        alertEl.textContent = message;
        alertEl.className = "alert error";
    }

    function setLoading(button, loading) {
        button.disabled = loading;
        button.textContent = loading ? "Un momento..." : button.dataset.label;
    }

    // ---------- LOGIN ----------
    const loginForm = document.getElementById("login-form");
    const loginAlert = document.getElementById("login-alert");
    const loginBtn = document.getElementById("login-btn");
    loginBtn.dataset.label = loginBtn.textContent;

    loginForm.addEventListener("submit", async (e) => {
        e.preventDefault();
        clearAlerts();

        const email = document.getElementById("login-email").value.trim();
        const password = document.getElementById("login-password").value;

        setLoading(loginBtn, true);
        try {
            await API.login({ email, password });
            window.location.href = "feed.html";
        } catch (err) {
            showError(loginAlert, err.message || "No se pudo iniciar sesion.");
        } finally {
            setLoading(loginBtn, false);
        }
    });

    // ---------- REGISTRO ----------
    const registerForm = document.getElementById("register-form");
    const registerAlert = document.getElementById("register-alert");
    const registerBtn = document.getElementById("register-btn");
    registerBtn.dataset.label = registerBtn.textContent;

    registerForm.addEventListener("submit", async (e) => {
        e.preventDefault();
        clearAlerts();

        const username = document.getElementById("register-username").value.trim();
        const email = document.getElementById("register-email").value.trim();
        const password = document.getElementById("register-password").value;
        const passwordConfirm = document.getElementById("register-password-confirm").value;

        // Validaciones basicas en el cliente (la API tambien valida, esto es solo UX)
        if (username.length < 3) {
            return showError(registerAlert, "El nombre de usuario debe tener al menos 3 caracteres.");
        }
        if (password.length < 6) {
            return showError(registerAlert, "La contrasena debe tener al menos 6 caracteres.");
        }
        if (password !== passwordConfirm) {
            return showError(registerAlert, "Las contrasenas no coinciden.");
        }

        setLoading(registerBtn, true);
        try {
            await API.register({ username, email, password });
            window.location.href = "feed.html";
        } catch (err) {
            showError(registerAlert, err.message || "No se pudo crear la cuenta.");
        } finally {
            setLoading(registerBtn, false);
        }
    });
});