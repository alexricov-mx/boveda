var userTokenExists = null;

$(document).ready(function () {
    setOpenMenu("menu_Administración", "DeviationSigns");
    //Buscar información del contrato
    $("#desvio-form-search").on("submit", function (e) {
        e.preventDefault();
        var _contract = $("#txtContrato").val();
        if(isNull(_contract))
            return infoAlert("Verifica el contrato.");
        if (_contract.length != 10)
            return infoAlert("Verifica el contrato, debe ser de 10 dígitos.");
            $.ajax({
                url: "Desvio/Documentos",
                type: "GET",
                data: { contrato: _contract },
                success: function (data) {
                    if (!isNull(data.success) && !data.success)
                        return infoAlert(data.message);
                    $("#desvio-div").empty();
                    $("#desvio-div").html(data);
                }
            });
        e.stopImmediatePropagation();
    });
    //Buscar información del nuevo firmante
    $(document).on("submit", "#nuevo-firmante-form-search", function (e) {
        e.preventDefault();
        var _ficha = $("#txtficha").val();
        if (isNull(_ficha))
            return infoAlert("Verifica la ficha.");
        if (_ficha.length != 6)
            return infoAlert("Verifica la ficha, debe ser de 6 dígitos.");
        $.ajax({
            url: "Desvio/GetFirmante",
            type: "GET",
            data: { Token: _ficha },
            success: function (data) {
                if (!isNull(data.success) && !data.success) {
                    userTokenExists = false;
                    $("#card-nuevo-firmante").addClass("d-none");
                    return infoAlert(data.message);
                }
                userTokenExists = true;
                $("#card-nuevo-firmante").removeClass("d-none");
                $("#signerName").text(data.data.nombre);
            }
        });
        e.stopImmediatePropagation();
    });
    $(document).on("click", "#btnDesviarFirma", function (e) {
        e.preventDefault();
        var objDesvio = {
            contrato: "",
            ordenSurtimiento: [],
            estimacionObra: [],
            recepcion: []
        };
        var OSItems = $("#supply-order-table").find("input[name='OSCheck']:checked");
        var ESItems = $("#estimacion-table").find("input[name='ESCheck']:checked");
        var REItems = $("#recepcion-table").find("input[name='RECheck']:checked");
        var _signerNew = $("#txtficha").val();
        var _motivo = $("#Motivo").val();

        if (OSItems.length == 0 & ESItems.length == 0 && REItems.length == 0)
            return infoAlert("Debe seleccionar por lo menos un documento para desvío.")
        if (isNaN(_signerNew))
            return infoAlert("La ficha debe ser numérica.");
        if (isNull(_signerNew))
            return infoAlert("Debe especificar el nuevo firmante.");
        if (userTokenExists == null)
            return infoAlert("Validar la ficha del nuevo firmante.")
        if (!userTokenExists)
            return infoAlert("La ficha del firmante ingresada no existe, favor de validar.");
        if (isNull(_motivo))
            return infoAlert("Debe especificar una justificación para el desvío de firma.");

        objDesvio.ordenSurtimiento = getSelectedOS(OSItems, _signerNew, _motivo);
        objDesvio.estimacionObra = getSelectedES(ESItems, _signerNew, _motivo);
        objDesvio.recepcion = getSelectedRE(REItems, _signerNew, _motivo);

        $.ajax({
            type: "post",
            url: "Desvio/Create",
            data: { desvio: objDesvio},
            success: function (data) {
                if (!data.success)
                    return errorAlert(data.message);
                $("#desvio-form-search").submit();
                userTokenExists = null;
                successAlert(data.message);
            }
        });
        e.stopImmediatePropagation();
    });
});

function getSelectedOS(supplyOrders, signerNew, Motivo) {
    var ordenSurtimiento = [];
    $.each(supplyOrders, function (index, item) {
        var itemOs = {
            contract: $(item).data("contract"),
            sapOrder: $(item).data("saporder"),
            documentType: 'OS',
            signer: $(item).data("ficha"),
            signerNew: signerNew,
            justificacion: Motivo
        };
        ordenSurtimiento.push(itemOs);
    });
    return ordenSurtimiento;
}
function getSelectedES(estimaciones, signerNew, Motivo) {
    var estimacion = [];
    $.each(estimaciones, function (index, item) {
        var itemEs = {
            contract: $(item).data("contract"),
            sapOrder: $(item).data("saporder"),
            documentType: 'ES',
            signer: $(item).data("ficha"),
            signerNew: signerNew,
            justificacion: Motivo
        };
        estimacion.push(itemEs);
    });
    return estimacion;
}
function getSelectedRE(recepciones, signerNew, Motivo) {
    var recepcion = [];
    $.each(recepciones, function (index, item) {
        var itemRE = {
            contract: $(item).data("contract"),
            sapOrder: $(item).data("saporder"),
            reception: $(item).data("reception"),
            documentType: 'RE',
            signer: $(item).data("ficha"),
            signerNew: signerNew,
            justificacion: Motivo
        };
        recepcion.push(itemRE);
    });
    return recepcion;
}