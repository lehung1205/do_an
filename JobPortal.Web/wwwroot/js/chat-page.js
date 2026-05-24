(function () {
    const root = document.getElementById('chatPageRoot');
    if (!root) return;

    const hubUrl = root.dataset.hubUrl;
    const apiBase = (root.dataset.apiBase || '').replace(/\/$/, '');
    const currentUserId = Number(root.dataset.userId || 0);
    const initialPartnerUserId = root.dataset.partnerUserId ? Number(root.dataset.partnerUserId) : null;
    const urlParams = new URLSearchParams(window.location.search);
    const legacyApplicationId = urlParams.get('applicationId') ? Number(urlParams.get('applicationId')) : null;

    const messagesEl = document.getElementById('chatMessages');
    const partnerNameEl = document.getElementById('chatPartnerName');
    const jobTitleEl = document.getElementById('chatJobTitle');
    const chatForm = document.getElementById('chatForm');
    const chatInput = document.getElementById('chatInput');
    const chatSendBtn = document.getElementById('chatSendBtn');
    const chatAlert = document.getElementById('chatAlert');
    const chatHeaderAvatar = document.getElementById('chatHeaderAvatar');
    const chatHeaderInitial = document.getElementById('chatHeaderInitial');
    const chatHeaderPresenceDot = document.getElementById('chatHeaderPresenceDot');
    const chatPartnerPresence = document.getElementById('chatPartnerPresence');
    const chatPartnerPresenceText = document.getElementById('chatPartnerPresenceText');

    let connection = null;
    let activePartnerUserId = null;
    let chatReady = false;
    const messageIds = new Set();

    function getThreadItem(partnerUserId) {
        return document.querySelector('.chat-thread-item[data-partner-user-id="' + partnerUserId + '"]');
    }

    function getThreadUnreadBadge(partnerUserId) {
        return document.querySelector('[data-thread-unread="' + partnerUserId + '"]');
    }

    function setPresenceDot(el, online) {
        if (!el) return;
        el.classList.remove('chat-presence-dot--online', 'chat-presence-dot--offline');
        el.classList.add(online ? 'chat-presence-dot--online' : 'chat-presence-dot--offline');
        el.title = online ? 'Đang trực tuyến' : 'Đang offline';
    }

    function setPartnerOnline(userId, online) {
        if (!userId) return;

        document.querySelectorAll('.chat-thread-item[data-partner-user-id="' + userId + '"]').forEach(item => {
            item.dataset.partnerOnline = online ? 'true' : 'false';
            const dot = item.querySelector('[data-presence-for="' + userId + '"]');
            setPresenceDot(dot, online);
        });

        document.querySelectorAll('[data-presence-for="' + userId + '"]').forEach(dot => {
            setPresenceDot(dot, online);
        });

        if (Number(activePartnerUserId) === Number(userId)) {
            updateHeaderPresence(online);
        }
    }

    function updateHeaderPresence(online) {
        if (!chatPartnerPresence || !chatPartnerPresenceText) return;

        chatPartnerPresence.classList.remove('d-none', 'chat-panel__presence--online', 'chat-panel__presence--offline');
        chatPartnerPresence.classList.add(online ? 'chat-panel__presence--online' : 'chat-panel__presence--offline');
        chatPartnerPresenceText.textContent = online ? 'Đang trực tuyến' : 'Offline';
        setPresenceDot(chatHeaderPresenceDot, online);
    }

    function showChatHeader(name, online) {
        const initial = (name || '?').trim().charAt(0).toUpperCase() || '?';
        if (chatHeaderAvatar) chatHeaderAvatar.classList.remove('d-none');
        if (chatHeaderInitial) chatHeaderInitial.textContent = initial;
        if (chatPartnerPresence) chatPartnerPresence.classList.remove('d-none');
        updateHeaderPresence(!!online);
    }

    function hideChatHeaderExtras() {
        if (chatHeaderAvatar) chatHeaderAvatar.classList.add('d-none');
        if (chatPartnerPresence) chatPartnerPresence.classList.add('d-none');
    }

    function setThreadUnreadCount(partnerUserId, count) {
        const item = getThreadItem(partnerUserId);
        const badge = getThreadUnreadBadge(partnerUserId);
        const n = Math.max(0, count);
        if (item) {
            item.dataset.unreadCount = String(n);
            item.classList.toggle('chat-thread-item--unread', n > 0);
        }
        if (badge) {
            if (n > 0) {
                badge.textContent = String(n);
                badge.title = n + ' tin chưa đọc';
                badge.classList.remove('d-none');
            } else {
                badge.classList.add('d-none');
            }
        }
    }

    function adjustThreadUnread(partnerUserId, delta) {
        const item = getThreadItem(partnerUserId);
        const current = item ? Number(item.dataset.unreadCount || 0) : 0;
        setThreadUnreadCount(partnerUserId, current + delta);
    }

    function partnerIdFromSender(senderUserId) {
        const sid = Number(senderUserId);
        if (!sid || sid === currentUserId) return null;
        return sid;
    }

    async function refreshUnreadSummary() {
        try {
            const res = await fetch(apiBase + '/api/chat/unread-summary', {
                headers: apiHeaders(),
                credentials: 'include'
            });
            if (!res.ok) return;
            const json = await res.json();
            const total = json.data?.totalUnreadCount ?? json.Data?.TotalUnreadCount ?? 0;
            const navBadge = document.getElementById('navChatUnreadBadge');
            const listTotal = document.getElementById('chatThreadsUnreadTotal');
            if (navBadge) {
                if (total > 0) {
                    navBadge.textContent = String(total);
                    navBadge.classList.remove('d-none');
                } else {
                    navBadge.classList.add('d-none');
                }
            }
            if (listTotal) {
                if (total > 0) {
                    listTotal.textContent = total + ' chưa đọc';
                    listTotal.classList.remove('d-none');
                } else {
                    listTotal.classList.add('d-none');
                }
            }
        } catch { /* ignore */ }
    }

    function getToken() {
        return root.dataset.accessToken || '';
    }

    function apiHeaders() {
        const token = getToken();
        const headers = { Accept: 'application/json' };
        if (token) headers.Authorization = 'Bearer ' + token;
        return headers;
    }

    const connectionBuilder = new signalR.HubConnectionBuilder()
        .withUrl(hubUrl, { accessTokenFactory: () => getToken() })
        .withAutomaticReconnect();

    function showAlert(text, isError) {
        if (!text) {
            chatAlert.classList.add('d-none');
            chatAlert.textContent = '';
            return;
        }
        chatAlert.textContent = text;
        chatAlert.classList.remove('d-none');
        chatAlert.classList.toggle('alert-warning', isError !== false);
        chatAlert.classList.toggle('alert-info', isError === false);
    }

    function updateComposer() {
        const ok = chatReady && activePartnerUserId != null &&
            connection && connection.state === signalR.HubConnectionState.Connected;
        chatInput.disabled = !ok;
        chatSendBtn.disabled = !ok;
    }

    function escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    function appendMessage(msg, skipDuplicate) {
        const id = msg.id ?? msg.Id;
        if (skipDuplicate && id && messageIds.has(id)) return;
        if (id) messageIds.add(id);

        const empty = messagesEl.querySelector('.chat-empty, .text-muted.text-center');
        if (empty) empty.remove();

        const senderId = Number(msg.senderUserId ?? msg.SenderUserId ?? 0);
        const isMine = currentUserId > 0 && senderId > 0
            ? senderId === currentUserId
            : (msg.isMine === true || msg.IsMine === true);

        const bubble = document.createElement('div');
        bubble.className = 'chat-bubble ' + (isMine ? 'chat-bubble--mine' : 'chat-bubble--theirs');

        const sentAt = msg.sentAt ?? msg.SentAt;
        const time = sentAt ? new Date(sentAt).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' }) : '';
        const sender = msg.senderName ?? msg.SenderName ?? '';
        const content = msg.content ?? msg.Content ?? '';

        bubble.innerHTML =
            (isMine ? '' : '<div class="fw-semibold small">' + escapeHtml(sender) + '</div>') +
            '<div>' + escapeHtml(content) + '</div>' +
            '<div class="chat-bubble__meta">' + escapeHtml(time) + '</div>';

        messagesEl.appendChild(bubble);
        messagesEl.scrollTop = messagesEl.scrollHeight;
    }

    async function loadHistory(partnerUserId) {
        const res = await fetch(
            apiBase + '/api/chat/partners/' + partnerUserId + '/messages?page=1&pageSize=100',
            { headers: apiHeaders(), credentials: 'include' }
        );
        if (!res.ok) {
            let detail = 'Không tải được lịch sử tin nhắn (HTTP ' + res.status + ').';
            try {
                const errJson = await res.json();
                detail = errJson.message || errJson.Message || detail;
            } catch { /* ignore */ }
            throw new Error(detail);
        }
        const json = await res.json();
        const items = json.data?.items ?? json.Data?.Items ?? [];
        messageIds.clear();
        messagesEl.innerHTML = '';
        items.forEach(m => appendMessage(m, false));
        if (items.length === 0) {
            messagesEl.innerHTML =
                '<div class="chat-empty"><i class="bi bi-chat-dots"></i>' +
                '<p class="small mb-0">Chưa có tin nhắn. Hãy gửi lời chào!</p></div>';
        }
    }

    function onUserPresenceChanged(payload) {
        const userId = Number(payload.userId ?? payload.UserId ?? 0);
        const online = payload.online === true || payload.Online === true;
        if (userId) setPartnerOnline(userId, online);
    }

    async function ensureConnection() {
        if (connection && connection.state === signalR.HubConnectionState.Connected) {
            return connection;
        }
        if (!connection) {
            connection = connectionBuilder.build();
            connection.on('ChatJoined', onChatJoined);
            connection.on('UserPresenceChanged', onUserPresenceChanged);
            connection.on('ReceiveMessage', (msg) => {
                const senderId = Number(msg.senderUserId ?? msg.SenderUserId ?? 0);
                const isMine = currentUserId > 0 && senderId === currentUserId;
                appendMessage(msg, true);
                if (!isMine) {
                    const partnerId = partnerIdFromSender(senderId);
                    if (partnerId && partnerId !== Number(activePartnerUserId)) {
                        adjustThreadUnread(partnerId, 1);
                    }
                    refreshUnreadSummary();
                }
            });
            connection.onreconnecting(() => { chatReady = false; updateComposer(); });
            connection.onreconnected(async () => {
                if (activePartnerUserId) await joinChat(activePartnerUserId, true);
            });
            connection.onclose(() => { chatReady = false; updateComposer(); });
        }
        if (connection.state === signalR.HubConnectionState.Disconnected) {
            await connection.start();
        }
        return connection;
    }

    function onChatJoined(info) {
        const partnerId = Number(info.partnerUserId ?? info.PartnerUserId ?? 0);
        if (partnerId) {
            activePartnerUserId = partnerId;
            document.querySelectorAll('.chat-thread-item').forEach(el => {
                el.classList.toggle('active', Number(el.dataset.partnerUserId) === partnerId);
            });
            const url = new URL(window.location.href);
            url.searchParams.delete('applicationId');
            url.searchParams.set('partnerUserId', String(partnerId));
            window.history.replaceState({}, '', url);
            if (!messageIds.size) {
                loadHistory(partnerId).catch(() => { /* history may already be loaded */ });
            }
        }

        chatReady = true;
        const partnerName = info.partnerName ?? info.PartnerName ?? '';
        const partnerOnline = info.partnerIsOnline === true || info.PartnerIsOnline === true;

        partnerNameEl.textContent = partnerName;
        jobTitleEl.textContent = info.jobTitle ?? info.JobTitle ?? '';
        showChatHeader(partnerName, partnerOnline);
        if (partnerId) setPartnerOnline(partnerId, partnerOnline);

        updateComposer();
        showAlert('', false);
        if (partnerId) setThreadUnreadCount(partnerId, 0);
        refreshUnreadSummary();
    }

    async function leaveChat(partnerUserId) {
        if (!connection || connection.state !== signalR.HubConnectionState.Connected || !partnerUserId) return;
        try {
            await connection.invoke('LeaveChat', partnerUserId);
        } catch { /* ignore */ }
    }

    async function joinChat(partnerUserId, isReconnect) {
        if (!isReconnect && activePartnerUserId && activePartnerUserId !== partnerUserId) {
            await leaveChat(activePartnerUserId);
        }
        activePartnerUserId = partnerUserId;
        chatReady = false;
        updateComposer();
        showAlert('Đang kết nối…', false);

        await loadHistory(partnerUserId);
        const conn = await ensureConnection();
        await conn.invoke('JoinChat', partnerUserId);
    }

    async function joinChatByApplication(applicationId) {
        activePartnerUserId = null;
        chatReady = false;
        updateComposer();
        showAlert('Đang kết nối…', false);

        const conn = await ensureConnection();
        await conn.invoke('JoinChatByApplication', applicationId);
    }

    async function openThread(partnerUserId, title) {
        document.querySelectorAll('.chat-thread-item').forEach(el => {
            el.classList.toggle('active', Number(el.dataset.partnerUserId) === Number(partnerUserId));
        });

        const item = getThreadItem(partnerUserId);
        const partnerOnline = item ? item.dataset.partnerOnline === 'true' : false;

        partnerNameEl.textContent = title || 'Đang tải…';
        jobTitleEl.textContent = '';
        showChatHeader(title || '', partnerOnline);

        const url = new URL(window.location.href);
        url.searchParams.delete('applicationId');
        url.searchParams.set('partnerUserId', String(partnerUserId));
        window.history.replaceState({}, '', url);
        await joinChat(partnerUserId, false);
    }

    document.querySelectorAll('.chat-thread-item').forEach(el => {
        el.addEventListener('click', e => {
            e.preventDefault();
            openThread(Number(el.dataset.partnerUserId), el.dataset.threadTitle || '');
        });
    });

    chatForm?.addEventListener('submit', async e => {
        e.preventDefault();
        if (!activePartnerUserId || !chatReady) return;
        const text = chatInput.value.trim();
        if (!text) return;
        try {
            chatSendBtn.disabled = true;
            await ensureConnection();
            await connection.invoke('SendMessage', activePartnerUserId, text);
            chatInput.value = '';
        } catch (err) {
            console.error(err);
            showAlert(err.message || 'Không gửi được tin nhắn.', true);
        } finally {
            updateComposer();
        }
    });

    (async function init() {
        if (!hubUrl || !getToken()) {
            showAlert('Phiên đăng nhập hết hạn. Vui lòng đăng nhập lại.', true);
            return;
        }
        try {
            await ensureConnection();
            if (initialPartnerUserId) {
                const link = document.querySelector('.chat-thread-item[data-partner-user-id="' + initialPartnerUserId + '"]');
                await openThread(initialPartnerUserId, link?.dataset.threadTitle || '');
            } else if (legacyApplicationId) {
                await joinChatByApplication(legacyApplicationId);
            } else {
                hideChatHeaderExtras();
            }
        } catch (err) {
            console.error(err);
            const msg = (err && err.message) ? err.message : String(err);
            if (msg.includes('Failed to fetch') || msg.includes('NetworkError')) {
                showAlert('Không kết nối được API/SignalR. Kiểm tra JobPortal.API đang chạy tại http://localhost:5068.', true);
            } else {
                showAlert(msg, true);
            }
        }
    })();
})();
