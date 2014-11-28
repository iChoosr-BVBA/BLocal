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

var statusCodeResponses = {
    403: function() { alert("You are no longer logged in. Please go back to the home page, where we will ask you to log in again."); }
}

$(document).ajaxError(function (event, jqXhr, settings, thrownError) {
    console.log(thrownError);
    console.log(jqXhr);

    if (statusCodeResponses[jqXhr.status])
        statusCodeResponses[jqXhr.status]();
    else
        alert("Something went wrong. Please go \"home\" and try again from there. If the problem persists, contact IT.");
});