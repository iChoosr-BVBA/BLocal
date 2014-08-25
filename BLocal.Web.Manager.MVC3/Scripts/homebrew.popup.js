$(document).ready(function () {
    $.popup = function (elements) {
        var overlayCode = '<div style="height:100%; width:100%; position: fixed; top: 0; left: 0; background-color: rgba(0, 0, 0, 0.6)"></div>';
        var innerCode = '<div style="min-height: 20px; min-width: 20px; max-width: 95%; margin-top: 50px; margin-left: auto; margin-right: auto; background-color: rgba(0, 0, 0, 0)"></div>';
        var me = {};
        var open = false;

        me.onOpen = function () { };
        me.onClose = function () { };
        me.onOpened = function () { };
        me.onClosed = function () { };

        me.close = function () {
            if (!open || me.onClose && me.onClose.call(me, me.el) === false)
                return me;

            me.overlay.remove();
            open = false;

            if (me.onClosed)
                me.onClosed.call(me, me.el);
            return me;
        };

        me.open = function () {

            if (open)
                return me;

            var overlay = $(overlayCode);
            var inner = $(innerCode);
            me.el = elements.clone(true);
            me.overlay = overlay;
            me.inner = inner;

            if (me.onOpen && me.onOpen.call(me, me.el) === false)
                return me;

            $("body").append(overlay);
            overlay.append(inner);
            inner.append(me.el);
            open = true;

            me.resize();
            me.el.show();

            overlay.click(function (event) {
                if (event.target != this)
                    return;
                me.close();
            });

            if (me.onOpened)
                me.onOpened.call(me, me.el);
            return me;
        };

        me.resize = function () {
            var width = 0;
            var height = 0;
            elements.each(function () {
                var element = $(this);
                if (element.outerWidth() > width)
                    width = element.outerWidth();
                height += element.outerHeight();
            });
            me.inner.css("height", height + "px");
            me.inner.css("width", width + "px");
            return me;
        };

        me.withOpen = function (callback) {
            if (!open)
                return me;

            callback.call(this, me.el);

            return me;
        };

        return me;
    };
});