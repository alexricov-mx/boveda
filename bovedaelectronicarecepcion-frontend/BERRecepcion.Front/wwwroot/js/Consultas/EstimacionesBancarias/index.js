$(function () {

    setOpenMenu("menu_Consulta", "ReceptionSignSOEstimations")

    $(document).on("click", ".page-link", function (e) {
        e.preventDefault();
        var pageNum = $(this).data("page_num");
        obtenerordensurtimientoTable(pageNum);
        e.stopImmediatePropagation();
    });

    $(document).on("submit", "#ordensurtimiento-form-search", function (e) {
        e.preventDefault();
        obtenerordensurtimientoTable();
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
            location.href = "Consultas/DownloadExcelGetAllEstimacionesBancarias?filtro=" + search.toString().replaceAll(',', '__') + "&start=" + start + "&end=" + end;
        }
        else {
            return infoAlert(dataValid.message);
        }
        e.stopImmediatePropagation();
    });

    //ver documento PDF
    $(document).on("click", ".image-pdf", function (e) {
        e.preventDefault();
        var id = $(this).data("id");
        var sapOrder = $(this).data("saporder");
        var organismo = $(this).data("organismclave");
        var functionarysigndate = $(this).data("functionarysigndate");
        $.ajax({
            type: "GET",
            url: "DocumentoPDF/GetDocumentoPDF",
            data: { SAPOrder: sapOrder, Organismo: organismo, DocumentoBEId: isNull(functionarysigndate) ? null : id },
            success: function (data) {
                if (!isNull(data.success) && !data.success)
                    return infoAlert(data.message);
                showFile(data.file, false);
            },
        });
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
    obtenerordensurtimientoTable(1);
}

function obtenerordensurtimientoTable(pageNum) {
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
            url: "Consultas/obtenerEstimacionesBancariasTable",
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
                $("#ordensurtimiento-card").empty();
                $("#ordensurtimiento-card").append(data);
                darkMode(getCookie("dark-mode") == "true");
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
