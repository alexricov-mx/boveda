$(function () {
    setOpenMenu("menu_Facturas", "ReceptionElectronicMultipleInvoice");
    //// BS-Stepper Init
    window.stepper = new Stepper(document.querySelector('.bs-stepper'));
});

$("#btnFacturas").on("click", function (e) {
    e.preventDefault();
    $("#fileFacturas").click();
    e.stopImmediatePropagation();
});

$("#btnNotasCredito").on("click", function (e) {
    e.preventDefault();
    $("#fileNotasCredito").click();
    e.stopImmediatePropagation();
});

$("#fileFacturas").on("change", function (e) {
    e.preventDefault();
    var input = $(this);
    if (input.prop("files").length > 0) {
        var formData = new FormData();
        $.each($("#fileFacturas").prop("files"), function (index, item) {
            formData.append("facturas", item);
        });
        formData.append("enviar", false);
        $.ajax({
            url: "FacturacionElectronicaMasiva/ValidarArchivos",
            type: "POST",
            data: formData,
            cache: false,
            contentType: false,
            processData: false,
            success: function (data) {
                if (!isNull(data.success) && !data.success) {
                    input.val(null);
                    $("#info-facturas").empty();
                    return infoAlert(data.message);
                }
                $("#info-facturas").empty();
                $("#info-facturas").html(data);
                $("#next-step").prop("disabled", false);
            },
            error: function (error) {
                window.reload();
            }
        });
    } else {
        $("#next-step").prop("disabled", true);
        $(".datos-factura").remove();
    }
    e.stopImmediatePropagation();
});

$("#fileNotasCredito").on("change", function (e) {
    e.preventDefault();
    var input = $(this);
    var formData = new FormData();
    $.each($("#fileFacturas").prop("files"), function (index, item) {
        formData.append("facturas", item);
    });
    $.each($("#fileNotasCredito").prop("files"), function (index, item) {
        formData.append("notasCredito", item);
    });
    formData.append("enviar", false);
    $.ajax({
        url: "FacturacionElectronicaMasiva/ValidarArchivos",
        type: "POST",
        data: formData,
        cache: false,
        contentType: false,
        processData: false,
        success: function (data) {
            if (!isNull(data.success) && !data.success) {
                input.val(null);
                $("#info-facturas").empty();
                return infoAlert(data.message);
            }
            $("#info-facturas").empty();
            $("#info-facturas").html(data);
        },
        error: function (error) {
            return errorAlert("Ocurrió un problema al procesar la información, si el problema persiste contacte a su administrador.")
        }
    });
    e.stopImmediatePropagation();
});

$(document).on("click", "#btnEnviarFactura", function (e) {
    e.preventDefault();
    var formData = new FormData();
    var _facturas = $("#fileFacturas").prop("files");
    var _notasCredito = $("#fileNotasCredito").prop("files");
    var _notasCreditoFaltantes = $(document).find("i.nota-invalida");
    $.each(_facturas, function (index, item) {
        formData.append("facturas", item);
    });
    $.each(_notasCredito, function (index, item) {
        formData.append("notasCredito", item);
    });
    
    if (_notasCreditoFaltantes.length > 0)
        return infoAlert("Debe seleccionar las notas de crédito faltantes para poder continuar, favor de verificar.");
    formData.append("enviar", true);
    $.ajax({
        url: "FacturacionElectronicaMasiva/ValidarArchivos",
        type: "POST",
        data: formData,
        cache: false,
        contentType: false,
        processData: false,
        success: function (data) {
            if (!isNull(data.success) && !data.success) {
                return infoAlert(data.message);
            }
            return successAlert(data.message);
        },
        error: function (error) {
            return errorAlert("Ocurrió un problema al procesar la información, si el problema persiste contacte a su administrador.")
        }
    });
    e.stopImmediatePropagation();
});
//Select para filtrar por facturas validadas y no validadas
$(document).on("change", "#status-facturas", function (e) {
    e.preventDefault();
    var option = $(this).val();
    switch (option) {
        case "factura-valid":
            $(document).find(".factura-valid").show();
            $(document).find(".factura-not-valid").hide();
            break;
        case "factura-not-valid":
            $(document).find(".factura-valid").hide();
            $(document).find(".factura-not-valid").show();
            break;
        default:
            $(document).find(".factura-valid").show();
            $(document).find(".factura-not-valid").show();
    }
    e.stopImmediatePropagation();
});

//Limpiar campos de informacion de factura
$(document).on("click", "#btnLimpiar", function (e) {
    e.preventDefault();
    $(document).find(".datos-factura").remove();
    $("#fileFacturas").val(null);
    $("#fileNotasCredito").val(null);
    $("#next-step").click();
    e.stopImmediatePropagation();
});
////Stepper
$("#next-step").on("click", function (e) {
    e.preventDefault();
    if ($("#fileFacturas").prop("files").length > 0)
        stepper.next();
    else
        $(this).prop("disabled", true);
    e.stopImmediatePropagation();
});



//$("#fileFacturas").on("change", function (e) {
//    e.preventDefault();
//    var input = $(this);
//    if (input.prop("files").length > 0) {
//        var formData = new FormData();
//        $.each($("#fileFacturas").prop("files"), function (index, item) {
//            formData.append("archivosXML", item);
//        });
//        $.ajax({
//            url: "FacturacionElectronicaMasiva/InfoFacturas",
//            type: "POST",
//            data: formData,
//            cache: false,
//            contentType: false,
//            processData: false,
//            success: function (data) {
//                if (!isNull(data.success) && !data.success) {
//                    input.val(null);
//                    $("#info-facturas").empty();
//                    return infoAlert(data.message);
//                }
//                $("#info-facturas").empty();
//                $("#info-facturas").html(data);
//                $("#next-step").prop("disabled", false);
//            },
//            error: function (error) {
//                window.reload();
//            }
//        });
//    } else {
//        $("#next-step").prop("disabled", true);
//    }
//    e.stopImmediatePropagation();
//});