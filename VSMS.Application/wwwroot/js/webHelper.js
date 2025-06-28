window.webHelper = {
    reloadPage: function () {
        location.reload();
    },

    goBack: function () {
        window.history.back();
    },

    reloadAfterReconnect: function () {
        if (!sessionStorage.getItem("reloadedAfterReconnect")) {
            sessionStorage.setItem("reloadedAfterReconnect", "true");
            setTimeout(() => {
                webHelper.reloadPage();
            }, 500);
        }
    }
};