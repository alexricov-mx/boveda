$(function () {
    let menuOpen;
    switch (redirect) {
        case "Consultas/OrdenSurtimiento":
            menuOpen = "StatisticsSupplyOrders";
            break;
        case "Consultas/EstimacionObra":
            menuOpen = "StatisticsSOEstimations";
            break;
        default:
            break;
    }
    setOpenMenu("menu_Consulta", menuOpen);
    darkMode(getCookie("dark-mode") == "true");
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


//ver documento PDF
$(document).on("click", ".image-pdf", function (e) {
    e.preventDefault();
    var id = $(this).data("id");
    var sapOrder = $(this).data("saporder");
    var organismo = $(this).data("organismclave");
    var documentoBEId = $(this).data("documentobeid");
    var tipo = $(this).data("tipo");

    var url = "DocumentoPDF/GetDocumentoPDF";
    var data = { SAPOrder: sapOrder, Organismo: organismo, DocumentoBEId: documentoBEId };

    if (tipo != undefined && tipo != null) {
        if (tipo == 'I') {
            url = 'DocumentoPDF/FacturaPDF';
            data = { InvoiceId: documentoBEId };
        }
        else if (tipo == 'N') {
            url = 'DocumentoPDF/NotaCreditoPDF';
            data = { NotaCreditoId: documentoBEId };
        }
    }

    $.ajax({
        url: url,
        data: data,
        success: function (data) {
            if (!isNull(data.success) && !data.success)
                return infoAlert(data.message);
            showFile(data.file);
        },
    });
    e.stopImmediatePropagation();
});

$(document).on("click", "i[name='pdfAP']", function (e) {
    e.preventDefault();
    var id = $(this).data("id");
    var ivCveTransportista = $(this).data("cvetransportista");
    var ivNumCliente = $(this).data("numcliente");
    var ivEjercicio = $(this).data("exercise");
    var ivAnalitico = $(this).data("analitico");
    $.ajax({
        type: "GET",
        url: "DocumentoPDF/GetDocumentoAnaliticoPagoPDF",
        data: {
            CveTransportista: ivCveTransportista,
            NumCliente: ivNumCliente,
            Ejercicio: ivEjercicio,
            Analitico: ivAnalitico
        },//que necesito enviar para recibir el pdf
        success: function (data) {
            if (!isNull(data.success) && !data.success)
                return infoAlert(data.message);
            showFile(data.file);
        },
    });
    e.stopImmediatePropagation();
});

function ComprobantePDF(UUid) {
    try {
        $.ajax({
            type: "GET",
            url: 'DocumentoPDF/ComprobantePDF',
            data: { PagosID: UUid },
            success: function (data) {
                if (data != '' && data != undefined) {
                    showFile(data.file);
                    return;
                } else {
                    infoAlert("No se pudo recuperar el archivo PDF");
                }

            }
        });
    }
    catch (e) {

    }

}
function pdfPreview(clave, sapOrder, reception, exercise, functionary1SignDate, CopadeId) {
    try {
        var copade =
        {
            Clave: clave,
            SapOrder: sapOrder,
            Reception: reception,
            Exercise: exercise,
            Functionary1SignDate: !isNull(functionary1SignDate),
            CopadeId: CopadeId
        }
        $.ajax({
            type: "POST",
            url: 'Copade/pdfPreview',
            data: { dto: copade },
            success: function (data) {
                if (!isNull(data)) {
                    showFile(data);
                    return;
                } else {
                    infoAlert("No se pudo recuperar el archivo PDF");
                }
            }
        });
    }
    catch (e) {

    }
}

$(document).on("click", "i[name='detalleFirma']", function (e) {
    e.preventDefault();
    var documentoBEId = $(this).data("documentobeid");
    var orden = $(this).data("orden");
    $.ajax({
        type: "GET",
        url: "ESign/ConsultaDocumentoFirma",
        data: { DocumentoBEId: documentoBEId, Orden: orden },
        success: function (data) {
            if (!isNull(data.success) && !data.success)
                return infoAlert(data.message);
            $("#evidencia-firma").empty();
            $("#evidencia-firma").append(data);
            $("#EvidenciaFirmaModal").modal("show");
            return;
        },
    });
    e.stopImmediatePropagation();
});

$(document).on("click", "i[name='prefactura']", function (e) {
    e.preventDefault();
    var dato = $(this).data("id");
    $.ajax({
        type: "GET",
        url: "ESign/PrefacturaConsultar",
        data: { filtro: dato},
        success: function (data) {
            if (!isNull(data.success) && !data.success)
                return infoAlert(data.message);
            $("#prefactura").empty();
            $("#prefactura").append(data);
            var txt = $("#cont").val();
            var prettyXmlText = new XmlBeautify().beautify(txt,
                { indent: "  ", useSelfClosingElement: true });
            $("#cont").empty();
            $("#cont").val(prettyXmlText);
            $("#PrefacturaModal").modal("show");
            return;
        },
    });
    e.stopImmediatePropagation();
});



