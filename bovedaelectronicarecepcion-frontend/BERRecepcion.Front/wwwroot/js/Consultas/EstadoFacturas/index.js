$(function () {

    setOpenMenu("menu_Consulta", "StatisticsReceptionInvoice")

    $(document).on("click", ".page-link", function (e) {
        e.preventDefault();
        var pageNum = $(this).data("page_num");
        obtenerEstadoFacturasTable(pageNum);
        e.stopImmediatePropagation();
    });

    $(document).on("submit", "#estadofacturas-form-search", function (e) {
        e.preventDefault();
        obtenerEstadoFacturasTable();
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
            location.href = "Consultas/DownloadExcelGetAllEstadoFacturas?search=" + search.toString().replaceAll(',','__') + "&start=" + start + "&end=" + end + "&pageNum=" + pageNum;
        }
        else {
            return infoAlert(dataValid.message);
        }
        e.stopImmediatePropagation();
    });

    var isRedirect = $("#isRedirectEstadoFacturas").val();
    if (isRedirect.toUpperCase() == "TRUE") {
        obtenerEstadoFacturasTable(1);
    }

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

function updateSearch() {
    obtenerEstadoFacturasTable(1);
}

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

function obtenerEstadoFacturasTable(pageNum) {
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
            url: "Consultas/obtenerEstadoFacturasTable",
            data: { start: start, end: end, search: search, pageNum: pageNum },
            success: function (data) {
                if (!isNull(data.success) && !data.success) {
                    return errorAlert(data.message);
                }
                else {
                    $("#ExportExcelAll").show();
                }
                $("#estadofacturas-card").empty();
                $("#estadofacturas-card").append(data);
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

$(document).on("mouseover mouseout", "i[name='btnExpediente']", function (e) {
    e.stopImmediatePropagation();
    setAnimation($(this), e.type == 'mouseover');
    e.stopImmediatePropagation();
});

$(document).on("mouseover mouseout", "i[name='btnExpedienteAnalitico']", function (e) {
    e.stopImmediatePropagation();
    setAnimation($(this), e.type == 'mouseover');
    e.stopImmediatePropagation();
});

function setAnimation(icon, isMouseOver) {
    if (isMouseOver)
        icon.removeClass("fa-folder").addClass("fa-folder-open");
    else
        icon.addClass("fa-folder").removeClass("fa-folder-open");
}

$(document).on("click", "i[name='btnExpediente']", function (e) {
    e.preventDefault();
    var copadeID = $(this).data("documentoid");
    if (copadeID != undefined || copadeID != '') {
        var url = "Consultas/Copade/ExpedienteElectronico";
        var form = $('<form action="' + url + '" method="post">' +
            '<input type="text" name="fechaInicial" value="' + $("#fecha-inicial").val() + '"  />' +
            '<input type="text" name="fechaFinal" value="' + $("#fecha-final").val() + '"  />' +
            '<input type="text" name="copadeID" value="' + copadeID + '"  />' +
            '<input type="text" name="search" value="' + $("#search").val() + '" />' +
            '<input type="text" name="rutaRegreso" value="Consultas/EstadoFacturas?isRedirect=true" />' +
            '</form>');
        $('body').append(form);
        form.submit().remove();
    }
    e.stopImmediatePropagation();
});

$(document).on("click", "i[name='btnExpedienteAnalitico']", function (e) {
    e.preventDefault();
    var AnaliticoPagoID = $(this).data("documentoid");
    var url = "Consultas/AnaliticoPago/ExpedienteElectronico";
    var form = $('<form action="' + url + '" method="post">' +
        '<input type="text" name="fechaInicial" value="' + $("#fecha-inicial").val() + '"  />' +
        '<input type="text" name="fechaFinal" value="' + $("#fecha-final").val() + '"  />' +
        '<input type="text" name="analiticopagoid" value="' + AnaliticoPagoID + '"  />' +
        '<input type="text" name="search" value="' + $("#Busqueda").val() + '" />' +
        '<input type="text" name="rutaRegreso" value="Consultas/EstadoFacturas?isRedirect=true" />' +
        '</form>');
    $('body').append(form);
    form.submit().remove();
    e.stopImmediatePropagation();
});