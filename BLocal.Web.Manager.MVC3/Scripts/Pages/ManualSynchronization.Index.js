pages.register('ManualSynchronization.Index', { ready: function () {
    window.urls = window.urls || {};

    $("button.select").click(function () {
        $(this).parents(".general").find("input[type=checkbox]").prop("checked", true);
    });

    $("button.deselect").click(function () {
        $(this).parents(".general").find("input[type=checkbox]").prop("checked", false);
    });

    $("button.delete, button.duplicate").click(function () {
        blockUI();
        var remove = $(this).hasClass("delete");
        var checkboxes = $(this).parents(".general").find("input[type=checkbox]:checked");
        if (!confirm((remove ? "Delete" : "Duplicate") + " " + checkboxes.length + " items?"))
            return true;

        var data = {};
        var counter = 0;
        checkboxes.each(function () {
            var affectedSide = (($(this).attr("data-side") === "Left") === remove) ? "Right" : "Left";
            data["items[" + counter + "].side"] = affectedSide;
            data["items[" + counter + "].part"] = $(this).attr("data-part");
            data["items[" + counter + "].key"] = $(this).attr("data-key");
            data["items[" + counter + "].locale"] = $(this).attr("data-locale");
            counter++;
        });


        $.ajax({
            url: remove ? window.urls.remove : window.urls.duplicate,
            data: data,
            type: 'POST',
            success: function () {
                window.location.reload();
            }
        });

        return true;
    });

    $("tr").click(function (e) {
        if ($(e.originalEvent.srcElement).is("input"))
            return true;

        var check = $(this).find("input[type=checkbox]");
        check.prop("checked", function (i, val) { return !val; });

        return true;
    });

    $(".locktoggle").click(function () {
        var toggler = $(this);
        var isOn = toggler.is(".on");
        if (isOn) {
            toggler.removeClass("on");
            $("section.comparison button").prop("disabled", true);
        } else {
            toggler.addClass("on");
            $("section.comparison button").prop("disabled", false);
        }
        var swap = toggler.attr("value");
        toggler.attr("value", toggler.attr("data-alt")).attr("data-alt", swap);
    });

    $("section.comparison button").click(function () {
        blockUI();
        var element = $(this).parents(".difference");
        $.ajax({
            url: urls.update,
            data: {
                'items[0].side': $(this).attr("data-side"),
                'items[0].part': $(this).attr("data-part"),
                'items[0].key': $(this).attr("data-key"),
                'items[0].locale': $(this).attr("data-locale")
            },
            type: 'POST',
            success: function () {
                element.remove();
                unblockUI();
            }
        });
    });
    
}});