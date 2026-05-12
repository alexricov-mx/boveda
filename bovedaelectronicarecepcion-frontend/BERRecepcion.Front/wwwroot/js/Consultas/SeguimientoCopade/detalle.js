$(document).ready(function () {
    setOpenMenu("menu_Consulta", "QueryTracing");    
    
});

$("#btnRegresar").on("click", function (e) {
    e.preventDefault();
    var url = $(this).data("redirect");
    var form = $('<form action="' + url + '" method="post">' +
        '<input type="text" name="fechaInicial" value="' + fechaInicial + '"  />' +
        '<input type="text" name="fechaFinal" value="' + fechaFinal + '"  />' +
        '<input type="text" name="search" value="' + search + '"  />' +
        '</form>');
    $('body').append(form);
    form.submit().remove()
    e.stopImmediatePropagation();
});

$("#btnExpedienteSeguimiento").on("click", function (e) {
    e.preventDefault();
    var CopadeID = $(this).data("copadeid");
    var url = "Consultas/Copade/ExpedienteElectronico";
    var form = $('<form action="' + url + '" method="post">' +
        '<input type="text" name="copadeid" value="' + CopadeID + '"  />' +
        '<input type="text" name="fechaInicial" value="' + fechaInicial + '"  />' +
        '<input type="text" name="fechaFinal" value="' + fechaFinal + '"  />' +
        '<input type="text" name="search" value="' + search + '"  />' +
        '<input type="text" name="esSeguimiento" value="true"  />' +
        '</form>');
    $('body').append(form);
    form.submit().remove();
    e.stopImmediatePropagation();
});