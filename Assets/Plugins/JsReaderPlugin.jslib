mergeInto(LibraryManager.library,
    {
        InjectionJs: function (url) {
            url = Pointer_stringify(url);
            var s = document.createElement("script");
            s.setAttribute('src', url);
            document.head.appendChild(s);
        },

        InjectionCSS: function (url) {
            url = Pointer_stringify(url);
            var s = document.createElement("link");
            s.setAttribute('href', url);
            s.setAttribute('rel', 'stylesheet');
            document.head.appendChild(s);
        },


    });