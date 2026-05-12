
$(document).ready(function () {
    setOpenMenu("menu_Administración", "ContractsRegister")
    AltaContratosTable();
});
//Buscar
$(document).on("submit", "#ac-form-search", function (e) {
    e.preventDefault();
    var search = $("#search").val();
    AltaContratosTable(1, search);
    e.stopImmediatePropagation();
});
//Tabla para carga inicial
let AltaContratosTable = function (pageNum = 1, search = "") {
    $.ajax({
        url: "AltaContratos/AltaContratosTable",
        data: { pageNum: pageNum, search: search },
    }).then(function (data) {
        if (!isNull(data.success) && !data.success)
            return errorAlert(data.message);
        $("#div-ac").empty();
        $("#div-ac").append(data);
    });
}
//Muestra modal para nuevo 
$("#btnAgregar").on("click", function (e) {
    e.preventDefault();
    $.ajax({
        url: "AltaContratos/ShowModal",
        data: { isEdit: false },
        success: function (data) {
            if (!isNull(data.success) && !data.success)
                return errorAlert(data.message);
            $("#modal-content").empty();
            $("#modal-content").append(data);
            $("#ModalAddEdit").modal("show");
        }
    })
    e.stopImmediatePropagation();
});

// Edicion de Contratos
$(document).on("click", "button[name='edit']", function (e) {
    e.preventDefault();
    var AltaContratoID = $(this).data("altacontratoid");
    $.ajax({
        url: "AltaContratos/ShowModal",
        data: { AltaContratoID: AltaContratoID, isEdit : true },
        success: function (data) {
            $("#modal-content").empty();
            $("#modal-content").append(data);
            $("#ModalAddEdit").modal("show");
        }
    });
    e.stopImmediatePropagation();
});
//Eliminar
$(document).on("click", "button[name='delete']", function (e) {
    let AltaContratoID = $(this).data('altacontratoid');
    e.preventDefault();
    Swal.fire({
        title: "Eliminar Contrato",
        text: "¿Desea eliminar el contrato seleccionado?",
        icon: 'question',
        showCancelButton: true,
        cancelButtonText: 'Cancelar',
        confirmButtonText: "Eliminar",
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                type: "POST",
                url: "AltaContratos/Eliminar",
                data: { AltaContratoID: AltaContratoID },
                success: function (data) {
                    if (!isNull(data.success) && !data.success)
                        return errorAlert(data.message);
                    AltaContratosTable();
                    return successAlert(data.message);
                }
            });
        }
    })
    e.stopImmediatePropagation();
});

//Guardar o editar 
$(document).on("click", "#btnSave", function (e) {
    e.preventDefault();
    let altaContratoId = $(this).data("altacontratoid");
    const altaContrato = {
        AltaContratoID: altaContratoId,
        Contract: $("#Contract").val(),
        Firma1: $("#Firma1").val(),
        Suplente1: $("#Suplente1").val(),
        Firma2: $("#Firma2").val(),
        Suplente2: $("#Suplente2").val()
    };
    let isEdit = !isNull(altaContratoId);
    if (!isEdit)
        delete altaContrato.AltaContratoID;
    save(altaContrato, isEdit);
    e.stopImmediatePropagation();
});

let save = function (altaContrato, isEdit) {
    $.ajax({
        type: "POST",
        url: isEdit ? "AltaContratos/Editar" : "AltaContratos/Agregar",
        data: { dto: altaContrato },
    }).then(function (data) {
        if (!isNull(data.success) && !data.success)
            return errorAlert(data.message);
        AltaContratosTable();
        $("#ModalAddEdit").modal("hide");
        return successAlert(data.message);
    });
}

$(document).on("click", "#ac-edit", function (e) {
    e.preventDefault();
    var _Contrato = $("#Contract").val();

    var _altaContratoEditado = {
        AltaContratoID: $("#AltaContratoID").val(),
        Contract: $("#Contract").val(),
        Firma1: $("#txtF1Edit").val(),
        Suplente1: $("#txtS1Edit").val(),
        Firma2: $("#txtF2Edit").val(),
        Suplente2: $("#txtS2Edit").val()

    };

    $.ajax({
        url: "AltaContratos/EditarAC",
        type: "POST",
        data: { dto: _altaContratoEditado },
        success: function (data) {
            if (!isNull(data.success) && !data.success)
                return errorAlert(data.message);
            GetACTable();
            return successAlert(data.message);
        }
    });
    e.stopImmediatePropagation();
});


//Guardar Nuevo
$(document).on("click", "#ac-save", function (e) {
    e.preventDefault();
    var ac = {
        Contract: $("#txtACNew").val(),
        DocumentType: $("#selectTipo").val(),
        Firma1: $("#txtF1New").val(),
        Suplente1: $("#txtS1New").val(),
        Firma2: $("#txtF2New").val(),
        Suplente2: $("#txtS2New").val()

    };
    $.ajax({
        url: "AltaContratos/NuevoAC",
        type: "POST",
        data: { dto: ac },
        success: function (data) {
            if (!isNull(data.success) && !data.success)
                return errorAlert(data.message);
            else if (data.message == "Registro de Contrato ya existe.") {
                GetACTable();
                return infoAlert(data.message);
            }
            GetACTable();
            return successAlert(data.message);


        }
    });
    e.stopImmediatePropagation();
});




$(document).on("click", "#cancelar", function (e) {
    e.preventDefault();
    GetACTable();
    e.stopImmediatePropagation();
});


function Busqueda_Usuario(token, indice) {
    $.ajax({
        type: "GET",
        url: "CentrosGestores/BusquedaCGUsuario",
        data: { ficha: token },
        beforeSend: function () {
            //showLoader();
        },
        success: function (data) {
            if (!isNull(data.success) && !data.success)
                return errorAlert(data.message);
            if (indice == 1) {
                $("#Firmante1Busqueda").html(data.nombre).show();
            } else if (indice == 2) {
                $("#Suplente1Busqueda").html(data.nombre).show();
            } else if (indice == 3) {
                $("#Firmante2Busqueda").html(data.nombre).show();
            } else if (indice == 4) {
                $("#Suplente2Busqueda").html(data.nombre).show();
            }
            //return successAlert(data.message);
        },
        complete: function () {
            hideLoader();
        }
    });
}