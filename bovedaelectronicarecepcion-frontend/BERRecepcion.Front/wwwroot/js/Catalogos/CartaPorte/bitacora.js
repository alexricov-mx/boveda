// Inicialización de la vista
$(function () {
    setOpenMenu("menu_CatalogosCartaPorte", "AdministrationCartaPorteBitacora");
    BitacoraTable();
});

// Selección al backend de la Bitácora Catálogos Carta Porte
let BitacoraTable = function (pageNumber = 1) {
    $.ajax({
        url: "Catalogos/BitacoraSeleccionar",
        data: { pageNumber: pageNumber },
    }).then(function (data) {
        if (!isNull(data.success) && !data.success) {
            return errorAlert(data.message);
        }

        $("#divBitacora").empty();
        $("#divBitacora").append(data);
    });
}

// Paginación de la Bitácora Catálogos Carta Porte
$(document).on("click", ".page-link", function (e) {
    e.preventDefault();
    var pageNumber = $(this).data("page_num");
    BitacoraTable(pageNumber);
    e.stopImmediatePropagation();
});
