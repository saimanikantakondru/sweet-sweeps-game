mergeInto(LibraryManager.library, {

    GetUrlParam: function (paramNamePtr) {
        var paramName = UTF8ToString(paramNamePtr);
        var urlParams = new URLSearchParams(window.location.search);
        var value     = urlParams.get(paramName) || "";
        var bufSize   = lengthBytesUTF8(value) + 1;
        var buf       = _malloc(bufSize);
        stringToUTF8(value, buf, bufSize);
        return buf;
    }

});
