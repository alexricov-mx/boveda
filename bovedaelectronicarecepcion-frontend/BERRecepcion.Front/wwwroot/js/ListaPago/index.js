$(function () {
    setOpenMenu("menu_Instrucciones", "PaymentList");
    ListaPagoCard();

    $(document).on("click", ".page-link", function (e) {
        e.preventDefault();
        var search = $("#search").val();
        var pageNum = $(this).data("page_num");
        ListaPagoCard(pageNum, search);
        e.stopImmediatePropagation();
    });

    //ver documento PDF
    $(document).on("click", ".image-pdf", function (e) {
        e.preventDefault();
        var id = $(this).data("id");
        var lpid = $(this).data("lpid");
        $.ajax({
            type: "GET",
            url: "DocumentoPDF/GetDocumentoLP",
            data: { ListaPago_id: lpid },
            success: function (data) {
                if (!isNull(data.success) && !data.success)
                    return infoAlert(data.message);
                showFile(data.file);
                $("#" + id).removeAttr("disabled");
            },
        });
        e.stopImmediatePropagation();
    });
    ////Buscar ordenes de surtimiento
    $(document).on("submit", "#lista-pago-form-search", function (e) {
        e.preventDefault();
        var search = $("#search").val();
        ListaPagoCard(pageNum = 1, search);
        e.stopImmediatePropagation();
    });
});

function ListaPagoCard(pageNum = 1, search = "") {
    $.ajax({
        type: "GET",
        url: "ListaPago/ListaPagoCard",
        data: { pageNum: pageNum, search: search },
        success: function (data) {
            if (!isNull(data.success) && !data.success)
                return errorAlert(data.message);
            $("#lista-pago-card").empty();
            $("#lista-pago-card").append(data);
            
        }
    });
}


