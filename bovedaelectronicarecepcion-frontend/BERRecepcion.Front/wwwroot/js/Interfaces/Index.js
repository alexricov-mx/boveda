$(function() {
    setOpenMenu("menu_Administración", "AdministrationInterfaces");
    InterfacesTable();
});

//Muestra información de las interfaces de SAP actuales
let InterfacesTable = function () {
    $.ajax({
        url: "Interfaces/InterfacesTable",
        success: function (data) {
            if (!isNull(data.success) && !data.success)
                return errorAlert(data.message);
            $("#interfaces-div").empty();
            $("#interfaces-div").append(data);
        }
    });
} 
//Editar la información de los roles por interfaz
$(document).on("click", "i[name='editar']", function (e) {
    e.preventDefault();
    var sapId = $(this).data("sapid");
    $.ajax({
        url: "Interfaces/ShowModal",
        data: { sapId: sapId },
        success: function (data) {
            if (!isNull(data.success) && !data.success)
                return errorAlert(data.message);
            $("#modal-content").empty();
            $("#modal-content").append(data);
            $("#ModalEdit").modal("show");
        }
    });
    e.stopImmediatePropagation();
});

//Editar 
$(document).on("click", "#btnSave", function (e) {
    e.preventDefault();
    if (isNull($("#Mensaje").val().trim()))
        return infoAlert("Debe especificar una justificación antes de continuar.");
    let sapId = $(this).data("sapid");
    let activar = $(this).data("activar");
    var interface = {
        SAPID: sapId,
        ControlInterfacesDetail: {
            Mensaje: $("#Mensaje").val()
        },
        ControlInterfacesRoles: []
    };

    $.map($(document).find("input[name='checkRol']"), function (x, y) {
        var rolObj = {
            rolid: $(x).data("rol"),
            status: $("#" + $(x).data("rol") + "").prop('checked')
        };
        interface.ControlInterfacesRoles.push(rolObj);
    });
    save(interface, activar);
    e.stopImmediatePropagation();
});

let save = function (interface, activar) {
    let data = { dto: interface };
    var url = activar != null ? "Interfaces/ActualizarTodo" : "Interfaces/Actualizar"
    if (activar != null) 
        data.activar = activar;
    $.ajax({
        type: "POST",
        url: url,
        data: data,
        success: function (data) {
            if (!isNull(data.success) && !data.success)
                return errorAlert(data.message);
            InterfacesTable();
            $("#ModalEdit").modal("hide");
            return successAlert(data.message);
        }
    })
}

$(document).on("click", "i[name='activar']", function (e) {
    e.preventDefault();
    var sapId = $(this).data("sapid");
    $.ajax({
        url: "Interfaces/ShowModal",
        data: { sapId: sapId, activar: true },
        success: function (data) {
            if (!isNull(data.success) && !data.success)
                return errorAlert(data.message);
            $("#modal-content").empty();
            $("#modal-content").append(data);
            $("#ModalEdit").modal("show");
        }
    });
    e.stopImmediatePropagation();
});

$(document).on("click", "i[name='bloquear']", function (e) {
    e.preventDefault();
    var sapId = $(this).data("sapid");
    $.ajax({
        url: "Interfaces/ShowModal",
        data: { sapId: sapId, activar: false },
        success: function (data) {
            if (!isNull(data.success) && !data.success)
                return errorAlert(data.message);
            $("#modal-content").empty();
            $("#modal-content").append(data);
            $("#ModalEdit").modal("show");
        }
    });
    e.stopImmediatePropagation();
});
