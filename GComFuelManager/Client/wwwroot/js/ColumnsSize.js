
const table = window.document.getElementById('miTabla');
let isResizing = false;
let initialX = 0;
let column = null;

table.addEventListener('mousedown', (e) => {
    if (e.target.classList.contains('th-resizable')) {
        isResizing = true;
        column = e.target;
        initialX = e.clientX;
    }
});

table.addEventListener('mousemove', (e) => {
    if (isResizing) {
        const width = column.offsetWidth + (e.clientX - initialX);
        column.style.width = width + 'px';
    }
});

table.addEventListener('mouseup', () => {
    if (isResizing) {
        isResizing = false;
    }
});