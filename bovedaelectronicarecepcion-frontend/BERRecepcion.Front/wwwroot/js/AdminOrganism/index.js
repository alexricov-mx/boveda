$(document).ready(function () {

	setOpenMenu("menu_Administración", "AdministrationAdefa")

	$(document).on("click", ".page-link", function (e) {
		e.preventDefault();
		var pageNum = $(this).data("page_num");
		obtenerReceptionAlmacenTable(pageNum);
		e.stopImmediatePropagation();
	});

	$("#btnAgregar").on("click", function (e) {
		e.preventDefault();
		$('#myModalAgre').modal('show');
		e.stopImmediatePropagation();
	});

	$("#fec_iniAgre").change(function () {
		var x = document.getElementById("fec_finAgre").min = $("#fec_iniAgre").val();
	});

	$("#btnAgregarAdef").click(function () {

		var uri = $('#urlEditar').val();
		showLoader();
		var Data = {
			Name: $('#ddOrganismo').val(),
			AnhioFactura: $("#ddFecha option:selected").text(),
			InicioVentana: $('#fec_iniAgre').val(),
			FinVentana: $('#fec_finAgre').val()
		};

		$.ajax({
			type: "post",
			datatype: 'json',
			url: uri,
			data: Data,
			success: function (response) {
				if (response.success) {
					hideLoader();
					$('#myModalAgre').modal('hide');

					Swal.fire({
						title: 'Organismo Actualizado',
						text: response.responseText,
						icon: 'success',
						showCancelButton: false,
						confirmButtonClass: "btn-success",
						confirmButtonText: "Ok",
					}).then((result) => {
						if (result.isConfirmed) {
							location.reload();
						}
					})


				} else {
					hideLoader();
					swal.fire({
						title: "Error",
						text: response.responseText,
						type: "Error",
						showCancelButton: false,
						confirmButtonClass: "btn-success",
						confirmButtonText: "Ok",
						closeOnConfirm: false
					},
						function () {
							location.reload();
						});

				}
			}
		});

	});

	$("#btnEditar").click(function () {

		var uri = $('#urlEditar').val();

		var Data = {
			OrganismID: $('#OrganismID').val(),
			Name: $('#txtName').val(),
			Clave: $("#txtClave").val(),
			Address: $('#txtAddress').val(),
			Rfc: $('#txtRfc').val()
		};

		$.ajax({
			type: "post",
			datatype: 'json',
			url: uri,
			data: Data,
			success: function (response) {
				if (response.success) {

					$('#myModal').modal('hide');
					$("#myModal").hide();


					Swal.fire({
						title: 'Organismo Actualizado',
						text: response.responseText,
						icon: 'success',
						showCancelButton: false,
						confirmButtonText: 'Ok'
					}).then((result) => {
						if (result.isConfirmed) {
							location.reload();
						}
					})




				} else {

					Swal.fire({
						title: "Error",
						text: response.responseText,
						type: "Error",
						showCancelButton: false,
						confirmButtonClass: "btn-success",
						confirmButtonText: "Ok",
						closeOnConfirm: false
					},
						function () {
							location.reload();
						})

				}
			}
		});

	});

	$("#fec_ini").change(function () {
		var x = document.getElementById("fec_fin").min = $("#fec_ini").val();
	});

});

function EditarAdefas(OrganismID, Name, Clave, address ,Rfc) {

	$("#OrganismID").val(OrganismID);
	$("#txtName").val(Name);
	$("#txtClave").val(Clave);
	$('#txtAddress').val(address);
	$("#txtRfc").val(Rfc);



	$('#myModal').modal('show');

}

function EliminarAdefas(idAdefa) {

	Swal.fire({
		title: "Adefa",
		text: "¿ Eliminar Adefa ?",
		icon: 'warning',
		showCancelButton: true,
		cancelButtonText: "No, cancelar",
		confirmButtonText: "Si, Eliminar",
	}).then((result) => {
		if (result.isConfirmed) {
			var uri = $('#urlEliminar').val();
			var datos = {
				AdefaID: idAdefa
			};

			$.ajax({
				type: "post",
				datatype: 'json',
				url: uri,
				data: datos,
				success: function (response) {
					if (response.success) {

						setTimeout(function () {


							Swal.fire({
								title: 'Adefa',
								text: response.responseText,
								icon: 'success',
								showCancelButton: false,
								confirmButtonText: 'Ok'
							}).then((result) => {
								if (result.isConfirmed) {
									location.reload();
								}
							})



						}, 2000);


					} else {

						Swal.fire({
							title: "Error",
							text: response.responseText,
							type: "Error",
							showCancelButton: false,
							confirmButtonClass: "btn-danger",
							confirmButtonText: "Ok",
							closeOnConfirm: false
						},
							function () {
								location.reload();
							});

					}
				}
			});
		}
	})


}