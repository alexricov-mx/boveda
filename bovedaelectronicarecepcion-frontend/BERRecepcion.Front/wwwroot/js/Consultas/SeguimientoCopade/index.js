$(document).ready(function () {
    setOpenMenu("menu_Consulta", "QueryTracing");    
    var fechaInicial = $("#FechaInicial").val();
    var fechaFinal = $("#FechaFinal").val();
    var search = [];
    $.each($(document).find('span[name="badgeTerms"]'), function (index, item) {
        search.push($(item).text());
    });
    ConsultaCopadeTable(fechaInicial, fechaFinal, search);

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

$(document).on("click", ".page-link", function (e) {
    e.preventDefault();
    var pageNum = $(this).data("page_num");
    var fechaInicial = $("#FechaInicial").val();
    var fechaFinal = $("#FechaFinal").val();
    var search = [];
    $.each($(document).find('span[name="badgeTerms"]'), function (index, item) {
        search.push($(item).text());
    });
    ConsultaCopadeTable(fechaInicial, fechaFinal, search, pageNum);
    e.stopImmediatePropagation();
});

var ConsultaCopadeTable = function (fechaInicial, fechaFinal, search = [], pageNum = 1) {
    $.ajax({
        type: "POST",
        url: "Consultas/ConsultaSeguimientoCopadeTable",
        data: { fechaInicial: fechaInicial, fechaFinal: fechaFinal, search: search, pageNum: pageNum },
        success: function (data) {
            if (!isNull(data.success) && !data.success)
                return errorAlert(data.message);
            $("#ConsultaCopadetable").empty();
            $("#ConsultaCopadetable").append(data);
        }
    });
}

function updateSearch() {
    var fechaInicial = $("#FechaInicial").val();
    var fechaFinal = $("#FechaFinal").val();
    var search = [];
    $.each($(document).find('span[name="badgeTerms"]'), function (index, item) {
        search.push($(item).text());
    });
    ConsultaCopadeTable(fechaInicial, fechaFinal, search);
}

$(document).on("submit", "#consulta-form-search", function (e) {
    e.preventDefault();
    var fechaInicial = $("#FechaInicial").val();
    var fechaFinal = $("#FechaFinal").val();
    var search = [];
    $.each($(document).find('span[name="badgeTerms"]'), function (index, item) {
        search.push($(item).text());
    });
    ConsultaCopadeTable(fechaInicial, fechaFinal, search);
    e.stopImmediatePropagation();
});

$(document).on("click", "i[name='btnExpediente']", function (e) {
    e.preventDefault();
    var copadeID = $(this).data("copadeid");
    var url = "Consultas/SeguimientoCopade/Detalle";
    let search = "";
    $.each($(document).find('span[name="badgeTerms"]'), function (index, item) {
        search += `${$(item).text().trim()};`;
    });
    var form = $('<form action="' + url + '" method="post">' +
        '<input type="text" name="fechaInicial" value="' + $("#FechaInicial").val() + '"  />' +
        '<input type="text" name="fechaFinal" value="' + $("#FechaFinal").val() + '"  />' +
        '<input type="text" name="copadeID" value="' + copadeID + '"  />' +
        '<input type="text" name="search" value="' + search + '" />' +
        '</form>');
    $('body').append(form);
    form.submit().remove();
    e.stopImmediatePropagation();
});

$("#btnExcel").on("click", function (e) {
    e.preventDefault();
    var fechaInicial = $("#FechaInicial").val();
    var fechaFinal = $("#FechaFinal").val();
    let search = "";
    $.each($(document).find('span[name="badgeTerms"]'), function (index, item) {
        search += `${$(item).text().trim()};`;
    });
    search = search.slice(0, -1);
    location.href = "Consultas/DownloadExcelSeguimientoCopade?fechaInicial=" + fechaInicial + "&fechaFinal=" + fechaFinal + "&search=" + search;
    e.stopImmediatePropagation();
});
