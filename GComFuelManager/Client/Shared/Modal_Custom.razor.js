var modal = document.getElementById("myModal");
var btn = document.getElementById("myBtn");

var span = document.getElementsByClassName("close")[0];

//span.onclick = function () {
//    modal.style.display = "none";
//    console.log("cerrado btn")
//}

window.onclick = function CloseModal(event, modalDisplay) {
    if (event.target == modal) {
        //modal.style.display = "none";
        modalDisplay = "none"
        console.log("cerrado")
    }
}

//window.addEventListener('click', function (e) {
//    if (document.getElementById('modal-content') != null) {
//        if (document.getElementById('modal-content').contains(e.target)) {
//            //this.alert("clic in")
//            console.log("in")
//        } else {
//            console.log("out")
//            //this.alert("clic out")
//            var modal_custom = document.getElementById("modal-custom");
//            modal_custom.style.display = "none";
//            modal_custom.classList.remove("show");
//            document.getElementById("modal-back").classList.remove("show");
//        }
//    }
//})