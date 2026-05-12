$(function () {

    setOpenMenu("menu_Consulta", "StatisticsPaymentSchedule")

    $(document).on("click", ".page-link", function (e) {
        e.preventDefault();
        var pageNum = $(this).data("page_num");
        obtenerPaymentScheduleTable(pageNum);
        e.stopImmediatePropagation();
    });

    $(document).on("submit", "#paymentschedule-form-search", function (e) {
        e.preventDefault();
        obtenerPaymentScheduleTable();
        e.stopImmediatePropagation();
    });

    $(document).on("click", "#ExportExcelAll", function (e) {
        e.preventDefault();
        var dataValid = validateData();
        if (dataValid.isValid) {
            var pageNum = 0;
            var start = $("#fecha-inicial").val();
            var end = $("#fecha-final").val();
            var search = [];
            $.each($(document).find('span[name="badgeTerms"]'), function (index, item) {
                search.push($(item).text());
            });
            location.href = "Consultas/DownloadExcelGetAllPaymentSchedule?search=" + search.toString().replaceAll(',', '__') + "&start=" + start + "&end=" + end;
        }
        else {
            return infoAlert(dataValid.message);
        }
        e.stopImmediatePropagation();
    });

    $("button").keypress(function (e) {
        if (e.which == 13) {
            return false;
        }
    });

    $("form").keypress(function (e) {
        if (e.which == 13) {
            return false;
        }
    });

});

function validateData() {
    var result = { isValid: true, mesage: "" };

    var start = $("#fecha-inicial").val();
    var end = $("#fecha-final").val();
    if (start != '' && start != undefined && end != '' && end != undefined) {
        if (end < start) {
            return { isValid: false, message: "El rango de fechas es incorrecto, favor de verificar." };
        }
    }

    return result;
}

function updateSearch() {
    obtenerPaymentScheduleTable(1);
}

function obtenerPaymentScheduleTable(pageNum) {
    $("#ExportExcelAll").hide();
    var start = $("#fecha-inicial").val();
    var end = $("#fecha-final").val();
    var search = [];
    $.each($(document).find('span[name="badgeTerms"]'), function (index, item) {
        search.push($(item).text());
    });
    if (!pageNum || pageNum == undefined) {
        pageNum = 1;
    }
    var dataValid = validateData();
    if (dataValid.isValid) {
        $.ajax({
            type: "POST",
            url: "Consultas/obtenerPaymentScheduleTable",
            data: {
                start: start,
                end: end,
                search: search,
                pageNum: pageNum
            },
            success: function (data) {
                if (!isNull(data.success) && !data.success) {
                    return errorAlert(data.message);
                }
                else {
                    $("#ExportExcelAll").show();
                }
                $("#paymentschedule-card").empty();
                $("#paymentschedule-card").append(data);
                darkMode(getCookie("dark-mode") == "true");
            }
        });
    }
    else {
        return infoAlert(dataValid.message);
    }
}

function padleft(sourceNumber, length) {
    var source = "" + sourceNumber;
    var le = source.length;
    for (var i = 0; i < length - le; i++) {
        source = "0" + source;
    }
    return source;
}

function pdfPreview(ProgramaPago_Id, Organismo) {
    try {
        $.ajax({
            type: "POST",
            url: 'DocumentoPDF/ConsultaProgramacionPagoList',
            data: JSON.stringify([{
                ProgramaPago_Id: ProgramaPago_Id,
                Organismo: Organismo
            }]),
            contentType: 'application/json',
            async: true,
            success: function (data) {
                if (data != '' && data != undefined) {
                    showFile(data, false);
                    return;
                } else {
                    infoAlert("No se pudo recuperar el archivo PDF");
                }
            }
        });
    }
    catch (e) {
        infoAlert("No se pudo recuperar el archivo PDF");
    }
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
