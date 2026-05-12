function ajustePantalla() {
    var x = $(document).width();
    var y = $(document).height();
    $("#topBar").removeClass("col-md-4");
    $("#topBar").removeClass("col-md-12");
    $("#topBar").removeClass("col-sm-4");
    $("#topBar").removeClass("col-lg-4");
    if (x < 1500) {
        $(".cardDataDisplay").removeClass("col-lg-6");
        $("#userPemex").removeClass("fixedTop120");
        $("#userProveedor").removeClass("fixedTop120");
        $("#userPemex").removeClass("col-lg-4");
        $("#userProveedor").removeClass("col-lg-4");
        if (x < 1300) {
            $("#topBar").removeClass("fixedTop");
            $("#cardBase").removeClass("topBase");
            $("#userPemex").height("auto");
            $("#userProveedor").height("auto");
            $("#topBar").addClass("col-md-12");
        }
        else {
            $("#userPemex").addClass("fixedTop120");
            $("#userProveedor").addClass("fixedTop120");
            $("#userPemex").addClass("col-lg-4");
            $("#userProveedor").addClass("col-lg-4");
            $("#topBar").addClass("fixedTop");
            $("#topBar").addClass("col-sm-4");
            var h = 400;
            $("#userPemex").height(h);
            $("#userProveedor"toke).height(h);
            $("#topBar").addClass("col-md-12");
            $("#topBar").addClass("col-lg-4");
        }

    }
    else {
        $(".cardDataDisplay").addClass("col-lg-6");
        $("#userPemex").addClass("fixedTop120");
        $("#userProveedor").addClass("fixedTop120");
        $("#topBar").addClass("fixedTop");
        $("#cardBase").addClass("topBase");
        var h = 500;
        $("#userPemex").height(h);
        $("#userProveedor").height(h);
        $("#userPemex").addClass("col-lg-4");
        $("#userProveedor").addClass("col-lg-4");
        $("#topBar").addClass("col-md-4");
        $("#topBar").addClass("col-sm-4");
        $("#topBar").addClass("col-lg-4");
    }
}

