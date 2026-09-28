(() => {
    const toggle = document.getElementById('menu-toggle');
    const sidebar = document.getElementById('main-sidebar');
    const overlay = document.getElementById('sidebar-overlay');
    const closeButton = document.getElementById('sidebar-close');

    if (!toggle || !sidebar || !overlay) return;

    const setMenu = (open) => {
        sidebar.classList.toggle('open', open);
        overlay.classList.toggle('show', open);
        toggle.setAttribute('aria-expanded', String(open));
        document.body.classList.toggle('menu-open', open);
    };

    toggle.addEventListener('click', (event) => {
        event.preventDefault();
        setMenu(!sidebar.classList.contains('open'));
    });

    overlay.addEventListener('click', () => setMenu(false));
    closeButton?.addEventListener('click', () => setMenu(false));

    sidebar.querySelectorAll('a').forEach((link) => {
        link.addEventListener('click', () => setMenu(false));
    });

    document.addEventListener('keydown', (event) => {
        if (event.key === 'Escape' && sidebar.classList.contains('open')) {
            setMenu(false);
            toggle.focus();
        }
    });

    const currentPath = window.location.pathname.replace(/\/$/, '') || '/';
    sidebar.querySelectorAll('a').forEach((link) => {
        const href = new URL(link.href, window.location.origin).pathname.replace(/\/$/, '') || '/';
        if (href === currentPath) {
            link.classList.add('active');
            link.setAttribute('aria-current', 'page');
        }
    });
})();
