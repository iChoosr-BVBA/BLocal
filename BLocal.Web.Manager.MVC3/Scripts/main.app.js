$(document).ready(function() {
    this.pages = this.pages || {};
    var $valueSection = $("section.values");
    
    $valueSection.hide();

    function getPageUrl(page) {
        var requestedPage = pages[page];
        if (requestedPage)
            return requestedPage;
        
        alert("Requested page not found");
        console.log('tried to acces ' + page + ', key not found in pages');
        return window.location;
    }

    $(".open").click(function() {
        $valueSection.show();
    });
    
    $(".collapse").click(function() {
        $valueSection.hide();
    });
    
    $(".reload").click(function() {
        window.location.reload();
    });
    
    $(".hardreload").click(function() {
         window.location = getPageUrl('current');
    });

     $("h2").click(function () {
         var valueChildren = $(this).parent(".part").find(".values");
         var anyVisible = valueChildren.filter(":visible").length > 0;
         if (anyVisible)
             valueChildren.hide();
         else
             valueChildren.show();
     });

     function createKey(part, key, locale, content, callback) {
         $.ajax({
             url: getPageUrl('createKey'),
             data: { part: part, key: key, locale: locale, content: content },
             type: 'POST',
             success: callback,
             error: function () {
                 alert("Something went wrong. Check your internet connection and try again. If the problem persists, contact IT.");
             }
         });
     }
     
     function removeKey(part, key, locale, callback) {
         $.ajax({
             url: getPageUrl('removeKey'),
             data: { part: part, key: key, locale: locale },
             type: 'POST',
             success: callback,
             error: function () {
                 alert("Something went wrong. Check your internet connection and try again. If the problem persists, contact IT.");
             }
         });
     };

     var details = $.popup($("#details"));
     $(".values > p").click(function () {
         var p = $(this);
         details.onOpen = function (elements) {
             var part = p.parent(".values").parent(".part").children("h2").children("span.title").text();
             var key = p.find(".key").text();
             var locale = p.find(".locale").text();
             var content = p.find(".content").text();

             var partEl = elements.find("input.part").val(part);
             var keyEl = elements.find("input.key").val(key);
             var localeEl = elements.find("input.locale").val(locale);
             var contentEl = elements.find("textarea.content").val(content);

             elements.find(".updatecopy").click(function () {
                 createKey(partEl.val(), keyEl.val(), localeEl.val(), contentEl.val(), function () {
                     if (partEl.val() == part && localeEl.val() == locale && keyEl.val() == key)
                         p.find(".content").val(contentEl.val());
                     details.close();
                 });
             });
             elements.find(".updaterecreate").click(function () {
                 removeKey(part, key, locale, function () {
                     createKey(partEl.val(), keyEl.val(), localeEl.val(), contentEl.val(), function () {
                         if (partEl.val() == part && localeEl.val() == locale && keyEl.val() == key)
                             p.find(".content").val(contentEl.val());
                         else
                             p.remove();
                         details.close();
                     });
                 });
             });
             elements.find(".updatedelete").click(function () {
                 removeKey(part, key, locale, function () {
                     p.remove();
                     details.close();
                 });
             });
         };
         details.open();
     });

     $("#filterform").submit(function (event) {
         event.preventDefault();

         $(".values > p").show().removeAttr("data-hidden");
         $("section.part").show();

         $("input.filter").each(function () {
             var filter = $(this);
             var filterval = $(this).val().toLowerCase();
             if (filterval.length == 0)
                 return true;

             var filterspans = $("span." + filter.attr("id"));
             filterspans.each(function () {
                 if ($(this).html().toLowerCase().indexOf(filterval) == -1)
                     $(this).parent("p").hide().attr("data-hidden", "");
             });

             return false;
         });

         $("section.part").each(function () {
             if ($(this).find("section.values > p:not([data-hidden])").length == 0)
                 $(this).hide();
         });
         var totalValueCount = $(".values > p").length;
         var visibleValueCount = (totalValueCount - $(".values > p[data-hidden]").length);
         $(".result").html("displaying " + visibleValueCount + " out of " + totalValueCount + " values");
         if (visibleValueCount < 500)
             $("span.open").click();
     });
});