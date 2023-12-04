window.makeTableResizable = () => {
    $("#miTabla th").resizable({
        handles: "e", // Permitir redimensionar desde el borde derecho
        minWidth: 20, // Ancho mínimo permitido
        maxWidth: 400 // Ancho máximo permitido
    });
};