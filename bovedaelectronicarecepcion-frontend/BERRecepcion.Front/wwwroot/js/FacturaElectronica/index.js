$(function () {
    setOpenMenu("menu_Facturas", "ReceptionElectronicInvoice");
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
            url: "FacturaElectronica/Validar",
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
        var inputNotas = $(document).find("input[name='notaCredito']");
        $.each(inputNotas, function (index, item) {
            if ($(item).prop("files").length > 0)
                formData.append("xmlNotasCredito", $(item).prop("files")[0]);
        });
        formData.append("xmlFactura", $("#fileFactura").prop("files")[0]);
        formData.append("xmlNotaCreditoActual", input.prop("files")[0]);
        formData.append("nota", JSON.stringify(input.data("notacredito")));
        $.ajax({
            url: "FacturasBase/ValidarNotasCredito",
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
    showLoader();
    $.ajax({
        url: "FacturaElectronica/EnviarFactura",
        type: "POST",
        data: formData,
        beforeSend: function () { },
        complete: function () { },
        cache: false,
        contentType: false,
        processData: false,
        success: function (data) {
            if (!isNull(data.success) && !data.success) {
                hideLoader();
                if (data.isTimeOut) {
                    $(document).find(".datos-factura").remove();
                    $("#fileFactura").val(null);
                    $("#file-name").text("seleccionar archivo");
                    $("#btnValidarFactura").addClass("d-none");
                }
                return infoAlert(data.message);
            }
            showLoader();
            $.ajax({
                url: "CFDI/SaveInvoice",
                type: "POST",
                data: { comprobante: data.result },
                beforeSend: function () { },
                complete: function () { },
                success: function (data) {
                    hideLoader();
                    if (!isNull(data.success) && !data.success)
                        return infoAlert(data.message);
                    if (!isNull(data) && data.status == -1 && data.validationErrors.length > 0) {
                        var cxpfound = false;
                        var cxpmessage = '';
                        for (var i = 0; i < data.validationErrors.length; i++) {
                            if (data.validationErrors[i].clave == '60000' || data.validationErrors[i].clave == '60001') {
                                cxpfound = true;
                                cxpmessage = data.validationErrors[i].descripcion;
                            }
                        }
                        if (cxpfound) {
                            successAlert(cxpmessage);
                            $(document).find(".datos-factura").remove();
                            $("#btnValidarFactura").addClass("d-none");
                            $("#file-name").text("seleccionar archivo")
                            $("#fileFactura").val(null);
                        }
                        else {
                            showLoader();
                            $.ajax({
                                url: "FacturasBase/ValidationErrors",
                                type: "POST",
                                beforeSend: function () { },
                                data: { errors: data.validationErrors },
                                success: function (data) {
                                    hideLoader();
                                    $("#validationErrorsFactura").empty();
                                    $("#validationErrorsFactura").html(data);
                                }
                            });
                        }
                    }
                    else {
                        successAlert("Se ha recibido correctamente la factura, se enviará un email con más información.");
                        $(document).find(".datos-factura").remove();
                        $("#btnValidarFactura").addClass("d-none");
                        $("#file-name").text("seleccionar archivo")
                        $("#fileFactura").val(null);
                    }
                }
            });
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
            showFile(data.file);
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