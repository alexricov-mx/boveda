var organismo;
$(function () {
    setOpenMenu("menu_Facturas", "ReceptionDocumentalInvoice");
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
        url: "FacturaDocumentalsinCFDI/GetCopadeByReception",
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

//Guardar la factura documental en la tabla de Invoice
$(document).on("click", "#btnEnviarFactura", function (e) {
    e.preventDefault();
    if (isNull($("#copade").val()))
        return infoAlert("Es necesario ingresar un copade para realizar la búsqueda.");
    var invoice = {
        DocumentoBEId: $(this).data("documentobeid"),
        Assignment: $("#factura-Region").val().trim(),
        Exercise: $("#factura-Ejercicio").val().trim(),
        InvoiceDate: $("#factura-FechaFactura").val(),
        Serie: $("#factura-Serie").val().trim(),
        Folio: $("#factura-Folio").val().trim(),
        TipoComprobante: "I",
        Total: $("#factura-TotalFactura").val().trim(),
        Uuid: $("#factura-UUID").val().trim(),
        IsCopade: true,
        ElectronicReception: "D",
        ViaPago: $("#factura-Region").val().trim(),
        ImporteOriginal: $("#factura-Total").val(),
        DiferencialCargo: "",
        DiferencialAbono: "",
        InvoiceNotaCredito: []
    };

    $.each($("div[name='div_notas_credito']"), function (index, item) {
        var notaCredito = {
            Serie: $(item).find("input[name='nota-credito-serie']").val().trim(),
            Folio: $(item).find("input[name='nota-credito-folio']").val().trim(),
            NotaCreditoDate: $(item).find("input[name='nota-credito-fecha']").val(),
            Uuid: $(item).find("input[name='nota-credito-uuid']").val().trim(),
            IsCopade: true,
            Subtotal: $(item).find("td[name='nota-credito-subtotal']").data("value"),
            Iva: $(item).find("td[name='nota-credito-iva']").data("value"),
            Total: $(item).find("td[name='nota-credito-total']").data("value"),
            Amount: $(item).find("td[name='nota-credito-amount']").data("value"),
            Descripcion: $(item).find("td[name='nota-credito-descripcion']").data("value")
        };
        invoice.InvoiceNotaCredito.push(notaCredito);
    });
    var _invoice = validateInvoice(invoice);
    if (_invoice.isValid) {
        var dataValid = validateData();
        if (dataValid.isValid) {
            $.ajax({
                url: "FacturaDocumentalsinCFDI/SaveInvoice",
                type: "POST",
                data: {
                    Organismo: $("#selectOrganismo").val().trim(),
                    Reception: $("#copade").val().trim(),
                    Exercise: $("#factura-Ejercicio").val(),
                    RFCReceptor: organismo.Rfc,
                    Correo: $("#factura-Correo").val().trim(),
                    dto: invoice
                },
                success: function (dataR) {
                    $("#validationErrorsFactura").empty();
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
                        $(document).find(".datos-factura").remove();
                        $("#selectOrganismo").val("");
                        $("#copade").val("");
                        $("#factura-Ejercicio").val("");
                        return infoAlert(dataR.message);
                    }
                }
            });
        }
        else {
            return infoAlert(dataValid.message);
        }
    }
    else
        return infoAlert(_invoice.message);
    e.stopImmediatePropagation();
});

function validaUUID(uuid) {
    var expregUUID = /^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$/;
    if (uuid.length == 36)
        if (!expregUUID.test(uuid)) {
            return infoAlert("El UUID no es correcto");
        }
        else {
            return true;
        }
}

var validateInvoice = function (invoice) {
    var result = { isValid: true, mesage: "" };
    if (isNull(invoice.Exercise)) return { isValid: false, message: "El ejercicio de la factura no debe estar vacío, favor de verificar." };
    if (isNull(invoice.InvoiceDate)) return { isValid: false, message: "La fecha de la factura no debe estar vacía, favor de verificar." };
    if (isNull(invoice.Total)) return { isValid: false, message: "El total de la factura no debe estar vacío, favor de verificar." };
    if (isNull(invoice.Uuid) && isNull(invoice.Folio)) return { isValid: false, message: "Debe ingresar el UUID de la factura o el folio, favor de verificar." };
    if (!isNull(invoice.Uuid) && !validaUUID(invoice.Uuid)) return { isValid: false, message: "El UUID de la factura no tiene el formato correcto, favor de verificar." };
    if (isNull(invoice.ViaPago)) return { isValid: false, message: "La asignación no debe estar vacía, favor de verificar." };
    $.each(invoice.InvoiceNotaCredito, function (index, item) {
        if (isNull(item.NotaCreditoDate)) {
            result.isValid = false; result.message = "La fecha de la nota de crédito no debe estar vacía, favor de verificar.";
        }
        if (isNull(item.Uuid) && isNull(item.Folio)) {
            result.isValid= false; result.message= "Debe ingresar el UUID de la nota de crédito o el folio, favor de verificar.";
        }
        if (!isNull(item.Uuid) && !validaUUID(item.Uuid)) {
            result.isValid= false; result.message= "El UUID de la de la nota de crédito no tiene el formato correcto, favor de verificar.";
        }
    });
    return result;
}

var validateData = function () {
    var result = { isValid: true, mesage: "" };
    var correo = $("#factura-Correo").val().trim();
    if (isNull(correo) || correo == '') return { isValid: false, message: "El correo no debe estar vacío, favor de verificar." };
    if (!(/^(([^<>()[\]\.,;:\s@\"]+(\.[^<>()[\]\.,;:\s@\"]+)*)|(\".+\"))@(([^<>()[\]\.,;:\s@\"]+\.)+[^<>()[\]\.,;:\s@\"]{2,})$/i.test(correo))) {
        return { isValid: false, message: "El formato de correo es incorrecto, favor de verificar." };
    }

    return result;
}

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

//Evento OnChange para el select de Organismo
$("#selectOrganismo").on("change", function (e) {
    e.preventDefault();
    organismo = $(this).find("option:selected").data("item");
    $("#organismoRfc").text(organismo.Rfc);
    $("#organismoNombre").text(organismo.Name);
    $("#organismoDireccion").text(organismo.Address);
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

//Formato de cantidad al hacer keyUp
$(document).on("keyup change", ".amount", function () {
    $(this).val(formatAmount($(this).val()));
});
