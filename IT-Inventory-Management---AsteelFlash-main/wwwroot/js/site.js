



window.scrollToElement = function (elementId) {
    var element = document.getElementById(elementId);
    if (element) {
        element.scrollIntoView({ behavior: 'smooth', block: 'start' });
        return true;
    }
    return false;
};



window.preventEnterKeyFormSubmission = function () {
    document.addEventListener('keydown', function (event) {
        if (event.key === 'Enter' && event.target.tagName.toLowerCase() !== 'textarea') {
            event.preventDefault();
            return false;
        }
    });
};

window.themeManager = window.themeManager || {
    storageKey: 'itstockm.theme',
    legacyStorageKey: 'theme',

    normalizeTheme: function (value) {
        if (typeof value !== 'string') {
            return null;
        }

        var normalized = value.trim().toLowerCase();
        if (!normalized) {
            return null;
        }

        if (normalized === 'dark' || normalized === 'humanistic-dark' || normalized.includes('dark')) {
            return 'dark';
        }

        if (normalized === 'light' || normalized === 'humanistic' || normalized.includes('light')) {
            return 'light';
        }

        return null;
    },

    getStoredTheme: function () {
        var modernValue = null;
        var legacyValue = null;

        try {
            modernValue = localStorage.getItem(this.storageKey);
            legacyValue = localStorage.getItem(this.legacyStorageKey);
        } catch {
            return null;
        }

        return this.normalizeTheme(modernValue) || this.normalizeTheme(legacyValue);
    },

    setStoredTheme: function (theme) {
        var normalized = this.normalizeTheme(theme);
        if (!normalized) {
            return;
        }

        try {
            localStorage.setItem(this.storageKey, normalized);
            localStorage.setItem(this.legacyStorageKey, normalized === 'dark' ? 'humanistic-dark' : 'humanistic');
        } catch {
        }
    },

    getSystemTheme: function () {
        try {
            return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
        } catch {
            return 'dark';
        }
    },

    applyTheme: function (theme) {
        var normalized = this.normalizeTheme(theme) || 'dark';
        var isDarkTheme = normalized === 'dark';

        var root = document.documentElement;
        var body = document.body;
        if (!body) {
            return;
        }

        root.classList.toggle('theme-dark', isDarkTheme);
        root.classList.toggle('theme-light', !isDarkTheme);
        root.classList.toggle('dark', isDarkTheme);
        root.setAttribute('data-theme', normalized);

        body.classList.toggle('theme-dark', isDarkTheme);
        body.classList.toggle('theme-light', !isDarkTheme);
        body.classList.toggle('dark', isDarkTheme);
        body.classList.add('theme-animate');
        body.setAttribute('data-theme', normalized);
    }
};

window.applyThemeClass = function (themeName) {
    window.themeManager.applyTheme(themeName);
};

(function applyInitialTheme() {
    var preferredTheme = window.themeManager.getStoredTheme() || window.themeManager.getSystemTheme();
    window.themeManager.applyTheme(preferredTheme);
})();
