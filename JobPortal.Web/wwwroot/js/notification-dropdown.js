(function () {
  function initDropdown(root) {
    const trigger = root.querySelector('[data-notif-trigger]');
    const panel = root.querySelector('[data-notif-panel]');
    const badge = root.querySelector('[data-notif-badge]');
    const markAllBtn = root.querySelector('[data-notif-mark-all]');
    const filterTabs = root.querySelectorAll('[data-notif-filter]');
    const emptyUnread = root.querySelector('[data-notif-empty-unread]');

    const markReadUrl = root.dataset.markReadUrl || '';
    const markAllUrl = root.dataset.markAllUrl || '';
    const csrfName = root.dataset.csrfName || '';
    const csrfToken = root.dataset.csrfToken || '';

    let activeFilter = 'all';
    let isOpen = false;

    function setOpen(open) {
      isOpen = open;
      if (!panel || !trigger) return;

      panel.hidden = !open;
      trigger.setAttribute('aria-expanded', open ? 'true' : 'false');
      trigger.classList.toggle('is-active', open);
    }

    function updateBadge(count) {
      if (!badge) return;

      if (count <= 0) {
        badge.classList.add('d-none');
        badge.textContent = '0';
        badge.setAttribute('aria-hidden', 'true');
        if (markAllBtn) {
          markAllBtn.style.display = 'none';
        }
        return;
      }

      badge.classList.remove('d-none');
      badge.textContent = count > 99 ? '99+' : String(count);
      badge.setAttribute('aria-hidden', 'false');
      if (markAllBtn) {
        markAllBtn.style.display = '';
      }
    }

    function postForm(url) {
      const body = new URLSearchParams();
      if (csrfName && csrfToken) {
        body.append(csrfName, csrfToken);
      }

      return fetch(url, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/x-www-form-urlencoded',
          'X-Requested-With': 'XMLHttpRequest'
        },
        body: body.toString()
      }).then(function (res) {
        return res.json();
      });
    }

    function markItemRead(itemEl) {
      if (!itemEl || itemEl.dataset.notifRead === 'true') return Promise.resolve();

      const id = itemEl.dataset.notifId;
      if (!id) return Promise.resolve();

      return postForm(markReadUrl + '&id=' + encodeURIComponent(id)).then(function (data) {
        if (!data || !data.success) return;

        itemEl.dataset.notifRead = 'true';
        itemEl.classList.remove('is-unread');
        itemEl.classList.add('is-read');

        const dot = itemEl.querySelector('.notif-dropdown__dot');
        if (dot) dot.remove();

        if (typeof data.unreadCount === 'number') {
          updateBadge(data.unreadCount);
        }

        applyFilter(activeFilter);
      });
    }

    function markAllRead() {
      if (!markAllUrl) return;

      postForm(markAllUrl).then(function (data) {
        if (!data || !data.success) return;

        root.querySelectorAll('[data-notif-item]').forEach(function (item) {
          item.dataset.notifRead = 'true';
          item.classList.remove('is-unread');
          item.classList.add('is-read');
          const dot = item.querySelector('.notif-dropdown__dot');
          if (dot) dot.remove();
        });

        updateBadge(0);
        applyFilter(activeFilter);
      });
    }

    function applyFilter(filter) {
      activeFilter = filter;
      let visibleUnread = 0;

      root.querySelectorAll('[data-notif-item]').forEach(function (item) {
        const isUnread = item.dataset.notifRead !== 'true';
        const show = filter === 'all' || isUnread;
        item.classList.toggle('is-hidden-by-filter', !show);
        if (show && isUnread) visibleUnread++;
      });

      root.querySelectorAll('[data-notif-section]').forEach(function (section) {
        const visibleItems = section.querySelectorAll('[data-notif-item]:not(.is-hidden-by-filter)');
        section.classList.toggle('is-hidden-by-filter', visibleItems.length === 0);
      });

      if (emptyUnread) {
        const hasAny = root.querySelectorAll('[data-notif-item]').length > 0;
        emptyUnread.classList.toggle('d-none', filter !== 'unread' || visibleUnread > 0 || !hasAny);
      }
    }

    trigger?.addEventListener('click', function (e) {
      e.stopPropagation();
      setOpen(!isOpen);
    });

    markAllBtn?.addEventListener('click', function (e) {
      e.preventDefault();
      e.stopPropagation();
      markAllRead();
    });

    filterTabs.forEach(function (tab) {
      tab.addEventListener('click', function () {
        const filter = tab.getAttribute('data-notif-filter') || 'all';
        filterTabs.forEach(function (t) {
          const active = t === tab;
          t.classList.toggle('is-active', active);
          t.setAttribute('aria-selected', active ? 'true' : 'false');
        });
        applyFilter(filter);
      });
    });

    root.querySelectorAll('[data-notif-link]').forEach(function (link) {
      link.addEventListener('click', function (e) {
        const item = link.closest('[data-notif-item]');
        if (!item || item.dataset.notifRead === 'true') return;

        e.preventDefault();
        const href = link.getAttribute('href');

        markItemRead(item).finally(function () {
          if (href) {
            window.location.href = href;
          }
        });
      });
    });

    root._notifClose = function () {
      setOpen(false);
    };

    root._notifIsOpen = function () {
      return isOpen;
    };

    root._notifFocusTrigger = function () {
      trigger?.focus();
    };
  }

  document.querySelectorAll('[data-notif-dropdown]').forEach(initDropdown);

  document.addEventListener('click', function (e) {
    document.querySelectorAll('[data-notif-dropdown]').forEach(function (root) {
      if (!root._notifIsOpen || !root._notifIsOpen()) return;
      if (root.contains(e.target)) return;
      root._notifClose?.();
    });
  });

  document.addEventListener('keydown', function (e) {
    if (e.key !== 'Escape') return;
    document.querySelectorAll('[data-notif-dropdown]').forEach(function (root) {
      if (root._notifIsOpen && root._notifIsOpen()) {
        root._notifClose?.();
        root._notifFocusTrigger?.();
      }
    });
  });
})();
