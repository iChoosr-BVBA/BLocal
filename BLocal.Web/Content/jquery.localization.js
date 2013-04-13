// define shortcut object
if(loc === undefined) {
    var loc = {
        val: function (key) {
            var local = localization.localizedValues[key];
            return local == null ? key : local.value;
        }
    };
}


// please use the LocalizedExtender with the JsLocalize method to automatically set me up

localization = {
    initialize: function (debugMode, changeUrl, defaultPart, locale) {
        localization.localizedValues = { };
        if ($ == undefined)
            throw ("debug support requires jquery on the '$' variable!");

        localization.debugMode = debugMode;
        localization.changeUrl = changeUrl;
        localization.defaultPart = defaultPart;
        localization.locale = locale;
        localization.store = new localization.Storage(defaultPart, locale);

        $(document).ready(function () {
            if (!debugMode)
                return;

            localization.overviewNode = $('<div id="loc-overview"><table><thead><tr><th>HTML</th><th>Attribute</th><th>Part</th><th>Key</th><th>Value</th></thead><tbody></tbody></table></div>');
            $('body').append(localization.overviewNode);
            localization.overviewNodeInside = localization.overviewNode.find("tbody");

            localization.overviewNodeOther = $('<div id="loc-overview-other"><table><thead><tr><th>Part</th><th>Key</th><th>Value</th></thead><tbody></tbody></table></div>');
            $('body').append(localization.overviewNodeOther);
            localization.overviewNodeOtherInside = localization.overviewNodeOther.find("tbody");

            localization.editorNode = $('<div id="loc-editor"><h3>title goes here</h3><textarea></textarea><p class="replacements"></p><input type="button" class="loc-edit" value="save changes and close" data-close="true" /> <input type="button" class="loc-edit" value="save changes and continue" /></div>');
            $('body').append(localization.editorNode);

            localization.localizedNodes = $("[data-loc-debug='true']");
            
            $(document).keyup(function (e) {
                if (e.keyCode == 13) { // enter
                    localization.showOverview();
                    localization.bindEnter(false);
                    return false;
                }
                if (e.keyCode == 27) { // esc
                    localization.overviewNode.fadeOut("slow");
                    localization.overviewNodeOther.fadeOut("slow");
                    localization.editorNode.fadeOut("slow");
                    return false;
                }
                return true;
            });

            localization.localizedNodes.bind("mouseover", function () {
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
            localization.localizedNodes.bind("mouseout", function () {
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
                        return parseFloat(value) - Math.floor(dHeight / 2) + "px";
                    },
                    marginBottom: function (index, value) {
                        return parseFloat(value) - Math.ceil(dHeight / 2) + "px";
                    },
                    marginLeft: function (index, value) {
                        return parseFloat(value) - Math.floor(dWidth / 2) + "px";
                    },
                    marginRight: function (index, value) {
                        return parseFloat(value) - Math.ceil(dWidth / 2) + "px";
                    }
                });
            });

            $(document).on("click", "#loc-overview tr.loc-ov-record", function () { localization.editValue($(this)); return false; });
            $(document).on("click", "#loc-overview-other tr.loc-ov-record", function () { localization.editValueOther($(this)); return false; });
            //$(document).on("click", "input.loc-edit", function () { });
        });
    },

    load: function (localizedValues) {
        localization.localizedValues = localizedValues;
    },

    setOtherValues: function (otherValues) {
        localization.otherValues = otherValues;

        $(document).ready(function () {
            for (var i = 0; i < localization.otherValues.length; i++) {
                var loc = localization.otherValues[i];
                loc.content = loc.origvalue;
                var part = loc.part;
                var key = loc.key;
                var content = loc.content;
                var row = $('<tr class="loc-ov-record"><td class="loc-ov-part" title="' + part + '">' + part + '</td><td class="loc-ov-key" title="' + key + '">' + key + '</td><td class="loc-ov-value" title="' + content + '">' + content + '</td></tr>');
                localization.overviewNodeOtherInside.append(row);
                row[0].loc = loc;
            }
        });
    },

    switchEditMode: function () {
        editorNode.toggleClass("loc-invisible");
    },

    showOverview: function () {
        localization.overviewNodeInside.empty();
        var hovered = $(".loc-hover");

        if (hovered.length) {
            hovered.each(function (index, value) {
                var node = new localization.LocalizedNode($(value));
                node.appendToOverview();
            });
            localization.overviewNode.fadeIn("slow");
        }
        else {
            localization.overviewNodeOther.fadeIn("slow");
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

    editValue: function (record) {
        if (record.length != 1)
            return;
        var loc = record[0].loc;

        var attrText = loc.attribute ? " " + loc.attribute : "";
        localization.editorNode.children("textarea").val(loc.content);
        localization.editorNode.children("h3").html("&lt;" + loc.nodename + attrText + "&gt; " + localization.locale + " - " + loc.part + " [" + loc.key + "]");
        localization.editorNode.children("input.loc-edit").unbind("click");

        var replacements = localization.editorNode.find("p.replacements");
        replacements.empty();
        $.each(loc.replacements, function (index, replacement) {
            replacements.append('<span>' + replacement.key + ' -> ' + replacement.value + ' |</span>');
        });

        localization.editorNode.children("input.loc-edit").click(function () {
            var close = $(this).attr("data-close");
            var newValue = localization.editorNode.find("textarea").val();
            localization.changeLocalization(loc.part, localization.locale, loc.key, newValue, loc.replacements, false, close);
        });

        localization.editorNode.fadeIn("slow");
    },

    editValueOther: function (record) {
        if (record.length != 1)
            return;
        var loc = record[0].loc;

        localization.editorNode.children("textarea").val(loc.content);
        localization.editorNode.children("h3").html(localization.locale + " - " + loc.part + " [" + loc.key + "]");
        localization.editorNode.children("input.loc-edit").unbind("click");

        localization.editorNode.children("input.loc-edit").click(function () {
            var close = $(this).attr("data-close");
            var newValue = localization.editorNode.find("textarea").val();
            localization.changeLocalization(loc.part, localization.locale, loc.key, newValue, [], false, close);
        });

        localization.editorNode.fadeIn("slow");
    },

    changeLocalization: function (prt, lcl, key, val, repl, reload, close) {
        $.ajax({
            type: 'POST',
            url: localization.changeUrl,
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
            var jsRef = localization.localizedValues[key];
            if (jsRef)
                jsRef.val = replVal;

            // update all possible attributes with the key
            $("[data-loc-localizations*='" + key + "']").each(function () {
                var curElem = $(this);
                var locs = localization.parseLocalizations(curElem);

                $.each(locs, function (index, loc) {
                    if (loc.key != key)
                        return true;

                    curElem.attr("data-loc-attribute-" + loc.attribute + "-content", val);
                    curElem.attr(loc.attribute, replVal);
                    return true;
                });
            });

            if (doClose) {
                localization.overviewNode.fadeOut("slow");
                localization.overviewNodeOther.fadeOut("slow");
            }
            localization.editorNode.fadeOut("slow");
        }
    },

    LocalizedNode: function (fromNode) {
        this.node = fromNode[0];
        this.nodeName = this.node.nodeName;
        this.htmlPart = fromNode.attr("data-loc-inner-part");
        this.htmlKey = fromNode.attr("data-loc-inner-key");
        this.htmlValue = fromNode.attr("data-loc-inner-value");
        this.localizations = localization.parseLocalizations(fromNode);
        this.replacements = localization.parseReplacements(fromNode);
        this.isValid = this.htmlKey != null || (this.localizations != null && this.localizations.length > 0);

        this.appendToOverview = function () {
            if (this.htmlKey)
                this.appendToOverviewNode(this.nodeName, "", this.htmlPart, this.htmlKey, this.htmlValue, localization.overviewNodeInside);

            var me = this;
            $.each(this.localizations, function (index, loc) {
                me.appendToOverviewNode(me.nodeName, loc.attribute, loc.part, loc.key, loc.content, localization.overviewNodeInside);
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
localization.Storage = function(part, locale) {
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

    me.forPart = function(newPart) {
        return new localization.Storage(newPart, locale);
    };
    me.forSubPart = function(subPart) {
        return new localization.Storage(part + "." + subPart, locale);
    };
    me.forLocale = function(newLocale) {
        return new localization.Storage(part, newLocale);
    };

    me.get = function (key, callback) {
        var found = null;
        if (hasStore) {
            $.each(parts, function() {
                var currentPart = this;

                found = store['[' + locale + "]" + currentPart + "-" + key];
                if (found != null) {
                    callback(found);
                    return false;
                }
                return true;
            });
        }
        if (found != null)
            return;

        localization.Storage.Repository.retrieveQualified(part, locale, key, function (qualifiers) {
            if (qualifiers == null || qualifiers.length == 0) {
                callback(null);
                return;
            }
            var qualifier = qualifiers[0];

            if (hasStore) {
                store['[' + qualifier.Locale + "]" + qualifier.Part + "-" + qualifier.Key] = qualifier.Value;
            }

            if(callback)
                callback(qualifier.Value);
        });
    };

    me.getMany = function (keys, callback) {
        if (!(keys && keys.length > 0))
            callback([]);
        
        var missingKeys = keys.slice(0);
        var result = {};
        
        if (hasStore) {
            $.each(keys, function() {
                var key = this;
                var found = null;
                
                $.each(parts, function() {
                    var currentPart = this;
                    found = store['[' + locale + "]" + currentPart + "-" + key];
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

        localization.Storage.Repository.retrieveQualified(part, locale, missingKeys, function (qualifiers) {
            $.each(qualifiers, function() {
                var qualifier = this;

                if (hasStore) {
                    store['[' + qualifier.Locale + "]" + qualifier.Part + "-" + qualifier.Key] = qualifier.Value;
                }
                result[qualifier.Key] = qualifier.Value;
            });
            callback(result);
        });
    };

    me.reset = function() {
        if (hasStore) {
            for (item in store) {
                store.removeItem(item);
            }
        }
    };
};

localization.Storage.Repository = {
    retrieveQualified: function(part, locale, keys, handler) {
        $.ajax({
            url: '/api/localization/qualifiedLocalizedValues',
            type: 'get',
            dataType: 'json',
            data: { part: part, locale: locale, keys: keys}
        }).success(function(data) {
            handler(data);
        }).error(function() {
            throw new Error("Could not retrieve localization for " + "[" + locale + "]" + part + "-" + key);
        });
    }
}