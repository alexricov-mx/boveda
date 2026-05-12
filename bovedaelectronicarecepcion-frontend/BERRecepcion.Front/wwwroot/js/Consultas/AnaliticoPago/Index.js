$(document).ready(function () {
	setOpenMenu("menu_Consulta", "StatisticsAnalyticalPayment");
	var search = $("#search").val();
	var fechaInicial = $("#FechaInicial").val();
	var fechaFinal = $("#FechaFinal").val();
	GetAPTable(1, fechaInicial, fechaFinal, search);

	$(document).on("click", ".page-link", function (e) {
		e.preventDefault();
		var pageNum = $(this).data("page_num");
		GetAPTable("",pageNum);
		e.stopImmediatePropagation();
	});
});
$(document).on("click", ".page-link", function (e) {
	e.preventDefault();
	var pageNum = $(this).data("page_num");
	var fechaInicial = $("#FechaInicial").val();
	var fechaFinal = $("#FechaFinal").val();
	var search = $("#search").val();
	GetAPTable(pageNum, fechaInicial, fechaFinal, search);
	e.stopImmediatePropagation();
});
$(document).on("submit", "#consulta-form-search", function (e) {
	e.preventDefault();
	var search = $("#search").val();
	var fechaInicial = $("#FechaInicial").val();
	var fechaFinal = $("#FechaFinal").val();
	GetAPTable(1, fechaInicial, fechaFinal, search);
	e.stopImmediatePropagation();
});

$(document).on("mouseover mouseout", "i[name='btnExpediente']", function (e) {
	e.stopImmediatePropagation();
	setAnimation($(this), e.type == 'mouseover');
	e.stopImmediatePropagation();
});

function setAnimation(icon, isMouseOver) {
	if (isMouseOver)
		icon.removeClass("fa-folder").addClass("fa-folder-open");
	else
		icon.addClass("fa-folder").removeClass("fa-folder-open");
}

let GetAPTable = function (pageNum = 1, fechaInicial = null, fechaFinal = null, search = "") {
	$.ajax({
		type: "GET",
		url: "Consultas/GetAPFactura",
		data: { fechaInicial: fechaInicial, fechaFinal: fechaFinal, pageNum: pageNum, search: search },
		success: function (data) {
			if (!isNull(data.success) && !data.success)
				return errorAlert(data.message);
			$("#consulta-table").empty();
			$("#consulta-table").append(data);
			darkMode(getCookie("dark-mode") == "true");
			if (!_hasData)
				$("#Excel").hide();
			else
				$("#Excel").show();
		}
	});
}

$(document).on("click", "i[name='btnExpediente']", function (e) {
	e.preventDefault();
	var AnaliticoPagoID = $(this).data("analiticopagoid");
	var url = "Consultas/AnaliticoPago/ExpedienteElectronico";
	var form = $('<form action="' + url + '" method="post">' +
		'<input type="text" name="analiticopagoid" value="' + AnaliticoPagoID + '"  />' +
		'<input type="text" name="fechaInicial" value="' + $("#FechaInicial").val() + '"  />' +
		'<input type="text" name="fechaFinal" value="' + $("#FechaFinal").val() + '"  />' +
		'<input type="text" name="search" value="' + $("#search").val() + '"  />' +
		'</form>');
	$('body').append(form);
	form.submit().remove();
	e.stopImmediatePropagation();
});

//Descarga excel
$("#Excel").on("click", function (e) {
	e.preventDefault();
	var fechaInicial = $("#FechaInicial").val();
	var fechaFinal = $("#FechaFinal").val();
	var busqueda = $("#search").val();
	location.href = "Consultas/DownloadExcelOrdenSurtimiento?fechaInicial=" + fechaInicial + "&fechaFinal=" + fechaFinal + "&search=" + busqueda;
	e.stopImmediatePropagation();
})