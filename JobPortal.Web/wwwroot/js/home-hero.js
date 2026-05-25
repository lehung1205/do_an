(function () {
  const mainPanel = document.getElementById('heroMainPanel');
  const bannerDefault = document.getElementById('heroBannerDefault');
  const center = document.getElementById('heroCenter');
  const categoryItems = document.querySelectorAll('[data-category-id]');
  const tagPanels = document.querySelectorAll('[data-category-tags]');
  const carouselEl = document.getElementById('heroCarousel');

  let activeCategoryId = null;
  let carouselInstance = null;

  if (carouselEl && typeof bootstrap !== 'undefined') {
    carouselEl.querySelectorAll('.jobsgo-hero__slide-img').forEach((img) => {
      const src = img.getAttribute('src');
      if (src) {
        const preload = new Image();
        preload.src = src;
      }
    });

    carouselInstance = bootstrap.Carousel.getOrCreateInstance(carouselEl, {
      interval: 5000,
      ride: 'carousel',
      touch: true
    });
  }

  function showCategoryTags(categoryId) {
    activeCategoryId = categoryId;

    categoryItems.forEach((btn) => {
      const active = btn.getAttribute('data-category-id') === categoryId;
      btn.classList.toggle('is-active', active);
    });

    tagPanels.forEach((panel) => {
      const show = panel.getAttribute('data-category-tags') === categoryId;
      panel.hidden = !show;
      panel.classList.toggle('is-visible', show);
    });

    if (bannerDefault) {
      bannerDefault.classList.add('is-hidden');
    }

    if (carouselInstance) {
      carouselInstance.pause();
    }
  }

  function showBannerDefault() {
    activeCategoryId = null;

    categoryItems.forEach((btn) => btn.classList.remove('is-active'));

    tagPanels.forEach((panel) => {
      panel.hidden = true;
      panel.classList.remove('is-visible');
    });

    if (bannerDefault) {
      bannerDefault.classList.remove('is-hidden');
    }

    if (carouselInstance) {
      carouselInstance.cycle();
    }
  }

  categoryItems.forEach((btn) => {
    const activate = () => {
      const id = btn.getAttribute('data-category-id');
      if (id) {
        showCategoryTags(id);
      }
    };

    btn.addEventListener('mouseenter', activate);
    btn.addEventListener('focus', activate);

    btn.addEventListener('click', (e) => {
      if (window.matchMedia('(hover: none)').matches) {
        e.preventDefault();
        const id = btn.getAttribute('data-category-id');
        if (id && activeCategoryId === id) {
          showBannerDefault();
        } else if (id) {
          showCategoryTags(id);
        }
      }
    });
  });

  if (center) {
    center.addEventListener('mouseenter', () => {
      if (activeCategoryId) {
        showCategoryTags(activeCategoryId);
      }
    });
  }

  if (mainPanel) {
    mainPanel.addEventListener('mouseleave', () => {
      showBannerDefault();
    });
  }

  const locationInput = document.getElementById('hero-location');
  const clearBtn = document.getElementById('hero-location-clear');

  function syncLocationClear() {
    if (!locationInput || !clearBtn) {
      return;
    }
    const hasValue = locationInput.value.trim().length > 0;
    clearBtn.classList.toggle('is-visible', hasValue);
    clearBtn.tabIndex = hasValue ? 0 : -1;
  }

  if (locationInput && clearBtn) {
    syncLocationClear();
    locationInput.addEventListener('input', syncLocationClear);
    clearBtn.addEventListener('click', (e) => {
      e.preventDefault();
      locationInput.value = '';
      syncLocationClear();
      locationInput.focus();
    });
  }
})();
