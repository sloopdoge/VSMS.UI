window.timezoneInterop = {
    getTimeZone: () => Intl.DateTimeFormat().resolvedOptions().timeZone
};