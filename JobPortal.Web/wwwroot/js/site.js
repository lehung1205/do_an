(function () {
  const tabButtons = {
    view: "account-tab-view-btn",
    edit: "account-tab-edit-btn",
    password: "account-tab-password-btn",
    resume: "account-tab-resume-btn",
  };

  function showAccountTab(tabName) {
    const buttonId = tabButtons[tabName] || tabButtons.view;
    let button = document.getElementById(buttonId);
    if (!button && tabName !== "view") {
      button = document.getElementById(tabButtons.view);
    }
    if (button) {
      bootstrap.Tab.getOrCreateInstance(button).show();
    }
  }

  function openAccountPanel(tabName) {
    const offcanvasEl = document.getElementById("accountOffcanvas");
    if (!offcanvasEl) {
      return;
    }

    showAccountTab(tabName);
    bootstrap.Offcanvas.getOrCreateInstance(offcanvasEl).show();
  }

  document.querySelectorAll("[data-account-open]").forEach((trigger) => {
    trigger.addEventListener("click", () => {
      const tab = trigger.getAttribute("data-account-tab") || "view";
      const dropdownToggle = trigger.closest(".dropdown")?.querySelector("[data-bs-toggle='dropdown']");
      if (dropdownToggle) {
        const dropdown = bootstrap.Dropdown.getInstance(dropdownToggle);
        dropdown?.hide();
      }
      openAccountPanel(tab);
    });
  });

  document.querySelectorAll("[data-account-switch-tab]").forEach((trigger) => {
    trigger.addEventListener("click", () => {
      const tab = trigger.getAttribute("data-account-switch-tab") || "view";
      showAccountTab(tab);
    });
  });

  const params = new URLSearchParams(window.location.search);
  if (params.get("accountOpen") === "1") {
    const tab = params.get("accountTab") || "view";
    openAccountPanel(tab);

    params.delete("accountOpen");
    params.delete("accountTab");
    const query = params.toString();
    const cleanUrl =
      window.location.pathname + (query ? "?" + query : "") + window.location.hash;
    window.history.replaceState({}, "", cleanUrl);
  }

  const offcanvasEl = document.getElementById("accountOffcanvas");
  if (offcanvasEl) {
    const defaultTab = offcanvasEl.getAttribute("data-active-tab");
    if (defaultTab && defaultTab !== "view") {
      showAccountTab(defaultTab);
    }
  }
})();
