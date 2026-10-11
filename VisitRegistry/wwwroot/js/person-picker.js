/* ==========================================================================
   جستجو و انتخاب شخص — ماژول قابل استفاده مجدد
   Data source is read from the [data-person-picker] container.
   ========================================================================== */
(() => {
    'use strict';

    const escapeHtml = (value) =>
        String(value ?? '')
            .replaceAll('&', '&amp;')
            .replaceAll('<', '&lt;')
            .replaceAll('>', '&gt;')
            .replaceAll('"', '&quot;')
            .replaceAll("'", '&#039;');

    document.querySelectorAll('[data-person-picker]').forEach((root) => {
        const searchInput = root.querySelector('.person-search-input');
        const hiddenId = root.querySelector('input[type="hidden"]');
        const results = root.querySelector('.person-search-results');
        const selectedBox = root.querySelector('.person-selected');
        const help = document.getElementById(root.dataset.helpId || '');
        const endpoint = root.dataset.endpoint;
        const form = document.getElementById(root.dataset.formId || '');

        if (!searchInput || !hiddenId || !results || !selectedBox || !endpoint) return;

        let timer = null;
        let controller = null;
        let items = [];
        let activeIndex = -1;

        const setHelp = (text, isError = false) => {
            if (!help) return;
            help.textContent = text;
            help.classList.toggle('field-error', isError);
        };

        const hideResults = () => {
            results.hidden = true;
            results.innerHTML = '';
            activeIndex = -1;
        };

        const clearSelected = () => {
            hiddenId.value = '';
            selectedBox.hidden = true;
            selectedBox.innerHTML = '';
        };

        const selectPerson = (person) => {
            hiddenId.value = person.id;
            searchInput.value = `${person.name} - ${person.nationalCode}`;
            selectedBox.innerHTML = `
                <div class="person-selected-name">${escapeHtml(person.name)}</div>
                <div class="person-selected-meta">کد ملی: ${escapeHtml(person.nationalCode)}${person.mobile ? ` | موبایل: ${escapeHtml(person.mobile)}` : ''}${person.activityUnit ? ` | واحد فعالیت: ${escapeHtml(person.activityUnit)}` : ''}</div>
                <button type="button" class="person-clear">تغییر شخص</button>`;
            selectedBox.hidden = false;
            setHelp('شخص انتخاب شد. برای تغییر، روی «تغییر شخص» بزنید.');

            selectedBox.querySelector('.person-clear').addEventListener('click', () => {
                clearSelected();
                searchInput.value = '';
                searchInput.focus();
                setHelp('حداقل ۲ حرف یا رقم وارد کنید.');
            });

            hideResults();
        };

        const highlight = (index) => {
            const nodes = results.querySelectorAll('.person-result');
            if (!nodes.length) return;
            activeIndex = (index + nodes.length) % nodes.length;
            nodes.forEach((node, i) => node.classList.toggle('active', i === activeIndex));
            nodes[activeIndex].scrollIntoView({ block: 'nearest' });
        };

        const render = (data) => {
            items = data;

            if (!items.length) {
                results.innerHTML = '<div class="person-no-result">شخصی با این مشخصات پیدا نشد.</div>';
                results.hidden = false;
                return;
            }

            results.innerHTML = items
                .map(
                    (person) => `
                    <button type="button" class="person-result" role="option" data-id="${escapeHtml(person.id)}">
                        <span class="person-result-name">${escapeHtml(person.name)}</span>
                        <span class="person-result-meta">کد ملی: ${escapeHtml(person.nationalCode)}${person.mobile ? ` | ${escapeHtml(person.mobile)}` : ''}</span>
                        ${person.activityUnit ? `<span class="person-result-unit">واحد فعالیت: ${escapeHtml(person.activityUnit)}</span>` : ''}
                    </button>`
                )
                .join('');

            results.querySelectorAll('.person-result').forEach((button) => {
                button.addEventListener('click', () => {
                    const person = items.find((x) => String(x.id) === button.dataset.id);
                    if (person) selectPerson(person);
                });
            });

            results.hidden = false;
            activeIndex = -1;
        };

        const search = async (term) => {
            controller?.abort();
            controller = new AbortController();
            results.innerHTML = '<div class="person-no-result">در حال جستجو...</div>';
            results.hidden = false;

            try {
                const response = await fetch(`${endpoint}?term=${encodeURIComponent(term)}`, {
                    headers: { 'X-Requested-With': 'XMLHttpRequest' },
                    signal: controller.signal
                });
                if (!response.ok) throw new Error('search-failed');
                render(await response.json());
            } catch (error) {
                if (error.name === 'AbortError') return;
                results.innerHTML = '<div class="person-no-result person-error">خطا در جستجوی اشخاص. دوباره تلاش کنید.</div>';
                results.hidden = false;
            }
        };

        searchInput.addEventListener('input', () => {
            clearSelected();
            const term = searchInput.value.trim();
            clearTimeout(timer);

            if (term.length < 2) {
                hideResults();
                setHelp('حداقل ۲ حرف یا رقم وارد کنید.');
                return;
            }

            setHelp('در حال جستجو...');
            timer = setTimeout(() => search(term), 300);
        });

        searchInput.addEventListener('focus', () => {
            if (searchInput.value.trim().length >= 2 && !hiddenId.value) {
                search(searchInput.value.trim());
            }
        });

        // ناوبری با صفحه‌کلید
        searchInput.addEventListener('keydown', (event) => {
            if (results.hidden || !items.length) return;

            if (event.key === 'ArrowDown') {
                event.preventDefault();
                highlight(activeIndex + 1);
            } else if (event.key === 'ArrowUp') {
                event.preventDefault();
                highlight(activeIndex - 1);
            } else if (event.key === 'Enter') {
                event.preventDefault();
                const person = items[activeIndex >= 0 ? activeIndex : 0];
                if (person) selectPerson(person);
            } else if (event.key === 'Escape') {
                hideResults();
            }
        });

        document.addEventListener('click', (event) => {
            if (!root.contains(event.target)) hideResults();
        });

        form?.addEventListener('submit', (event) => {
            if (hiddenId.value) return;
            event.preventDefault();
            clearSelected();
            searchInput.focus();
            setHelp('ابتدا یک شخص را از نتایج جستجو انتخاب کنید.', true);
        });
    });
})();
