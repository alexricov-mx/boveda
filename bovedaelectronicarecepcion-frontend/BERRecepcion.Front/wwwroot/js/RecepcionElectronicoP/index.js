$(function () {
    setOpenMenu("menu_Facturas", "ReceptionElectronicInvoiceREP");
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
            url: "RecepcionElectronicoP/ExisteREP",
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
                $("#div-datos-factura").empty();
                $("#div-datos-factura").html(data);
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
            url: "RecepcionElectronicoP/Validar",
            type: "POST",
            data: formData,
            cache: false,
            contentType: false,
            processData: false,
            success: function (data) {
                if (!isNull(data.success) && !data.success) {
                    return infoAlert(data.message);
                }
                $(document).find(".datos-factura").remove();
                $("#fileFactura").val(null);
                $("#file-name").text("seleccionar archivo")
                $("#btnValidarFactura").addClass("d-none");
            }
        });
    } else {
        $("#btnValidarFactura").addClass("d-none");
        $(document).find(".datos-factura").remove();
        return infoAlert("No ha seleccionado un archivo");
    }
    e.stopImmediatePropagation();
});

$(document).on("click", "button[name='btnNotaCredito']", function (e) {
    e.preventDefault();
    var id = $(this).data("buttonid");
    $("#" + id).click();
    e.stopImmediatePropagation();
});

$(document).on("change", "input[name='notaCredito']", function (e) {
    e.preventDefault();
    var input = $(this);
    var id = input.data("fileid");
    var fileName = input.prop("files").length > 0 ? input.prop("files")[0].name : "seleccionar archivo";
    if (input.prop("files").length > 0) {
        var formData = new FormData();
        formData.append("xmlNotaCredito", input.prop("files")[0]);
        formData.append("nota", JSON.stringify(input.data("notacredito")));
        $.ajax({
            url: "RecepcionElectronicoP/ValidarNotaCredito",
            type: "POST",
            data: formData,
            cache: false,
            contentType: false,
            processData: false,
            success: function (data) {
                if (data.success) {
                    $("#filename_" + id).text(fileName);
                    $(document).find("#progress_status_" + id).removeClass("badge-warning");
                    $(document).find("#progress_status_" + id).addClass("badge-success");
                    $(document).find("#progress_status_" + id).text("Validada");
                } else {
                    $(document).find("#progress_status_" + id).addClass("badge-warning");
                    $(document).find("#progress_status_" + id).removeClass("badge-success");
                    $(document).find("#progress_status_" + id).text("Pendiente");
                    $("#filename_" + id).text("seleccionar archivo");
                    input.val(null);
                    infoAlert(data.message);
                }
            }
        });
    } else {
        $(document).find("#progress_status_" + id).addClass("badge-warning");
        $(document).find("#progress_status_" + id).removeClass("badge-success");
        $(document).find("#progress_status_" + id).text("Pendiente");
        $("#filename_" + id).text(fileName);
    }
    e.stopImmediatePropagation();
});

$(document).on("click", "#btnEnviarFactura", function (e) {
    e.preventDefault();
    var formData = new FormData();
    if (!isNull(document.getElementById("txtAddenda")) && isNull($("#txtAddenda").val()))
        return infoAlert("Es necesario ingresar el XML de la Addenda para poder continuar.");
    else
        formData.append("addenda", encode($.trim($("#txtAddenda").val())));
    formData.append("factura", $("#fileFactura").prop("files")[0]);
    var notasCreditoInput = $(document).find("input[name='notaCredito']");
    var notasCreditoIsValid = true;
    $.each(notasCreditoInput, function (index, item) {
        var notaCredito = $(item).prop("files")[0];
        if (isNull(notaCredito))
            notasCreditoIsValid = false;
        formData.append("notasCredito", notaCredito);
    });
    if (!notasCreditoIsValid)
        return infoAlert("Es necesario ingresar las notas de crédito correspondientes para poder continuar, favor de verificar.");

    $.ajax({
        url: "RecepcionElectronicoP/EnviarFactura",
        type: "POST",
        data: formData,
        cache: false,
        contentType: false,
        processData: false,
        success: function (data) {
            if (!isNull(data.success) && !data.success)
            {

                infoAlert(data.message);
                $(document).find(".datos-factura").remove();
                $("#fileFactura").val(null);
                $("#file-name").text("seleccionar archivo")
                $("#btnValidarFactura").addClass("d-none");

            }
                
            if (!isNull(data) && data.status == -1 && data.validationErrors.length > 0)
                return infoAlert(data.message);
            else {
                successAlert(data.message);
                $(document).find(".datos-factura").remove();
                $("#fileFactura").val(null);
                $("#file-name").text("seleccionar archivo")
                $("#btnValidarFactura").addClass("d-none");
            }
        }
    });
    e.stopImmediatePropagation();
});

$(document).on("click", "#btnDocumentoCopade", function (e) {
    e.preventDefault();
    var documentoBEId = $(this).data("copadeid");
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

//Limpiar campos de informacion de factura
$(document).on("click", "#btnLimpiar", function (e) {
    e.preventDefault();
    $(document).find(".datos-factura").remove();
    $("#fileFactura").val(null);
    $("#file-name").text("seleccionar archivo")
    $("#btnValidarFactura").addClass("d-none");
    e.stopImmediatePropagation();
});