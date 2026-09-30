/* ==========================================================================
   سامانه ثبت مراجعات — اسکریپت اصلی
   Theme, navigation, toasts, confirm dialogs, table engine, pickers, UX.
   ========================================================================== */
(() => {
    'use strict';

    const THEME_KEY = 'vr.theme';
    const PAGE_SIZE_KEY = 'vr.pageSize';

    const $ = (selector, scope = document) => scope.querySelector(selector);
    const $$ = (selector, scope = document) => Array.from(scope.querySelectorAll(selector));

    const prefersDark = window.matchMedia('(prefers-color-scheme: dark)');

    /* ----------------------------------------------------------------------
       1. پوسته روشن / تاریک
       ---------------------------------------------------------------------- */
    const Theme = {
        get() {
            try {
                return localStorage.getItem(THEME_KEY);
            } catch {
                return null;
            }
        },
        set(mode) {
            document.documentElement.setAttribute('data-theme', mode);
            try {
                localStorage.setItem(THEME_KEY, mode);
            } catch {
                /* حالت خصوصی مرورگر */
            }
            $$('[data-theme-toggle]').forEach((btn) => {
                btn.setAttribute('aria-pressed', String(mode === 'dark'));
                btn.title = mode === 'dark' ? 'تغییر به پوسته روشن' : 'تغییر به پوسته تاریک';
            });
        },
        current() {
            return document.documentElement.getAttribute('data-theme') || 'light';
        },
        toggle() {
            this.set(this.current() === 'dark' ? 'light' : 'dark');
        }
    };

    // اعمال پوسته پیش از رندر برای جلوگیری از پرش رنگ
    Theme.set(Theme.get() || (prefersDark.matches ? 'dark' : 'light'));
    prefersDark.addEventListener('change', (event) => {
        if (!Theme.get()) Theme.set(event.matches ? 'dark' : 'light');
    });

    const initThemeToggles = () => {
        $$('[data-theme-toggle]').forEach((btn) => {
            btn.setAttribute('aria-pressed', String(Theme.current() === 'dark'));
            btn.title = Theme.current() === 'dark' ? 'تغییر به پوسته روشن' : 'تغییر به پوسته تاریک';
            btn.addEventListener('click', () => Theme.toggle());
        });
    };

    /* ----------------------------------------------------------------------
       2. منوی کناری (Off-canvas)
       ---------------------------------------------------------------------- */
    const initSidebar = () => {
        const toggle = $('#menu-toggle');
        const sidebar = $('#main-sidebar');
        const overlay = $('#sidebar-overlay');
        const closeButton = $('#sidebar-close');
        if (!toggle || !sidebar || !overlay) return;

        const isOpen = () => sidebar.classList.contains('open');

        const setMenu = (open) => {
            sidebar.classList.toggle('open', open);
            overlay.classList.toggle('show', open);
            toggle.setAttribute('aria-expanded', String(open));
            document.body.classList.toggle('menu-open', open);
        };

        toggle.addEventListener('click', () => setMenu(!isOpen()));
        overlay.addEventListener('click', () => setMenu(false));
        closeButton?.addEventListener('click', () => setMenu(false));
        $$('a', sidebar).forEach((link) => link.addEventListener('click', () => setMenu(false)));

        document.addEventListener('keydown', (event) => {
            if (event.key === 'Escape' && isOpen()) {
                setMenu(false);
                toggle.focus();
            }
        });

        // فعال‌سازی خودکار لینک منطبق با مسیر جاری
        const current = window.location.pathname.replace(/\/+$/, '') || '/';
        $$('a', sidebar).forEach((link) => {
            const path = new URL(link.href, window.location.origin).pathname.replace(/\/+$/, '') || '/';
            if (path === current) {
                link.classList.add('active');
                link.setAttribute('aria-current', 'page');
            }
        });
    };

    /* ----------------------------------------------------------------------
       3. منوی کشویی کاربر
       ---------------------------------------------------------------------- */
    const initDropdowns = () => {
        $$('[data-dropdown]').forEach((trigger) => {
            const menu = document.getElementById(trigger.dataset.dropdown);
            if (!menu) return;

            const setOpen = (open) => {
                trigger.setAttribute('aria-expanded', String(open));
                menu.classList.toggle('show', open);
            };

            trigger.addEventListener('click', (event) => {
                event.stopPropagation();
                const willOpen = !menu.classList.contains('show');
                $$('[data-dropdown]').forEach((other) => {
                    const otherMenu = document.getElementById(other.dataset.dropdown);
                    otherMenu?.classList.remove('show');
                    other.setAttribute('aria-expanded', 'false');
                });
                setOpen(willOpen);
            });

            document.addEventListener('click', (event) => {
                if (!menu.contains(event.target) && event.target !== trigger) setOpen(false);
            });
        });

        document.addEventListener('keydown', (event) => {
            if (event.key !== 'Escape') return;
            $$('[data-dropdown]').forEach((trigger) => {
                trigger.setAttribute('aria-expanded', 'false');
                document.getElementById(trigger.dataset.dropdown)?.classList.remove('show');
            });
        });
    };

    /* ----------------------------------------------------------------------
       4. پیام شناور (Toast)
       ---------------------------------------------------------------------- */
    const ICONS = {
        success: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M20 6 9 17l-5-5"/></svg>',
        danger: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><path d="m15 9-6 6M9 9l6 6"/></svg>',
        warning: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M10.3 3.9 1.8 18a2 2 0 0 0 1.7 3h17a2 2 0 0 0 1.7-3L13.7 3.9a2 2 0 0 0-3.4 0Z"/><path d="M12 9v4M12 17h.01"/></svg>',
        info: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><path d="M12 16v-4M12 8h.01"/></svg>'
    };

    const Toast = {
        show(message, type = 'info', timeout = 5200) {
            let stack = $('.toast-stack');
            if (!stack) {
                stack = document.createElement('div');
                stack.className = 'toast-stack';
                stack.setAttribute('role', 'status');
                stack.setAttribute('aria-live', 'polite');
                document.body.appendChild(stack);
            }

            const toast = document.createElement('div');
            toast.className = `toast ${type}`;
            toast.innerHTML = `${ICONS[type] || ICONS.info}<div></div>`;
            toast.lastElementChild.textContent = message;
            stack.appendChild(toast);

            const remove = () => {
                toast.classList.add('hide');
                setTimeout(() => toast.remove(), 240);
            };
            setTimeout(remove, timeout);
            toast.addEventListener('click', remove);
        }
    };

    // تبدیل پیام‌های TempData به پیام شناور
    const initAlerts = () => {
        $$('.alert[data-auto-dismiss]').forEach((alert) => {
            const type = alert.classList.contains('success')
                ? 'success'
                : alert.classList.contains('danger')
                    ? 'danger'
                    : alert.classList.contains('warning')
                        ? 'warning'
                        : 'info';

            const close = $('.close', alert);
            close?.addEventListener('click', () => alert.remove());

            Toast.show(alert.textContent.trim(), type, 5200);
            setTimeout(() => {
                alert.classList.add('hide');
                setTimeout(() => alert.remove(), 240);
            }, 5200);
        });

        // نمایش خطاهای اعتبارسنجی به‌صورت پیام شناور
        const summary = $('.validation-summary-errors');
        if (summary) {
            const messages = $$('li', summary).map((li) => li.textContent.trim()).filter(Boolean);
            if (messages.length) {
                summary.hidden = true;
                Toast.show(messages.join(' — '), 'danger', 7000);
            }
        }
    };

    /* ----------------------------------------------------------------------
       5. دیالوگ تأیید (جایگزان confirm مرورگر)
       ---------------------------------------------------------------------- */
    const Confirm = (() => {
        let backdrop = null;
        let resolver = null;

        const build = () => {
            backdrop = document.createElement('div');
            backdrop.className = 'modal-backdrop';
            backdrop.setAttribute('role', 'dialog');
            backdrop.setAttribute('aria-modal', 'true');
            backdrop.innerHTML = `
                <div class="modal">
                    <div class="modal-head">
                        <span class="modal-icon danger" aria-hidden="true">${ICONS.danger}</span>
                        <h3>تأیید عملیات</h3>
                    </div>
                    <div class="modal-body" data-confirm-text></div>
                    <div class="modal-foot">
                        <button type="button" class="button danger" data-confirm-ok>بله، انجام شود</button>
                        <button type="button" class="button secondary" data-confirm-cancel>انصراف</button>
                    </div>
                </div>`;
            document.body.appendChild(backdrop);

            $('[data-confirm-ok]', backdrop).addEventListener('click', () => close(true));
            $('[data-confirm-cancel]', backdrop).addEventListener('click', () => close(false));
            backdrop.addEventListener('mousedown', (event) => {
                if (event.target === backdrop) close(false);
            });
            document.addEventListener('keydown', (event) => {
                if (event.key === 'Escape' && backdrop.classList.contains('show')) close(false);
            });
        };

        const close = (result) => {
            backdrop?.classList.remove('show');
            const done = resolver;
            resolver = null;
            setTimeout(() => {
                backdrop?.remove();
                backdrop = null;
            }, 200);
            done?.(result);
        };

        return (message) => {
            if (!backdrop) build();
            $('[data-confirm-text]', backdrop).textContent = message;
            backdrop.classList.add('show');
            setTimeout(() => $('[data-confirm-ok]', backdrop).focus(), 60);
            return new Promise((resolve) => {
                resolver = resolve;
            });
        };
    })();

    const initConfirmForms = () => {
        document.addEventListener('submit', (event) => {
            const form = event.target;
            if (!(form instanceof HTMLFormElement)) return;

            const message = form.dataset.confirm;
            if (!message) return;

            if (form.dataset.confirmAccepted === '1') {
                delete form.dataset.confirmAccepted;
                return;
            }

            event.preventDefault();
            const submitter = event.submitter;
            Confirm(message).then((ok) => {
                if (!ok) return;
                form.dataset.confirmAccepted = '1';
                if (submitter && typeof form.requestSubmit === 'function') form.requestSubmit(submitter);
                else form.submit();
            });
        });
    };

    /* ----------------------------------------------------------------------
       6. نوار جستجو
       ---------------------------------------------------------------------- */
    const initSearchBars = () => {
        $$('.searchbar').forEach((bar) => {
            const input = $('input[type="search"], input[type="text"]', bar);
            const clear = $('.clear-search', bar);
            if (!input) return;

            const sync = () => clear?.classList.toggle('show', input.value.length > 0);
            sync();

            input.addEventListener('input', sync);
            clear?.addEventListener('click', () => {
                input.value = '';
                sync();
                input.focus();
                bar.requestSubmit?.();
            });
        });
    };

    /* ----------------------------------------------------------------------
       7. موتور جدول: مرتب‌سازی، جستجوی زنده و صفحه‌بندی
       ---------------------------------------------------------------------- */
    const initTables = () => {
        $$('table[data-table]').forEach((table) => {
            const card = table.closest('.card');
            const body = $('tbody', table);
            if (!body) return;

            const originalRows = $$('tr', body).filter((row) => !row.dataset.emptyRow);
            const emptyRow = $('tr[data-empty-row]', body);
            const toolbar = card ? $('.js-table-search', card) : null;
            const filterInput = toolbar ? $('input', toolbar) : null;
            const filterClear = toolbar ? $('.clear-search', toolbar) : null;
            const countEl = $('.js-row-count', card);
            const footer = $('.js-pager-wrap', card);
            const pager = $('.js-pager', card);
            const perPageSelect = $('.js-page-size', card);

            const state = {
                page: 1,
                perPage: Number(perPageSelect?.value) || 15,
                query: '',
                sortIndex: -1,
                sortDir: 0
            };

            const cellText = (row, index) => {
                const cell = row.cells[index];
                if (!cell) return '';
                const clone = cell.cloneNode(true);
                clone.querySelectorAll('form, button, .actions').forEach((node) => node.remove());
                return (clone.textContent || '').trim();
            };

            const render = () => {
                let rows = originalRows.slice();

                if (state.query) {
                    const needle = state.query.toLowerCase();
                    rows = rows.filter((row) => row.textContent.toLowerCase().includes(needle));
                }

                if (state.sortIndex >= 0 && state.sortDir !== 0) {
                    const index = state.sortIndex;
                    const dir = state.sortDir;
                    rows.sort((a, b) => {
                        const result = cellText(a, index).localeCompare(cellText(b), 'fa', { numeric: true });
                        return result === 0 ? 0 : result * dir;
                    });
                }

                const total = rows.length;
                const pages = Math.max(1, Math.ceil(total / state.perPage));
                if (state.page > pages) state.page = pages;
                const start = (state.page - 1) * state.perPage;
                const visible = new Set(rows.slice(start, start + state.perPage));

                originalRows.forEach((row) => {
                    row.hidden = !visible.has(row);
                });

                if (emptyRow) emptyRow.hidden = total !== 0;

                if (countEl) {
                    countEl.innerHTML = total === 0
                        ? 'بدون رکورد'
                        : `نمایش <strong>${(start + 1).toLocaleString('fa-IR')}</strong> تا <strong>${Math.min(start + state.perPage, total).toLocaleString('fa-IR')}</strong> از <strong>${total.toLocaleString('fa-IR')}</strong> رکورد`;
                }

                if (pager) {
                    pager.innerHTML = '';
                    const addButton = (label, page, options = {}) => {
                        const btn = document.createElement('button');
                        btn.type = 'button';
                        btn.textContent = label;
                        if (options.active) {
                            btn.classList.add('active');
                            btn.setAttribute('aria-current', 'page');
                        }
                        if (options.disabled) btn.disabled = true;
                        btn.addEventListener('click', () => {
                            state.page = page;
                            render();
                            card?.scrollIntoView({ behavior: 'smooth', block: 'start' });
                        });
                        pager.appendChild(btn);
                        return btn;
                    };

                    addButton('قبلی', state.page - 1, { disabled: state.page === 1 });

                    const numbers = [];
                    for (let i = 1; i <= pages; i += 1) {
                        if (i === 1 || i === pages || Math.abs(i - state.page) <= 1) numbers.push(i);
                        else if (numbers[numbers.length - 1] !== '…') numbers.push('…');
                    }
                    numbers.forEach((n) => {
                        if (n === '…') {
                            const span = document.createElement('span');
                            span.textContent = '…';
                            span.style.opacity = '.5';
                            span.style.padding = '0 4px';
                            pager.appendChild(span);
                        } else {
                            addButton(n.toLocaleString('fa-IR'), n, { active: n === state.page });
                        }
                    });

                    addButton('بعدی', state.page + 1, { disabled: state.page === pages });
                }

                if (footer) footer.hidden = pages <= 1;
            };

            // مرتب‌سازی بر اساس شماره واقعی ستون
            const headerCells = $$('thead tr:first-child th', table);
            headerCells.forEach((th) => {
                if (!th.classList.contains('sortable')) return;

                const index = Array.prototype.indexOf.call(th.parentElement.cells, th);
                if (index < 0) return;

                th.setAttribute('tabindex', '0');
                th.setAttribute('role', 'button');

                const sort = () => {
                    if (state.sortIndex === index) {
                        state.sortDir = state.sortDir === 1 ? -1 : state.sortDir === -1 ? 0 : 1;
                    } else {
                        state.sortIndex = index;
                        state.sortDir = 1;
                    }
                    headerCells.forEach((other) => other.classList.remove('asc', 'desc'));
                    if (state.sortDir === 1) th.classList.add('asc');
                    if (state.sortDir === -1) th.classList.add('desc');
                    state.page = 1;
                    render();
                };
                th.addEventListener('click', sort);
                th.addEventListener('keydown', (event) => {
                    if (event.key === 'Enter' || event.key === ' ') {
                        event.preventDefault();
                        sort();
                    }
                });
            });

            // جستجوی زنده درون جدول
            const syncFilter = () => filterClear?.classList.toggle('show', !!filterInput?.value);

            const applyFilter = () => {
                state.query = filterInput.value.trim();
                state.page = 1;
                syncFilter();
                render();
            };

            filterInput?.addEventListener('input', applyFilter);

            filterClear?.addEventListener('click', () => {
                if (!filterInput) return;
                filterInput.value = '';
                applyFilter();
                filterInput.focus();
            });

            if (filterInput) syncFilter();

            perPageSelect?.addEventListener('change', () => {
                state.perPage = Number(perPageSelect.value) || 15;
                state.page = 1;
                try {
                    localStorage.setItem(PAGE_SIZE_KEY, String(state.perPage));
                } catch {
                    /* نادیده گرفته می‌شود */
                }
                render();
            });

            // به یادآوری تعداد سطر در هر صفحه
            try {
                const saved = localStorage.getItem(PAGE_SIZE_KEY);
                if (saved && perPageSelect && [...perPageSelect.options].some((o) => o.value === saved)) {
                    perPageSelect.value = saved;
                    state.perPage = Number(saved);
                }
            } catch {
                /* نادیده گرفته می‌شود */
            }

            render();
        });
    };

    /* ----------------------------------------------------------------------
       8. انتخابگر تاریخ شمسی
       ---------------------------------------------------------------------- */
    const initDatePickers = () => {
        const pickerInputs = $$('[data-jalali-picker]');
        if (!pickerInputs.length || typeof window.jQuery === 'undefined') return;

        const $ = window.jQuery;
        if (typeof $.fn.pDatepicker !== 'function') return;

        pickerInputs.forEach((input) => {
            const options = {
                viewMode: 'date',
                minViewMode: 'date',
                format: 'YYYY/MM/DD',
                calendar: 'jalali',
                showOtherMonths: true,
                todayBtn: true,
                clearBtn: false,
                submitBtn: true,
                submitText: 'تایید',
                todayText: 'امروز',
                displayMode: 'datepicker',
                orientation: 'auto',
                autoPosition: true
            };

            const min = input.dataset.jalaliMin;
            const max = input.dataset.jalaliMax;
            if (min) options.minDate = min;
            if (max) options.maxDate = max;

            try {
                $(input).pDatepicker(options);
            } catch {
                /* در صورت خطا، ورودی متنی ساده باقی می‌ماند */
            }

            // دکمه میانبر بازکننده تقویم
            const group = input.closest('.field-with-btn');
            const button = group ? $('.field-btn', group) : null;
            button?.addEventListener('click', () => {
                try {
                    $(input).pDatepicker('show');
                } catch {
                    input.focus();
                }
            });

            // اعتبارسنجی سبک در سمت کاربر
            input.addEventListener('blur', () => {
                const value = input.value.trim();
                input.classList.remove('input-error');
                if (!value) return;
                if (!/^\d{2,4}\/\d{1,2}\/\d{1,2}$/.test(value)) input.classList.add('input-error');
            });
        });
    };

    /* ----------------------------------------------------------------------
       9. انتخابگر ساعت
       ---------------------------------------------------------------------- */
    const initTimePickers = () => {
        const inputs = $$('[data-time-picker]');
        if (!inputs.length || typeof window.jQuery === 'undefined') return;

        const $ = window.jQuery;
        if (typeof $.fn.timepicker !== 'function') return;

        inputs.forEach((input) => {
            // مقدار اولیه را به قالب HH:mm تبدیل می‌کنیم
            const raw = input.value.trim();
            const match = raw.match(/^(\d{1,2}):(\d{2})/);
            if (match) {
                input.value = `${match[1].padStart(2, '0')}:${match[2]}`;
            } else if (raw) {
                const now = new Date();
                input.value = `${String(now.getHours()).padStart(2, '0')}:${String(now.getMinutes()).padStart(2, '0')}`;
            }

            try {
                $(input).timepicker({
                    showMeridian: false,
                    showInputs: false,
                    showSeconds: false,
                    showClear: false,
                    template: false,
                    minuteStep: 1,
                    secondStep: 1,
                    disableMousewheel: false,
                    scrollDefault: false
                });
            } catch {
                /* در صورت خطا، ورودی متنی ساده باقی می‌ماند */
            }

            const group = input.closest('.field-with-btn');
            const button = group ? $('.field-btn', group) : null;
            button?.addEventListener('click', () => {
                try {
                    $(input).timepicker('showWidget');
                } catch {
                    input.focus();
                }
            });
        });
    };

    /* ----------------------------------------------------------------------
       10. وضعیت بارگذاری هنگام ارسال فرم
       ---------------------------------------------------------------------- */
    const initFormLoading = () => {
        const loading = $('.form-loading');
        document.addEventListener('submit', (event) => {
            const form = event.target;
            if (!(form instanceof HTMLFormElement)) return;
            if (form.dataset.noLoading === '1') return;
            if (form.dataset.confirm && form.dataset.confirmAccepted !== '1') return;
            if (form.method.toLowerCase() === 'get') return;

            if (!loading) {
                const box = document.createElement('div');
                box.className = 'form-loading';
                box.innerHTML = '<div class="loader-box"><span class="spinner"></span><span>در حال ثبت اطلاعات...</span></div>';
                document.body.appendChild(box);
                return void box.classList.add('show');
            }
            loading.classList.add('show');
        });
    };

    /* ----------------------------------------------------------------------
       11. چاپ صفحه — پیش از چاپ، تمام سطرهای مخفی جدول نمایش داده می‌شوند
       ---------------------------------------------------------------------- */
    const initPrint = () => {
        window.addEventListener('beforeprint', () => {
            $$('table[data-table] tr[hidden]').forEach((row) => {
                row.dataset.printRestore = '1';
                row.hidden = false;
            });
        });

        window.addEventListener('afterprint', () => {
            $$('tr[data-print-restore]').forEach((row) => {
                row.hidden = true;
                delete row.dataset.printRestore;
            });
        });

        $$('[data-print]').forEach((button) => {
            button.addEventListener('click', () => window.print());
        });
    };

    /* ----------------------------------------------------------------------
       12. دکمه بازگشت به بالا
       ---------------------------------------------------------------------- */
    const initToTop = () => {
        const button = $('#to-top');
        if (!button) return;

        const sync = () => button.classList.toggle('show', window.scrollY > 320);
        window.addEventListener('scroll', sync, { passive: true });
        sync();

        button.addEventListener('click', () => window.scrollTo({ top: 0, behavior: 'smooth' }));
    };

    /* ----------------------------------------------------------------------
       13. راه‌اندازی
       ---------------------------------------------------------------------- */
    const boot = () => {
        initThemeToggles();
        initSidebar();
        initDropdowns();
        initAlerts();
        initConfirmForms();
        initSearchBars();
        initTables();
        initDatePickers();
        initTimePickers();
        initFormLoading();
        initPrint();
        initToTop();
    };

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', boot);
    } else {
        boot();
    }

    // در دسترس قرار دادن پیام شناور برای اسکریپت‌های داخل صفحه
    window.appToast = Toast.show;
})();
