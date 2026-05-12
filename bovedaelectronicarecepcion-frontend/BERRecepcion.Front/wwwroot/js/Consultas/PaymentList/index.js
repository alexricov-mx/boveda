$(function () {

    setOpenMenu("menu_Consulta", "StatisticsPaymentList")

    $(document).on("click", ".page-link", function (e) {
        e.preventDefault();
        var pageNum = $(this).data("page_num");
        obtenerPaymentListTable(pageNum);
        e.stopImmediatePropagation();
    });

    $(document).on("submit", "#paymentlist-form-search", function (e) {
        e.preventDefault();
        obtenerPaymentListTable();
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

            for (let i = 0; i < search.length; i++)
                search[i] = search[i].replaceAll(',', '_');
            location.href = "Consultas/DownloadExcelGetAllPaymentList?search=" + search.toString().replaceAll(',', '__') + "&start=" + start + "&end=" + end;
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
    obtenerPaymentListTable(1);
}

function obtenerPaymentListTable(pageNum) {
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
            url: "Consultas/obtenerPaymentListTable",
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
                $("#paymentlist-card").empty();
                $("#paymentlist-card").append(data);
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

function pdfPreview(ListaPago_Id) {
    try {
        $.ajax({
            type: "GET",
            url: 'DocumentoPDF/ConsultaPagoList',
            data: { ListaPago_Id: ListaPago_Id },
            success: function (data) {
                if (data != '' && data != undefined) {
                    showFile(data, false);
                } else {
                    infoAlert("No se pudo recuperar el archivo PDF");
                }
            }
        });
    }
    catch (e) {

    }
}
