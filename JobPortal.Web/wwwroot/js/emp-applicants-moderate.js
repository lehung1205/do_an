(function () {
  function init() {
    const modalEl = document.getElementById('empAppModerateModal');
    if (!modalEl) {
      return;
    }

    const BootstrapModal = window.bootstrap && window.bootstrap.Modal;
    if (!BootstrapModal) {
      return;
    }

    const modal = BootstrapModal.getOrCreate(modalEl);
    const iconEl = document.getElementById('empAppModerateModalIcon');
    const titleEl = document.getElementById('empAppModerateModalLabel');
    const leadEl = document.getElementById('empAppModerateModalLead');
    const detailEl = document.getElementById('empAppModerateModalDetail');
    const confirmBtn = document.getElementById('empAppModerateModalConfirm');

    const copy = {
      accept: {
        modalClass: 'emp-app-moderate-modal--accept',
        iconClass: 'bi-check-lg',
        title: 'Xác nhận chấp nhận ứng viên',
        lead: 'Ứng viên sẽ được chấp nhận cho vị trí này. Bạn có thể theo dõi tiến độ công việc sau khi xác nhận.',
        confirmLabel: 'Chấp nhận',
        confirmBtnClass: 'emp-apps-btn--success',
        confirmIcon: 'bi-check-lg'
      },
      reject: {
        modalClass: 'emp-app-moderate-modal--reject',
        iconClass: 'bi-x-lg',
        title: 'Xác nhận từ chối ứng viên',
        lead: 'Ứng viên sẽ được đánh dấu từ chối. Trạng thái này không thể đổi ngược.',
        confirmLabel: 'Từ chối',
        confirmBtnClass: 'emp-apps-btn--danger',
        confirmIcon: 'bi-x-lg'
      }
    };

    function resolveTrigger(element) {
      return element && element.closest
        ? element.closest('.js-emp-app-moderate')
        : null;
    }

    function escapeHtml(text) {
      const div = document.createElement('div');
      div.textContent = text;
      return div.innerHTML;
    }

    function populateFromTrigger(trigger) {
      const action = trigger.dataset.moderateAction;
      const cfg = copy[action];
      if (!cfg) {
        return;
      }

      const formId = trigger.dataset.moderateForm;
      modalEl.dataset.pendingFormId = formId || '';

      const applicantName = trigger.dataset.applicantName || '';
      const jobTitle = trigger.dataset.jobTitle || '';

      modalEl.classList.remove('emp-app-moderate-modal--accept', 'emp-app-moderate-modal--reject');
      modalEl.classList.add(cfg.modalClass);

      if (iconEl) {
        iconEl.innerHTML = `<i class="bi ${cfg.iconClass}"></i>`;
      }
      if (titleEl) {
        titleEl.textContent = cfg.title;
      }
      if (leadEl) {
        leadEl.textContent = cfg.lead;
      }
      if (detailEl) {
        detailEl.innerHTML = applicantName
          ? `<div><strong>${escapeHtml(applicantName)}</strong></div>${jobTitle ? `<div class="text-muted small mt-1">${escapeHtml(jobTitle)}</div>` : ''}`
          : '';
      }
      if (confirmBtn) {
        confirmBtn.className = `btn emp-apps-btn ${cfg.confirmBtnClass}`;
        confirmBtn.innerHTML = `<i class="bi ${cfg.confirmIcon} me-1"></i> ${cfg.confirmLabel}`;
      }
    }

    document.querySelectorAll('.js-emp-app-moderate').forEach((btn) => {
      btn.addEventListener('click', (event) => {
        const trigger = resolveTrigger(event.currentTarget);
        if (trigger) {
          populateFromTrigger(trigger);
        }
      });
    });

    modalEl.addEventListener('show.bs.modal', (event) => {
      const trigger = resolveTrigger(event.relatedTarget);
      if (trigger) {
        populateFromTrigger(trigger);
      }
    });

    if (confirmBtn) {
      confirmBtn.addEventListener('click', () => {
        const formId = modalEl.dataset.pendingFormId;
        const form = formId ? document.getElementById(formId) : null;
        if (!form) {
          return;
        }

        modal.hide();

        window.setTimeout(() => {
          const submitBtn = form.querySelector('button[type="submit"]');
          if (typeof form.requestSubmit === 'function') {
            form.requestSubmit(submitBtn || undefined);
          } else if (submitBtn) {
            submitBtn.click();
          } else {
            form.submit();
          }
        }, 150);
      });
    }

    modalEl.addEventListener('hidden.bs.modal', () => {
      delete modalEl.dataset.pendingFormId;
    });
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', init);
  } else {
    init();
  }
})();
