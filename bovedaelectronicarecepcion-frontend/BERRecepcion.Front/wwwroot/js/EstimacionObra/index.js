$(function () {

    setOpenMenu("menu_OSRecepción", "ReceptionSignSOEstimations")
    EstimacionObraTable();

    $(document).on("click", ".page-link", function (e) {
        e.preventDefault();
        var pageNum = $(this).data("page_num");
        EstimacionObraTable(pageNum);
        e.stopImmediatePropagation();
    });

    $(document).on("click", ".image-pdf", function (e) {
        e.preventDefault();
        var id = $(this).data("id");
        var sapOrder = $(this).data("saporder");
        var organismo = $(this).data("organismclave");
        var providersigndate = $(this).data("providersigndate");
        $.ajax({
            type: "GET",
            url: "DocumentoPDF/GetDocumentoPDF",
            data: { SAPOrder: sapOrder, Organismo: organismo, DocumentoBEId: isNull(providersigndate) ? null : id },
            success: function (data) {
                if (!isNull(data.success) && !data.success)
                    return infoAlert(data.message);
                showFile(data.file);
                $("#" + id).removeAttr("disabled");
            },
        });
        e.stopImmediatePropagation();
    });
});

function EstimacionObraTable(pageNum = 1) {
    $.ajax({
        type: "GET",
        url: "EstimacionObra/EstimacionObraTable",
        data: { pageNum: pageNum },
        success: function (data) {
            if (!isNull(data.success) && !data.success)
                return errorAlert(data.message);
            $("#estimacion-card").empty();
            $("#estimacion-card").append(data);
            darkMode(getCookie("dark-mode") == "true");
        }
    });
}
