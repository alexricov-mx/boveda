// Inicialización de la vista
$(function () {
    setOpenMenu("menu_CatalogosCartaPorte", "AdministrationMaintenanceCartaPorte");
    CartaPorteTable();
});

// Selección en el dropdown de los Catálogos Carta Porte de paso
$("#idCatalogo").on("change", function () {
    CartaPorteTable();
});

// Selección al backend de los Catálogos Carta Porte de paso
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

// Barra de progreso de carga
var barProgress = $(".progress-bar");

// Set de la barra de progreso de carga
let setProgress = function (percentComplete) {
    barProgress.html(percentComplete + "%");
    barProgress.width(percentComplete + "%");
}

// Show modal de carga
$("#btnModalCargar").on("click", function (e) {
    setProgress(0);
    $("#formCatalogo").trigger("reset");
    $("#modalCargar").modal("show");
});

// Actualización del proceso de carga
$("#formCatalogo").ajaxForm({
    beforeSend: function () {
    },
    uploadProgress: function (event, position, total, percentComplete) {
        setProgress(percentComplete);
    },
    complete: function (xhr) {
        var result = xhr.responseJSON;
        if (result.success) {
            successAlert(result.message);
        }
        else {
            errorAlert(result.message);
        }

        $("#modalCargar").modal("hide");
    }
});

// Validación de los Catálogos Carta Porte de paso y aceptación del proceso de mantenimiento
$("#btnAceptar").on("click", function (e) {
    $.ajax({
        type: "POST",
        url: "Catalogos/Validar",
        success: function (data) {
            if (!isNull(data.success) && !data.isValid) {
                return errorAlert(data.message);
            }

            Swal.fire({
                title: "Aceptar Catálogos Carta Porte",
                text: "¿Desea aceptar los Catálogos Carta Porte?",
                icon: 'question',
                showCancelButton: true,
                cancelButtonText: 'Cancelar',
                confirmButtonText: "Aceptar",
            }).then((result) => {
                if (result.isConfirmed) {
                    $.ajax({
                        type: "POST",
                        url: "Catalogos/Aceptar",
                        success: function (data) {
                            if (!isNull(data.success) && !data.success) {
                                return errorAlert(data.message);
                            }

                            CartaPorteTable();

                            return successAlert(data.message);
                        }
                    });
                }
            });
        }
    });
});