$(document).ready(function () {
    // ajuste para pantalla movil
    ajustePantalla();
    //Busca Usuario
    setOpenMenu("menu_Administración", "AdministrationUsers")

    $("#filter").click(function () {
        searchData();
    });

    //Función para abrir el calendario pemex

    $("#fec_iniAgre").change(function () {
        var x = document.getElementById("fec_finAgre").min = $("#fec_iniAgre").val();
    });

    //Función para abrir el calendario proveedor

    $("#fec_iniAgreP").change(function () {
        var x = document.getElementById("fec_finAgreP").min = $("#fec_iniAgreP").val();
    });

    //muestra ciertos campos para crear un usuario ya sea proveedor, auditor o ppi

    $("#ddUsuarios").change(function () {

        var usuario = $('#ddUsuarios').val();

        if (usuario == 0) {

            $("#txtTelefono").val('');
            $("#txtUsuarioP").val('');
            document.getElementById('txtUsuarioP').disabled = false;
            $("#txtUserIdPP").val('');
            $("#txtNombrePP").val('');
            document.getElementById('ddOrganismos').disabled = false;
            $("#txtCompania").val('');
            $("#txtEmailP").val('');
            $("#txtAcreedorP").val('');
            document.getElementById('txtAcreedorP').disabled = false;
            $("#txtRFC").val('');
            $("#fec_iniAgreP").val('');
            $("#fec_finAgreP").val('');

            $("#TipoAPPI").hide();
            $("#TipoNombreProv").show();
            $("#txtUserIdPP").hide();
            $("#PerfilOrganismProv").show();
            $("#CompRFCProv").show();
            $("#AcreedorEmailProv").show();
            $("#UsuarioTelProv").show();
            $("#FechasProveedor").show();
            $("#txtCreditorRFC").val('');
            $("#txtCreditorRFC").show();
        }
        else if (usuario == 1) {

            $("#txtUsuarioP").val('');
            document.getElementById('txtUsuarioP').disabled = false;
            $("#txtNombrePP").val('');
            $("#txtCompania").val('');
            $("#txtEmailP").val('');
            $("#llTu").hide();

            $("#TipoAPPI").show();
            $("#txtTipoUsuarioAPPI").show();
            $('#txtTipoUsuarioAPPI').val('Auditor');
            $("#TipoNombreProv").show();
            $("#txtUserIdPP").hide();
            $("#PerfilOrganismProv").show();
            $("#CompRFCProv").show();
            $("#AcreedorEmailProv").hide();
            $("#UsuarioTelProv").hide();
            $("#FechasProveedor").hide();
            $("#txtCreditorRFC").val('');
            $("#txtCreditorRFC").show();
        }
        else {

            $("#txtUsuarioP").val('');
            document.getElementById('txtUsuarioP').disabled = false;
            $("#txtNombrePP").val('');
            $("#txtCompania").val('');
            $("#txtEmailP").val('');
            $("#llTu").hide();

            $("#TipoAPPI").show();
            $("#txtTipoUsuarioAPPI").show();
            $('#txtTipoUsuarioAPPI').val('UsuarioPPI');
            $("#TipoNombreProv").show();
            $("#txtUserIdPP").hide();
            $("#PerfilOrganismProv").show();
            $("#CompRFCProv").show();
            $("#AcreedorEmailProv").hide();
            $("#UsuarioTelProv").hide();
            $("#FechasProveedor").hide();
            $("#txtCreditorRFC").val('');
            $("#txtCreditorRFC").show();
        }

        $("#btnAgregarUproveedor").show();
        $("#btnEditarUproveedor").hide();
    });

    //Consumir el wcf y mostrar los demas div

    $("#BuscarFichaWCF").on("click", function (e) {

        var ficha = $("#txtFicha").val();
        var url = $('#urlBuscarSIO').val();
        var dataCheck = { ficha: ficha };
        var dataFicha = { Ficha: ficha };

        var ef = /^[0-9]*$/;
        if (!ef.test(ficha)) {
            errorAlert('Ficha inválida');
            return;
        }

        clearPemex();
        $("#txtFicha").val(ficha);

        //ocultar datos
        $("#userPemexBody").hide();
        showLoader();

        if (ficha.length >= 6) {

            $.post('/AdmonUser/ExisteUsuario', dataCheck).done(function (data) {

                if (data == true) {
                    hideLoader();
                    errorAlert('Se encontró un usuario previamente registrado con la ficha ingresada');
                    return;
                }

                $.post(url, dataFicha).done(function (data) {

                    if (data.responseText == undefined) {
                        //mostrar datos
                        $("#userPemexBody").show();

                        if (data.email == null) {
                            $("#txtNombreI").val(data.nombres + ' ' + data.aP_PATERNO + ' ' + data.aP_MATERNO);
                            //document.getElementById('txtNombreI').disabled = true;
                            $("#txtEmailI").val(data.email);
                            //document.getElementById('txtEmailI').disabled = false;
                            $("#txtCentroI").val(data.deptO_CLAVE);
                            //document.getElementById('txtCentroI').disabled = true;
                            $("#txtRFCI").val(data.rfC_SAT);
                            //document.getElementById('txtRFCI').disabled = true;
                        }
                        else if (data.rfC_SAT == null) {
                            $("#txtNombreI").val(data.nombres + ' ' + data.aP_PATERNO + ' ' + data.aP_MATERNO);
                            //document.getElementById('txtNombreI').disabled = true;
                            $("#txtEmailI").val(data.email);
                            //document.getElementById('txtEmailI').disabled = true;
                            $("#txtCentroI").val(data.deptO_CLAVE);
                            //document.getElementById('txtCentroI').disabled = true;
                            $("#txtRFCI").val(data.rfC_SAT);
                            //document.getElementById('txtRFCI').disabled = false;
                        }
                        else if (data.rfC_SAT == null && data.email == null) {
                            $("#txtNombreI").val(data.nombres + ' ' + data.aP_PATERNO + ' ' + data.aP_MATERNO);
                            //document.getElementById('txtNombreI').disabled = true;
                            $("#txtEmailI").val(data.email);
                            //document.getElementById('txtEmailI').disabled = false;
                            $("#txtCentroI").val(data.deptO_CLAVE);
                            //document.getElementById('txtCentroI').disabled = true;
                            $("#txtRFCI").val(data.rfC_SAT);
                            //document.getElementById('txtRFCI').disabled = false;
                        }
                        else {
                            $("#txtNombreI").val(data.nombres + ' ' + data.aP_PATERNO + ' ' + data.aP_MATERNO);
                            //document.getElementById('txtNombreI').disabled = true;
                            $("#txtEmailI").val(data.email);
                            //document.getElementById('txtEmailI').disabled = true;
                            $("#txtCentroI").val(data.deptO_CLAVE);
                            //document.getElementById('txtCentroI').disabled = true;
                            $("#txtRFCI").val(data.rfC_SAT);
                            //document.getElementById('txtRFCI').disabled = true;
                        }
                        $("#txtUsuario").val(data.ficha);
                        hideLoader();
                    }
                    else {

                        hideLoader();
                        errorAlert('No se encontraron datos con la ficha ingresada');

                    }
                });
            });
        }
        else {
            hideLoader();
            errorAlert('El número de ficha debe ser de 6 dígitos');
        }
    });

    $(window).resize(function () {
        ajustePantalla();
    });

    $("#txtAcreedor").change(function () {
        var x = $("#txtAcreedor").val();
        if (x != undefined) {
            if (x != '') {
                $("#txtAcreedor").val(padAcreedor(x));
            }
        }
    });

    $("#txtAcreedorP").change(function () {
        var x = $("#txtAcreedorP").val();
        if (x != undefined) {
            if (x != '') {
                $("#txtAcreedorP").val(padAcreedor(x))
            }
        }
    });
});

function padAcreedor(str) {
    var r = '';
    if (str.length > 10) {
        r = str.substring(0, 10);
    }
    else if (str.length == 10) {
        r = str;
    }
    else {
        r = str;
        for (var i = str.length; i < 10; i++) {
            r = '0' + r;
        }
    }
    return r;
}

