$(function () {
    $(function () {
        setOpenMenu("menu_Facturas", "AdministrationCxpExecution");
    });

    InvoiceAPCxPTable();

    $(document).on("click", ".page-link", function (e) {
        e.preventDefault();
        var pageNum = $(this).data("page_num");
        InvoiceAPCxPTable(pageNum);
        e.stopImmediatePropagation();
    });


    $(document).on("click", "#btnValidarInfo", function (e) {
        e.preventDefault();
        return $(document).find("input[name ='checkPDF']:checked").length > 0 ? $("#modalFirma").modal("show") : infoAlert("Debes seleccionar al menos un documento para firmar.");
        e.stopImmediatePropagation();
    });

    $(document).on("submit", "#cg-form-search", function (e) {
        e.preventDefault();
        var filtro = $("#search").val();
        InvoiceAPCxPFiltroTable(filtro);
        e.stopImmediatePropagation();
    });

});

function InvoiceAPCxPTable(pageNum = 1) {
    $.ajax({
        type: "GET",
        url: "InvoiceAPCxP/InvoiceAPCxPTable",
        data: { pageNum: pageNum },
        success: function (data) {
            if (!isNull(data.success) && !data.success)
                return errorAlert(data.message);
            $("#InvoiceAPCxP-card").empty();
            $("#InvoiceAPCxP-card").append(data);
        }
    });
}

function InvoiceAPCxPFiltroTable(filtro, pageNum = 1) {
    $.ajax({
        type: "GET",
        url: "InvoiceAPCxP/InvoiceFiltroAPCxPTable",
        data: { filtro: filtro, pageNum: pageNum },
        success: function (data) {
            if (!isNull(data.success) && !data.success)
                return errorAlert(data.message);
            $("#InvoiceAPCxP-card").empty();
            $("#InvoiceAPCxP-card").append(data);
        },
    });
}


function SaveInvoiceAPCxP(InvoiceId, Reception, CxpSendDate, res, clave, Exercise, SapOrder, Serie, Folio, Usuario, Uuid, Assignment, InvoiceDate, ReceptionDate, Creditor,Total, idAnalitico) {
    showLoader();

    var uri = $('#urlSaveInvoiceAPCxP').val();

    var Data = {
        InvoiceId: InvoiceId,
        Reception: Reception,
        CxpSendDate: CxpSendDate,
        res: res,
        clave: clave,
        Exercise: Exercise,
        SapOrder: SapOrder,
        Serie: Serie,
        Folio: Folio,
        Usuario: Usuario,
        Uuid: Uuid,
        Assignment: Assignment,
        InvoiceDate: InvoiceDate,
        ReceptionDate: ReceptionDate,
        Creditor: Creditor,
        Total: Total,
        idAnalitico: idAnalitico
    };


    $.ajax({
        type: "post",
        datatype: 'json',
        url: uri,
        data: Data,
        success: function (response) {
            if (response.success) {
                hideLoader();
                InvoiceAPCxPTable();
                successAlert(response.message);

            } else {
                hideLoader();
                InvoiceAPCxPTable();
                errorAlert(response.message);                
            }
        }
    });

}