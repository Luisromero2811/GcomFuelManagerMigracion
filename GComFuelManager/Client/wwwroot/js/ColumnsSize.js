
const table = window.document.getElementById('miTabla');
let isResizing = false;
let initialX = 0;
let column = null;
console.log("script");
console.log(table);
table.addEventListener('mousedown', (e) => {
    if (e.target.classList.contains('th-resizable')) {
        isResizing = true;
        column = e.target;
        initialX = e.clientX;
    }
});

window.document.addEventListener('mousemove', (e) => {
    if (isResizing) {
        const width = column.offsetWidth + (e.clientX - initialX);
        column.style.width = width + 'px';
    }
});

window.document.addEventListener('mouseup', () => {
    if (isResizing) {
        isResizing = false;
    }
});
//console.log("columnsize");

//export function SizeColumn() {
//    return "Hola";
//}

//window.columnSize = {
//    SetTable: function (idTabla) {
//        console.log(idTabla);
//        table = window.document.getElementById(idTabla);
//        console.log(table);
//    }
//}