function searchData() {
    var url = $('#urlBuscar').val();
    var texto = $("#textFilter").val();
    var data = { texto: texto };

    if (texto.length > 0) {

        var en = /^[0-9a-zA-Z.,áéíóúÁÉÍÓÚñÑ* ]*$/;
        if (!en.test(texto)) {
            errorAlert('Búsqueda inválida');
            return;
        }

        showLoader();
        $.post(url, data).done(function (data) {

            if (!data.includes('Nombre')) {
                errorAlert('Sin datos para la información proporcionada');
            }

            $("#DatosUsuario").html(data);
            $("#DatosUsuario").show();
            ajustePantalla();
            hideLoader();
        });
    }
    else {
        errorAlert("El texto de búsqueda está vacío");
    }
}

function newUserDisplay(d) {
    clearPemex();
    clearProveedor();
    $("#userPemexBody").hide();
    document.getElementById('BuscarFichaWCF').disabled = false;
    document.getElementById('txtFicha').disabled = false;
    document.getElementById('txtUsuario').disabled = false;
    if (d == 0) {
        showPemex();
    }
    else if (d == 1) {
        document.getElementById('ddUsuarios').disabled = false;
        showProveedor();
    }
}

//Para mandar los organismos de un usuario pemex

function seleccionaOrganismosI() {
    var selectedI = [];
    $(document).find("input[name='SelectOrganismI']").each(function () {
        //Mediante la función push agregamos al arreglo los values de los checkbox
        var objOrganismo = {
            Organismo: $(this).val(),
            Status: $(this).prop("checked")
        }
        selectedI.push(objOrganismo);
    });
    return selectedI;
}


//Para mandar los organismos de un usuario pemex


function seleccionaOrganismos() {
    var selected = [];
    $(document).find("input[name='SelectOrganism']").each(function () {
        //Mediante la función push agregamos al arreglo los values de los checkbox
        var objOrganismo = {
            Organismo: $(this).val(),
            Status: $(this).prop("checked")
        }
        selected.push(objOrganismo);
    });
    return selected;
}

//carga para edicion

function edicionOrganismosI(OrganismosId) {
    if (OrganismosId == undefined || OrganismosId == '') {
        return;
    }

    var selectedI = OrganismosId.split(',');

    $(document).find("input[name='SelectOrganismI']").each(function () {
        var organismo = $(this).val();
        if (organismo) {
            $(this).prop("checked", false);
        }
    });

    $(document).find("input[name='SelectOrganismI']").each(function () {
        var organismo = $(this).val();
        if (organismo) {
            if (selectedI.length > 0) {
                for (var i = 0; i < selectedI.length; i++) {
                    if (selectedI[i] == organismo) {
                        $(this).prop("checked", true);
                    }
                }
            }
        }
    });
    return selectedI;
}

//Para mandar los datos de un usuario pemex al modal y poder editar

function EditarUsuario(UserId, ProfileId, Ficha, Usuario, Nombre, Email, Centro, RFC, ValidoDesde, ValidoHasta, OrganismosId) {

    $('#userPemex').show();
    $('#userPemexBody').show();
    $('#userProveedor').hide();
    $('#userPemex').scrollTop(0);

    $("#txtUserIdI").hide();
    $("#txtUserIdI").val(UserId);
    $('#ddPerfilesI').val(ProfileId),

        document.getElementById('BuscarFichaWCF').disabled = true;
    document.getElementById('txtFicha').disabled = true;
    document.getElementById('txtUsuario').disabled = true;

    $("#txtFicha").val(Ficha);
    $("#txtUsuario").val(Usuario);
    $("#NombreDivI").show();
    $("#txtNombreI").val(Nombre);
    $("#txtEmailI").val(Email);
    $("#EmailCenterI").show();
    $("#txtCentroI").val(Centro);
    $("#txtRFCI").val(RFC);
    $("#fec_iniAgre").val(ValidoDesde);
    $("#fec_finAgre").val(ValidoHasta);

    edicionOrganismosI(OrganismosId);

    ajustePantalla();

    var x = $(document).width();
    if (x < 1300) {
        $(document).scrollTop(0);
    }
}


//Para mandar los datos de un proveedor al modal y poder editar

