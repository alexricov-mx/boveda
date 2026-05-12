setOpenMenu("menu_Instrucciones", "CancelPaymentList");

$(document).ready(function () {
	
	$(document).on("click", ".page-link", function (e) {
		e.preventDefault();
		var fechaInicial = $("#FechaInicial").val();
		var fechaFinal = $("#FechaFinal").val();
		var busqueda = $("#Busqueda").val();
		var pageNum = $(this).data("page_num");
		BuscarCCTable(fechaInicial, fechaFinal, busqueda, pageNum);
		e.stopImmediatePropagation();
	});

	$(document).on("click", "#btnBuscar", function (e) {

		e.preventDefault();
		var fechaInicial = $("#FechaInicial").val();
		var fechaFinal = $("#FechaFinal").val();
		var busqueda = $("#Busqueda").val();
		BuscarCCTable(fechaInicial, fechaFinal, busqueda, pageNum = 1)
		e.stopImmediatePropagation();
	});


	$(document).on("click", "#btnCancelar", function (e) {

		Swal.fire({
			title: '¿ Esta seguro de Cancelar la Lista de Pago ?',
			text: "¡ Esta acción no puede deshacerse !",
			icon: 'warning',
			showCancelButton: true,
			confirmButtonColor: '#3085d6',
			cancelButtonColor: '#d33',
			cancelButtonText: 'No',
			confirmButtonText: 'Si'
		}).then((result) => {
			if (result.isConfirmed) {

				var checks = $(document).find("input[name ='checkCancelCopade']:checked");
				if (checks.length == 0)
					return infoAlert("Debes seleccionar al menos un documento para Cancelar.");

				var listPago = {
					PaymentListID: $("#profileId").val(),
					Status: true,
					Mensaje: $('#txtMensaje').val(),
					LPP: []
				};

				$.map($(document).find("input[name='checkCancelCopade']"), function (x, y) {

					var rolObj = {
						PaymentListID: $(x).data("rol"),
						ListaPago_Id: $(x).data("lista"),
						Status: $("#" + $(x).data("rol") + "").prop('checked')
					};

					listPago.LPP.push(rolObj);
				});

				var fechaInicial = $("#FechaInicial").val();
				var fechaFinal = $("#FechaFinal").val();
				var busqueda = $("#Busqueda").val();
				var pageNum = 1;

				$.ajax({
					url: "Cancelaciones/CancelarListaPago",
					type: "POST",
					data: { dto: listPago },
					success: function (data) {
						if (!isNull(data.success) && !data.success)
							return errorAlert(data.message);
						BuscarCCTable(fechaInicial, fechaFinal, busqueda, pageNum);
						hideLoader();
						return successAlert(data.message);
					}
				});

			}
		})

		e.stopImmediatePropagation();
	});

});

function BuscarCCTable(fechaInicial, fechaFinal, busqueda, pageNum = 1) {
	$.ajax({
		type: "GET",
		url: "Cancelaciones/BuscarCancelacionesListaPago",
		data: { fechaInicial: fechaInicial, fechaFinal: fechaFinal, busqueda: busqueda, pageNum: pageNum },
		success: function (data) {
			if (!isNull(data.success) && !data.success)
				return errorAlert(data.message);
			$("#cancelacionesListaPago-card").empty();
			$("#cancelacionesListaPago-card").append(data);
		}
	});
}















