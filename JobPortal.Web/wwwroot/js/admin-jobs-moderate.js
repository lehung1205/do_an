(function () {
  function init() {
    const modalEl = document.getElementById('adminJobModerateModal');
    if (!modalEl) {
      return;
    }

    const BootstrapModal = window.bootstrap && window.bootstrap.Modal;
    if (!BootstrapModal) {
      console.warn('admin-jobs-moderate: Bootstrap Modal not found.');
      return;
    }

    const modal = BootstrapModal.getOrCreate(modalEl);
    const iconEl = document.getElementById('adminJobModerateModalIcon');
    const titleEl = document.getElementById('adminJobModerateModalLabel');
    const leadEl = document.getElementById('adminJobModerateModalLead');
    const jobTitleEl = document.getElementById('adminJobModerateModalJobTitle');
    const confirmBtn = document.getElementById('adminJobModerateModalConfirm');

    const copy = {
      approve: {
        modalClass: 'admin-job-moderate-modal--approve',
        iconClass: 'bi-check-lg',
        title: 'Xác nhận duyệt tin',
        lead: 'Tin sẽ hiển thị công khai cho ứng viên sau khi bạn xác nhận.',
        confirmLabel: 'Xác nhận duyệt',
        confirmBtnClass: 'admin-jobs-btn--approve',
        confirmIcon: 'bi-check-lg'
      },
      reject: {
        modalClass: 'admin-job-moderate-modal--reject',
        iconClass: 'bi-x-lg',
        title: 'Xác nhận từ chối tin',
        lead: 'Lượt đăng tin sẽ được hoàn lại cho nhà tuyển dụng. Tin sẽ không hiển thị công khai.',
        confirmLabel: 'Xác nhận từ chối',
        confirmBtnClass: 'admin-jobs-btn--reject',
        confirmIcon: 'bi-x-lg'
      }
    };

    function resolveTrigger(element) {
      return element && element.closest
        ? element.closest('.js-admin-job-moderate')
        : null;
    }

    function populateFromTrigger(trigger) {
      const action = trigger.dataset.moderateAction;
      const cfg = copy[action];
      if (!cfg) {
        return;
      }

      const formId = trigger.dataset.moderateForm;
      modalEl.dataset.pendingFormId = formId || '';

      const jobTitle = trigger.dataset.jobTitle || '';

      modalEl.classList.remove('admin-job-moderate-modal--approve', 'admin-job-moderate-modal--reject');
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
      if (jobTitleEl) {
        jobTitleEl.textContent = jobTitle;
        jobTitleEl.hidden = !jobTitle;
      }
      if (confirmBtn) {
        confirmBtn.className = `btn admin-jobs-btn ${cfg.confirmBtnClass}`;
        confirmBtn.innerHTML = `<i class="bi ${cfg.confirmIcon} me-1"></i> ${cfg.confirmLabel}`;
      }
    }

    document.querySelectorAll('.js-admin-job-moderate').forEach((btn) => {
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