function EditarUsuarioP(opcion, UserId, ProfileId, OrganismoId, Nombre, Compania, RFC, Nacreedor, Email, Usuario, PhoneNumber, ValidoDesde, ValidoHasta, CreditorRFC) {

    $('#userPemex').hide();
    $('#userProveedor').show();
    $('#userProveedor').scrollTop(0);

    if (opcion == 'Proveedor') {
        $("#ddUsuarios").prop('selectedIndex', 0);
    }
    else if (opcion == 'Auditor') {
        $("#ddUsuarios").prop('selectedIndex', 1);
    }
    else {
        $("#ddUsuarios").prop('selectedIndex', 2);
    }
    document.getElementById('ddUsuarios').disabled = true;

    $("#TipoAPPI").hide();
    $("#TipoNombreProv").show();
    $("#txtUserIdPP").hide();
    $("#PerfilOrganismProv").show();
    $("#CompRFCProv").show();
    $("#AcreedorEmailProv").show();
    $("#UsuarioTelProv").show();
    $("#FechasProveedor").show();

    $("#txtUserIdPP").val(UserId);
    $("#txtUserIdPP").hide();

    $('#ddPerfilesP').val(ProfileId);

    //si tiene mas de un organismo obtener el primero
    if (OrganismoId.indexOf(',') == -1) {
        $('#ddOrganismos').val(OrganismoId);
    }
    else {
        var orgId = OrganismoId.split(',')[0];
        $('#ddOrganismos').val(orgId);
    }

    $("#txtNombrePP").val(Nombre);

    $("#txtCompania").val(Compania);

    $("#txtRFC").val(RFC);

    //document.getElementById('txtAcreedorP').disabled = true;
    $("#txtAcreedorP").val(Nacreedor);

    $("#txtEmailP").val(Email);

    document.getElementById('txtUsuarioP').disabled = true;
    $("#txtUsuarioP").val(Usuario);

    $("#txtTelefono").val(PhoneNumber);

    $("#fec_iniAgreP").val(ValidoDesde.replace(' ', ''));

    $("#fec_finAgreP").val(ValidoHasta.replace(' ', ''));

    $("#txtCreditorRFC").val(CreditorRFC);

    var usuario = $('#ddUsuarios').val();

    if (usuario == 0) {

        $("#TipoAPPI").hide();
        $("#TipoNombreProv").show();
        $("#txtUserIdPP").hide();
        $("#PerfilOrganismProv").show();
        $("#CompRFCProv").show();
        $("#AcreedorEmailProv").show();
        $("#UsuarioTelProv").show();
        $("#FechasProveedor").show();
    }
    else if (usuario == 1) {

        $("#TipoAPPI").show();
        $("#txtTipoUsuarioAPPI").show();
        $('#txtTipoUsuarioAPPI').val('Auditor');
        $("#TipoNombreProv").show();
        $("#txtUserIdPP").hide();
        $("#PerfilOrganismProv").show();
        $("#CompRFCProv").show();
        $("#AcreedorEmailProv").hide();
        $("#UsuarioTelProv").hide();
        $("#FechasProveedor").hide();
    }
    else {

        $("#TipoAPPI").show();
        $("#txtTipoUsuarioAPPI").show();
        $('#txtTipoUsuarioAPPI').val('UsuarioPPI');
        $("#TipoNombreProv").show();
        $("#txtUserIdPP").hide();
        $("#PerfilOrganismProv").show();
        $("#CompRFCProv").show();
        $("#AcreedorEmailProv").hide();
        $("#UsuarioTelProv").hide();
        $("#FechasProveedor").hide();
    }

    ajustePantalla();

    var x = $(document).width();
    if (x < 1300) {
        $(document).scrollTop(0);
    }
}


//Para mandar los datos de un auditorPpi al modal y poder editar

function EditarUsuarioAuditorPpi(UserId, Nombre, Compania, CreditorNumber, Email, Usuario, UserType, CreditorRFC, OrganismoId) {

    $('#userPemex').hide();
    $('#userProveedor').show();

    if (UserType == 'Auditor') {
        $('#ddUsuarios').val(1);
    }
    if (UserType == 'UsuarioPPI') {
        $('#ddUsuarios').val(2);
    }

    $("#txtUserIdPP").val(UserId);
    $("#txtUserIdPP").hide();
    $("#txtNombrePP").val(Nombre);
    $("#txtCompania").val(Compania);

    $("#txtEmailP").val(Email);

    document.getElementById('txtUsuarioP').disabled = true;
    $("#txtUsuarioP").val('Proveedor');

    $("#TipoAPPI").hide();
    $('#txtTipoUsuarioAPPI').hide();

    document.getElementById('ddUsuarios').disabled = true;
    $("#UsuarioTelProv").hide();

    $("#TipoNombreProv").show();
    $("#txtUserIdPP").hide();
    $("#PerfilOrganismProv").hide();
    $("#CompRFCProv").show();
    $("#AcreedorEmailProv").hide();
    $("#UsuarioTelProv").hide();
    $("#FechasProveedor").hide();
    $("#txtCreditorRFC").val(CreditorRFC);

    ajustePantalla();
}

//Cambia el estatus del usuario pemex

function EstatusUsuario(UserName, Name, Email, UserId) {

    Swal.fire({
        title: "Usuarios",
        text: "¿Estas seguro de cambiar el estatus de este usuario?",
        type: "warning",
        showCancelButton: true,
        cancelButtonText: "Cancelar",
        confirmButtonText: "Actualizar",
        closeOnConfirm: true,
    }).then((result) => {
        if (result.isConfirmed) {

            var url = $('#urlEstatusPemex').val();
            var userId = UserId;
            var data = {
                userId: userId, Name: Name, Email: Email, UserName: UserName
            };
            showLoader();

            $.post(url, data).done(function (data) {

                var url = $('#urlBuscarPemex').val();

                var ficha = $("#txtFichaB").val();

                var dataf = { Ficha: ficha };

                $.post(url, dataf).done(function (dataf) {

                    if ($("#textFilter").val() != '') {
                        $("#filter").click();
                    }
                    hideLoader();
                });
            });
        }

    });
}

//Cambia el estatus del usuario proveedor

function EstatusUsuarioP(UserId) {

    Swal.fire({
        title: "Usuarios",
        text: "¿Estas seguro de cambiar el estatus de este usuario?",
        type: "warning",
        showCancelButton: true,
        cancelButtonText: "Cancelar",
        confirmButtonText: "Actualizar",
        closeOnConfirm: true,
    }).then((result) => {
        if (result.isConfirmed) {
            var url = $('#urlEstatusPemex').val();
            var userId = UserId;
            var data = { userId: userId };
            showLoader();

            $.post(url, data).done(function (data) {
                var url = $('#urlBuscarProveedor').val();
                var acreedor = $("#txtAcreedor").val();
                var datap = { creditorNumber: acreedor };

                $.post(url, datap).done(function (datap) {

                    if ($("#textFilter").val() != '') {
                        $("#filter").click();
                    }
                    hideLoader();
                });
            });
        }
    });
}

