$(document).ready(function () {
    ProfilesTable();
    setOpenMenu("menu_Administración", "AdministrationProfiles");

    //Abre pantalla para crear un nuevo perfil
    $(document).on("click", "#profile-new", function (e) {
        e.preventDefault();

        $.ajax({
            type: "GET",
            url: "Perfiles/CardNew",
            success: function (data) {
                if (!isNull(data.success) && !data.success)
                    return errorAlert(data.message);
                $("#profiles-card").empty();
                $("#profiles-card").append(data);
            }
        });
        e.stopImmediatePropagation();
    });

    //Guarda un nuevo perfil
    $(document).on("click", "#profile-save", function (e) {
        e.preventDefault();
        var catergorias = $("a[name='checkCategoria']");
        var profile = {
            Name: $("#profileName").val().trim(),
            status: true,
            ProfilesRoles: []
        };
        $.each(catergorias, function (index, item) {
            var categoria = $(item).prop("id");
            $.map($(document).find("input:checkbox[name='" + categoria + "']:checked"), function (x, y) {
                var rolObj = {
                    RolId: $(x).data("rol")
                };
                profile.ProfilesRoles.push(rolObj);
            });
        });
        $.ajax({
            url: "Perfiles/InsertaPerfil",
            type: "POST",
            data: { dto: profile },
            success: function (data) {
                if (!isNull(data.success) && !data.success)
                    return errorAlert(data.message);
                ProfilesTable();
                return successAlert(data.message);
            }
        });
        e.stopImmediatePropagation();
    });

    //Carga la pantalla para editar un perfil
    $(document).on("click", "a[name='profile-edit']", function (e) {
        e.preventDefault();
        var _profileID = $(this).data("profile");
        $.ajax({
            type: "GET",
            url: "Perfiles/CardEdit",
            data: { ProfileID: _profileID },
            success: function (data) {
                if (!isNull(data.success) && !data.success)
                    return errorAlert(data.message);
                $("#profiles-card").empty();
                $("#profiles-card").append(data);
            }
        });
        e.stopImmediatePropagation();
    });


    //Guarda la edición de un perfil
    $(document).on("click", "#profile-save-edit", function (e) {
        e.preventDefault();

        var catergorias = $("a[name='checkCategoria_edit']");
        var profile = {
            ProfileID: $("#profileName").data("profile"),
            Name: $("#profileName").val().trim(),
            status: true,
            ProfilesRoles: []
        };
        $.each(catergorias, function (index, item) {
            var categoria = $(item).prop("id");
            $.map($(document).find("input:checkbox[name='" + categoria + "']:checked"), function (x, y) {
                var rolObj = {
                    RolId: $(x).data("rol")
                };
                profile.ProfilesRoles.push(rolObj);
            });
        });
        $.ajax({
            url: "Perfiles/ActualizaPerfil",
            type: "POST",
            data: { dto: profile },
            success: function (data) {
                if (!isNull(data.success) && !data.success)
                    return errorAlert(data.message);
                ProfilesTable();
                return successAlert(data.message);
            }
        });
        e.stopImmediatePropagation();
    });

    //Cancela la acción de nuevo o editar perfil y carga la vista inicial
    $(document).on("click", "#profile-cancel", function (e) {
        e.preventDefault();
        confirmAlert('Perfiles', 'Los cambios se perderán ¿cancelar de todos modos?', ProfilesTable);
        e.stopImmediatePropagation();
    });

    //Eliminar perfil
    $(document).on("click", "a[name='profile-delete']", function (e) {
        e.preventDefault();
        var _profileID = $(this).data("profile");

        confirmAlert('Perfiles', '¿Desea eliminar el perfil?', function () {
            deleteProfile(_profileID);
        });
        e.stopImmediatePropagation();
    });

    $(document).on("submit", "#profiles-form-search", function (e) {
        e.preventDefault();
        var search = $("#search").val();
        ProfilesTable(1, search);
        e.stopImmediatePropagation();
    });
});

function deleteProfile(ProfileID) {
    $.ajax({
        url: "Perfiles/Delete",
        type: "GET",
        data: { ProfileID: ProfileID },
        success: function (data) {
            if (!isNull(data.success) && !data.success)
                return errorAlert(data.message);
            ProfilesTable();
            return successAlert(data.message);
        }
    });
}

function ProfilesTable(pageNum = 1, search = "") {
    $.ajax({
        type: "GET",
        url: "Perfiles/ProfilesTable",
        data: { pageNum: pageNum, search: search },
        success: function (data) {
            if (!isNull(data.success) && !data.success)
                return errorAlert(data.message);
            $("#profiles-card").empty();
            $("#profiles-card").append(data);
        }
    });
}

function successProfileDeleted(data) {
    if (data.success) {
        ProfilesTable();        
        return successAlert(data.message);
    } else
        return errorAlert(data.message);
}

$(document).on("click", "input[name='checkCategoria']", function (e) {
    var categoria = $(this).prop("id");
    $(document).find("input:checkbox[name='" + categoria + "']").prop("checked", $(this).prop("checked"));
    e.stopImmediatePropagation();
});

$(document).on("click", "input[name='checkCategoria_edit']", function (e) {
    var categoria = $(this).prop("id");
    $(document).find("input:checkbox[name='" + categoria + "']").prop("checked", $(this).prop("checked"));
    e.stopImmediatePropagation();
});

$(document).on("click", "input.checkRol", function (e) {
    var categoria = $(this).prop("name");    
    var cantidad = $(document).find("input:checkbox[name='" + categoria + "']").length;
    var checked = $(document).find("input:checkbox[name='" + categoria + "']:checked").length;
    $(document).find("input:checkbox[id='" + categoria + "']").prop("checked", cantidad == checked);    
    e.stopImmediatePropagation();
});

$(document).on("click", "input.checkRol_edit", function (e) {
    var categoria = $(this).prop("name");    
    var cantidad = $(document).find("input:checkbox[name='" + categoria + "']").length;
    var checked = $(document).find("input:checkbox[name='" + categoria + "']:checked").length;
    $(document).find("input:checkbox[id='" + categoria + "']").prop("checked", cantidad == checked);    
    e.stopImmediatePropagation();
});

//Descarga excel
$(document).on("click", "#Excel", function (e) {
    e.preventDefault();
    var search = $("#search").val();
    location.href = "Perfiles/DownloadExcel?search=" + search;
    e.stopImmediatePropagation();
})