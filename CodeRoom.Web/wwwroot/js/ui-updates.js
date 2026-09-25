(() => {
    const closeMenus = () => {
        document.querySelectorAll('[data-account-menu], [data-notification-menu]').forEach(menu => {
            menu.hidden = true;
        });
        document.querySelectorAll('[data-account-toggle], [data-notification-toggle]').forEach(button => {
            button.setAttribute('aria-expanded', 'false');
        });
    };

    const wireMenu = (toggleSelector, menuSelector) => {
        const toggle = document.querySelector(toggleSelector);
        const menu = document.querySelector(menuSelector);
        if (!toggle || !menu) return;

        toggle.addEventListener('click', event => {
            event.stopPropagation();
            const wasHidden = menu.hidden;
            closeMenus();
            menu.hidden = !wasHidden;
            toggle.setAttribute('aria-expanded', String(wasHidden));
        });

        menu.addEventListener('click', event => {
            event.stopPropagation();
        });
    };

    wireMenu('[data-account-toggle]', '[data-account-menu]');
    wireMenu('[data-notification-toggle]', '[data-notification-menu]');

    document.addEventListener('click', closeMenus);
    document.addEventListener('keydown', event => {
        if (event.key === 'Escape') closeMenus();
    });

    document.querySelectorAll('[data-password-toggle]').forEach(button => {
        button.addEventListener('click', () => {
            const field = button.previousElementSibling;
            if (!field || field.tagName !== 'INPUT') return;

            const showing = field.type === 'text';
            field.type = showing ? 'password' : 'text';
            button.setAttribute('aria-label', showing ? 'Show password' : 'Hide password');
            button.title = showing ? 'Show password' : 'Hide password';
            button.classList.toggle('is-visible', !showing);
        });
    });

    const toast = document.querySelector('[data-toast]');
    if (toast) {
        const close = toast.querySelector('[data-toast-close]');
        const dismiss = () => toast.remove();
        close?.addEventListener('click', dismiss);
        window.setTimeout(dismiss, 5000);
    }
})();
