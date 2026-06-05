$(function () {
    setOpenMenu("menu_OSRecepción", "ReceptionSignSupplyOrders");
    OrdenSurtimientoCard();

    $(document).on("click", ".page-link", function (e) {
        e.preventDefault();
        var pageNum = $(this).data("page_num");
        var search = $("#search").val();
        OrdenSurtimientoCard(pageNum, search);
        e.stopImmediatePropagation();
    });

    //ver documento PDF
    $(document).on("click", ".image-pdf", function (e) {
        e.preventDefault();
        var id = $(this).data("id");
        var sapOrder = $(this).data("saporder");
        var organismo = $(this).data("organismclave");
        var functionarysigndate = $(this).data("functionarysigndate");
        $.ajax({
            type: "GET",
            url: "DocumentoPDF/GetDocumentoPDF",
            data: {SAPOrder: sapOrder, Organismo: organismo, DocumentoBEId: isNull(functionarysigndate) ? null : id},
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
    $(document).on("submit", "#supply-orders-form-search", function (e) {
        e.preventDefault();
        var search = $("#search").val();
        OrdenSurtimientoCard(pageNum = 1, search);
        e.stopImmediatePropagation();
    });
});



function OrdenSurtimientoCard(pageNum = 1, search = "") {
    apiGet("OrdenSurtimiento/GetOrdenSurtimiento", {pageNum: pageNum, search: search},
        onSuccess = (data) => {
            $("#ordenSurtimiento-card").empty();
            $("#ordenSurtimiento-card").append(data);
            darkMode(getCookie("dark-mode") == "true");
        }, onErrror = (error) => {
            console.log(error);
            $("#ordenSurtimiento-card").empty();
            errorAlert(error.message);
        });
}



