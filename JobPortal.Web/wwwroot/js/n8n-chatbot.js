(function () {
  "use strict";

  const root = document.getElementById("n8n-chatbot");
  if (!root) return;

  const endpoint = root.dataset.endpoint || "/chatbot/webhook";
  const launcher = document.getElementById("n8nChatbotLauncher");
  const panel = document.getElementById("n8nChatbotPanel");
  const messagesEl = document.getElementById("n8nChatbotMessages");
  const form = document.getElementById("n8nChatbotForm");
  const input = document.getElementById("n8nChatbotInput");
  const sendBtn = document.getElementById("n8nChatbotSend");
  const errorEl = document.getElementById("n8nChatbotError");
  const clearBtn = document.getElementById("n8nChatbotClear");

  let isOpen = false;
  let isLoading = false;
  let typingEl = null;

  function formatTime(date) {
    return date.toLocaleTimeString("vi-VN", { hour: "2-digit", minute: "2-digit" });
  }

  function scrollToBottom() {
    messagesEl.scrollTop = messagesEl.scrollHeight;
  }

  function setOpen(open) {
    isOpen = open;
    root.classList.toggle("is-open", open);
    launcher.setAttribute("aria-expanded", open ? "true" : "false");
    panel.hidden = !open;
    panel.setAttribute("aria-hidden", open ? "false" : "true");
    if (open) {
      setTimeout(() => input.focus(), 200);
    }
  }

  function showError(message) {
    if (!message) {
      errorEl.classList.add("d-none");
      errorEl.textContent = "";
      return;
    }
    errorEl.textContent = message;
    errorEl.classList.remove("d-none");
  }

  function setLoading(loading) {
    isLoading = loading;
    root.classList.toggle("is-loading", loading);
    input.disabled = loading;
    sendBtn.disabled = loading;
  }

  function appendMessage(text, role, options) {
    const opts = options || {};
    const wrap = document.createElement("div");
    wrap.className =
      "n8n-chatbot__msg n8n-chatbot__msg--" +
      (role === "user" ? "user" : opts.isError ? "error" : "bot");

    const bubble = document.createElement("div");
    bubble.className = "n8n-chatbot__bubble";
    bubble.textContent = text;

    const time = document.createElement("time");
    time.className = "n8n-chatbot__time";
    time.dateTime = new Date().toISOString();
    time.textContent = formatTime(new Date());

    wrap.appendChild(bubble);
    wrap.appendChild(time);
    messagesEl.appendChild(wrap);
    scrollToBottom();
    return wrap;
  }

  function showTyping() {
    removeTyping();
    const wrap = document.createElement("div");
    wrap.className = "n8n-chatbot__msg n8n-chatbot__msg--bot";
    wrap.id = "n8nChatbotTyping";
    wrap.innerHTML =
      '<div class="n8n-chatbot__typing" aria-label="Đang trả lời">' +
      "<span></span><span></span><span></span></div>";
    messagesEl.appendChild(wrap);
    typingEl = wrap;
    scrollToBottom();
  }

  function removeTyping() {
    if (typingEl) {
      typingEl.remove();
      typingEl = null;
    }
    const existing = document.getElementById("n8nChatbotTyping");
    if (existing) existing.remove();
  }

  function autoResizeInput() {
    input.style.height = "auto";
    input.style.height = Math.min(input.scrollHeight, 120) + "px";
  }

  async function sendMessage(text) {
    const trimmed = text.trim();
    if (!trimmed || isLoading) return;

    showError("");
    appendMessage(trimmed, "user");
    input.value = "";
    autoResizeInput();
    setLoading(true);
    showTyping();

    try {
      const response = await fetch(endpoint, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Accept: "application/json",
        },
        credentials: "same-origin",
        body: JSON.stringify({ message: trimmed }),
      });

      const data = await response.json().catch(() => ({}));

      removeTyping();

      if (!response.ok || !data.success) {
        const errText =
          data.error ||
          "Không nhận được phản hồi. Kiểm tra n8n workflow đang active.";
        showError(errText);
        appendMessage(errText, "bot", { isError: true });
        return;
      }

      appendMessage(data.reply || "Đã nhận phản hồi.", "bot");
    } catch (err) {
      removeTyping();
      const msg =
        "Lỗi kết nối. Đảm bảo n8n chạy tại localhost:5678 và workflow đã bật.";
      showError(msg);
      appendMessage(msg, "bot", { isError: true });
      console.error("[n8n-chatbot]", err);
    } finally {
      setLoading(false);
    }
  }

  launcher.addEventListener("click", () => setOpen(!isOpen));

  form.addEventListener("submit", (e) => {
    e.preventDefault();
    sendMessage(input.value);
  });

  input.addEventListener("input", autoResizeInput);

  input.addEventListener("keydown", (e) => {
    if (e.key === "Enter" && !e.shiftKey) {
      e.preventDefault();
      sendMessage(input.value);
    }
  });

  clearBtn.addEventListener("click", () => {
    const welcome = messagesEl.querySelector(".n8n-chatbot__msg--bot");
    messagesEl.innerHTML = "";
    if (welcome) {
      messagesEl.appendChild(welcome);
    } else {
      appendMessage(
        "Cuộc trò chuyện đã được xóa. Bạn có thể hỏi tiếp nhé!",
        "bot"
      );
    }
    showError("");
  });

  document.addEventListener("keydown", (e) => {
    if (e.key === "Escape" && isOpen) setOpen(false);
  });
})();
