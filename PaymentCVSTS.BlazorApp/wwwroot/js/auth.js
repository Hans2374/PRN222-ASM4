// wwwroot/js/auth.js
window.authFunctions = {
    logout: function () {
        document.cookie.split(';').forEach(function (c) {
            document.cookie = c.trim().split('=')[0] + '=;' + 'expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;';
        });
        location.href = '/Account/Login';
        return true;
    }
};