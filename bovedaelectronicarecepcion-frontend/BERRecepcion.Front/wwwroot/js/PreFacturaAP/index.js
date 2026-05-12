

$(document).ready(function () {
	setOpenMenu("menu_Consulta", "QueryPrefectureAnalytic");

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

		var prefacturasAP = {

			prefacturasAPXML: []
		};

		$.map($(document).find("input[name='checkPDF']"), function (x, y) {

			var rolObj = {
				AnaliticoPagoID: $(x).data("rol"),
				status: $("#" + $(x).data("rol") + "").prop('checked')
			};

			prefacturasAP.prefacturasAPXML.push(rolObj);
		});

		var Interfases = '';
		if (prefacturasAP.prefacturasAPXML.length > 0) {
			for (var i = 0; i < prefacturasAP.prefacturasAPXML.length; i++) {
				if (prefacturasAP.prefacturasAPXML[i].status == true) {
					Interfases += prefacturasAP.prefacturasAPXML[i].AnaliticoPagoID + '_';
				}
			}
			if (Interfases != '') {
				Interfases = Interfases.substring(0, Interfases.length - 1);
			}
		}

		if (Interfases == '') {
			return errorAlert("Por favor selecciona algun elemento de la lista");
		}

		location.href = "PreFacturaAP/DownloadXmlZip?dto=" + Interfases;

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
		url: "PreFacturaAP/ConsultaPreFacturaAP",
		data: { start: start, end: end, search: search, pageNum: pageNum },
		success: function (data) {
			if (!isNull(data.success) && !data.success)
				return errorAlert(data.message);
			$("#preFacturaAP-card").empty();
			$("#preFacturaAP-card").append(data);
			$("#btnExcel").show();
			$("#btnValidarInfo").show();
		},
		complete: function () {
			hideLoader();
		}
	});
}

function pdfPreview(CveTransportista, NumCliente, Ejercicio, Analitico) {
	try {
		$.ajax({
			type: "GET",
			url: 'DocumentoPDF/GetDocumentoAnaliticoPagoPDF',
			data: {
				CveTransportista: CveTransportista,
				NumCliente: NumCliente,
				Ejercicio: Ejercicio,
				Analitico: Analitico
			},
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

function base64ToArrayBuffer(data) {
	var bString = window.atob(data);
	var bLength = bString.length;
	var bytes = new Uint8Array(bLength);
	for (var i = 0; i < bLength; i++) {
		var ascii = bString.charCodeAt(i);
		bytes[i] = ascii;
	}
	return bytes;
};

$("#btnExcel").on("click", function (e) {
	e.preventDefault();
	var fechaInicial = $("#FechaInicial").val();
	var fechaFinal = $("#FechaFinal").val();
	let search = "";
	$.each($(document).find('span[name="badgeTerms"]'), function (index, item) {
		search += `${$(item).text().trim()};`;
	});
	search = search.slice(0, -1);
	location.href = "PreFacturaAP/DownloadExcel?fechaInicial=" + fechaInicial + "&fechaFinal=" + fechaFinal + "&search=" + search;
	e.stopImmediatePropagation();
});
