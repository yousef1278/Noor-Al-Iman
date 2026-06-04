// Quran Center - Site JavaScript

// Auto-dismiss alerts after 4 seconds
document.addEventListener('DOMContentLoaded', function () {
    setTimeout(function () {
        document.querySelectorAll('.alert.alert-success, .alert.alert-danger').forEach(function (el) {
            var bsAlert = new bootstrap.Alert(el);
            bsAlert.close();
        });
    }, 4000);

    // Force date inputs to English (LTR) regardless of page lang
    document.querySelectorAll('input[type="date"]').forEach(function (el) {
        el.setAttribute('lang', 'en');
        el.style.direction = 'ltr';
        el.style.textAlign = 'left';
    });

    // Highlight active sidebar link
    var path = window.location.pathname.toLowerCase();
    document.querySelectorAll('.sidebar .nav-link').forEach(function (link) {
        var href = link.getAttribute('href')?.toLowerCase();
        if (href && path.startsWith(href) && href !== '/') {
            link.classList.add('active');
        }
    });

    // Confirm before delete form submissions
    document.querySelectorAll('form[asp-action="Delete"], form[action*="Delete"]').forEach(function (form) {
        form.addEventListener('submit', function (e) {
            if (!confirm('Are you sure you want to delete this record?')) {
                e.preventDefault();
            }
        });
    });

    // Animate stat cards on dashboard
    document.querySelectorAll('.card .fs-4.fw-bold').forEach(function (el) {
        var target = parseInt(el.textContent, 10);
        if (isNaN(target)) return;
        var current = 0;
        var step = Math.max(1, Math.floor(target / 20));
        var timer = setInterval(function () {
            current = Math.min(current + step, target);
            el.textContent = current;
            if (current >= target) clearInterval(timer);
        }, 50);
    });

    // Live table search for Students page
    var searchInput = document.querySelector('input[name="search"]');
    if (searchInput) {
        searchInput.addEventListener('input', function () {
            var val = this.value.toLowerCase();
            document.querySelectorAll('tbody tr').forEach(function (row) {
                var text = row.textContent.toLowerCase();
                row.style.display = text.includes(val) ? '' : 'none';
            });
        });
    }
});
