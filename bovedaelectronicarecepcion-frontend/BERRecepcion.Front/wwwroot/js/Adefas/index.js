$(document).ready(function () {
    setOpenMenu("menu_Administración", "AdministrationAdefa")
    ConsultaAdefas();    
});

$(document).on("click", ".page-link", function (e) {
    e.preventDefault();
    var pageNum = $(this).data("page_num");
    var search = $("#search").val();
    ConsultaAdefas(pageNum, search);
    e.stopImmediatePropagation();
});

var ConsultaAdefas = function (pageNum = 1, search = "") {
    $.ajax({
        url: "Adefas/ConsultaAdefas",
        data: { pageNum: pageNum, search: search },
        success: function (data) {
            if (!isNull(data.success) && !data.success)
                return errorAlert(data.message);
            $("#div-adefas").empty();
            $("#div-adefas").append(data);
        }
    });
}

//Guardar o editar adefa
$(document).on("click", "#btnSave", function (e) {
    e.preventDefault();
    let adefaId = $(this).data("adefaid");
    let adefa = {
        AdefaID: adefaId,
        AnhioFactura: $("#AnhioFactura").val(),
        OrganismID: $("#OrganismID").val(),
        InicioVentana: $("#InicioVentana").val(),
        FinVentana: $("#FinVentana").val()
    };
    if (isNull(adefa.AnhioFactura))
        return infoAlert("Debe seleccionar el año de la factura.");
    if (isNull(adefa.OrganismID))
        return infoAlert("Debe seleccionar organismo.");
    if (isNull(adefa.InicioVentana))
        return infoAlert("Debe seleccionar el inicio de ventana.");
    if (isNull(adefa.FinVentana))
        return infoAlert("Debe seleccionar el fin de ventana.");
    if (adefa.InicioVentana > adefa.FinVentana)
        return infoAlert("El inicio de ventana no debe ser mayor al fin de ventana");
    saveAdefa(adefa, !isNull(adefaId));
    e.stopImmediatePropagation();
});

let saveAdefa = function (adefa, isEdit) {
    $.ajax({
        type: "POST",
        url: isEdit ? "Adefas/Editar" : "Adefas/Agregar",
        data: { adefa },
        success: function (data) {
            if (!isNull(data.success) && !data.success)
                return errorAlert(data.message);
            ConsultaAdefas();
            $("#ModalAddEdit").modal("hide");
            return successAlert(data.message);
        }
    })
}

//Muestra modal para agregar adefa
$("#btnAgregar").on("click", function (e) {
    e.preventDefault();
    $.ajax({
        url: "Adefas/ShowModal",
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
//Buscar adefas
$(document).on("submit", "#adefas-form-search", function (e) {
    e.preventDefault();
    var search = $("#search").val();
    ConsultaAdefas(1, search);
    e.stopImmediatePropagation();
});

//Mostrar modal para edicion de adefa
$(document).on("click", "button[name='edit']", function (e) {
    e.preventDefault();
    let adefaID = $(this).data('adefaid');
    $.ajax({
        url: "Adefas/ShowModal",
        data: { adefaID: adefaID, isEdit: true },
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
//Eliminar adefa
$(document).on("click", "button[name='delete']", function (e) {
    let AdefaID = $(this).data('adefaid');
    e.preventDefault();
    Swal.fire({
        title: "Eliminar Adefa",
        text: "¿Desea eliminar el periodo de Adefa seleccionado?",
        icon: 'question',
        showCancelButton: true,
        cancelButtonText: 'Cancelar',
        confirmButtonText: "Eliminar",
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                type: "POST",
                url: "Adefas/Eliminar",
                data: { AdefaID: AdefaID },
                success: function (data) {
                    if (!isNull(data.success) && !data.success)
                        return errorAlert(data.message);
                    ConsultaAdefas();
                    return successAlert(data.message);
                }
            });
        }
    })
    e.stopImmediatePropagation();
});
//Descarga excel
$("#Excel").on("click", function (e) {
    e.preventDefault();
    var search = $("#search").val();
    location.href = "Adefas/DownloadExcel?search=" + search;
    e.stopImmediatePropagation();
})