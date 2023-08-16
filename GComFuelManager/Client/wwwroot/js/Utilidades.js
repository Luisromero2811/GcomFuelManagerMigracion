function timerInactivo(dotnetHelper) {
    var timer;
    document.onmousemove = resetTimer;
    document.onkeypress = resetTimer;

    function resetTimer() {
        clearTimeout(timer);
        timer = setTimeout(logout, 1 * 40000);
    }

    function logout() {
        dotnetHelper.invokeMethodAsync("Logoute");
    }
}
