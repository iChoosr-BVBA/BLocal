pages.register('ManualSynchronization.Index', { ready: function () {
    window.urls = window.urls || {};

    $("button.select").click(function () {
        $(this).parents(".general").find("input[type=checkbox]").prop("checked", true);
    });

    $("button.deselect").click(function () {
        $(this).parents(".general").find("input[type=checkbox]").prop("checked", false);
    });

    $("#authorcheck").change(function () {
        if ($(this).is(":checked")) {
            var searchAuthor = $(this).data("author");

            $("[data-author]").addClass("hidden").each(function() {
                var authors = $(this).data("author").split('|');
                for (var i = 0; i < authors.length; i++) {
                    if (authors[i] === searchAuthor) {
                        $(this).removeClass("hidden");
                        break;
                    }
                }

            });
        } else {
            $("[data-author]").removeClass("hidden");
        }
    }).change();

    $("button.delete, button.duplicate").click(function () {
        var remove = $(this).hasClass("delete");
        var checkboxes = $(this).parents(".general").find("input[type=checkbox]:checked");
        if (!confirm((remove ? "Delete" : "Duplicate") + " " + checkboxes.length + " items?"))
            return true;
        blockUI();

        var data = {};
        var counter = 0;
        checkboxes.each(function () {
            var affectedSide = (($(this).attr("data-side") === "left") === remove) ? "Right" : "Left";
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

    $("button.accept").click(function () {
        var resolvedDifferences = $(".difference input:checked");
        var leftResolvedDifferences = resolvedDifferences.filter("[data-affected-side='left']");
        var rightResolvedDifferences = resolvedDifferences.filter("[data-affected-side='right']");

        var confirmation = "";
        if (leftResolvedDifferences.length > 0) {
            confirmation += leftResolvedDifferences.length + " changes on " + leftResolvedDifferences.first().attr("data-provider");
        }
        if (rightResolvedDifferences.length > 0) {
            if (confirmation !== "")
                confirmation += " and ";
            confirmation += rightResolvedDifferences.length + " changes on " + rightResolvedDifferences.first().attr("data-provider");
        }
        if (confirmation.length == 0) {
            alert("no changes detected");
            return true;
        }
        if (!confirm("execute " + confirmation + "?"))
            return true;

        var data = {};
        var counter = 0;
        resolvedDifferences.each(function () {
            var affectedSide = $(this).attr("data-affected-side");
            data["items[" + counter + "].side"] = affectedSide;
            data["items[" + counter + "].part"] = $(this).attr("data-part");
            data["items[" + counter + "].key"] = $(this).attr("data-key");
            data["items[" + counter + "].locale"] = $(this).attr("data-locale");
            counter++;
        });
        $.ajax({
            url: urls.update,
            data: data,
            type: 'POST',
            success: function () {
                resolvedDifferences.parents(".difference").remove();
                unblockUI();
            }
        });
        return true;
    });

    $(".comparison input[type=checkbox]").change(function () {
        var newAffectedSide = "";
        if ($(this).is(":checked")) {
            newAffectedSide = $(this).attr("data-affected-side");
        }
        var checkboxes = $(this).parents(".difference").find("[type=checkbox]");
        checkboxes.prop("checked", false);
        checkboxes.filter("[data-affected-side='" + newAffectedSide + "']").prop("checked", true);
        $(this).parents(".difference").removeClass("left").removeClass("right").addClass(newAffectedSide);
    });

}});