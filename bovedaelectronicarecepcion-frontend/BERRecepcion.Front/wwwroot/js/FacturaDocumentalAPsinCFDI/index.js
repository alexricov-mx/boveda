var requiereAddenda = false;
$(function () {
    setOpenMenu("menu_Facturas", "ReceptionDocumentalAnalyticPaymentInvoice");
});

//Buscar analitico
$(document).on("submit", "#analitico-form-search", function (e) {
    e.preventDefault();
    if (isNull($("#selectOrganismo").val()))
        return infoAlert("Debe seleccionar un organismo para poder continuar.");
    if (isNull($("#analitico").val()))
        return infoAlert("Es necesario ingresar un analitico para realizar la búsqueda.");
    $.ajax({
        url: "FacturaDocumentalAPsinCFDI/GetAPByReception",
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

//Guardar la factura documental en la tabla de Invoice
$(document).on("click", "#btnEnviarFactura", function (e) {
    e.preventDefault();
    if (isNull($("#analitico").val()))
        return infoAlert("Es necesario ingresar un analitico de pago para poder continuar, favor de verificar.");
    var invoice = {
        DocumentoBEId: $(this).data("documentobeid"),
        Assignment: $("#factura-Region").val().trim(),
        InvoiceDate: $("#factura-FechaFactura").val(),
        Serie: $("#factura-Serie").val().trim(),
        Folio: $("#factura-Folio").val().trim(),
        TipoComprobante: "I",
        Total: $("#factura-TotalFactura").val().trim(),
        Uuid: $("#factura-UUID").val().trim(),
        IsCopade: false,
        ElectronicReception: "D",
        ViaPago: $("#factura-Region").val().trim(),
        ImporteOriginal: $("#factura-Total").val(),
        DiferencialCargo: "",
        DiferencialAbono: "",
        InvoiceNotaCredito: []
    };

    var _invoice = validateInvoice(invoice);
    if (_invoice.isValid) {
        var dataValid = validateData();
        if (dataValid.isValid) {
            $.ajax({
                url: "FacturaDocumentalAPsinCFDI/SaveInvoice",
                type: "POST",
                data: {
                    Organismo: $("#selectOrganismo").val().trim(),
                    IdAnaliticoPago: $("#analitico").val().trim(),
                    RFCReceptor: organismo.Rfc,
                    Correo: $("#factura-Correo").val().trim(),
                    dto: invoice
                },
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
        }
        else {
            return infoAlert(dataValid.message);
        }
    }
    else
        return infoAlert(_invoice.message);
    e.stopImmediatePropagation();
});

var validateInvoice = function (invoice) {
    var result = { isValid: true, mesage: "" };
    if (isNull(invoice.InvoiceDate)) return { isValid: false, message: "La fecha de la factura no debe estar vacía, favor de verificar." };
    if (isNull(invoice.Total)) return { isValid: false, message: "El total de la factura no debe estar vacío, favor de verificar." };
    if (isNull(invoice.Uuid) && isNull(invoice.Folio)) return { isValid: false, message: "Debe ingresar el UUID de la factura o el folio, favor de verificar." };
    if (!isNull(invoice.Uuid) && !validaUUID(invoice.Uuid)) return { isValid: false, message: "El UUID de la factura no tiene el formato correcto, favor de verificar." };
    if (isNull(invoice.ViaPago)) return { isValid: false, message: "La asignación no debe estar vacía, favor de verificar." };
    return result;
}

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

var validateData = function () {
    var result = { isValid: true, mesage: "" };
    var correo = $("#factura-Correo").val().trim();
    if (isNull(correo) || correo == '') return { isValid: false, message: "El correo no debe estar vacío, favor de verificar." };
    if (!(/^(([^<>()[\]\.,;:\s@\"]+(\.[^<>()[\]\.,;:\s@\"]+)*)|(\".+\"))@(([^<>()[\]\.,;:\s@\"]+\.)+[^<>()[\]\.,;:\s@\"]{2,})$/i.test(correo))) {
        return { isValid: false, message: "El formato de correo es incorrecto, favor de verificar." };
    }
    
    return result;
}

//Ver documento PDF relacionado al Analitico Pago
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
    $("#analitico").val("");
    e.stopImmediatePropagation();
});

//Formato de cantidad al hacer keyUp
$(document).on("keyup change", ".amount", function () {
    $(this).val(formatAmount($(this).val()));
});


