$(function () {

    setOpenMenu("menu_Consulta", "ReportRejectedInvoice")

    $(document).on("click", ".page-link", function (e) {
        e.preventDefault();
        var pageNum = $(this).data("page_num");
        obtenerRechazosTable(pageNum);
        e.stopImmediatePropagation();
    });

    $(document).on("submit", "#rechazos-form-search", function (e) {
        e.preventDefault();
        obtenerRechazosTable();
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
            location.href = "Consultas/DownloadExcelGetAllRechazosFactura?search=" + search.toString().replaceAll(',', '__') + "&start=" + start + "&end=" + end;
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
    obtenerRechazosTable(1);
}

function obtenerRechazosTable(pageNum) {
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
            url: "Consultas/obtenerRechazosFacturaTable",
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
                $("#rechazos-card").empty();
                $("#rechazos-card").append(data);
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
    for (var i = 0; i < length-le; i++) {
        source = "0" + source;
    }
    return source;
}