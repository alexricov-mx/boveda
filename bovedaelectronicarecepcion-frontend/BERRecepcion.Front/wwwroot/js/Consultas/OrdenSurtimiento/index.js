$(function () {
    setOpenMenu("menu_Consulta", "StatisticsSupplyOrders");
    let search = [];
    $.each($(document).find('span[name="badgeTerms"]'), function (index, item) {
        search.push($(item).text());
    });
    let fechaInicial = $("#FechaInicial").val();
    let fechaFinal = $("#FechaFinal").val();
    OrdenSurtimientoConsultaTable(1, fechaInicial, fechaFinal, search);

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
    var fechaInicial = $("#FechaInicial").val();
    var fechaFinal = $("#FechaFinal").val();
    var search = [];
    $.each($(document).find('span[name="badgeTerms"]'), function (index, item) {
        search.push($(item).text());
    });
    OrdenSurtimientoConsultaTable(1, fechaInicial, fechaFinal, search);
}

var OrdenSurtimientoConsultaTable = function (pageNum = 1, fechaInicial = null, fechaFinal = null, search = []) {
    $.ajax({
        type: "POST",
        url: "Consultas/OrdenSurtimientoConsultaTable",
        data: { fechaInicial: fechaInicial, fechaFinal : fechaFinal, pageNum : pageNum, search: search },
        success: function (data) {
            if (!isNull(data.success) && !data.success)
                return errorAlert(data.message);
            $("#os-consulta-table").empty();
            $("#os-consulta-table").append(data);
            darkMode(getCookie("dark-mode") == "true");
            if (!_hasData)
                $("#Excel").hide();
            else
                $("#Excel").show();
        }
    });
}
//Buscar órdenes de surtimiento
$(document).on("submit", "#consulta-form-search", function (e) {
    e.preventDefault();
    let search = [];
    $.each($(document).find('span[name="badgeTerms"]'), function (index, item) {
        search.push($(item).text().trim());
    });
    let fechaInicial = $("#FechaInicial").val();
    let fechaFinal = $("#FechaFinal").val();
    OrdenSurtimientoConsultaTable(1, fechaInicial, fechaFinal, search);
    e.stopImmediatePropagation();
});
$(document).on("mouseover mouseout", "i[name='btnExpediente']", function (e) {
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
//Ver expediente
$(document).on("click", "i[name='btnExpediente']", function (e) {
    e.preventDefault();
    var saporder = $(this).data("saporder");
    var url = "Consultas/OrdenSurtimiento/ExpedienteElectronico";
    let search = "";
    $.each($(document).find('span[name="badgeTerms"]'), function (index, item) {
        search += `${$(item).text().trim()};`;
    });
    search = search.slice(0, -1);

    var form = $('<form action="' + url + '" method="POST">' +
        '<input type="text" name="SAPOrder" value="' + saporder + '"  />' +
        '<input type="text" name="fechaInicial" value="' + $("#FechaInicial").val() + '"  />' +
        '<input type="text" name="fechaFinal" value="' + $("#FechaFinal").val() + '"  />' +
        '<input type="text" name="search" value="' + search + '"  />' +
        '</form>');
    
    $('body').append(form);
    form.submit().remove();
    e.stopImmediatePropagation();
});
//Paginacion
$(document).on("click", ".page-link", function (e) {
    e.preventDefault();
    var pageNum = $(this).data("page_num");
    var fechaInicial = $("#FechaInicial").val();
    var fechaFinal = $("#FechaFinal").val();
    let search = [];
    $.each($(document).find('span[name="badgeTerms"]'), function (index, item) {
        search.push($(item).text().trim());
    });
    OrdenSurtimientoConsultaTable(pageNum, fechaInicial, fechaFinal, search);
    e.stopImmediatePropagation();
});
//Descarga excel
$("#Excel").on("click", function (e) {
    e.preventDefault();
    var fechaInicial = $("#FechaInicial").val();
    var fechaFinal = $("#FechaFinal").val();
    let search = "";
    $.each($(document).find('span[name="badgeTerms"]'), function (index, item) {
        search += `${$(item).text().trim()};`;
    });
    search = search.slice(0, -1);
    location.href = "Consultas/DownloadExcelOrdenSurtimiento?fechaInicial=" + fechaInicial + "&fechaFinal=" + fechaFinal + "&search=" + search;
    e.stopImmediatePropagation();
})