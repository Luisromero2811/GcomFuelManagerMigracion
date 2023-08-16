function timerInactivo(dotnetHelper) {
    var timer;
    document.onmousemove = resetTimer;
    document.onkeypress = resetTimer;

    function resetTimer() {
        clearTimeout(timer);
        timer = setTimeout(logout, 4*10000);
    }
    function logout() {
        dotnetHelper.invokeMethodAsync("Logoute");
    }
}
