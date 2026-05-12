$(function () {

    setOpenMenu("menu_Consulta", "StatisticsCopadeBanking")

    $(document).on("click", ".page-link", function (e) {
        e.preventDefault();
        var pageNum = $(this).data("page_num");
        obtenerCOPADETable(pageNum);
        e.stopImmediatePropagation();
    });

    $(document).on("submit", "#copade-form-search", function (e) {
        e.preventDefault();
        obtenerCOPADETable();
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
            location.href = "Consultas/DownloadExcelGetAllCOPADE?search=" + search.toString().replaceAll(',', '__') + "&start=" + start + "&end=" + end + "&pageNum=" + pageNum;
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
    obtenerCOPADETable(1);
}

function obtenerCOPADETable(pageNum) {
    $("#ExportExcelAll").hide();
    var start = $("#fecha-inicial").val();
    var end = $("#fecha-final").val();
    var search = [];
    $.each($(document).find('span[name="badgeTerms"]'), function (index, item) {
        search.push($(item).text());
    });
    var dataValid = validateData();
    if (dataValid.isValid) {
        $.ajax({
            type: "POST",
            url: "Consultas/obtenerCOPADETable",
            data: { start: start, end: end, search: search, pageNum: pageNum },
            success: function (data) {
                if (!isNull(data.success) && !data.success) {
                    return errorAlert(data.message);
                }
                else {
                    $("#ExportExcelAll").show();
                }
                $("#copade-card").empty();
                $("#copade-card").append(data);
            },
        });
    }
    else {
        return infoAlert(dataValid.message);
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

function pdfPreview(clave, sapOrder, reception, exercise, functionary1SignDate, CopadeId) {
    try {
        var copade = {
            Clave: clave,
            SapOrder: sapOrder,
            Reception: reception,
            Exercise: exercise,
            Functionary1SignDate: !isNull(functionary1SignDate),
            CopadeId: CopadeId
        };
        $.ajax({
            type: "POST",
            url: 'Consultas/pdfPreviewCOPADE',
            data: { dto: copade},
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

