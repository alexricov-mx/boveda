$(document).ready(function () {	   

	setOpenMenu("menu_Administración", "AdministrationValidations")

	$("#btnRP").on("click", function (e) {
		e.preventDefault();
		var uri = $('#urlBuscarRP').val();
		showLoader();
		var Data = {
			SAPOrder: $('#txtSapOrder').val(),
		};
		apiGet("ReactivaProcesos/GetReactivarProcesosBySAPOrder", Data, onSuccess = (data) => {
			hideLoader();
			if(!data) {
				console.log("Regreso 404");
				$("#detalle").html($('<div class="callout callout-info"><p>No se han encontrado registros.</p></div>'));
				return
			}
			// if (!isNull(data.success) && !data.success)
			// 	return errorAlert(data.message);
			
			$("#detalle").html(data);
			$("#detalle2").html(data);
		}, onErrror = (error) => {
			console.log(error);
			$("#ordenSurtimiento-card").empty();
			errorAlert(error.message);
		});
		e.stopImmediatePropagation();
	});   
	   
});

function InsertarRP(SAPOrderRP, organismo, TypeDocument, Type, clave, Contract,valida)
{

	var uri = $('#urlFirmarRP').val();
	showLoader();
	var Data = {
		SAPOrderRP: SAPOrderRP,
		Name: organismo,
		Contract: Contract,
		TypeDocument: TypeDocument,
		Type: Type,
		Clave: clave,
		valida: valida
		
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

				hideLoader();
				var uri = $('#urlBuscarRP').val();
				showLoader();
				var Data = {
					SAPOrder: SAPOrderRP,
				};

				$.ajax({
					type: "post",
					datatype: 'json',
					url: uri,
					data: Data,
					success: function (response) {
						hideLoader();
						$("#detalle").html(response);
						$("#detalle2").html(response);
						successAlert("reenvio ejecutado exitosamente");
					}
				});
				

			} else {
				hideLoader();
				
				errorAlert('Ocurrió un problema con el reenvio de la informacion, intentalo mas tarde');
			}

		}
	});

}