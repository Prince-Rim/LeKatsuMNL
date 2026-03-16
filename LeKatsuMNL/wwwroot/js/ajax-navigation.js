/**
 * AJAX Navigation & History Management
 * 
 * Intercepts GET form submissions and pagination links to update page content
 * without full refreshes or browser history pollution.
 */

document.addEventListener('DOMContentLoaded', function () {
    const ajaxContainerSelector = '[data-ajax-container]';
    
    // Handle Form Submissions (Search/Filter)
    document.addEventListener('submit', function (e) {
        const form = e.target;
        if (form.method.toLowerCase() === 'get' && form.hasAttribute('data-ajax-form')) {
            e.preventDefault();
            const url = new URL(form.action || window.location.href);
            const formData = new FormData(form);
            const params = new URLSearchParams(formData);
            
            // Remove empty parameters
            for (const [key, value] of Array.from(params.entries())) {
                if (!value) params.delete(key);
            }
            
            url.search = params.toString();
            updateContent(url.toString());
        }
    });

    // Handle Pagination and other AJAX links
    document.addEventListener('click', function (e) {
        const link = e.target.closest('a');
        if (link && link.hasAttribute('data-ajax-link')) {
            e.preventDefault();
            updateContent(link.href);
        }
    });

    /**
     * Updates the AJAX container content and browser URL
     */
    async function updateContent(url) {
        const container = document.querySelector(ajaxContainerSelector);
        if (!container) return;

        // Add loading state
        container.style.opacity = '0.5';
        container.style.pointerEvents = 'none';

        try {
            const response = await fetch(url, {
                headers: { 'X-Requested-With': 'XMLHttpRequest' }
            });

            if (!response.ok) throw new Error('Network response was not ok');

            const html = await response.text();
            const parser = new DOMParser();
            const doc = parser.parseFromString(html, 'text/html');
            const newContent = doc.querySelector(ajaxContainerSelector);

            if (newContent) {
                container.innerHTML = newContent.innerHTML;
                
                // Update URL using replaceState to avoid history pollution
                window.history.replaceState(null, '', url);

                // Re-initialize specific scripts if needed
                reinitializeScripts(container);
            } else {
                // Fallback to full reload if container not found in response
                window.location.href = url;
            }
        } catch (error) {
            console.error('AJAX Load Error:', error);
            window.location.href = url; // Fallback to full reload
        } finally {
            container.style.opacity = '1';
            container.style.pointerEvents = 'auto';
        }
    }

    /**
     * Re-initializes event listeners and scripts for newly loaded content
     */
    function reinitializeScripts(container) {
        // Handle autofocus if present
        const focusEl = container.querySelector('[autofocus]');
        if (focusEl) {
            const val = focusEl.value;
            focusEl.value = '';
            focusEl.value = val;
            focusEl.focus();
        }

        // Search timeout re-binding for pages that define it
        // This is a bit tricky since scopes change, but most pages use global-ish scripts
        // Custom events can be dispatched if needed
        document.dispatchEvent(new CustomEvent('ajaxPageUpdated', { detail: { url: window.location.href } }));
    }
});
