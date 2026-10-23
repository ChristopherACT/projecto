// =====================================================
// feed.js — feed vertical estilo Instagram
// =====================================================

document.addEventListener("DOMContentLoaded", () => {
    if (!API.isLoggedIn()) {
        window.location.href = "index.html";
        return;
    }

    const user = API.getCurrentUser();
    const container = document.getElementById("feed-container");
    const toast = document.getElementById("toast");

    let currentType = "Image";

    // ---------- Navbar ----------
    document.getElementById("navbar-username").textContent = user.username;
    document.getElementById("navbar-avatar").textContent = user.username.charAt(0).toUpperCase();

    document.getElementById("logout-btn").addEventListener("click", () => {
        API.logout();
        window.location.href = "index.html";
    });

    const tabs = document.querySelectorAll(".navbar-tab");
    tabs.forEach(tab => {
        tab.addEventListener("click", () => {
            tabs.forEach(t => t.classList.remove("active"));
            tab.classList.add("active");
            currentType = tab.dataset.type;
            loadFeed();
        });
    });

    // ---------- Toast ----------
    let toastTimer;
    function showToast(message, isError = false) {
        toast.textContent = message;
        toast.className = "toast show" + (isError ? " error" : "");
        clearTimeout(toastTimer);
        toastTimer = setTimeout(() => toast.classList.remove("show"), 2500);
    }

    function renderState(icon, title, message) {
        container.innerHTML = `
      <div class="feed-state">
        <div class="icon">${icon}</div>
        <h3>${title}</h3>
        <p>${message}</p>
      </div>`;
    }

    // ---------- Cargar feed ----------
    async function loadFeed() {
        renderState("⏳", "Cargando...", "Un momento por favor.");

        try {
            const posts = await API.request(`/posts/feed?type=${currentType}&pageSize=30`);

            if (!posts || posts.length === 0) {
                const labels = { Image: "imagenes", Video: "videos", Mod: "mods", Map: "mapas", Guide: "guias" };
                renderState("📭", "Nada por aqui todavia", `Se el primero en subir ${labels[currentType] || "contenido"}.`);
                return;
            }

            const list = document.createElement("div");
            list.className = "feed-list";
            posts.forEach(post => list.appendChild(buildPostCard(post)));

            container.innerHTML = "";
            container.appendChild(list);
        } catch (err) {
            renderState("⚠️", "No se pudo cargar el feed", err.message || "Intenta de nuevo en unos segundos.");
        }
    }

    // ---------- Construir una tarjeta de post ----------
    function buildPostCard(post) {
        const card = document.createElement("article");
        card.className = "ig-card";

        // --- Header ---
        const header = document.createElement("div");
        header.className = "ig-header";

        const avatar = document.createElement("div");
        avatar.className = "avatar-badge";
        avatar.textContent = post.user.username.charAt(0).toUpperCase(); // seguro

        const headerText = document.createElement("div");
        headerText.className = "ig-header-text";
        const uname = document.createElement("span");
        uname.className = "ig-username";
        uname.textContent = post.user.username; // seguro
        headerText.appendChild(uname);
        if (post.gameName) {
            const game = document.createElement("span");
            game.className = "ig-game";
            game.textContent = post.gameName; // seguro
            headerText.appendChild(game);
        }

        header.appendChild(avatar);
        header.appendChild(headerText);

        // --- Media ---
        const media = document.createElement("div");
        media.className = "ig-media";
        const firstFile = post.fileUrls && post.fileUrls[0];

        if (firstFile && post.type === "Image") {
            const img = document.createElement("img");
            img.src = safeUrl(firstFile);
            img.alt = "";
            img.loading = "lazy";
            media.appendChild(img);
        } else if (firstFile && post.type === "Video") {
            const video = document.createElement("video");
            video.src = safeUrl(firstFile);
            video.controls = true;
            video.playsInline = true;
            media.appendChild(video);
        } else {
            const placeholder = document.createElement("div");
            placeholder.className = "placeholder-icon";
            const icons = { Mod: "🧩", Map: "🗺️", Guide: "📄" };
            placeholder.textContent = icons[post.type] || "📦";
            media.appendChild(placeholder);
        }

        // --- Acciones (like / comentar) ---
        const actions = document.createElement("div");
        actions.className = "ig-actions";

        const likeBtn = document.createElement("button");
        likeBtn.className = "icon-btn";
        likeBtn.type = "button";
        likeBtn.title = "Me gusta";
        likeBtn.innerHTML = `<span class="heart">♡</span>`;

        const commentBtn = document.createElement("button");
        commentBtn.className = "icon-btn";
        commentBtn.type = "button";
        commentBtn.title = "Comentar";
        commentBtn.textContent = "💬";

        actions.appendChild(likeBtn);
        actions.appendChild(commentBtn);
        const spacer = document.createElement("span");
        spacer.className = "spacer";
        actions.appendChild(spacer);

        if (post.type === "Mod" || post.type === "Map") {
            const dlBtn = document.createElement("button");
            dlBtn.className = "icon-btn";
            dlBtn.title = "Descargar";
            dlBtn.textContent = "⬇";
            dlBtn.addEventListener("click", () => downloadPost(post.id, dlBtn));
            actions.appendChild(dlBtn);
        }

        // --- Cuerpo: likes, caption, tags ---
        const body = document.createElement("div");
        body.className = "ig-body";

        const likesLine = document.createElement("p");
        likesLine.className = "ig-likes";
        let likeCount = Number(post.likesCount) || 0;
        likesLine.textContent = `${likeCount} me gusta`;

        likeBtn.addEventListener("click", async () => {
            const isLiked = likeBtn.classList.contains("liked");
            try {
                if (isLiked) {
                    await API.request(`/posts/${post.id}/like`, { method: "DELETE", auth: true });
                    likeCount = Math.max(0, likeCount - 1);
                    likeBtn.classList.remove("liked");
                    likeBtn.querySelector(".heart").textContent = "♡";
                } else {
                    await API.request(`/posts/${post.id}/like`, { method: "POST", auth: true });
                    likeCount += 1;
                    likeBtn.classList.add("liked");
                    likeBtn.querySelector(".heart").textContent = "♥";
                }
                likesLine.textContent = `${likeCount} me gusta`;
            } catch (err) {
                showToast(err.message || "No se pudo procesar el like.", true);
            }
        });

        const caption = document.createElement("p");
        caption.className = "ig-caption";
        const capUser = document.createElement("span");
        capUser.className = "cap-user";
        capUser.textContent = post.user.username; // seguro
        caption.appendChild(capUser);
        caption.appendChild(document.createTextNode(post.title)); // seguro, nunca ejecuta HTML

        body.appendChild(likesLine);
        body.appendChild(caption);

        if (post.description) {
            const desc = document.createElement("p");
            desc.className = "ig-caption";
            desc.style.color = "var(--text-dim)";
            desc.textContent = post.description; // seguro
            body.appendChild(desc);
        }

        if (post.tags && post.tags.length > 0) {
            const tagsWrap = document.createElement("div");
            tagsWrap.className = "ig-tags";
            post.tags.forEach(tag => {
                const pill = document.createElement("span");
                pill.className = "tag-pill";
                pill.textContent = "#" + tag; // seguro
                tagsWrap.appendChild(pill);
            });
            body.appendChild(tagsWrap);
        }

        // --- Comentarios (panel expandible) ---
        const commentsToggle = document.createElement("button");
        commentsToggle.className = "ig-comments-toggle";
        commentsToggle.type = "button";
        commentsToggle.textContent = post.commentsCount > 0
            ? `Ver los ${post.commentsCount} comentarios`
            : "Se el primero en comentar";

        const commentsPanel = document.createElement("div");
        commentsPanel.className = "ig-comments-panel";
        let commentsLoaded = false;

        async function openComments() {
            commentsPanel.classList.add("open");
            if (!commentsLoaded) {
                commentsPanel.innerHTML = `<p class="ig-comments-empty">Cargando comentarios...</p>`;
                try {
                    const comments = await API.request(`/posts/${post.id}/comments`);
                    commentsLoaded = true;
                    renderComments(commentsPanel, comments, post.id);
                } catch (err) {
                    commentsPanel.innerHTML = "";
                    const errMsg = document.createElement("p");
                    errMsg.className = "ig-comments-empty";
                    errMsg.textContent = "No se pudieron cargar los comentarios.";
                    commentsPanel.appendChild(errMsg);
                }
            }
        }

        commentsToggle.addEventListener("click", () => {
            const isOpen = commentsPanel.classList.contains("open");
            if (isOpen) {
                commentsPanel.classList.remove("open");
            } else {
                openComments();
            }
        });
        commentBtn.addEventListener("click", () => {
            openComments();
            commentsPanel.classList.add("open");
            setTimeout(() => commentsPanel.querySelector("input")?.focus(), 50);
        });

        body.appendChild(commentsToggle);
        body.appendChild(commentsPanel);

        card.appendChild(header);
        card.appendChild(media);
        card.appendChild(actions);
        card.appendChild(body);
        return card;
    }

    // ---------- Renderizar lista de comentarios + form para agregar ----------
    function renderComments(panelEl, comments, postId) {
        panelEl.innerHTML = "";

        if (comments.length === 0) {
            const empty = document.createElement("p");
            empty.className = "ig-comments-empty";
            empty.textContent = "Todavia no hay comentarios.";
            panelEl.appendChild(empty);
        } else {
            comments.forEach(c => {
                const line = document.createElement("p");
                line.className = "ig-comment";
                const cu = document.createElement("span");
                cu.className = "cu";
                cu.textContent = c.user.username; // seguro
                const ct = document.createElement("span");
                ct.className = "ct";
                ct.textContent = c.content; // seguro — el mas importante, texto libre del usuario
                line.appendChild(cu);
                line.appendChild(ct);
                panelEl.appendChild(line);
            });
        }

        // --- Form para agregar comentario ---
        const form = document.createElement("form");
        form.className = "ig-comment-form";

        const input = document.createElement("input");
        input.type = "text";
        input.placeholder = "Escribe un comentario...";
        input.maxLength = 500;
        input.required = true;

        const submitBtn = document.createElement("button");
        submitBtn.type = "submit";
        submitBtn.textContent = "Publicar";
        submitBtn.disabled = true;

        form.appendChild(input);
        form.appendChild(submitBtn);

        input.addEventListener("input", () => {
            submitBtn.disabled = input.value.trim().length === 0;
        });

        form.addEventListener("submit", async (e) => {
            e.preventDefault();
            const content = input.value.trim();
            if (!content) return;

            submitBtn.disabled = true;
            try {
                const newComment = await API.request(`/posts/${postId}/comments`, {
                    method: "POST",
                    auth: true,
                    body: { content }
                });

                const empty = panelEl.querySelector(".ig-comments-empty");
                if (empty) empty.remove();

                const line = document.createElement("p");
                line.className = "ig-comment";
                const cu = document.createElement("span");
                cu.className = "cu";
                cu.textContent = newComment.user.username; // seguro
                const ct = document.createElement("span");
                ct.className = "ct";
                ct.textContent = newComment.content; // seguro
                line.appendChild(cu);
                line.appendChild(ct);
                panelEl.insertBefore(line, form);

                input.value = "";
            } catch (err) {
                showToast(err.message || "No se pudo publicar el comentario.", true);
            } finally {
                submitBtn.disabled = input.value.trim().length === 0;
            }
        });

        panelEl.appendChild(form);
    }

    // ---------- Descargar (mods/mapas) ----------
    async function downloadPost(postId) {
        try {
            const result = await API.request(`/posts/${postId}/download`, { method: "POST", auth: true });
            const url = result.fileUrls && result.fileUrls[0];
            if (url) {
                window.open(safeUrl(url), "_blank");
            } else {
                showToast("Este post no tiene archivos adjuntos.", true);
            }
        } catch (err) {
            showToast(err.message || "No se pudo iniciar la descarga.", true);
        }
    }

    loadFeed();
});