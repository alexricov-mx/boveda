$(document).ready(function () {
	setOpenMenu("menu_COPADEAnalitico", "AnalyticalPayment")
	GetAPTable();

	$(document).on("click", ".page-link", function (e) {
		e.preventDefault();
		var pageNum = $(this).data("page_num");
		var search = $("#search").val();
		GetAPTable(search, pageNum);
		e.stopImmediatePropagation();
	});

	$(document).on("submit", "#ap-form-search", function (e) {
		e.preventDefault();
		var search = $("#search").val();
		GetAPTable(search, 1);
		e.stopImmediatePropagation();
	});

    $(document).on("click", ".image-pdf", function (e) {
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
});

function GetAPTable(search = "", pageNum = 1) {
	$.ajax({
		url: "AnaliticoPago/GetAP",
		data: { search: search, pageNum: pageNum },
		success: function (data) {
			if (!isNull(data.success) && !data.success)
				return errorAlert(data.message);
			$("#analiticopago-card").empty();
			$("#analiticopago-card").append(data);
			darkMode(getCookie("dark-mode") == "true")
		}
	});
}