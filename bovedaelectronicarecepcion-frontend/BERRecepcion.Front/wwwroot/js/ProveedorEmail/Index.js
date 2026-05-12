$(function () {
    setOpenMenu("menu_Consulta", "ReportEmails")
    $("#ExportExcelAll").hide();

    $(document).on("click", ".page-link", function (e) {
        e.preventDefault();
        var pageNum = $(this).data("page_num");
        obtenerProveedoresEmailTable(pageNum);
        e.stopImmediatePropagation();
    });

    $(document).on("click", "#ExportExcelAll", function (e) {
        e.preventDefault();
        var dataValid = validateData();
        if (dataValid.isValid) {
            var pageNum = 0;
            var fechaInicial = $("#fecha-inicial").val();
            var fechaFinal = $("#fecha-final").val();
            var search = [];
            $.each($(document).find('span[name="badgeTerms"]'), function (index, item) {
                search.push($(item).text());
            });

            for (let i = 0; i < search.length; i++)
                search[i] = search[i].replaceAll(',', '_');
            location.href = "ProveedoresEmail/DownloadExcelGetAll?fechaInicial=" + fechaInicial + "&fechaFinal=" + fechaFinal + "&search=" + search.toString().replaceAll(',', '__');
        }
        else {
            return infoAlert(dataValid.message);
        }
        e.stopImmediatePropagation();
    });
    $(document).on("submit", "#proveedor-form-search", function (e) {
        e.preventDefault();
        var dataValid = validateData();
        if (dataValid.isValid) {
            var fechaInicial = $("#fecha-inicial").val();
            var fechaFinal = $("#fecha-final").val();
            if (validateData(fechaInicial, fechaFinal)) {
                obtenerProveedoresEmailTable();
            }
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

    var fechaInicial = $("#fecha-inicial").val();
    var fechaFinal = $("#fecha-final").val();
    if (fechaInicial == null || fechaInicial == undefined || fechaInicial == '') return { isValid: false, message: "Fecha inicial requerida, favor de verificar." };
    if (fechaFinal == null || fechaFinal == undefined || fechaFinal == '') return { isValid: false, message: "Fecha final requerida, favor de verificar." };
    if (fechaFinal < fechaInicial) return { isValid: false, message: "El rango de fechas es incorrecto, favor de verificar." };

    return result;
}

function updateSearch() {
    obtenerProveedoresEmailTable(1);
}

function obtenerProveedoresEmailTable(pageNum) {
    $("#ExportExcelAll").hide();
    var fechaInicial = $("#fecha-inicial").val();
    var fechaFinal = $("#fecha-final").val();
    var search = [];
    $.each($(document).find('span[name="badgeTerms"]'), function (index, item) {
        search.push($(item).text());
    });
    if (!pageNum || pageNum == undefined) {
        pageNum = 1;
    }
    $.ajax({
        type: "POST",
        url: "ProveedoresEmail/obtenerProveedoresEmail",
        data: {
            fechaInicial: fechaInicial,
            fechaFinal: fechaFinal,
            search: search,
            pageNum: pageNum
        },
        success: function (data) {
            $("#ExportExcelAll").show();
            if (!isNull(data.success) && !data.success) {
                return errorAlert(data.message);
            }
            
            $("#proveedoresemail-card").empty();
            $("#proveedoresemail-card").append(data);
        }
    });
}
function padleft(sourceNumber, length) {
    var source = "" + sourceNumber;
    var le = source.length;
    for (var i = 0; i < length - le; i++) {
        source = "0" + source;
    }
    return source;
}

function validateData() {
    var result = { isValid: true, mesage: "" };

    var fechaInicial = $("#fecha-inicial").val();
    var fechaFinal = $("#fecha-final").val();
    if (fechaFinal < fechaInicial) return { isValid: false, message: "El rango de fechas es incorrecto, favor de verificar." };

    return result;
}