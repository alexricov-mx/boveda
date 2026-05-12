$(function () {
    setOpenMenu("menu_Administración", "AdministrationManagementCenters")
    CentrosGestoresTable();
});
//Paginacion de centro gestor
$(document).on("click", ".page-link", function (e) {
    e.preventDefault();
    var pageNum = $(this).data("page_num");
    var search = $("#search").val();
    CentrosGestoresTable(pageNum, search);
    e.stopImmediatePropagation();
});
//Buscar centros gestores
$(document).on("submit", "#cg-form-search", function (e) {
    e.preventDefault();
    var search = $("#search").val();
    CentrosGestoresTable(1, search);
    e.stopImmediatePropagation();
});
let CentrosGestoresTable = function (pageNum = 1, search = "") {
    $.ajax({
        url: "CentrosGestores/CentrosGestoresTable",
        data: { pageNum: pageNum, search: search },
    }).then(function (data) {
        if (!isNull(data.success) && !data.success)
            return errorAlert(data.message);
        $("#div-cg").empty();
        $("#div-cg").append(data);
    });
}
//Muestra modal para agregar centro gestor
$("#btnAgregar").on("click", function (e) {
    e.preventDefault();
    $.ajax({
        url: "CentrosGestores/ShowModal",
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

//Mostrar modal para edicion de Centro Gestor
$(document).on("click", "button[name='edit']", function (e) {
    e.preventDefault();
    let ManagementCenterID = $(this).data('managementcenterid');
    $.ajax({
        url: "CentrosGestores/ShowModal",
        data: { ManagementCenterID: ManagementCenterID, isEdit: true },
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

//Eliminar Centro Gestor
$(document).on("click", "button[name='delete']", function (e) {
    let ManagementCenterID = $(this).data('managementcenterid');
    e.preventDefault();
    Swal.fire({
        title: "Eliminar Centro Gestor",
        text: "¿Desea eliminar el centro gestor seleccionado?",
        icon: 'question',
        showCancelButton: true,
        cancelButtonText: 'Cancelar',
        confirmButtonText: "Eliminar",
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                type: "POST",
                url: "CentrosGestores/Eliminar",
                data: { ManagementCenterID: ManagementCenterID },
                success: function (data) {
                    if (!isNull(data.success) && !data.success)
                        return errorAlert(data.message);
                    CentrosGestoresTable();
                    return successAlert(data.message);
                }
            });
        }
    })
    e.stopImmediatePropagation();
});

//Guardar o editar centro gestor
$(document).on("click", "#btnSave", function (e) {
    e.preventDefault();
    let managementcenterid = $(this).data("managementcenterid");
    var centroGestor = {
        ManagementCenterID: managementcenterid,
        OrganismID: $("#OrganismID").val(),
        Number: $("#Number").val(),
        Type: $("#Type").val(),
        Signer1: $("#Signer1").val(),
        Alternate1: $("#Alternate1").val(),
        Signer2: $("#Signer2").val(),
        Alternate2: $("#Alternate2").val(),
        Description: $("#Description").val()
    };
    let isEdit = !isNull(managementcenterid);
    if (!isEdit)
        delete centroGestor.ManagementCenterID;
    saveCentroGestor(centroGestor, isEdit);
    e.stopImmediatePropagation();
});

let saveCentroGestor = function (centroGestor, isEdit) {
    $.ajax({
        type: "POST",
        url: isEdit ? "CentrosGestores/Editar" : "CentrosGestores/Agregar",
        data: { dto: centroGestor },
    }).then(function (data) {
        if (!isNull(data.success) && !data.success)
            return errorAlert(data.message);
        CentrosGestoresTable();
        $("#ModalAddEdit").modal("hide");
        return successAlert(data.message);
    });
}

//Descarga excel
$("#ExcelDown").on("click", function (e) {
    e.preventDefault();
    var search = $("#search").val();
    location.href = "CentrosGestores/DownloadExcel?search=" + search;
    e.stopImmediatePropagation();
});
