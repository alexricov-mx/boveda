
$(document).ready(function () {
	setOpenMenu("menu_Consulta", "QueryPrefecture");

	$(document).on("click", "#buscar", function (e) {
		ConsultaPrefactura(document.getElementById('txtCG').value);
	});

	$(document).on("submit", "#consulta-form-search", function (e) {
		e.preventDefault();
		var search = $("#search").val();
		ConsultaPrefactura(1, search);
		e.stopImmediatePropagation();
	});

	$(document).on("click", ".page-link", function (e) {
		e.preventDefault();
		var pageNum = $(this).data("page_num");
		ConsultaPrefactura(pageNum);
		e.stopImmediatePropagation();
	});

	$(document).on("click", "#btnValidarInfo", function (e) {
		e.preventDefault();

		var Interfase = {
			
			CopadesID: []
		};

		$.map($(document).find("input[name='checkPDF']"), function (x, y) {

			var rolObj = {
				CopadeID: $(x).data("rol"),
				status: $("#" + $(x).data("rol") + "").prop('checked')
			};

			Interfase.CopadesID.push(rolObj);
		});

		var Interfases = '';
		if (Interfase.CopadesID.length > 0) {
			for (var i = 0; i < Interfase.CopadesID.length; i++) {
				if (Interfase.CopadesID[i].status == true) {
					Interfases += Interfase.CopadesID[i].CopadeID + '_';
				}
			}
			if (Interfases != '') {
				Interfases = Interfases.substring(0, Interfases.length - 1);
			}
        }

		if (Interfases == '') {
			return errorAlert("Por favor selecciona algun elemento de la lista");
		}

		location.href = "PreFactura/DownloadXmlZip?dto=" + Interfases;
		e.stopImmediatePropagation();
	});

	$("button").keypress(function (e) {
		if (e.which == 13) {
			return false;
		}
	});

	$("form").keypress(function (e) {
		if (e.which == 13) {
			return false;
		}
	});
});

function updateSearch() {
	ConsultaPrefactura(1);
}

function ConsultaPrefactura(pageNum = 1) {

	$("#btnExcel").hide();
	$("#btnValidarInfo").hide();
	var start = $("#FechaInicial").val();
	var end = $("#FechaFinal").val();
	var search = [];
	$.each($(document).find('span[name="badgeTerms"]'), function (index, item) {
		search.push($(item).text());
	});
	$.ajax({
		type: "POST",
		url: "PreFactura/ConsultaPreFactura",
		data: { start: start, end: end, search: search, pageNum: pageNum },
		success: function (data) {
			if (!isNull(data.success) && !data.success)
				return errorAlert(data.message);
			$("#preFactura-card").empty();
			$("#preFactura-card").append(data);
			$("#btnExcel").show();
			$("#btnValidarInfo").show();
		},
		complete: function () {
			hideLoader();
		}
	});
}

function pdfPreview(sapOrder, Organismo) {
	try {
		$.ajax({
			type: "GET",
			url: 'DocumentoPDF/GetDocumentoPDF',
			data: {
				SAPOrder: sapOrder,
				Organismo: Organismo },
			success: function (data) {
				hideLoader();
				if (!isNull(data.success) && !data.success)
					return infoAlert(data.message);
				showFile(data.file, false);
			},
			complete: function () {
				hideLoader();
			}
		});
	}
	catch (e) {

	}
}

function BuscarCCTable(fechaInicial, fechaFinal, busqueda1, pageNum = 1) {
	$("#btnExcel").hide();
	$("#btnValidarInfo").hide();
	var search = [];
	$.each($(document).find('span[name="badgeTerms"]'), function (index, item) {
		search.push($(item).text());
	});
	$.ajax({
		type: "POST",
		url: "PreFactura/BuscarPreFactura",
		ddata: { search: search, fechaInicial: fechaInicial, fechaFinal: fechaFinal,  pageNum: pageNum },
		success: function (data) {
			if (!isNull(data.success) && !data.success)
				return errorAlert(data.message);
			$("#preFactura-card").empty();
			$("#preFactura-card").append(data);
			$("#btnExcel").show();
			$("#btnValidarInfo").show();
		},
		complete: function () {
			hideLoader();
		}
	});
}

$("#btnExcel").on("click", function (e) {
	e.preventDefault();
	var fechaInicial = $("#FechaInicial").val();
	var fechaFinal = $("#FechaFinal").val();
	let search = "";
	$.each($(document).find('span[name="badgeTerms"]'), function (index, item) {
		search += `${$(item).text().trim()};`;
	});
	search = search.slice(0, -1);
	location.href = "PreFactura/DownloadExcel?fechaInicial=" + fechaInicial + "&fechaFinal=" + fechaFinal + "&search=" + search;
	e.stopImmediatePropagation();
});
