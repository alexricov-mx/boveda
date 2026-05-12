$(function () {
    GetCopades();
    setOpenMenu("menu_COPADEAnalitico", "ReceptionSignCopade")

    $(document).on("click", ".page-link", function (e) {
        e.preventDefault();
        var search = $("#search").val();
        var pageNum = $(this).data("page_num");
        GetCopades(search, pageNum);
        e.stopImmediatePropagation();
    });

    $(document).on("submit", "#copade-form-search", function (e) {
        e.preventDefault();
        var search = $("#search").val();
        GetCopades(search);
        e.stopImmediatePropagation();
    });

    //$(document).on("click", "input[name='checkAllPDF']", function (e) {
    //    $(document).find("input[name='checkPDF']").prop("checked", $(this).prop("checked"));
    //    e.stopImmediatePropagation();
    //});
});
let GetCopades = function (pageNum = 1, search = "") {
    $.ajax({
        url: "Copade/GetCopades",
        data: { pageNum: pageNum, search: search },
        success: function (data) {
            if (!isNull(data.success) && !data.success)
                return errorAlert(data.message);
            $("#copade-card").empty();
            $("#copade-card").append(data);
            darkMode(getCookie("dark-mode") == "true");
        }
    });
}

function pdfPreview(clave, sapOrder, reception, exercise, functionary1SignDate, CopadeId) {
    try {
        var copade = {
            Clave: clave,
            SapOrder: sapOrder,
            Reception: reception,
            Exercise: exercise,
            Functionary1SignDate: !isNull(functionary1SignDate),
            CopadeId: CopadeId
        }        
        $.ajax({
            type: "POST",
            url: 'Copade/pdfPreview',
            data: { dto : copade},
            dataType: "text",
            success: function (data) {
                console.log("=== DEBUG pdfPreview ===");
                console.log("Type of data:", typeof data);
                console.log("Data value:", data);
                console.log("Data constructor:", data.constructor.name);
                
                if (!isNull(data)){
                    showFile(data);
                    return;
                } else {
                    infoAlert("No se pudo recuperar el archivo PDF");
                }
            },
            statusCode: {
                400: function (xhr) {
                    var response = xhr.responseJSON;
                    errorAlert(response?.message || "Solicitud inválida al generar PDF");
                },
                404: function (xhr) {
                    var response = xhr.responseJSON;
                    errorAlert(response?.message || "PDF no encontrado");
                },
                500: function (xhr) {
                    var response = xhr.responseJSON;
                    errorAlert(response?.message || "Error al generar vista previa del PDF");
                }
            },
            error: function (xhr) {
                // Maneja otros códigos de error no especificados en statusCode
                if (xhr.status !== 400 && xhr.status !== 404 && xhr.status !== 500) {
                    var response = xhr.responseJSON;
                    errorAlert(response?.message || "Error al procesar la solicitud de PDF");
                }
            }
        });
    }
    catch (e) {
        console.error("Exception en pdfPreview:", e);
        errorAlert("Error al solicitar vista previa del PDF");
    }
}

