// define shortcut object
if (loc === undefined) {
    var loc = {
        val: function (key) {
            var local = blocal.localizedValues[key];
            return local == null ? key : local.value;
        }
    };
}


// please use the LocalizedExtender with the JsLocalize method to automatically set me up

blocal = {
    initialize: function (debugMode, changeUrl, retrieveQualifiedUrl, defaultPart, locale) {
        blocal.localizedValues = {};
        if ($ == undefined)
            throw ("debug support requires jquery on the '$' variable!");

        blocal.debugMode = debugMode;
        blocal.changeUrl = changeUrl;
        blocal.retrieveQualifiedUrl = retrieveQualifiedUrl;
        blocal.defaultPart = defaultPart;
        blocal.locale = locale;
        blocal.store = new blocal.Storage(defaultPart, locale);

        $(document).ready(function () {
            if (!debugMode)
                return;

            blocal.overviewNode = $('<div id="loc-overview"><table><thead><tr><th>HTML</th><th>Attribute</th><th>Part</th><th>Key</th><th>Value</th></thead><tbody></tbody></table></div>');
            $('body').append(blocal.overviewNode);
            blocal.overviewNodeInside = blocal.overviewNode.find("tbody");

            blocal.overviewNodeOther = $('<div id="loc-overview-other"><table><thead><tr><th>Part</th><th>Key</th><th>Value</th></thead><tbody></tbody></table></div>');
            $('body').append(blocal.overviewNodeOther);
            blocal.overviewNodeOtherInside = blocal.overviewNodeOther.find("tbody");

            blocal.editorNode = $('<div id="loc-editor"><h3>title goes here</h3><textarea></textarea><p class="replacements"></p><input type="button" class="loc-edit" value="save changes and close" data-close="true" /> <input type="button" class="loc-edit" value="save changes and continue" /></div>');
            $('body').append(blocal.editorNode);

            blocal.localizedNodes = $("[data-loc-debug='true']");

            $(document).keyup(function (e) {
                if (e.keyCode == 13) { // enter
                    blocal.showOverview();
                    return false;
                }
                if (e.keyCode == 27) { // esc
                    blocal.overviewNode.fadeOut("slow");
                    blocal.overviewNodeOther.fadeOut("slow");
                    blocal.editorNode.fadeOut("slow");
                    return false;
                }
                return true;
            });

            blocal.localizedNodes.bind("mouseover", function () {
                var height = $(this).outerHeight(true);
                var width = $(this).outerWidth(true);

                $(this).addClass("loc-hover");
                if (this.tagName == 'SELECT') {
                    $(this).children('option').addClass("loc-hover");
                }

                var dHeight = $(this).outerHeight(true) - height;
                var dWidth = $(this).outerWidth(true) - width;

                $(this).css({
                    marginTop: function (index, value) {
                        return parseFloat(value) - Math.ceil(dHeight / 2) + "px";
                    },
                    marginBottom: function (index, value) {
                        return parseFloat(value) - Math.floor(dHeight / 2) + "px";
                    },
                    marginLeft: function (index, value) {
                        return parseFloat(value) - Math.ceil(dWidth / 2) + "px";
                    },
                    marginRight: function (index, value) {
                        return parseFloat(value) - Math.floor(dWidth / 2) + "px";
                    }
                });
            });
            blocal.localizedNodes.bind("mouseout", function () {
                var height = $(this).outerHeight(true);
                var width = $(this).outerWidth(true);

                $(this).removeClass("loc-hover");
                if (this.tagName == 'SELECT') {
                    $(this).children('option').removeClass("loc-hover");
                }

                var dHeight = $(this).outerHeight(true) - height;
                var dWidth = $(this).outerWidth(true) - width;

                $(this).css({
                    marginTop: function (index, value) {
                        return parseFloat(value) - Math.ceil(dHeight / 2) + "px";
                    },
                    marginBottom: function (index, value) {
                        return parseFloat(value) - Math.floor(dHeight / 2) + "px";
                    },
                    marginLeft: function (index, value) {
                        return parseFloat(value) - Math.ceil(dWidth / 2) + "px";
                    },
                    marginRight: function (index, value) {
                        return parseFloat(value) - Math.floor(dWidth / 2) + "px";
                    }
                });
            });

            $(document).on("click", "#loc-overview tr.loc-ov-record", function () { blocal.editValue($(this)); return false; });
            $(document).on("click", "#loc-overview-other tr.loc-ov-record", function () { blocal.editValueOther($(this)); return false; });
            //$(document).on("click", "input.loc-edit", function () { });
        });
    },

    load: function (localizedValues) {
        blocal.localizedValues = localizedValues;
    },

    setOtherValues: function (otherValues) {
        blocal.otherValues = otherValues;

        $(document).ready(function () {
            for (var i = 0; i < blocal.otherValues.length; i++) {
                var loc = blocal.otherValues[i];
                loc.content = loc.origvalue;
                var part = loc.part;
                var key = loc.key;
                var content = loc.content;
                var row = $('<tr class="loc-ov-record"><td class="loc-ov-part" title="' + part + '">' + part + '</td><td class="loc-ov-key" title="' + key + '">' + key + '</td><td class="loc-ov-value" title="' + content + '">' + content + '</td></tr>');
                blocal.overviewNodeOtherInside.append(row);
                row[0].loc = loc;
            }
        });
    },

    switchEditMode: function () {
        editorNode.toggleClass("loc-invisible");
    },

    showOverview: function () {
        blocal.overviewNodeInside.empty();
        var hovered = $(".loc-hover");

        if (hovered.length) {
            hovered.each(function (index, value) {
                var node = new blocal.LocalizedNode($(value));
                node.appendToOverview();
            });
            blocal.overviewNode.fadeIn("slow");
        }
        else {
            blocal.overviewNodeOther.fadeIn("slow");
        }
    },

    parseLocalizations: function (node) {
        var localizations = new Array();
        var valueSeparator = String.fromCharCode(30);
        var pairSeparator = String.fromCharCode(31);
        var localizationString = node.attr("data-loc-localizations");
        if (!localizationString)
            return localizations;

        $.each(localizationString.split(pairSeparator), function (index, atp) {
            if (!atp)
                return true;

            var attrPair = atp.split(valueSeparator);
            var attrName = attrPair[0];
            var attrKey = attrPair[1];
            localizations.push({
                attribute: attrName,
                key: attrKey,
                part: $(node).attr("data-loc-attribute-" + attrName + "-part"),
                content: $(node).attr("data-loc-attribute-" + attrName + "-content")
            });
            return true;
        });
        return localizations;
    },

    parseReplacements: function (node) {
        var replacements = new Array();
        var valueSeparator = String.fromCharCode(30);
        var pairSeparator = String.fromCharCode(31);
        var replacementString = node.attr("data-loc-replacements");
        if (!replacementString)
            return replacements;

        $.each(replacementString.split(pairSeparator), function (index, kvp) {
            if (!kvp)
                return true;

            var keyValuePair = kvp.split(valueSeparator);
            replacements.push({
                key: keyValuePair[0],
                value: keyValuePair[1]
            });
            return true;
        });
        return replacements;
    },

    retrieveQualifiedValue: function (part, locale, keys, handler) {
        $.ajax({
            url: blocal.retrieveQualifiedUrl,
            type: 'get',
            dataType: 'json',
            data: { part: part, locale: locale, keys: keys }
        }).success(function (data) {
            handler(data);
        }).error(function () {
            throw new Error("Connection error retrieving localization for " + "[" + locale + "]" + part + "-" + keys);
        });
    },

    editValue: function (record) {
        if (record.length != 1)
            return;
        var loc = record[0].loc;

        var attrText = loc.attribute ? " " + loc.attribute : "";
        blocal.editorNode.children("textarea").val(loc.content);
        blocal.editorNode.children("h3").html("&lt;" + loc.nodename + attrText + "&gt; " + blocal.locale + " - " + loc.part + " [" + loc.key + "]");
        blocal.editorNode.children("input.loc-edit").unbind("click");

        var replacements = blocal.editorNode.find("p.replacements");
        replacements.empty();
        $.each(loc.replacements, function (index, replacement) {
            replacements.append('<span>' + replacement.key + ' -> ' + replacement.value + ' |</span>');
        });

        blocal.editorNode.children("input.loc-edit").click(function () {
            var close = $(this).attr("data-close");
            var newValue = blocal.editorNode.find("textarea").val();
            blocal.changeLocalization(loc.part, blocal.locale, loc.key, newValue, loc.replacements, false, close);
        });

        blocal.editorNode.fadeIn("slow");
    },

    editValueOther: function (record) {
        if (record.length != 1)
            return;
        var loc = record[0].loc;

        blocal.editorNode.children("textarea").val(loc.content);
        blocal.editorNode.children("h3").html(blocal.locale + " - " + loc.part + " [" + loc.key + "]");
        blocal.editorNode.children("input.loc-edit").unbind("click");

        blocal.editorNode.children("input.loc-edit").click(function () {
            var close = $(this).attr("data-close");
            var newValue = blocal.editorNode.find("textarea").val();
            blocal.changeblocal(loc.part, blocal.locale, loc.key, newValue, [], false, close);
        });

        blocal.editorNode.fadeIn("slow");
    },

    changeLocalization: function (prt, lcl, key, val, repl, reload, close) {
        $.ajax({
            type: 'POST',
            url: blocal.changeUrl,
            data: { part: prt, locale: lcl, key: key, value: val },
            success: function (data) {
                if (data.Success)
                    showUpdate(data, close);
                else
                    alert("The system did not save your value!");
            },
            error: function (notthe, problem) {
                if (problem != "error")
                    alert("Could not communicate with the system!");
                location.reload();
            },
            dataType: 'json'
        });

        function showUpdate(data, doClose) {
            if (reload)
                location.reload();

            var replVal = data.Value;
            $.each(repl, function (index, rep) {
                replVal = replVal.replace(rep.key, rep.value);
            });

            // update inner htmls
            $("[data-loc-inner-key='" + key + "']").each(function () {
                $(this).html(replVal);
                $(this).attr("data-loc-inner-value", val);
            });

            // update javascript references
            var jsRef = blocal.localizedValues[key];
            if (jsRef)
                jsRef.val = replVal;

            // update all possible attributes with the key
            $("[data-loc-localizations*='" + key + "']").each(function () {
                var curElem = $(this);
                var locs = blocal.parseLocalizations(curElem);

                $.each(locs, function (index, loc) {
                    if (loc.key != key)
                        return true;

                    curElem.attr("data-loc-attribute-" + loc.attribute + "-content", val);
                    curElem.attr(loc.attribute, replVal);
                    return true;
                });
            });

            if (doClose) {
                blocal.overviewNode.fadeOut("slow");
                blocal.overviewNodeOther.fadeOut("slow");
            }
            blocal.editorNode.fadeOut("slow");
        }
    },

    LocalizedNode: function (fromNode) {
        this.node = fromNode[0];
        this.nodeName = this.node.nodeName;
        this.htmlPart = fromNode.attr("data-loc-inner-part");
        this.htmlKey = fromNode.attr("data-loc-inner-key");
        this.htmlValue = fromNode.attr("data-loc-inner-value");
        this.localizations = blocal.parseLocalizations(fromNode);
        this.replacements = blocal.parseReplacements(fromNode);
        this.isValid = this.htmlKey != null || (this.localizations != null && this.localizations.length > 0);

        this.appendToOverview = function () {
            if (this.htmlKey)
                this.appendToOverviewNode(this.nodeName, "", this.htmlPart, this.htmlKey, this.htmlValue, blocal.overviewNodeInside);

            var me = this;
            $.each(this.localizations, function (index, loc) {
                me.appendToOverviewNode(me.nodeName, loc.attribute, loc.part, loc.key, loc.content, blocal.overviewNodeInside);
            });
        };

        this.appendToOverviewNode = function (nodeName, attributeName, part, key, content, appendingNode) {
            if (!this.isValid)
                return;

            var appendingHtml = '<tr class="loc-ov-record"><td class="loc-ov-html" title="' + nodeName + '">' + nodeName + '<td class="loc-ov-attr" title="' + attributeName + '">' + attributeName + '</td><td class="loc-ov-part" title="' + part + '">' + part + '</td><td class="loc-ov-key" title="' + key + '">' + key + '</td><td class="loc-ov-value" title="' + content + '">' + content + '</td></tr>';
            appendingNode.append(appendingHtml);
            var node = appendingNode.children().last()[0];

            var loc = {
                nodename: nodeName,
                attribute: attributeName,
                part: part,
                key: key,
                content: content,
                replacements: this.replacements
            };

            node.loc = loc;
        };
    }
};

