// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

const prefers12Hour = (() => {
    try {
        const format = new Intl.DateTimeFormat(undefined, { hour: 'numeric' });
        const options = format.resolvedOptions();
        return options.hourCycle === 'h12';
    } catch {
        return false;
    }
})();

document.querySelectorAll('.utc-date').forEach(span => {
    const utc = span.dataset.utc;
    const date = new Date(utc);
    const formatted = date.toLocaleString(undefined, {
        day: 'numeric',
        month: 'long',
        year: 'numeric',
        hour: 'numeric',
        minute: '2-digit',
        hour12: prefers12Hour
    });
    span.textContent = formatted;
});

document.querySelectorAll('.utc-date-only').forEach(span => {
    const utc = span.dataset.utc;
    const date = new Date(utc);
    const formatted = date.toLocaleDateString(undefined, {
        day: 'numeric',
        month: 'long',
        year: 'numeric'
    });
    span.textContent = formatted;
});