mergeInto(LibraryManager.library, {
    IsMobileBrowser: function() {
        var ua = navigator.userAgent || "";
        var uaMobile = /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(ua);

        var coarseOnly = false;
        if (window.matchMedia) {
            var hasCoarse = window.matchMedia("(any-pointer: coarse)").matches;
            var hasFine = window.matchMedia("(any-pointer: fine)").matches;
            coarseOnly = hasCoarse && !hasFine;
        }

        return uaMobile || coarseOnly;
    }
});