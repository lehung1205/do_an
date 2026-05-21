(function () {
  const modalEl = document.getElementById("applicantProfileModal");
  if (!modalEl) return;

  const loadingEl = document.getElementById("applicantProfileLoading");
  const errorEl = document.getElementById("applicantProfileError");
  const contentEl = document.getElementById("applicantProfileContent");
  const infoTabBtn = document.getElementById("applicant-profile-info-tab");
  const reviewsBadgeEl = document.getElementById("applicantProfileReviewsBadge");
  const reviewsSummaryEl = document.getElementById("applicantProfileReviewsSummary");
  const reviewsListEl = document.getElementById("applicantProfileReviewsList");

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

  function resetTabs() {
    if (!infoTabBtn || !window.bootstrap?.Tab) return;
    bootstrap.Tab.getOrCreateInstance(infoTabBtn).show();
  }

  function renderStars(rating, max = 5) {
    const rounded = Math.round(Number(rating) || 0);
    let html = "";
    for (let i = 1; i <= max; i++) {
      const filled = i <= rounded;
      html += `<i class="bi ${filled ? "bi-star-fill text-warning" : "bi-star text-muted"}" aria-hidden="true"></i>`;
    }
    return html;
  }

  function renderReviews(reviews) {
    const totalCount = reviews?.totalCount ?? 0;
    const average = reviews?.averageRating;
    const items = reviews?.items ?? [];

    if (reviewsBadgeEl) {
      if (totalCount > 0) {
        reviewsBadgeEl.textContent = String(totalCount);
        reviewsBadgeEl.classList.remove("d-none");
      } else {
        reviewsBadgeEl.classList.add("d-none");
      }
    }

    if (!reviewsSummaryEl || !reviewsListEl) return;

    if (totalCount > 0 && average != null) {
      const avgText = Number(average).toFixed(1);
      reviewsSummaryEl.innerHTML = `
        <div class="text-muted small mb-1">Điểm trung bình từ nhà tuyển dụng</div>
        <div class="d-flex align-items-center justify-content-center gap-2 flex-wrap">
          <span class="display-6 fw-bold text-warning mb-0">${escapeHtml(avgText)}</span>
          <span class="text-muted">/ 5</span>
        </div>
        <div class="mt-1">${renderStars(average)}</div>
        <p class="text-muted small mb-0 mt-2">${totalCount} đánh giá</p>`;
    } else {
      reviewsSummaryEl.innerHTML = `
        <p class="text-muted mb-0 py-2">
          <i class="bi bi-star me-1"></i> Ứng viên chưa có đánh giá nào.
        </p>`;
    }

    if (items.length === 0) {
      reviewsListEl.innerHTML = "";
      return;
    }

    reviewsListEl.innerHTML = items
      .map((item) => {
        const comment = item.comment?.trim()
          ? `<p class="small mb-0 mt-2" style="white-space: pre-wrap;">${escapeHtml(item.comment)}</p>`
          : "";

        return `
          <div class="border rounded p-3 applicant-profile-review-item">
            <div class="d-flex flex-wrap justify-content-between gap-2 mb-1">
              <div class="min-w-0">
                <div class="fw-semibold small">${escapeHtml(item.employerName ?? "—")}</div>
                <div class="text-muted small text-truncate">${escapeHtml(item.jobTitle ?? "")}</div>
              </div>
              <div class="text-nowrap">${renderStars(item.rating)}</div>
            </div>
            ${comment}
          </div>`;
      })
      .join("");
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
    resetTabs();

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

      renderReviews(data.reviews);

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
    resetTabs();
    if (reviewsSummaryEl) reviewsSummaryEl.innerHTML = "";
    if (reviewsListEl) reviewsListEl.innerHTML = "";
    reviewsBadgeEl?.classList.add("d-none");
  });
})();
