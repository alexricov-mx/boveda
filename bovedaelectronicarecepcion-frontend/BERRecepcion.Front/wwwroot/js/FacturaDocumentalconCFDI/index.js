$(function () {
    setOpenMenu("menu_Facturas", "ReceptionDocumentalInvoiceFromXML");
});
//Evento OnChange para el select de Organismo
$("#selectOrganismo").on("change", function (e) {
    e.preventDefault();
    var organismo = $(this).find("option:selected").data("item");
    $("#organismoRfc").text(organismo.Rfc);
    $("#organismoNombre").text(organismo.Name);
    $("#organismoDireccion").text(organismo.Address);
    e.stopImmediatePropagation();
});
//Buscar copade
$(document).on("submit", "#copade-form-search", function (e) {
    e.preventDefault();
    if (isNull($("#selectOrganismo").val()))
        return infoAlert("Debe seleccionar un organismo para poder continuar.");
    if (isNull($("#copade").val()))
        return infoAlert("Es necesario ingresar un copade para realizar la búsqueda.");
    if (isNull($("#factura-Ejercicio").val()))
        return infoAlert("Es necesario ingresar un ejercicio para realizar la búsqueda.");
    $.ajax({
        url: "FacturaDocumentalconCFDI/GetCopadeByReception",
        type: "GET",
        data: {
            Clave: $("#selectOrganismo").val(),
            Reception: $("#copade").val().trim(),
            Exercise: $("#factura-Ejercicio").val().trim()
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
$(document).on("click", "#btnFactura",function (e) {
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
        formData.append("entrada", $("#copade").val());
        formData.append("ejercicio", $("#factura-Ejercicio").val());

        $.ajax({
            url: "FacturaDocumentalconCFDI/InfoFactura",
            type: "POST",
            data: formData,
            cache: false,
            contentType: false,
            processData: false,
            success: function (data) {
                if (!isNull(data.success) && !data.success) {
                    $("#fileFactura").val(null);
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

//Accion para mostar el dialogo de seleccionar archivo de nota de credito
$(document).on("click", "button[name='btnNotaCredito']", function (e) {
    e.preventDefault();
    var id = $(this).data("buttonid");
    $("#" + id).click();
    e.stopImmediatePropagation();
});

//Validaciones para la nota de credito ingresada
$(document).on("change", "input[name='notaCredito']", function (e) {
    e.preventDefault();
    var input = $(this);
    if ($("#fileFactura").prop("files").length == 0) {
        input.val(null);
        return infoAlert("Es necesario ingresar la factura antes de seleccionar las notas de crédito, favor de verificar.");
    }
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

var validateData = function () {
    var result = { isValid: true, mesage: "" };
    var correo = $("#factura-Correo").val().trim();
    if (isNull(correo) || correo == '') return { isValid: false, message: "El correo no debe estar vacío, favor de verificar." };
    if (!(/^(([^<>()[\]\.,;:\s@\"]+(\.[^<>()[\]\.,;:\s@\"]+)*)|(\".+\"))@(([^<>()[\]\.,;:\s@\"]+\.)+[^<>()[\]\.,;:\s@\"]{2,})$/i.test(correo))) {
        return { isValid: false, message: "El formato de correo es incorrecto, favor de verificar." };
    }

    return result;
}

//Guardar la factura documental en la tabla de Invoice
$(document).on("click", "#btnEnviarFactura", function (e) {
    e.preventDefault();
    var formData = new FormData();
    if (isNull($("#fileFactura").prop("files")[0]))
        return infoAlert("Debe seleccionar la factura, favor de verificar.");
    if (isNull($("#copade").val()))
        return infoAlert("Es necesario ingresar un copade para poder continuar, favor de verificar.");
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

    if (isNull($("#factura-Region").val()))
        return infoAlert("El campo VU / Asignación es requerido, favor de verificar");

    if (isNull($("#factura-Correo").val()))
        return infoAlert("El campo de correo electronico es requerido, favor de verificar");

    var vd = validateData();
    if (!vd.isValid) {
        return infoAlert(vd.message);
    }

    var organismo = $("#selectOrganismo").find("option:selected").data("item");

    formData.append("ViaPago", $("#factura-Region").val().trim());
    formData.append("Correo", $("#factura-Correo").val().trim());
    formData.append("CopadeId", $("#copade").val());
    formData.append("Clave", organismo.Clave);
    formData.append("RFCReceptor", organismo.Rfc);
    formData.append("DocumentoBEId", $("#btnDocumentoCopade").data("copadeid"));

    $.ajax({
        url: "FacturaDocumentalconCFDI/EnviarFactura",
        type: "POST",
        data: formData,
        cache: false,
        contentType: false,
        processData: false,
        success: function (dataR) {
                if (!dataR.success) {
                    if (dataR.criticalError) {
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
                    if (dataR.message == '') {
                        successAlert("Se ha recibido correctamente la factura, se enviará un email con más información.");
                    }
                    else {
                        successAlert(dataR.message);
                    }
                    $("#selectOrganismo").val("");
                    $("#copade").val("");
                    $("#factura-Ejercicio").val("");
                    $(document).find(".datos-factura").remove();
                    $("#file-name").text("seleccionar archivo")
                    $("#fileFactura").val(null);
                }
        }
    });
    e.stopImmediatePropagation();
});

//Ver documento PDF relacionado al COPADE
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
    $("#copade").val("");
    $("#factura-Ejercicio").val("");
    e.stopImmediatePropagation();
});
