mergeInto(LibraryManager.library, {

    SS_RequestFullscreen: function () {
        try {
            var enabled = document.fullscreenEnabled
                || document.webkitFullscreenEnabled
                || document.mozFullScreenEnabled
                || document.msFullscreenEnabled;

            if (!enabled) {
                console.warn("[FullscreenBridge] fullscreen not allowed here - skipping request.");
                return;
            }

            var c = (typeof Module !== "undefined" && Module.canvas)
                ? Module.canvas
                : (document.querySelector("#unity-canvas") || document.querySelector("canvas"));
            if (!c) return;

            var req = c.requestFullscreen
                || c.webkitRequestFullscreen
                || c.mozRequestFullScreen
                || c.msRequestFullscreen;

            if (!req) return;

            var p = req.call(c);
            if (p && typeof p.catch === "function") {
                p.catch(function (e) {
                    console.warn("[FullscreenBridge] request rejected: " + e);
                });
            }
        } catch (e) {
            console.warn("[FullscreenBridge] request failed: " + e);
        }
    },

    SS_ExitFullscreen: function () {
        try {
            var exit = document.exitFullscreen
                || document.webkitExitFullscreen
                || document.mozCancelFullScreen
                || document.msExitFullscreen;

            if (exit) exit.call(document);
        } catch (e) {
            console.warn("[FullscreenBridge] exit failed: " + e);
        }
    },

    SS_IsFullscreen: function () {
        var el = document.fullscreenElement
            || document.webkitFullscreenElement
            || document.mozFullScreenElement
            || document.msFullscreenElement;

        return el ? 1 : 0;
    }

});
