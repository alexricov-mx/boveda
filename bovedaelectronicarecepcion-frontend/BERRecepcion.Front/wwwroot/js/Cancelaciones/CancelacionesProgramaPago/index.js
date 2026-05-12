setOpenMenu("menu_Instrucciones", "CancelPaymentSchedule");

$(document).ready(function () {

	$(document).on("click", ".page-link", function (e) {
		e.preventDefault();
		var fechaInicial = $("#FechaInicial").val();
		var fechaFinal = $("#FechaFinal").val();
		var busqueda = $("#Busqueda").val();
		var pageNum = $(this).data("page_num");
		BuscarCCTable(fechaInicial, fechaFinal, busqueda, pageNum)
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
			title: '¿ Esta seguro de Cancelar el Programa de Pagos ?',
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
					return infoAlert("Debes seleccionar al menos un documento para firmar.");

				var copades = {
					rolid: $("#profileId").val(),
					Status: true,
					Mensaje: $('#txtMensaje').val(),
					CPP: []
				};

				$.map($(document).find("input[name='checkCancelCopade']"), function (x, y) {

					var rolObj = {
						PaymentScheduleID: $(x).data("rol"),
						ProgramaPago_Id: $(x).data("programa"),
						Status: $("#" + $(x).data("rol") + "").prop('checked')
					};

					copades.CPP.push(rolObj);
				});

				var fechaInicial = $("#FechaInicial").val();
				var fechaFinal = $("#FechaFinal").val();
				var busqueda = $("#Busqueda").val();
				var pageNum = 1;
				
				$.ajax({
					url: "Cancelaciones/CancelarProgramaPago",
					type: "POST",
					data: { dto: copades },
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
		url: "Cancelaciones/BuscarCancelacionesProgramaPago",
		data: { fechaInicial: fechaInicial, fechaFinal: fechaFinal, busqueda: busqueda, pageNum: pageNum },
		success: function (data) {
			if (!isNull(data.success) && !data.success)
				return errorAlert(data.message);
			$("#cancelacionesProgramaPago-card").empty();
			$("#cancelacionesProgramaPago-card").append(data);
		}
	});
}















