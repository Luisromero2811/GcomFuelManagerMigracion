function CloseMenu() {
    document.getElementById("mySidepanel").style.width = "0px";
}

window.addEventListener('click', function (e) {
    if (document.getElementById('mySidepanel').contains(e.target) || document.getElementById('buttonSidepanel').contains(e.target)) {
        // this.alert("clic in")
        console.log("clic in")
    } else {
        console.log("clic out")
        document.getElementById("mySidepanel").style.width = "0px";
    }
})