// =====================================================
// api.js — capa central de comunicacion con el backend
// y utilidades de seguridad. Todas las paginas del sitio
// deben cargar este archivo ANTES que cualquier otro JS.
// =====================================================

const API = (() => {
    // Cambia esto si tu API corre en otro puerto/dominio
    const BASE_URL = "https://localhost:7035/api";

    const TOKEN_KEY = "fg_token";
    const USER_KEY = "fg_user";

    // ---------- Manejo de sesion ----------
    function getToken() {
        return localStorage.getItem(TOKEN_KEY);
    }

    function getCurrentUser() {
        const raw = localStorage.getItem(USER_KEY);
        return raw ? JSON.parse(raw) : null;
    }

    function setSession(token, user) {
        localStorage.setItem(TOKEN_KEY, token);
        localStorage.setItem(USER_KEY, JSON.stringify(user));
    }

    function clearSession() {
        localStorage.removeItem(TOKEN_KEY);
        localStorage.removeItem(USER_KEY);
    }

    function isLoggedIn() {
        return !!getToken();
    }

    // ---------- Llamadas HTTP ----------
    // path ej: "/users/register" ; body es un objeto JS (se convierte a JSON)
    async function request(path, { method = "GET", body = null, auth = false } = {}) {
        const headers = { "Content-Type": "application/json" };

        if (auth) {
            const token = getToken();
            if (!token) throw new ApiError("No hay sesion iniciada.", 401);
            headers["Authorization"] = `Bearer ${token}`;
        }

        let response;
        try {
            response = await fetch(BASE_URL + path, {
                method,
                headers,
                body: body ? JSON.stringify(body) : undefined
            });
        } catch (networkErr) {
            // El servidor no respondio (apagado, CORS bloqueado, sin internet, etc)
            throw new ApiError("No se pudo conectar con el servidor. Verifica que la API este corriendo.", 0);
        }

        // Si la sesion expiro o el token es invalido, limpiamos para forzar login de nuevo
        if (response.status === 401 && auth) {
            clearSession();
        }

        let data = null;
        const text = await response.text();
        if (text) {
            try { data = JSON.parse(text); } catch { data = text; }
        }

        if (!response.ok) {
            const message = typeof data === "string" ? data : (data?.title || data?.message || "Ocurrio un error.");
            throw new ApiError(message, response.status, data);
        }

        return data;
    }

    class ApiError extends Error {
        constructor(message, status, data) {
            super(message);
            this.status = status;
            this.data = data;
        }
    }

    // ---------- Endpoints de Auth ----------
    async function register({ username, email, password }) {
        const data = await request("/users/register", { method: "POST", body: { username, email, password } });
        setSession(data.token, data.user);
        return data;
    }

    async function login({ email, password }) {
        const data = await request("/users/login", { method: "POST", body: { email, password } });
        setSession(data.token, data.user);
        return data;
    }

    function logout() {
        clearSession();
    }

    return {
        request, ApiError,
        getToken, getCurrentUser, isLoggedIn, clearSession,
        register, login, logout
    };
})();


// =====================================================
// Seguridad: proteccion contra XSS
// =====================================================
// REGLA DE ORO DE TODO EL FRONTEND:
// Cualquier texto que venga de un usuario (username, bio,
// titulo de post, comentario, nombre de grupo, etc.) NUNCA
// se inserta con innerHTML directo. Siempre pasa por
// escapeHtml() primero, o se usa textContent en vez de
// innerHTML (que es automaticamente seguro).
// =====================================================

function escapeHtml(str) {
    if (str === null || str === undefined) return "";
    const div = document.createElement("div");
    div.textContent = String(str);
    return div.innerHTML;
}

// Helper para armar HTML con datos dinamicos de forma segura.
// Uso: safeHtml`<p>Hola ${username}</p>`  -> escapa automaticamente
// cada valor interpolado, dejando el resto del markup intacto.
function safeHtml(strings, ...values) {
    return strings.reduce((result, str, i) => {
        const value = values[i - 1];
        return result + (value !== undefined ? escapeHtml(value) : "") + str;
    });
}

// Sanitiza un valor antes de usarlo como atributo href/src, para
// evitar links tipo javascript:alert(1) escondidos en datos de usuario.
function safeUrl(url) {
    if (!url) return "#";
    const trimmed = String(url).trim();
    if (/^(https?:|\/|\.\/|mailto:)/i.test(trimmed)) return trimmed;
    return "#";
}
