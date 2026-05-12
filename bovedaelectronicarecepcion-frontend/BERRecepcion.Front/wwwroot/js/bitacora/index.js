$(document).ready(function () {
    setOpenMenu("menu_Administración", "AdministrationLog");
    
});

let BitacoraTable = function (fechaInicial, fechaFinal, busqueda, pageNum = 1) {
    $.ajax({
        type: "GET",
        url: "Bitacora/BitacoraTable",
        data: { fechaInicial: fechaInicial, fechaFinal: fechaFinal, busqueda: busqueda, pageNum: pageNum },
    }).then(function (data) {
        if (!isNull(data.success) && !data.success)
            return errorAlert(data.message);
        $("#bitacora-div").empty();
        $("#bitacora-div").append(data);
    });
}

$("#btnBuscar").on("click", function (e) {
    e.preventDefault();
    var fechaInicial = $("#FechaInicial").val();
    var fechaFinal = $("#FechaFinal").val();
    var busqueda = $("#Busqueda").val();
    if (!isNull(fechaInicial) && !isNull(fechaFinal))
        BitacoraTable(fechaInicial, fechaFinal, busqueda);
    else
        infoAlert("Es necesario ingresar ambas fechas antes de continuar");
    e.stopImmediatePropagation();
});

$(document).on("click", ".page-link", function (e) {
    e.preventDefault();
    var pageNum = $(this).data("page_num");
    var fechaInicial = $("#FechaInicial").val();
    var fechaFinal = $("#FechaFinal").val();
    var busqueda = $("#Busqueda").val();
    BitacoraTable(fechaInicial, fechaFinal, busqueda, pageNum);
    e.stopImmediatePropagation();
});
$("#ExcelBitacora").on("click", function (e) {
    e.preventDefault();
    var fechaInicial = $("#FechaInicial").val();
    var fechaFinal = $("#FechaFinal").val();
    var busqueda = $("#Busqueda").val();
    location.href = "Bitacora/DownloadExcel?fechaInicial=" + fechaInicial + "&fechaFinal=" + fechaFinal + "&busqueda=" + busqueda;
    e.stopImmediatePropagation();
});