$(function () {
    setOpenMenu("menu_Instrucciones", "ReceptionSignPaymentSchedule");
    ProgramaPagoCard();

    $(document).on("click", ".page-link", function (e) {
        e.preventDefault();
        var pageNum = $(this).data("page_num");
        var search = $("#search").val();
        ProgramaPagoCard(pageNum, search);
        e.stopImmediatePropagation();
    });

    //ver documento PDF
    $(document).on("click", ".image-pdf", function (e) {
        e.preventDefault();
        var id = $(this).data("id");
        var ppid = $(this).data("ppid");
        var organismo = $(this).data("organismclave");
        $.ajax({
            type: "GET",
            url: "DocumentoPDF/GetDocumentoPP",
            data: { PPId: ppid, Organismo: organismo },
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
    $(document).on("submit", "#programa-pago-form-search", function (e) {
        e.preventDefault();
        var search = $("#search").val();
        ProgramaPagoCard(pageNum = 1, search);
        e.stopImmediatePropagation();
    });
});

function ProgramaPagoCard(pageNum = 1, search = "") {
    $.ajax({
        type: "GET",
        url: "ProgramaPago/ProgramaPagoCard",
        data: { pageNum: pageNum, search: search },
        success: function (data) {
            if (!isNull(data.success) && !data.success)
                return errorAlert(data.message);
            $("#programa-pago-card").empty();
            $("#programa-pago-card").append(data);
            
        }
    });
}


