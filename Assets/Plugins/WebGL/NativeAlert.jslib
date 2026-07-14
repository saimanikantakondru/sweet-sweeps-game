mergeInto(LibraryManager.library, {
    ShowNativeAlert: function(messagePtr) {
        var message = UTF8ToString(messagePtr);
        if (confirm(message)) {
        }
    }
});