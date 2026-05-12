var requiereAddenda = false;

$(function () {
    setOpenMenu("menu_Facturas", "ReceptionDocumentalAnalyticPaymentInvoiceFromXML");
});

$("#selectOrganismo").on("change", function (e) {
    e.preventDefault();
    var organismo = $(this).find("option:selected").data("item");
    $("#organismoRfc").text(organismo.Rfc);
    $("#organismoNombre").text(organismo.Name);
    $("#organismoDireccion").text(organismo.Address);
    e.stopImmediatePropagation();
});

//Buscar analitico
$(document).on("submit", "#analitico-form-search", function (e) {
    e.preventDefault();
    if (isNull($("#selectOrganismo").val()))
        return infoAlert("Debe seleccionar un organismo para poder continuar.");
    if (isNull($("#analitico").val()))
        return infoAlert("Es necesario ingresar un analítico para realizar la búsqueda.");
    $.ajax({
        url: "FacturaDocumentalAPconCFDI/GetAPByReception",
        type: "GET",
        data: {
            Clave: $("#selectOrganismo").val(),
            IdAnaliticoPago: $("#analitico").val().trim()
        },
        success: function (data) {
            if (!isNull(data.success) && !data.success) {
                $(document).find(".datos-factura").remove();
                return infoAlert(data.message);
            }
            $("#div-datos-factura").empty();
            $("#div-datos-factura").html(data);
        }
    });

    e.stopImmediatePropagation();
});

//Accion para mostar el dialogo de seleccionar archivo de factura
$(document).on("click", "#btnFactura", function (e) {
    e.preventDefault();
    $("#fileFactura").click();
    e.stopImmediatePropagation();
});

//Validaciones para mostrar los datos del archivo de la factura
$(document).on("change", "#fileFactura", function (e) {
    e.preventDefault();
    var fileName = $(this).prop("files").length > 0 ? $(this).prop("files")[0].name : "seleccionar archivo";
    if ($(this).prop("files").length > 0) {
        var formData = new FormData();
        formData.append("factura", $(this).prop("files")[0]);
        $.ajax({
            url: "FacturaDocumentalAPconCFDI/InfoFactura",
            type: "POST",
            data: formData,
            cache: false,
            contentType: false,
            processData: false,
            success: function (data) {
                if (!isNull(data.success) && !data.success) {
                    $("#fileFactura").val(null);
                    $(document).find(".info-factura").remove();
                    $("#file-name").text("seleccionar archivo");
                    return infoAlert(data.message);
                }
                $("#file-name").text(fileName);
                $("#info-archivo-factura").empty();
                $("#info-archivo-factura").html(data);
            }
        });
    } else {
        $(document).find(".info-factura").remove();
        $("#file-name").text(fileName);
    }
    e.stopImmediatePropagation();
});

$(document).on("click", "#btnEnviarFactura", function (e) {
    e.preventDefault();
    if (isNull($("#fileFactura").prop("files")[0]))
        return infoAlert("Debe seleccionar la factura, favor de verificar.");
    if (isNull($("#factura-Region").val()))
        return infoAlert("El campo VU / Asignación es requerido, favor de verificar");
    if (isNull($("#analitico").val()))
        return infoAlert("Es necesario ingresar un analítico para poder continuar, favor de verificar.");
    var formData = new FormData();
    formData.append("factura", $("#fileFactura").prop("files")[0]);
    var organismo = $("#selectOrganismo").find("option:selected").data("item");
    var idAnalitico = $("#analitico").val().trim();
    var documentoBEId = $("#btnDocumentoAnalitico").data("idanaliticopago");
    var clave = organismo.Clave;
    var rfcReceptor = organismo.Rfc;

    formData.append("ViaPago", $("#factura-Region").val().trim());
    formData.append("IdAnaliticoPago", idAnalitico);
    formData.append("Clave", clave);
    formData.append("RFCReceptor", rfcReceptor);
    formData.append("DocumentoBEId", documentoBEId);

    $.ajax({
        url: "FacturaDocumentalAPconCFDI/EnviarFactura",
        type: "POST",
        data: formData,
        processData: false,  // tell jQuery not to process the data
        contentType: false,
        success: function (dataR) {
            $("#validationErrorsFactura").empty();
            if (!dataR.success) {
                if (dataR.criticalError) {
                    if (dataR.isTimeOut) {
                        $(document).find(".datos-factura").remove();
                        $("#analitico").val("");
                    }
                    return infoAlert(dataR.message);
                }
                else {
                    $.ajax({
                        url: "FacturasBase/ValidationErrors",
                        type: "POST",
                        data: { errors: dataR.validationErrors },
                        success: function (data) {
                            $("#validationErrorsFactura").empty();
                            $("#validationErrorsFactura").html(data);
                        }
                    });
                }
            }
            else {
                $(document).find(".datos-factura").remove();
                $("#analitico").val("");
                return infoAlert(dataR.message);
            }
        }
    });
    e.stopImmediatePropagation();

    e.stopImmediatePropagation();
});

$(document).on("click", "#btnDocumentoAnalitico", function (e) {
    e.preventDefault();
    var documentoBEId = $(this).data("idanaliticopago");
    $.ajax({
        url: "DocumentoPDF/GetDocumentoAPByDocumentoBEId",
        type: "GET",
        data: { DocumentoBEId: documentoBEId },
        success: function (data) {
            if (!isNull(data.success) && !data.success)
                return infoAlert(data.message);
            //$("#pdfDocument").prop("src", "data:application/pdf;base64," + data.file);
            //$("#ModalDocumento").modal("show");
            showFile(data.file, false);
        }
    });
    e.stopImmediatePropagation();
});

//Limpiar campos de informacion de factura
$(document).on("click", "#btnLimpiar", function (e) {
    e.preventDefault();
    $(document).find(".datos-factura").remove();
    $("#analitico").val("");
    e.stopImmediatePropagation();
});

$("#analitico").on("focus", function (e) {
    e.preventDefault();
    $(this).select();
    e.stopImmediatePropagation();
});