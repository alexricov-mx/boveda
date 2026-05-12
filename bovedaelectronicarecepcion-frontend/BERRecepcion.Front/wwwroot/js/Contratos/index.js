$(function () {
    ContratosTable();
    setOpenMenu("menu_Consulta", "StatisticsContracts")

    $(document).on("click", ".page-link", function (e) {
        e.preventDefault();
        var pageNum = $(this).data("page_num");
        ContratosTable(pageNum);
        e.stopImmediatePropagation();
    });

    $(document).on("submit", "#copade-form-search", function (e) {
        e.preventDefault();
        var InvoiceId = $("#search").val();
        GetFacturaPDF(InvoiceId);
        //FacturaPDF(InvoiceId);
        //NotaCreditoPDF(InvoiceId);
        e.stopImmediatePropagation();
    });
});

function ContratosTable(pageNum = 1) {
    $.ajax({
        type: "GET",
        url: "FacturaPDF/obtenerContrato",
        data: { pageNum: pageNum },
        success: function (data) {
            if (!isNull(data.success) && !data.success)
                return errorAlert(data.message);
            $("#contratos-card").empty();
            $("#contratos-card").append(data);
        }
    });
}

function base64ToArrayBuffer(data) {
    var bString = window.atob(data);
    var bLength = bString.length;
    var bytes = new Uint8Array(bLength);
    for (var i = 0; i < bLength; i++) {
        var ascii = bString.charCodeAt(i);
        bytes[i] = ascii;
    }
    return bytes;
};

function FacturaPDF(InvoiceId) {
    try {
        $.ajax({
            type: "post",
            url: '/DocumentoPDF/FacturaPDF',
            data: { InvoiceId: InvoiceId },
            contentType: 'application/json',
            async: true,
            success: function (data) {
                if (data != '' && data != undefined) {
                    var bufferArray = base64ToArrayBuffer(data);
                    $("#pdfDocument").prop("src", "data:application/pdf;base64," + data);
                    $("#ModalDocumento").modal("show");
                    return;
                } else {
                    infoAlert("No se pudo recuperar el archivo PDF");
                }

            }
        });
    }
    catch (e) {

    }

}
function NotaCreditoPDF(InvoiceId) {
    try {
        $.ajax({
            type: "post",
            url: '/DocumentoPDF/NotaCreditoPDF',
            data: { InvoiceId: InvoiceId },
            contentType: 'application/json',
            async: true,
            success: function (data) {
                if (data != '' && data != undefined) {
                    var bufferArray = base64ToArrayBuffer(data);
                    $("#pdfDocument").prop("src", "data:application/pdf;base64," + data);
                    $("#ModalDocumento").modal("show");
                    return;
                } else {
                    infoAlert("No se pudo recuperar el archivo PDF");
                }

            }
        });
    }
    catch (e) {

    }

}
function ComprobantePDF(clave) {
    try {
        $.ajax({
            type: "post",
            url: '/DocumentoPDF/ComprobantePDF',
            //data: JSON.stringify([{
            //    Clave: clave, SapOrder: sapOrder, Reception: reception, Exercise: exercise
            //}]),
            contentType: 'application/json',
            async: true,
            success: function (data) {
                if (data != '' && data != undefined) {
                    var bufferArray = base64ToArrayBuffer(data);
                    $("#pdfDocument").prop("src", "data:application/pdf;base64," + data);
                    $("#ModalDocumento").modal("show");
                    return;
                } else {
                    infoAlert("No se pudo recuperar el archivo PDF");
                }

            }
        });
    }
    catch (e) {

    }

}
$("#btnFactura").on("click", function (e) {
    e.preventDefault();
    $("#fileFactura").click();
    e.stopImmediatePropagation();
});

$("#fileFactura").on("change", function (e) {
    $("#fileFactura")
    e.preventDefault();
    var fileName = $(this).prop("files").length > 0 ? $(this).prop("files")[0].name : "seleccionar archivo";
    if ($(this).prop("files").length > 0) {
        var formData = new FormData();
        formData.append("factura", $(this).prop("files")[0]);
        $.ajax({
            type: "post",
            url: '/DocumentoPDF/pdfPreview',
            //url: "DocumentoPDF/ExisteAddenda",
            //type: "POST",
            data: formData,
            cache: false,
            contentType: false,
            processData: false,
            success: function (data) {
                if (data != '' && data != undefined) {
                    var bufferArray = base64ToArrayBuffer(data);
                    $("#pdfDocument").prop("src", "data:application/pdf;base64," + data);
                    $("#ModalDocumento").modal("show");
                    return;
                } else {
                    infoAlert("No se pudo recuperar el archivo PDF");
                }
                //if (!isNull(data.success) && !data.success) {
                //    $("#btnValidarFactura").addClass("d-none");
                //    $("#fileFactura").val(null);
                //    return infoAlert(data.message);
                //}
                //$("#file-name").text(fileName);
                //$(document).find(".datos-factura").remove();
                //$("#div-text-addenda").empty();
                //$("#div-text-addenda").html(data);
                //$("#btnValidarFactura").removeClass("d-none");
            }
        });
    } else {
        $(document).find(".datos-factura").remove();
        $("#btnValidarFactura").addClass("d-none");
        $("#file-name").text(fileName);
    }

    e.stopImmediatePropagation();
});

$(document).on("click", "#btnDocumentoCopade", function (e) {
    e.preventDefault();
    var documentoBEId = $(this).data("idcopade");
    $.ajax({
        url: "DocumentoPDF/GetDocumentoByDocumentoBEId",
        type: "GET",
        data: { DocumentoBEId: documentoBEId },
        success: function (data) {
            if (!isNull(data.success) && !data.success)
                return infoAlert(data.message);
            $("#pdfDocument").prop("src", "data:application/pdf;base64," + data.file);
            $("#ModalDocumento").modal("show");
        }
    });
    e.stopImmediatePropagation();
});

function GetFacturaPDF(InvoiceId) {
    $.ajax({
        type: "GET",
        url: "DocumentoPDF/GetFacturaPDF",
        data: { InvoiceId: InvoiceId },
        success: function (data) {
            if (!isNull(data.success) && !data.success)
                return errorAlert(data.message);
            $("#contratos-card").empty();
            $("#contratos-card").append(data);
        }
    });
}