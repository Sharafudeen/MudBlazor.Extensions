window.localStorageFunctions = {
    setItem: function (key, value) {
        localStorage.setItem(key, JSON.stringify(value));
    },
    getItem: function (key) {
        return JSON.parse(localStorage.getItem(key));
    },
    removeItem: function (key) {
        localStorage.removeItem(key);
    },
    getAllItems: function () {
        let keys = Object.keys(localStorage);
        let values = keys.map(k => JSON.parse(localStorage.getItem(k)));
        return values;
    },
    getAllThemeItems: function () {
        let keys = Object.keys(localStorage);
        let themeKeys = keys.filter(k => k.startsWith("theme-"));
        let themeItems = themeKeys.map(k => { return { key: k, value: JSON.parse(localStorage.getItem(k)) }; });
        return themeItems;
    }
};