pages.register('DirectEditing.Index', { ready: function() {
    window.urls = window.urls || {};
    var $valueSection = $("section.values");

    $valueSection.hide();

    function getPageUrl(page) {
        var requestedPage = window.urls[page];
        if (requestedPage)
            return requestedPage;

        alert("Requested page not found");
        console.log('tried to acces ' + page + ', key not found in pages');
        throw "Requested page not found";
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

    $(document).on("click", "h2", function() {
        var valueChildren = $(this).parent(".part").children(".values");
        var anyVisible = valueChildren.filter(":visible").length > 0;
        if (anyVisible)
            valueChildren.hide();
        else
            valueChildren.show();
    });

    function updateCreateValue(part, key, locale, content, callback) {
        $.ajax({
            url: getPageUrl('updateCreateValue'),
            data: { part: part, key: key, locale: locale, content: content },
            type: 'POST',
            success: callback,
            error: unblockUI
        });
    }

    function deleteValue(part, key, locale, callback) {
        $.ajax({
            url: getPageUrl('deleteValue'),
            data: { part: part, key: key, locale: locale },
            type: 'POST',
            success: callback,
            error: unblockUI
        });
    };

    function findSection(partName, createIfNotExists) {
        var section = $(".part[data-part='" + partName + "']");

        if (section.length > 0)
            return section;
        if (!createIfNotExists)
            return null;

        var partSection = $('<section class="part" data-part="' + partName + '"><h2><span class="title">' + partName + '</span> [<span class="count">1</span>]</h2><section class="values"></section></section>');

        var parts = partName.split('.');
        parts.splice(-1, 1);
        if (parts.length > 0) {
            var parentPartName = "";
            for (var i = 0; i < parts.length; i++)
                parentPartName += parts[i] + ".";
            parentPartName = parentPartName.substring(0, parentPartName.length - 1);

            var parentSection = findSection(parentPartName, createIfNotExists);
            partSection.appendTo(parentSection);
        } else {
            partSection.insertAfter($("section.general"));
        }

        return partSection;
    }

    function showValue(part, key, locale, content) {
        var partSection = findSection(part, true);
        var keyParagraph = partSection.find("[data-key='" + key + "'][data-locale='" + locale + "']");
        if (keyParagraph.length == 0) {
            keyParagraph = $('<p class="value" data-key="' + key + '" data-locale="' + locale + '"><span class="key">' + key + '</span><span class="locale"><nobr>' + locale + '</nobr></span><span class="content"></span><span class="clear"></span>');
            keyParagraph.appendTo(partSection.children(".values"));
            partSection.children("h2").find(".count").text(partSection.children(".values").children(".value").length);
        }
        keyParagraph.find(".content").text(content);
    }

    function remove(p) {
        var parents = p.parents(".part");
        p.remove();
        parents.each(function () {
            var parentPart = $(this);
            var totalValueCount = parentPart.find(".values").children(".value").length;
            var directValueCount = parentPart.children(".values").children(".value").length;
            if (totalValueCount == 0)
                $(this).remove();
            else
                $(this).children("h2").find(".count").text(directValueCount);
        });
    }

    var details = $.popup($("#details"));
    $(document).on("click", ".values > p", function() {
        var p = $(this);
        details.onOpen = function(elements) {
            var part = p.parent(".values").parent(".part").children("h2").children("span.title").text();
            var key = p.find(".key").text();
            var locale = p.find(".locale").text();
            var content = p.find(".content").text();

            var partEl = elements.find("input.part").val(part);
            var keyEl = elements.find("input.key").val(key);
            var localeEl = elements.find("input.locale").val(locale);
            var contentEl = elements.find("textarea.content").val(content);

            var update = function (newPart, newKey, newLocale, newContent, callback) {
                updateCreateValue(newPart, newKey, newLocale, newContent, function () {
                    showValue(partEl.val(), keyEl.val(), localeEl.val(), contentEl.val());
                    details.close();
                    if (callback)
                        callback();
                });
            };

            elements.find(".save, .updatecopy").click(function () {
                blockUI();
                var newPart = partEl.val().trim(), newKey = keyEl.val().trim(), newLocale = localeEl.val().trim(), newContent = contentEl.val();
                update(newPart, newKey, newLocale, newContent, unblockUI);
            });

            elements.find(".updaterecreate").click(function () {
                blockUI();
                var newPart = partEl.val().trim(), newKey = keyEl.val().trim(), newLocale = localeEl.val().trim(), newContent = contentEl.val();

                if (part == newPart && key == newKey && locale == newLocale) {
                    update(newPart, newKey, newLocale, newContent, unblockUI);
                } else {
                    deleteValue(part, key, locale, function () {
                        remove(p);
                        update(newPart, newKey, newLocale, newContent, unblockUI);
                    });
                }
            });

            elements.find(".updatedelete").click(function () {
                blockUI();
                deleteValue(part, key, locale, function() {
                    remove(p);
                    details.close();
                    unblockUI();
                });
            });

            elements.find(".part, .key, .locale").keyup(function() {
                var newPart = partEl.val().trim(), newKey = keyEl.val().trim(), newLocale = localeEl.val().trim(), newContent = contentEl.val();
                var qualifierChanged = part != newPart || key != newKey || locale != newLocale;
                elements.find(".save").toggle(!qualifierChanged);
                elements.find(".updaterecreate, .updatecopy").toggle(qualifierChanged);
            });
        };
        details.open();
    });

    

    $("#filterform").submit(function(event) {
        event.preventDefault();

        $(".values > p").show().removeAttr("data-hidden");
        $("section.part").show();

        $("input.filter").each(function() {
            var filter = $(this);
            var filterval = $(this).val().toLowerCase().trim();
            if (filterval.length == 0)
                return true;

            var filterspans = $("span." + filter.attr("id"));
            filterspans.each(function() {
                if ($(this).html().toLowerCase().indexOf(filterval) == -1)
                    $(this).parent("p").hide().attr("data-hidden", "");
            });
        });

        $("select.part").each(function() {
            var selectedPart = $(this).val();
            if (!selectedPart)
                return;

            var level = $(this).data("level");

            $(".values > p:not([data-hidden])").each(function () {
                var part = $(this).closest(".part").data("part");
                var splitPart = part.split(".");

                if (splitPart.length <= level || splitPart[level] != selectedPart)
                    $(this).hide().attr("data-hidden", "");
            });
        });

        $("section.part").each(function() {
            if ($(this).find("section.values > p:not([data-hidden])").length == 0)
                $(this).hide();
        });

        var totalValueCount = $(".values > p").length;
        var visibleValueCount = (totalValueCount - $(".values > p[data-hidden]").length);
        $(".result").html("displaying " + visibleValueCount + " out of " + totalValueCount + " values");
    });

    $("select.part").change(function() { $("#filterform").submit(); });
}});