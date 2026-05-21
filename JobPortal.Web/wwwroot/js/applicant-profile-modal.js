(function () {
  const modalEl = document.getElementById("applicantProfileModal");
  if (!modalEl) return;

  const loadingEl = document.getElementById("applicantProfileLoading");
  const errorEl = document.getElementById("applicantProfileError");
  const contentEl = document.getElementById("applicantProfileContent");

  function setText(id, text) {
    const el = document.getElementById(id);
    if (el) el.textContent = text ?? "";
  }

  function setHtmlLink(id, value, type) {
    const el = document.getElementById(id);
    if (!el) return;
    if (!value || value === "—") {
      el.innerHTML = '<span class="text-muted">—</span>';
      return;
    }
    if (type === "email") {
      el.innerHTML = `<a href="mailto:${escapeAttr(value)}">${escapeHtml(value)}</a>`;
    } else if (type === "phone") {
      el.innerHTML = `<a href="tel:${escapeAttr(value)}">${escapeHtml(value)}</a>`;
    } else {
      el.textContent = value;
    }
  }

  function escapeHtml(str) {
    return String(str)
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;");
  }

  function escapeAttr(str) {
    return String(str).replace(/"/g, "&quot;");
  }

  function showState(state) {
    loadingEl?.classList.toggle("d-none", state !== "loading");
    errorEl?.classList.toggle("d-none", state !== "error");
    contentEl?.classList.toggle("d-none", state !== "content");
  }

  function renderAvatar(imageUrl, name) {
    const wrap = document.getElementById("applicantProfileAvatarWrap");
    if (!wrap) return;

    if (imageUrl) {
      wrap.innerHTML = `<img src="${escapeAttr(imageUrl)}" alt="" class="applicant-profile-modal__avatar-img rounded-circle object-fit-cover" width="72" height="72" />`;
    } else {
      const initial = name && name.length > 0 ? name.trim()[0].toUpperCase() : "?";
      wrap.innerHTML = `<span class="applicant-profile-modal__avatar--placeholder d-inline-flex align-items-center justify-content-center rounded-circle fw-semibold" style="width:72px;height:72px;font-size:1.5rem;">${escapeHtml(initial)}</span>`;
    }
  }

  async function openProfile(applicationId) {
    showState("loading");
    errorEl.textContent = "";

    const url = `/Employer/Applicants?handler=ApplicantProfile&applicationId=${encodeURIComponent(applicationId)}`;

    try {
      const res = await fetch(url, { headers: { Accept: "application/json" } });
      if (!res.ok) {
        showState("error");
        errorEl.textContent = "Không tải được thông tin ứng viên.";
        return;
      }

      const data = await res.json();
      renderAvatar(data.profileImage, data.name);
      setText("applicantProfileName", data.name);
      setText(
        "applicantProfileJob",
        `Ứng tuyển: ${data.jobTitle} · ${data.appliedAt}`
      );

      const descWrap = document.getElementById("applicantProfileDescWrap");
      if (data.description && data.description.trim()) {
        descWrap?.classList.remove("d-none");
        setText("applicantProfileDesc", data.description);
      } else {
        descWrap?.classList.add("d-none");
      }

      setHtmlLink("applicantProfileEmail", data.email, "email");
      setHtmlLink("applicantProfilePhone", data.phone, "phone");
      setText("applicantProfileDob", data.dateOfBirth);
      setText("applicantProfileGender", data.gender);
      setText("applicantProfilePermanent", data.permanentAddress);
      setText("applicantProfileTemporary", data.temporaryAddress);
      setText("applicantProfileResume", data.resumeTitle);
      setText("applicantProfileStatus", data.applicationStatus);

      showState("content");
    } catch {
      showState("error");
      errorEl.textContent = "Không tải được thông tin ứng viên.";
    }
  }

  document.querySelectorAll(".applicant-profile-trigger").forEach((trigger) => {
    trigger.addEventListener("click", () => {
      const applicationId = trigger.getAttribute("data-application-id");
      if (!applicationId) return;

      const modal = bootstrap.Modal.getOrCreateInstance(modalEl);
      modal.show();
      openProfile(applicationId);
    });
  });

  modalEl.addEventListener("hidden.bs.modal", () => {
    showState("loading");
    errorEl.textContent = "";
  });
})();