//Cambia el estatus del usuario auditor o ppi

function EstatusUsuarioAuditorPpi(UserId) {

    Swal.fire({
        title: "Usuarios",
        text: "¿Estas seguro de cambiar el estatus de este usuario?",
        type: "warning",
        showCancelButton: true,
        cancelButtonText: "Cancelar",
        confirmButtonText: "Actualizar",
        closeOnConfirm: true,
    }).then((result) => {
        if (result.isConfirmed) {
            var url = $('#urlEstatusPemex').val();
            var userId = UserId;
            var data = { userId: userId };
            showLoader();

            $.post(url, data).done(function (data) {

                var url1 = $('#urlBuscarCorreo').val();
                var correo = $("#txtCorreo").val();
                var datap = { Correo: correo };

                $.post(url1, datap).done(function (datap) {

                    if ($("#textFilter").val() != '') {
                        $("#filter").click();
                    }
                    hideLoader();
                });

            });
        }
    });
}

//Convertir a Super Administrador

function SuperAdmin(UserId) {

    Swal.fire({
        title: "Usuarios",
        text: "¿Estas seguro de cambiar el tipo de usuario?",
        type: "warning",
        showCancelButton: true,
        cancelButtonText: "Cancelar",
        confirmButtonText: "Actualizar",
        closeOnConfirm: true,
    }).then((result) => {
        if (result.isConfirmed) {
            var url = $('#urlSuperAdmin').val();
            var userId = UserId;
            var data = { userId: userId };
            showLoader();

            $.post(url, data).done(function (data) {

                var url = $('#urlBuscarPemex').val();

                var ficha = $("#txtFichaB").val();

                var dataf = { Ficha: ficha };

                $.post(url, dataf).done(function (dataf) {

                    if ($("#textFilter").val() != '') {
                        $("#filter").click();
                    }
                    hideLoader();
                });
            });
        }
    });
}

//Convertir a Administrador

function Admin(UserId) {

    Swal.fire({
        title: "Usuarios",
        text: "¿Estas seguro de cambiar el tipo de usuario?",
        type: "warning",
        showCancelButton: true,
        cancelButtonText: "Cancelar",
        confirmButtonText: "Actualizar",
        closeOnConfirm: true,
    }).then((result) => {
        if (result.isConfirmed) {
            var url = $('#urlAdmin').val();
            var userId = UserId;
            var data = { userId: userId };
            showLoader();

            $.post(url, data).done(function (data) {

                var url = $('#urlBuscarPemex').val();

                var ficha = $("#txtFichaB").val();

                var dataf = { Ficha: ficha };

                $.post(url, dataf).done(function (dataf) {

                    if ($("#textFilter").val() != '') {
                        $("#filter").click();
                    }
                    hideLoader();
                });
            });
        }
    });
}

//limpiar usuario pemex

function clearPemex() {
    $('#txtUserIdI').val('');
    $('#txtNombreI').val('');
    $('#txtFicha').val('');
    $('#txtCentroI').val('');
    $('#txtRFCI').val('');
    $('#ddPerfilesI').attr('selectedIndex', 0);
    $('#txtUsuario').val('');
    $('#txtEmailI').val('');
    $('#fec_iniAgre').val('');
    $('#fec_finAgre').val('');

    $(document).find("input[name='SelectOrganismI']").each(function () {
        $(this).prop("checked", false);
    });

    $('#fec_iniAgre').val('');
    $('#fec_finAgre').val('');
}

//limpiar usuario proveedor

function clearProveedor() {
    $('#txtUserIdPP').val('');
    $('#txtCompania').val('');
    $('#txtRFC').val('');
    $('#ddPerfilesP').attr('selectedIndex', 0);
    $('#txtNombrePP').val('');
    $('#txtEmailP').val('');
    $('#txtTelefono').val('');
    $('#ddOrganismos').attr('selectedIndex', 0);
    $('#fec_iniAgreP').val('');
    $('#fec_finAgreP').val('');
    $("#ddUsuarios").val('0').change();

    $('#fec_iniAgreP').val('');
    $('#fec_finAgreP').val('');
    $("#txtCreditorRFC").val('');

}

//agregar usuario pemex

function showPemex() {
    $('#userPemex').show();
    $('#userProveedor').hide();
    $("#txtFicha").css('border-color', 'lightgrey');
    $('#userPemex').scrollTop(0);
}

function hidePemex() {
    clearPemex();
    $('#userPemex').hide();
}

//agregar usuario proveedor

function showProveedor() {
    $('#userPemex').hide();
    $('#userProveedor').show();
    $('#userProveedor').scrollTop(0);
}

function hideProveedor() {
    clearProveedor();
    $('#userProveedor').hide();
}

