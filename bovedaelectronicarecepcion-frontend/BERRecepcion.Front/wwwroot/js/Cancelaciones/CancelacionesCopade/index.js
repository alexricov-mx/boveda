
setOpenMenu("menu_COPADEAnalitico", "CancelCopade");

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
		BuscarCCTable(fechaInicial, fechaFinal, busqueda, pageNum = 1);
		e.stopImmediatePropagation();
	});


	$(document).on("click", "#btnCancelar", function (e) {

		Swal.fire({
			title: '¿ Esta seguro de Cancelar el Copade ?',
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
					return infoAlert("Debes seleccionar al menos un documento para cancelar.");

				var copades = {
					rolid: $("#profileId").val(),
					Status: true,
					Mensaje: $('#txtMensaje').val(),
					listCopades: []
				};

				var qty = 0;
				$.map($(document).find("input[name='checkCancelCopade']"), function (x, y) {

					var rolObj = {
						CopadeID: $(x).data("rol"),
						Reception: $(x).data("reception"),
						Clave: $(x).data("clave"),
						Status: $("#" + $(x).data("rol") + "").prop('checked')
					};

					copades.listCopades.push(rolObj);
					qty++;
				});

				if (qty == 0)
					return infoAlert("Debes seleccionar al menos un documento para cancelar.");

				$.ajax({
					url: "Cancelaciones/Cancelar",
					type: "POST",
					data: { dto: copades },
					success: function (data) {
						if (!isNull(data.success) && !data.success)
							return errorAlert(data.message);
						var fechaInicial = $("#FechaInicial").val();
						var fechaFinal = $("#FechaFinal").val();
						var busqueda = $("#Busqueda").val();
						BuscarCCTable(fechaInicial, fechaFinal, busqueda, pageNum = 1)
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
		url: "Cancelaciones/BuscarCancelacionesCopade",
		data: { fechaInicial: fechaInicial, fechaFinal: fechaFinal, busqueda: busqueda, pageNum: pageNum },
		success: function (data) {
			if (!isNull(data.success) && !data.success)
				return errorAlert(data.message);
			$("#cancelacionesCopade-card").empty();
			$("#cancelacionesCopade-card").append(data);
		}
	});
}















