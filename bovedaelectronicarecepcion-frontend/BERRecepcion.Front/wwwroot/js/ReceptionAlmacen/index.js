$(function () {
    setOpenMenu("menu_OSRecepción", "ReceptionSignReception");
    ReceptionCard();

    $(document).on("click", ".page-link", function (e) {
        e.preventDefault();
        var pageNum = $(this).data("page_num");
        var search = $("#search").val();
        ReceptionCard(pageNum, search);
        e.stopImmediatePropagation();
    });

    //ver documento PDF
    $(document).on("click", ".image-pdf", function (e) {
        e.preventDefault();
        let ReceptionId = $(this).data('receptionid');
        $.ajax({
            url: "ReceptionAlmacen/GetDocumentoPDF",
            data: { ReceptionId: ReceptionId },
            success: function (data) {
                if (!isNull(data.success) && !data.success)
                    return infoAlert(data.message);
                showFile(data.file);
            },
        });
        e.stopImmediatePropagation();
    });

    ////Buscar recepciones
    $(document).on("submit", "#reception-form-search", function (e) {
        e.preventDefault();
        var search = $("#search").val();
        ReceptionCard(1, search);
        e.stopImmediatePropagation();
    });
});

function ReceptionCard(pageNum = 1, search = "") {
    $.ajax({
        url: "ReceptionAlmacen/ReceptionCard",
        data: { pageNum: pageNum, search: search },
    }).then(function (data) {
        if (!isNull(data.success) && !data.success)
            return errorAlert(data.message);
        $("#reception-card").empty();
        $("#reception-card").append(data);
        darkMode(getCookie("dark-mode") == "true");
    });
}