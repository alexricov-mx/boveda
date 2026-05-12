$(function () {
    setOpenMenu("menu_Facturas", "ReceptionElectronicAnalyticPaymentInvoice");
});

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
            url: "FacturaElectronica/ExisteAddenda",
            type: "POST",
            data: formData,
            cache: false,
            contentType: false,
            processData: false,
            success: function (data) {
                if (!isNull(data.success) && !data.success) {
                    $("#btnValidarFactura").addClass("d-none");
                    $("#fileFactura").val(null);
                    return infoAlert(data.message);
                }
                $("#file-name").text(fileName);
                $(document).find(".datos-factura").remove();
                $("#div-text-addenda").empty();
                $("#div-text-addenda").html(data);
                $("#btnValidarFactura").removeClass("d-none");
            }
        });
    } else {
        $(document).find(".datos-factura").remove();
        $("#btnValidarFactura").addClass("d-none");
        $("#file-name").text(fileName);
    }

    e.stopImmediatePropagation();
});

$("#btnValidarFactura").on("click", function (e) {
    e.preventDefault();
    if ($("#fileFactura").prop("files").length > 0) {
        var formData = new FormData();
        formData.append("factura", $("#fileFactura").prop("files")[0]);
        if (!isNull(document.getElementById("txtAddenda")) && isNull($("#txtAddenda").val()))
            return infoAlert("Es necesario ingresar el XML de la Addenda para poder continuar.");
        else
            formData.append("addenda", encode($.trim($("#txtAddenda").val())));
        $.ajax({
            url: "FacturaElectronicaAP/Validar",
            type: "POST",
            data: formData,
            cache: false,
            contentType: false,
            processData: false,
            success: function (data) {
                if (!isNull(data.success) && !data.success) {
                    $("#btnValidarFactura").addClass("d-none");
                    $("#fileFactura").val(null);
                    $("#file-name").text("seleccionar archivo");
                    return infoAlert(data.message);
                }
                $("#div-datos-factura").empty();
                $("#div-datos-factura").html(data);
                $("#btnValidarFactura").removeClass("d-none");
            }
        });
    } else {
        $("#btnValidarFactura").addClass("d-none");
        $(document).find(".datos-factura").remove();
        return infoAlert("No ha seleccionado un archivo");
    }
    e.stopImmediatePropagation();
});

$(document).on("click", "#btnEnviarFactura", function (e) {
    e.preventDefault();
    var documentoBEId = $("#btnDocumentoAnalitico").data("analiticoid");
    var formData = new FormData();
    if (!isNull(document.getElementById("txtAddenda")) && isNull($("#txtAddenda").val()))
        return infoAlert("Es necesario ingresar el XML de la Addenda para poder continuar.");
    
    formData.append("addenda", encode($.trim($("#txtAddenda").val())));
    formData.append("factura", $("#fileFactura").prop("files")[0]);
    formData.append("documentoBEId", documentoBEId);
    formData.append("viaPago", $("#factura-Region").val());
    
    showLoader();
    $.ajax({
        url: "FacturaElectronicaAP/EnviarFactura",
        type: "POST",
        data: formData,
        beforeSend: function () { },
        complete: function () { },
        cache: false,
        contentType: false,
        processData: false,
        success: function (dataR) {
            $("#validationErrorsFactura").empty();
            hideLoader();
            if (!dataR.success) {
                if (dataR.criticalError) {
                    if (dataR.isTimeOut) {
                        $(document).find(".datos-factura").remove();
                        $("#fileFactura").val(null);
                        $("#file-name").text("seleccionar archivo")
                        $("#btnValidarFactura").addClass("d-none");
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
});

$(document).on("click", "#btnDocumentoAnalitico", function (e) {
    e.preventDefault();
    var documentoBEId = $(this).data("analiticoid");
    $.ajax({
        url: "DocumentoPDF/GetDocumentoByDocumentoBEId",
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
    $("#fileFactura").val(null);
    $("#file-name").text("seleccionar archivo")
    $("#btnValidarFactura").addClass("d-none");
    e.stopImmediatePropagation();
});

//Evento OnChange para el select de Organismo
$("#selectOrganismo").on("change", function (e) {
    e.preventDefault();
    organismo = $(this).find("option:selected").data("item");
    $("#organismoRfc").text(organismo.Rfc);
    $("#organismoNombre").text(organismo.Name);
    $("#organismoDireccion").text(organismo.Address);
    e.stopImmediatePropagation();
});