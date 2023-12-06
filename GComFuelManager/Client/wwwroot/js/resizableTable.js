$(function () {
    var pressed = false;
    var start = undefined;
    var startX, startWidth;

    $("table th").mousedown(function (e) {
        start = $(this);
        pressed = true;
        startX = e.pageX;
        startWidth = $(this).width();
        $(start).addClass("resizing");
    })

    $(document).mousemove(function (e) {
        if (pressed) {
            $(start).width(startWidth + (e.pageX - startX));
        }
    });

    $(document).mouseup(function () {
        if (pressed) {
            $(start).removeClass("resizing");
            pressed = false;
        }
    });
});