//validaciones
function validateUserPemex() {
    var r = '';

    var ficha = $("#txtFicha").val();
    var nombre = $("#txtNombreI").val();
    var email = $("#txtEmailI").val();
    var centro = $("#txtCentroI").val();
    var rfc = $("#txtRFCI").val();
    var inicio = $("#fec_iniAgre").val();
    var fin = $("#fec_finAgre").val();

    if (ficha == '') {
        r = "Ficha requerida";
    }
    else {
        if (nombre == '') {
            r = "Nombre requerido";
        }
        else {
            if (email == '') {
                r = 'Email requerido';
            }
            else {
                if (centro == '') {
                    r = 'Centro requerido';
                }
                else {
                    if (rfc == '') {
                        r = 'RFC requerido';
                    }
                    else {
                        if (inicio == '') {
                            r = 'Fecha inicial requerida';
                        }
                        else {
                            if (fin == '') {
                                r = 'Fecha final requerida';
                            }
                            else {
                                var ef = /^[0-9]*$/;
                                if (!ef.test(ficha)) {
                                    r = 'Ficha inválida';
                                }
                                else {
                                    var en = /^[0-9a-zA-Z.,áéíóúÁÉÍÓÚñÑ ]*$/;
                                    if (!en.test(nombre)) {
                                        r = 'Nombre inválido';
                                    }
                                    else {
                                        var ec = /^[0-9a-zA-Z.,áéíóúÁÉÍÓÚñÑ ]*$/;
                                        if (!ec.test(centro)) {
                                            r = 'Centro inválido';
                                        }
                                        else {
                                            if (ficha.length < 6) {
                                                r = 'Ficha debe ser por lo menos de 6 digitos';
                                            }
                                            else {
                                                if (rfc != '') {
                                                    if (!rfcValido(rfc, true)) {
                                                        r = 'RFC incorrecto';
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    return r;
}

function validateUserProveedor() {
    var r = '';

    var nombre = $("#txtNombrePP").val();
    var telefono = $("#txtTelefono").val();
    var compania = $("#txtCompania").val();
    var email = $("#txtEmailP").val();
    var acreedor = $("#txtAcreedorP").val();
    var rfc = $("#txtRFC").val();
    var inicio = $("#fec_iniAgreP").val();
    var fin = $("#fec_finAgreP").val();

    var vnombre = $("#txtNombrePP").is(":visible");
    var vtelefono = $("#txtTelefono").is(":visible");
    var vcompania = $("#txtCompania").is(":visible");
    var vemail = $("#txtEmailP").is(":visible");
    var vacreedor = $("#txtAcreedorP").is(":visible");
    var vrfc = $("#txtRFC").is(":visible");
    var vinicio = $("#fec_iniAgreP").is(":visible");
    var vfin = $("#fec_finAgreP").is(":visible");

    if (vnombre) {
        if (nombre == '') {
            r = "Nombre requerido";
        }
        else {
            var en = /^[0-9a-zA-Z.,áéíóúÁÉÍÓÚñÑ ]*$/;
            if (!en.test(nombre)) {
                r = 'Nombre inválido';
            }
        }
    }

    if (vtelefono) {
        if (telefono == '') {
            r = 'Teléfono requerido';
        }
        else {
            var et = /^[0-9]*$/;
            if (!et.test(telefono)) {
                r = 'Teléfono inválido';
            }
            else {
                if (telefono.length != 10) {
                    r = 'Teléfono requerido con 10 digitos';
                }
            }
        }
    }

    if (vcompania) {
        if (compania == '') {
            r = "Compañia requerida";
        }
    }
    else {
        var ec = /^[0-9a-zA-Z.,&áéíóúÁÉÍÓÚñÑ ]*$/;
        if (!ec.test(compania)) {
            r = 'Nombre de compañía inválido';
        }
    }

    if (vemail) {
        if (email == '') {
            r = 'Correo requerido';
        }
    }

    if (vacreedor) {
        if (acreedor == '') {
            r = 'Acreedor requerido';
        }
        else {
            var ea = /^[0-9a-zA-Z.,áéíóúÁÉÍÓÚñÑ ]*$/;
            if (!ea.test(acreedor)) {
                r = 'Acreedor inválido';
            }
        }
    }

    if (vrfc) {
        if (rfc == '') {
            r = 'RFC requerido';
        }
        else {
            if (!rfcValido(rfc, true)) {
                r = 'RFC incorrecto';
            }
        }
    }

    if (vinicio) {
        if (inicio == '') {
            r = 'Fecha de inicio requerida';
        }
    }

    if (vfin) {
        if (fin == '') {
            r = 'Fecha final requerida';
        }
    }

    return r;
}

//procesar usuario

function saveUser() {
    if ($('#userPemex').is(":visible")) {
        var msg = validateUserPemex();
        if (msg != '') {
            errorAlert(msg);
            return;
        }
        if ($('#txtUserIdI').val() == '' || $('#txtUserIdI').val() == undefined) {
            saveNewUserPemex();
        }
        else {
            updateUserPemex();
        }
    }
    else if ($('#userProveedor').is(":visible")) {
        var msgP = validateUserProveedor();
        if (msgP != '') {
            errorAlert(msgP);
            return;
        }
        if ($('#txtUserIdPP').val() == '' || $('#txtUserIdPP').val() == undefined) {
            saveNewUserProveedor();
        }
        else {
            updateUserProveedor();
        }
    }
}

//acción agregar usuario pemex

function saveNewUserPemex() {

    var uri = $('#urlAgregar').val();

    var Data = {
        Name: $('#txtNombreI').val(),
        Ficha: $('#txtFicha').val(),
        Centro: $('#txtCentroI').val(),
        RFC: $('#txtRFCI').val(),
        Perfil: $('#ddPerfilesI').val(),
        userName: $('#txtFicha').val(),
        Email: $('#txtEmailI').val(),
        ValidoDesde: $('#fec_iniAgre').val(),
        ValidoHasta: $('#fec_finAgre').val(),
        CreditorRFC: '',
        Organismos: seleccionaOrganismosI()
    };

    $.ajax({
        type: "post",
        datatype: 'json',
        url: uri,
        data: Data,
        success: function (response) {
            if (response.success) {

                $('#userPemex').hide();
                if ($("#textFilter").val() != '') {
                    $("#filter").click();
                }

                successAlert(response.responseText);

                clearPemex();
                clearProveedor();

            } else {

                errorAlert(response.responseText);

            }
        }
    });

}

//acción agregar usuario proveedor

function saveNewUserProveedor() {

    var usuario = $('#ddUsuarios').val();
    var uri = $('#urlAgregarProveedor').val();
    var uriA = $('#urlAgregarAuditorPpi').val();

    if (usuario == 0) {

        var Data = {
            Compania: $('#txtCompania').val(),
            RFC: $('#txtRFC').val(),
            CreditorNumber: $('#txtAcreedorP').val(),
            userName: $('#txtUsuarioP').val(),
            ValidoDesde: $('#fec_iniAgreP').val(),
            ValidoHasta: $('#fec_finAgreP').val(),
            Perfil: $('#ddPerfilesP').val(),
            Organismo: $('#ddOrganismos').val(),
            Name: $('#txtNombrePP').val(),
            Email: $('#txtEmailP').val(),
            PhoneNumber: $('#txtTelefono').val(),
            CreditorRFC: $("#txtCreditorRFC").val()
        };

        $.ajax({
            type: "post",
            datatype: 'json',
            url: uri,
            data: Data,
            success: function (response) {
                if (response.success) {

                    $('#userProveedor').hide();
                    if ($("#textFilter").val() != '') {
                        $("#filter").click();
                    }
                    successAlert(response.responseText);

                    clearPemex();
                    clearProveedor();

                } else {

                    errorAlert(response.responseText);

                }
            }
        });

    }

    else if (usuario == 1) {
        var Data = {
            userType: 'Auditor',
            Compania: $('#txtCompania').val(),
            userName: $('#txtUsuarioP').val(),
            Name: $('#txtNombrePP').val(),
            Email: $('#txtEmailP').val(),
            RFC: $('#txtRFC').val(),
            Perfil: $('#ddPerfilesP').val(),
            Organismo: $('#ddOrganismos').val(),
            CreditorRFC: $("#txtCreditorRFC").val()
        };

        $.ajax({
            type: "post",
            datatype: 'json',
            url: uriA,
            data: Data,
            success: function (response) {
                if (response.success) {

                    $('#userProveedor').hide();
                    if ($("#textFilter").val() != '') {
                        $("#filter").click();
                    }
                    successAlert(response.responseText);

                } else {

                    errorAlert(response.responseText);

                }
            }
        });
    }

    else {
        var Data = {
            userType: 'UsuarioPPI',
            Compania: $('#txtCompania').val(),
            userName: $('#txtUsuarioP').val(),
            Name: $('#txtNombrePP').val(),
            RFC: $('#txtRFC').val(),
            Email: $('#txtEmailP').val(),
            Perfil: $('#ddPerfilesP').val(),
            Organismo: $('#ddOrganismos').val(),
            CreditorRFC: $("#txtCreditorRFC").val()
        };

        $.ajax({
            type: "post",
            datatype: 'json',
            url: uriA,
            data: Data,
            success: function (response) {
                if (response.success) {

                    $('#userProveedor').hide();
                    if ($("#textFilter").val() != '') {
                        $("#filter").click();
                    }
                    successAlert(response.responseText);

                } else {

                    successAlert(response.responseText);

                }
            }
        });

    }

}

//accion guardar edicion

function updateUser() {
    if ($('#userPemex').is(":visible")) {
        updateUserPemex();
    }
    else if ($('#userProveedor').is(":visible")) {
        updateUserProveedor();
    }
}

//acción guardar edicion un usuario pemex

function updateUserPemex() {

    var uri = $('#urlEditar').val();

    var Data = {
        UserId: $('#txtUserIdI').val(),
        Name: $('#txtNombreI').val(),
        Ficha: $('#txtFicha').val(),
        Centro: $('#txtCentroI').val(),
        RFC: $('#txtRFCI').val(),
        Perfil: $('#ddPerfilesI').val(),
        userName: $('#txtFicha').val(),
        Email: $('#txtEmailI').val(),
        ValidoDesde: $('#fec_iniAgre').val(),
        ValidoHasta: $('#fec_finAgre').val(),
        CreditorRFC: '',
        Organismos: seleccionaOrganismosI()
    };

    $.ajax({
        type: "post",
        datatype: 'json',
        url: uri,
        data: Data,
        success: function (response) {
            if (response.success) {

                $('#userPemex').hide();
                if ($("#textFilter").val() != '') {
                    $("#filter").click();
                }
                successAlert(response.responseText);

                clearPemex();
                clearProveedor();

            } else {

                errorAlert(response.responseText);

            }
        }
    });

}

//acción guardar edicion usuario proveedor

function updateUserProveedor() {
    var tipoUs = $('#ddUsuarios').val();
    if (tipoUs == 0) {
        var uri = $('#urlEditarProveedor').val();

        var Data = {
            UserId: $('#txtUserIdPP').val(),
            Compania: $('#txtCompania').val(),
            RFC: $('#txtRFC').val(),
            Perfil: $('#ddPerfilesP').val(),
            Name: $('#txtNombrePP').val(),
            Email: $('#txtEmailP').val(),
            PhoneNumber: $('#txtTelefono').val(),
            Organismo: $('#ddOrganismos').val(),
            ValidoDesde: $('#fec_iniAgreP').val(),
            ValidoHasta: $('#fec_finAgreP').val(),
            CreditorRFC: $("#txtCreditorRFC").val()
        };

        $.ajax({
            type: "post",
            datatype: 'json',
            url: uri,
            data: Data,
            success: function (response) {
                if (response.success) {

                    $("#userProveedor").hide();
                    if ($("#textFilter").val() != '') {
                        $("#filter").click();
                    }
                    successAlert(response.responseText);

                    clearPemex();
                    clearProveedor();

                } else {

                    errorAlert(response.responseText);

                }
            }
        });
    }
    else if (tipoUs == 1) {

        var uri = $('#urlEditarAuditorPpi').val();

        var Data = {
            UserId: $("#txtUserIdPP").val(),
            userType: 'Auditor',
            Compania: $('#txtCompania').val(),
            RFC: $('#txtRFC').val(),
            userName: 'Proveedor',
            Name: $('#txtNombrePP').val(),
            Email: $('#txtEmailP').val(),
            Perfil: $('#ddPerfilesP').val(),
            Organismo: $('#ddOrganismos').val(),
            CreditorRFC: $("#txtCreditorRFC").val()
        };

        $.ajax({
            type: "post",
            datatype: 'json',
            url: uri,
            data: Data,
            success: function (response) {
                if (response.success) {

                    $("#userProveedor").hide();
                    if ($("#textFilter").val() != '') {
                        $("#filter").click();
                    }
                    successAlert(response.responseText);

                    clearPemex();
                    clearProveedor();

                } else {

                    errorAlert(response.responseText);

                }
            }
        });
    }
    else {

        var uri = $('#urlEditarAuditorPpi').val();

        var Data = {
            UserId: $("#txtUserIdPP").val(),
            userType: 'UsuarioPPI',
            Compania: $('#txtCompania').val(),
            RFC: $('#txtRFC').val(),
            userName: $('#txtUsuarioP').val(),
            Name: $('#txtNombrePP').val(),
            Email: $('#txtEmailP').val(),
            Perfil: $('#ddPerfilesP').val(),
            Organismo: $('#ddOrganismos').val(),
            CreditorRFC: $("#txtCreditorRFC").val()
        };

        $.ajax({
            type: "post",
            datatype: 'json',
            url: uri,
            data: Data,
            success: function (response) {
                if (response.success) {

                    $("#userProveedor").hide();
                    if ($("#textFilter").val() != '') {
                        $("#filter").click();
                    }
                    successAlert(response.responseText);

                    clearPemex();
                    clearProveedor();

                } else {

                    errorAlert(response.responseText);

                }
            }
        });
    }

}

function onKeyDownHandler(event) {

    var codigo = event.which || event.keyCode;

    if (codigo === 13) {
        $("#filter").click();
    }
}

function rfcValido(rfc, aceptarGenerico = true) {
    const re = /^([A-ZÑ&]{3,4}) ?(?:- ?)?(\d{2}(?:0[1-9]|1[0-2])(?:0[1-9]|[12]\d|3[01])) ?(?:- ?)?([A-Z\d]{2})([A\d])$/;
    var validado = rfc.match(re);

    if (!validado)  //Coincide con el formato general del regex?
        return false;

    //Separar el dígito verificador del resto del RFC
    const digitoVerificador = validado.pop(),
        rfcSinDigito = validado.slice(1).join(''),
        len = rfcSinDigito.length,

        //Obtener el digito esperado
        diccionario = "0123456789ABCDEFGHIJKLMN&OPQRSTUVWXYZ Ñ",
        indice = len + 1;
    var suma,
        digitoEsperado;

    if (len == 12) suma = 0
    else suma = 481; //Ajuste para persona moral

    for (var i = 0; i < len; i++)
        suma += diccionario.indexOf(rfcSinDigito.charAt(i)) * (indice - i);
    digitoEsperado = 11 - suma % 11;
    if (digitoEsperado == 11) digitoEsperado = 0;
    else if (digitoEsperado == 10) digitoEsperado = "A";

    //El dígito verificador coincide con el esperado?
    // o es un RFC Genérico (ventas a público general)?
    if ((digitoVerificador != digitoEsperado)
        && (!aceptarGenerico || rfcSinDigito + digitoVerificador != "XAXX010101000"))
        return false;
    else if (!aceptarGenerico && rfcSinDigito + digitoVerificador == "XEXX010101000")
        return false;
    return rfcSinDigito + digitoVerificador;
}