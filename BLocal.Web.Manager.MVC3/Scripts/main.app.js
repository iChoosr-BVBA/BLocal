var currentPageName = document.getElementsByTagName("html")[0].attributes["data-page"].value;

pages = {
    register: function(pageName, page) {
        if (currentPageName != pageName)
            return;

        $(document).ready(function () {
            if(page.ready)
                page.ready();
        });
    }
}