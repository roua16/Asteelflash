



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

window.applyThemeClass = function (themeName) {
    var isDarkTheme = typeof themeName === 'string' && themeName.toLowerCase().includes('dark');

    document.body.classList.toggle('theme-dark', isDarkTheme);
    document.body.classList.toggle('theme-light', !isDarkTheme);
};