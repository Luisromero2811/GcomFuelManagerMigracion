function timerInactivo(dotnetHelper) {
    var timer;
    document.onmousemove = resetTimer;
    document.onkeypress = resetTimer;

    function resetTimer() {
        clearTimeout(timer);
        timer = setTimeout(logout, 16 * 1000 * 60);
    }
    function logout() {
        dotnetHelper.invokeMethodAsync("Logoute");
    }
}
