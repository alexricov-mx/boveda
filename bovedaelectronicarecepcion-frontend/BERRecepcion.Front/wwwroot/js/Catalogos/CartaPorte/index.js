// Inicialización de la vista
$(function () {
    setOpenMenu("menu_CatalogosCartaPorte", "AdministrationCartaPorte");
    CartaPorteTable();
});

// Selección en el dropdown de los Catálogos Carta Porte finales
$("#idCatalogo").on("change", function () {
    CartaPorteTable();
});

// Selección al backend de los Catálogos Carta Porte finales
let CartaPorteTable = function (pageNumber = 1, filtroBusqueda = null) {
    var tableName = $("#idCatalogo").val();

    $.ajax({
        url: "Catalogos/Seleccionar",
        data: { tableName: tableName, pageNumber: pageNumber, filtroBusqueda: filtroBusqueda },
    }).then(function (data) {
        if (!isNull(data.success) && !data.success) {
            return errorAlert(data.message);
        }

        $("#divCatalogo").empty();
        $("#divCatalogo").append(data);
    });
}

// Paginación de los Catálogos Carta Porte finales
$(document).on("click", ".page-link", function (e) {
    e.preventDefault();
    var pageNumber = $(this).data("page_num");
    CartaPorteTable(pageNumber);
    e.stopImmediatePropagation();
});