var store = window.localStorage;
var hasStore = window.localStorage != null;
blocal.Storage = function (part, locale) {
    var me = this;
    var partPieces = part.split(".");
    var parts = [], i, j;

    for (i = 0; i < partPieces.length; i++) {
        var constructPart = "";
        for (j = 0; j <= i; j++) {
            constructPart += partPieces[j] + ".";
        }
        parts[i] = constructPart.substr(0, constructPart.length - 1);
    }

    me.forPart = function (newPart) {
        return new blocal.Storage(newPart, locale);
    };
    me.forSubPart = function (subPart) {
        return new blocal.Storage(part + "." + subPart, locale);
    };
    me.forLocale = function (newLocale) {
        return new blocal.Storage(part, newLocale);
    };

    me.get = function (key, callback) {
        var found = null;
        if (hasStore) {
            $.each(parts, function () {
                var currentPart = this;

                found = store['blocal[' + locale + "]" + currentPart + "-" + key];
                if (found != null) {
                    callback(found);
                    return false;
                }
                return true;
            });
        }
        if (found != null)
            return;

        blocal.retrieveQualifiedValue(part, locale, key, function (qualifiers) {
            if (qualifiers == null || qualifiers.length == 0) {
                callback(null);
                return;
            }
            var qualifier = qualifiers[0];

            if (hasStore) {
                store['blocal[' + qualifier.Locale + "]" + qualifier.Part + "-" + qualifier.Key] = qualifier.Value;
            }

            if (callback)
                callback(qualifier.Value);
        });
    };

    me.getMany = function (keys, callback) {
        if (!(keys && keys.length > 0))
            callback([]);

        var missingKeys = keys.slice(0);
        var result = {};

        if (hasStore) {
            $.each(keys, function () {
                var key = this;
                var found = null;

                $.each(parts, function () {
                    var currentPart = this;
                    found = store['blocal[' + locale + "]" + currentPart + "-" + key];
                    return found == null;
                });

                if (found != null) {
                    missingKeys.splice(missingKeys.indexOf(key + ''), 1);
                    result[key] = found;
                }
            });
        }

        if (!(missingKeys && missingKeys.length > 0)) {
            callback(result);
            return;
        }

        blocal.retrieveQualifiedValues(part, locale, missingKeys, function (qualifiers) {
            $.each(qualifiers, function () {
                var qualifier = this;

                if (hasStore) {
                    store['blocal[' + qualifier.Locale + "]" + qualifier.Part + "-" + qualifier.Key] = qualifier.Value.Content;
                }
                result[qualifier.Key] = qualifier.Value;
            });
            callback(result);
        });
    };

    me.reset = function () {
        if (hasStore) {
            for (item in store) {
                store.removeItem(item);
            }
        }
    };
};
