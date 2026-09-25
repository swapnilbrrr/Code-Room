(() => {
    const root = document.documentElement;
    const header = document.querySelector('.site-header');
    const themeToggle = document.querySelector('[data-theme-toggle]');
    const mobileToggle = document.querySelector('[data-mobile-toggle]');
    const mobileMenu = document.querySelector('[data-mobile-menu]');

    root.dataset.theme = localStorage.getItem('code-room-theme') || 'light';

    const updateThemeButton = () => {
        if (!themeToggle) return;
        const dark = root.dataset.theme === 'dark';
        themeToggle.textContent = dark ? '☀' : '☾';
        themeToggle.setAttribute('aria-label', dark ? 'Switch to light mode' : 'Switch to dark mode');
        themeToggle.setAttribute('title', dark ? 'Switch to light mode' : 'Switch to dark mode');
    };

    updateThemeButton();

    themeToggle?.addEventListener('click', () => {
        root.dataset.theme = root.dataset.theme === 'dark' ? 'light' : 'dark';
        localStorage.setItem('code-room-theme', root.dataset.theme);
        updateThemeButton();
    });

    let lastScrollY = window.scrollY;

    window.addEventListener('scroll', () => {
        const currentScrollY = window.scrollY;
        if (!header) return;

        if (currentScrollY <= 20 || currentScrollY < lastScrollY) {
            header.classList.remove('is-hidden');
        } else if (currentScrollY > lastScrollY + 4) {
            header.classList.add('is-hidden');
        }

        lastScrollY = currentScrollY;
    }, { passive: true });

    mobileToggle?.addEventListener('click', () => {
        const expanded = mobileToggle.getAttribute('aria-expanded') === 'true';
        mobileToggle.setAttribute('aria-expanded', String(!expanded));
        mobileMenu?.classList.toggle('is-open', !expanded);
    });

    mobileMenu?.querySelectorAll('a').forEach(link => {
        link.addEventListener('click', () => {
            mobileMenu.classList.remove('is-open');
            mobileToggle?.setAttribute('aria-expanded', 'false');
        });
    });

    const search = document.querySelector('[data-course-search]');
    const category = document.querySelector('[data-course-category]');
    const level = document.querySelector('[data-course-level]');
    const cards = [...document.querySelectorAll('[data-course-card]')];
    const empty = document.querySelector('[data-course-empty]');

    const filterCourses = () => {
        if (!cards.length) return;

        const query = (search?.value || '').trim().toLowerCase();
        const selectedCategory = category?.value || 'all';
        const selectedLevel = level?.value || 'all';
        let visible = 0;

        cards.forEach(card => {
            const text = card.textContent.toLowerCase();
            const matchesSearch = !query || text.includes(query);
            const matchesCategory = selectedCategory === 'all' || card.dataset.category === selectedCategory;
            const matchesLevel = selectedLevel === 'all' || card.dataset.level === selectedLevel;
            const show = matchesSearch && matchesCategory && matchesLevel;

            card.hidden = !show;
            if (show) visible += 1;
        });

        if (empty) empty.hidden = visible !== 0;
    };

    search?.addEventListener('input', filterCourses);
    category?.addEventListener('change', filterCourses);
    level?.addEventListener('change', filterCourses);
    filterCourses();
})();
