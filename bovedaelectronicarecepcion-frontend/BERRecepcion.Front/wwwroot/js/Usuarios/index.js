$(function () {
    setOpenMenu("menu_Administración", "AdministrationUsers");
});

$(document).on('click', '#user-table .dropdown-menu', function (e) {
    e.stopPropagation();
});

//Consulta de usuarios
$("#user-form-search").on("submit", function (e) {
    e.preventDefault();
    let _search = $("#search").val();
    if (isNull(_search))
        return;
    $.ajax({
        url: "Usuarios/UserTable",
        data: { search: _search },
        success: function (data) {
            // Si es un objeto JSON con success=false, mostrar error
            if (typeof data === 'object' && data.success === false) {
                return errorAlert(data.message);
            }
            // Si es HTML (string), insertarlo en el DOM
            $("#div-user-table").empty();
            $("#div-user-table").append(data);
        },
        error: function(xhr, status, error) {
            errorAlert("Ocurrió un error al procesar la información, por favor intente de nuevo.");
        }
    });
    e.stopImmediatePropagation();
});

//Nuevo usuario Pemex
$("#user-pemex").on("click", function () {
    $.ajax({
        url: "Usuarios/ModalAdd",
        data: { EsPemex: true },
        success: function (data) {
            if (!isNull(data.success) && !data.success)
                return errorAlert(data.message);
            $("#content-pemex").empty();
            $("#content-pemex").append(data);
            $("#modal-add-pemex").modal("show");
            ValidarPemex();
        }
    });
});

//Nuevo usuario Proveedor
$("#user-external").on("click", function () {
    $.ajax({
        url: "Usuarios/ModalAdd",
        data: { EsPemex: false },
        success: function (data) {
            if (!isNull(data.success) && !data.success)
                return errorAlert(data.message);
            $("#content-external").empty();
            $("#content-external").append(data);
            $("#modal-add-external").modal("show");
            ValidarProveedor();
        }
    });
});

//Editar usuario 
$(document).on("click", "a[name='user-edit']", function (e) {
    e.preventDefault();
    let userId = $(this).data("userid");
    let vup = $(this).data("token") ? true : false;

    $.ajax({
        url: "Usuarios/ModalAdd",
        data: { UserID: userId, EsPemex: vup },
        success: function (data) {
            if (!isNull(data.success) && !data.success)
                return errorAlert(data.message);

            if (vup) {
                $("#content-pemex").empty();
                $("#content-pemex").append(data);
                $("#modal-add-pemex").modal("show");
                ValidarPemex();
            }
            else {
                $("#content-external").empty();
                $("#content-external").append(data);
                $("#modal-add-external").modal("show");
                ValidarProveedor();
            }
        }
    });
    e.stopImmediatePropagation();
});

//Actualizar estatus de activo
$(document).on("change", "input[name='activo']", function (e) {
    var element = $(this);
    var user = element.closest("li[name='btn-activo']").data("user");
    var statusCheck = !element.prop("checked");
    confirmAlert("Información", "El estatus del usuario será actualizado <br>  <strong>¿Desea continuar?</strong>", function () {
        $.ajax({
            url: "Usuarios/StatusUsuarioPemex",
            data: user,
            success: function (data) {
                if (!isNull(data.success) && !data.success) {
                    element.prop("checked", statusCheck);
                    return toast.error(data.message);
                }
                return successAlert(data.message);
            }
        });
    }, function () {
        element.prop("checked", statusCheck);
    });
});

//Actualizar estatus de administrador
$(document).on("change", "input[name='admin']", function (e) {
    var element = $(this);
    var userId = element.closest("li[name='btn-admin']").data("userid");
    var statusCheck = !element.prop("checked");
    confirmAlert("Información", "El tipo de usuario será actualizado <br>  <strong>¿Desea continuar?</strong>", function () {
        $.ajax({
            url: "Usuarios/AdminUsuario",
            data: { userId: userId },
            success: function (data) {
                if (!isNull(data.success) && !data.success) {
                    element.prop("checked", statusCheck);
                    return toast.error(data.message);
                }
                return successAlert(data.message);
            }
        });
    }, function () {
        element.prop("checked", statusCheck);
    });
});

//Actualizar estatus de super administrador
$(document).on("change", "input[name='super-admin']", function (e) {
    var element = $(this);
    var userId = element.closest("li[name='btn-super-admin']").data("userid");
    var statusCheck = !element.prop("checked");
    $.ajax({
        url: "Usuarios/SuperUsuario",
        data: { userId: userId },
        success: function (data) {
            if (!isNull(data.success) && !data.success) {
                element.prop("checked", statusCheck);
                return toast.error(data.message);
            }
            return successAlert(data.message);
        }
    });
});

//Guardar usuario Pemex
$(document).on("submit", "#formPemex", function (e) {
    e.preventDefault();
    if ($("#formPemex").valid()) {
        const inputs = new FormData(e.target);
        const values = Object.fromEntries(inputs.entries());
        let index = 0;

        var formData = new FormData();
        formData.append("pemexInDto[UserID]", $("#userID").val());
        formData.append("pemexInDto[Name]", values.Name);
        formData.append("pemexInDto[Ficha]", values.Token);
        formData.append("pemexInDto[Centro]", values.ManagementCenter);
        formData.append("pemexInDto[RFC]", values.RFC.toUpperCase());
        formData.append("pemexInDto[Perfil]", values.ProfileID);
        formData.append("pemexInDto[Email]", values.Email);
        formData.append("pemexInDto[ValidoDesde]", values.DateInitialValid);
        formData.append("pemexInDto[ValidoHasta]", values.DateEndValid);
        formData.append("pemexInDto[UserType]", values.UserType);
        $('#OrganismosContenedor input:checked').each(function () {
            formData.append(`pemexInDto[Organismos][${index}][usuarioId]`, $("#userID").val());
            formData.append(`pemexInDto[Organismos][${index}][OrganismID]`, $(this).attr('id'));
            index++;
        });
        formData.append("esPemex", true);
        formData.append("fileID", fileID);
        formData.append("fileDP", fileDP);

        if ($("#userID").val() === '') {
            $.ajax({
                method: "POST",
                url: "Usuarios/Agregar",
                contentType: "multipart/form-data",
                data: formData,
                processData: false,
                contentType: false,
                success: function (data) {
                    if (!isNull(data.success) && !data.success)
                        return errorAlert(data.responseText);

                    successAlert("Usuario Creado");
                },
                error: function (xhr, exception) {
                    errorAlert(exception);
                }
            });
        }
        else {
            $.ajax({
                method: "POST",
                url: "Usuarios/Actualizar",
                contentType: "multipart/form-data",
                data: formData,
                processData: false,
                contentType: false,
                success: function (data) {
                    if (!isNull(data.success) && !data.success)
                        return errorAlert(data.responseText);

                    successAlert("Usuario Actualizado");
                },
                error: function (xhr, exception) {
                    errorAlert(exception);
                }
            });
        }
    }
});

//Consultar ficha
$(document).on("click", "#btnBuscarFicha", function (e) {
    e.preventDefault();

    var ficha = $("#Token").val();
    if (ficha.length !== 6) {
        return false;
    }

    showLoader();

    $.ajax({
        url: "/Usuarios/ExisteUsuario/",
        data: { ficha: ficha },
        success: function (data) {
            if (data == true) {
                hideLoader();
                errorAlert('Se encontró un usuario previamente registrado con la ficha ingresada');
                return;
            }

            $.ajax({
                url: "/Usuarios/UsuarioSIO/",
                data: { ficha: ficha },
                success: function (data) {
                    hideLoader();
                    if (data.responseText == undefined) {
                        $("#Name").val(data.fichaResult.nombres + ' ' + data.fichaResult.aP_PATERNO + ' ' + data.fichaResult.aP_MATERNO);
                        $("#Email").val(data.fichaResult.email);
                        $("#RFC").val(data.fichaResult.rfC_SAT);
                        $("#ManagementCenter").val(data.fichaResult.deptO_CLAVE);
                        successAlert("Ficha Encontrada");
                    }
                    else {
                        errorAlert(data.responseText);
                    }
                }
            });
        }
    });
});

//Guardar usuario Proveedor
$(document).on
    ("submit", "#formExternos", function (e) {
    e.preventDefault();

    if ($("#formExternos").valid()) {
        const inputs = new FormData(e.target);
        const values = Object.fromEntries(inputs.entries());
        let index = 0;

        var formData = new FormData();
        formData.append("pemexInDto[UserID]", $("#userID").val());
        formData.append("pemexInDto[Name]", values.Name);
        formData.append("pemexInDto[Centro]", values.ManagementCenter);
        formData.append("pemexInDto[RFC]", values.RFC.toUpperCase());
        formData.append("pemexInDto[Perfil]", values.ProfileID);
        formData.append("pemexInDto[ValidoDesde]", values.DateInitialValid);
        formData.append("pemexInDto[ValidoHasta]", values.DateEndValid);
        formData.append("pemexInDto[PhoneNumber]", values.PhoneNumber);
        formData.append("pemexInDto[UserType]", values.UserType);
        $('#external-organims-table tbody tr').each(function () {
            formData.append(`pemexInDto[Organismos][${index}][usuarioId]`, $("#userID").val());
            formData.append(`pemexInDto[Organismos][${index}][EmailAlternate]`, $(this.cells[1]).text());
            formData.append(`pemexInDto[Organismos][${index}][Company]`, $(this.cells[2]).text());
            formData.append(`pemexInDto[Organismos][${index}][CreditorNumber]`, $(this.cells[3]).text());
            formData.append(`pemexInDto[Organismos][${index}][CreditorRFC]`, $(this.cells[4]).text());
            formData.append(`pemexInDto[Organismos][${index}][OrganismID]`, $(this.cells[5]).find('input').val());
            index++;
        });
        formData.append("esPemex", false);
        formData.append("fileID", fileID);
        formData.append("fileDP", fileDP);

        if ($("#userID").val() === '') {
            $.ajax({
                method: "POST",
                url: "Usuarios/Agregar",
                contentType: "multipart/form-data",
                data: formData,
                processData: false,
                contentType: false,
                success: function (data) {
                    if (!isNull(data.success) && !data.success)
                        return errorAlert(data.responseText);

                    successAlert("Usuario Creado");
                },
                error: function (xhr, exception) {
                    errorAlert(exception);
                }
            });
        }
        else {
            $.ajax({
                method: "POST",
                url: "Usuarios/Actualizar",
                contentType: "multipart/form-data",
                data: formData,
                processData: false,
                contentType: false,
                success: function (data) {
                    if (!isNull(data.success) && !data.success)
                        return errorAlert(data.responseText);

                    successAlert("Usuario Actualizado");
                },
                error: function (xhr, exception) {
                    errorAlert(exception);
                }
            });
        }
    }
});

//validaciones
function ValidarPemex() {
    $("#formPemex").validate({
        errorPlacement: function (error, element) {
            // Append error within linked label
            $(element)
                .closest("form")
                .find("label[for='" + element.attr("id") + "']")
                .append(error);
        },
        errorElement: "span",
        rules: {
            UserType: {
                required: true,
            },
            Token: {
                required: true,
                number: true,
                minlength: 6,
                maxlength: 6
            },
            Name: {
                required: true,
                regex: '^[0-9a-zA-Z.,áéíóúÁÉÍÓÚñÑ ]*$'
            },
            Email: {
                required: true,
                emailRX: true
            },
            DateInitialValid: {
                required: true,
            },
            DateEndValid: {
                required: true,
            },
            RFC: {
                required: true,
                rfcRX: true
            },
            ManagementCenter: {
                required: true,
            },
            ProfileID: {
                required: true,
            },
        },
        messages: {
            UserType: {
                required: " (*)",
            },
            Token: {
                required: " (*)",
                number: " * Solo numeros.",
                minlength: " * Mínimo 6 caracteres",
                maxlength: " * Máximo 6 caracteres",
            },
            Name: {
                required: " (*)",
            },
            Email: {
                required: " (*)",
            },
            DateInitialValid: {
                required: " (*)",
            },
            DateEndValid: {
                required: " (*)",
            },
            RFC: {
                required: " (*)",
            },
            ManagementCenter: {
                required: " (*)",
            },
            ProfileID: {
                required: " (*)",
            },
        }
    });
}
function ValidarProveedor() {
    $("#formExternos").validate({
        errorPlacement: function (error, element) {
            // Append error within linked label
            $(element)
                .closest("form")
                .find("label[for='" + element.attr("id") + "']")
                .append(error);
        },
        errorElement: "span",
        rules: {
            UserType: {
                required: true,
            },
            Name: {
                required: true,
                regex: '^[0-9a-zA-Z.,áéíóúÁÉÍÓÚñÑ ]*$'
            },
            RFC: {
                required: true,
                rfcRX: true
            },
            DateInitialValid: {
                required: true,
            },
            DateEndValid: {
                required: true,
            },
            PhoneNumber: {
                required: true,
                number: true
            },
            ManagementCenter: {
                required: true,
            },
            ProfileID: {
                required: true,
            },
        },
        messages: {
            UserType: {
                required: " (*)",
            },
            Name: {
                required: " (*)",
            },
            RFC: {
                required: " (*)",
            },
            DateInitialValid: {
                required: " (*)",
            },
            DateEndValid: {
                required: " (*)",
            },
            PhoneNumber: {
                required: " (*)",
                number: " * Solo numeros."
            },
            ManagementCenter: {
                required: " (*)",
            },
            ProfileID: {
                required: " (*)",
            },
        }
    });
}
function rfcValido(rfc, aceptarGenerico = true) {
    rfc = rfc.toUpperCase();
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
    return true;
}
function emailValido(email) {
    const re = "^[a-z0-9!#$%&' * +/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?$";
    var validado = email.match(re);

    if (!validado)  //Coincide con el formato general del regex?
        return false;

    return true;
        
}
jQuery.validator.addMethod("regex", function (value, element, regexp) {
    var re = new RegExp(regexp);
    return this.optional(element) || re.test(value);
}, ' * Caracteres incorrectos');
jQuery.validator.addMethod("rfcRX", function (value, element) {
    return this.optional(element) || rfcValido(value);
}, ' RFC incorrecto.');
jQuery.validator.addMethod("emailRX", function (value, element) {
    return this.optional(element) || emailValido(value);
}, ' Correo invalido.');

// Carga de Archivos
let fileID;
let fileDP
$(document).on("click", "#btnFileID", function (e) {
    e.preventDefault();
    $("#fileID").click();
    e.stopImmediatePropagation();
});
$(document).on("click", "#btnFileDP", function (e) {
    e.preventDefault();
    $("#fileDP").click();
    e.stopImmediatePropagation();
});
$(document).on("change", "#fileID", function (e) {
    e.preventDefault();

    if ($(this).prop("files").length > 0) {
        var fileName = $(this).prop("files")[0].name;
        if (fileName.substr(fileName.length - 4) !== '.pdf') {
            errorAlert('Solo se permiten archivos PDF.')
        }
        else {
            fileID = $(this).prop("files")[0];
            successAlert('Archivo Cargado Correctamente.');
        }
    } else {
        errorAlert('Error al Cargar Archivo.')
    }

    e.stopImmediatePropagation();
});
$(document).on("change", "#fileDP", function (e) {
    e.preventDefault();

    if ($(this).prop("files").length > 0) {
        var fileName = $(this).prop("files")[0].name;
        if (fileName.substr(fileName.length - 4) !== '.pdf') {
            errorAlert('Solo se permiten archivos PDF.')
        }
        else {
            fileDP = $(this).prop("files")[0];
            successAlert('Archivo Cargado Correctamente.');
        }
    } else {
        errorAlert('Error al Cargar Archivo.');
    }

    e.stopImmediatePropagation();
});

//Agregar o modificar un organismo externo
let esNuevo = false;
const tdFinal = (row) => `
            <tr>
                <td class="d-none">${row.id}</td>
                <td>${row.emailAlternate}</td>
                <td>${row.company}</td>
                <td>${row.creditorNumber}</td>
                <td>${row.creditorRFC}</td>
                <td>
                    ${row.organismText}
                    <input type="hidden" value="${row.organismID}" />
                </td>
                <td class="d-flex flex-row">
                    <div id="delete-org" class="btn btn-sm btn-default border-1 bg-white pb-2 mr-2">
                        <i class="fas fa-trash-alt text-secondary mt-2 p-1"></i>
                    </div>
                    <div id="update-org" class="btn btn-sm btn-default border-1 bg-white pb-2 px-2">
                        <i class="fa fa-pen text-secondary mt-2 p-1"></i>
                    </div>
                </td>
            </tr>
         `;
$(document).on("click", "#new-organism-external", function (e) {
    e.preventDefault();
    if (esNuevo) {
        $("#external-organims-table tbody tr:first").each(function () {
            let tr = $(this);
            tr.attr("style", "background-color:#ffecb3 !Important")

            setTimeout(function () {
                tr.attr("style", "background-color:")
            }, 100);
        });
    }
    else {
        var t = document.querySelector('#row-template');
        var clone = document.importNode(t.content, true);
        clone.querySelector('tr').setAttribute('Class', 'new');

        $("#external-organims-table tbody").prepend($("#row-template").html());
        esNuevo = true;
    }
});
$(document).on("click", "#delete-org", function (e) {
    e.preventDefault();
    let row = $(this).closest('tr');
    row.remove();
});
$(document).on("click", "#update-org", function (e) {
    e.preventDefault();
    let row = $(this).closest('tr')[0];
    let rowIndex = $('#external-organims-table tbody tr').index($(this).closest('tr'));

    let rowValues = {
        emailAlternate: $(row.cells[1]).text().trim(),
        company: $(row.cells[2]).text().trim(),
        creditorNumber: $(row.cells[3]).text().trim(),
        creditorRFC: $(row.cells[4]).text().trim().toUpperCase(),
        organismID: $(row.cells[5]).find('input').val(),
        organismText: $(row.cells[5]).text().trim()
    }

    var t = document.querySelector('#row-template');
    var clone = document.importNode(t.content, true);
    clone.querySelector('tr').setAttribute('Class', 'selected');
    clone.querySelector('#EmailAlternate').value = rowValues.emailAlternate;
    clone.querySelector('#Company').value = rowValues.company;
    clone.querySelector('#CreditorNumber').value = rowValues.creditorNumber;
    clone.querySelector('#CreditorRFC').value = rowValues.creditorRFC;
    clone.querySelector('#OrganismID option[value="' + rowValues.organismID + '"]').setAttribute('selected', true)

    $("#external-organims-table tbody tr").eq(rowIndex).before(clone);
    row.remove();
    esModificado = true;
});
$(document).on("click", "#add-org", function (e) {
    e.preventDefault();
    let row = $(this).closest('tr');
    let rowIndex = $('#external-organims-table tbody tr').index($(this).closest('tr'));
    let validForm = false;

    let rowNewValues = {
        emailAlternate: row.find("#EmailAlternate").val().trim(),
        company: row.find("#Company").val().trim(),
        creditorNumber: row.find("#CreditorNumber").val().trim(),
        creditorRFC: row.find("#CreditorRFC").val().trim().toUpperCase(),
        organismID: row.find("#OrganismID").val(),
        organismText: row.find("#OrganismID option:selected").text()
    }
    if (rowNewValues.emailAlternate == '') {
        row.find("#EmailAlternate").attr('style', 'border: 1px solid red !Important;');
        validForm = true;
    }
    else {
        if (!emailValido(rowNewValues.emailAlternate)) {
            row.find("#EmailAlternate").attr('style', 'border: 1px solid red !Important;');
            errorAlert('Correo Invalido.');
            validForm = true;
        }
        else {
            row.find("#EmailAlternate").css({ "border": "" });
        }
    }

    if (rowNewValues.company == '') {
        row.find("#Company").attr('style', 'border: 1px solid red !Important;');
        validForm = true;
    }
    else {
        row.find("#Company").css({ "border": "" });
    }

    if (rowNewValues.creditorNumber == '') {
        row.find("#CreditorNumber").attr('style', 'border: 1px solid red !Important;');
        validForm = true;
    }
    else {
        row.find("#CreditorNumber").css({ "border": "" });
    }

    if (rowNewValues.creditorRFC == '') {
        row.find("#CreditorRFC").val(rowNewValues.creditorRFC.toUpperCase());
        row.find("#CreditorRFC").attr('style', 'border: 1px solid red !Important;');
        validForm = true;
    }
    else {
        if (!rfcValido(rowNewValues.creditorRFC)) {
            row.find("#CreditorRFC").val(rowNewValues.creditorRFC.toUpperCase());
            row.find("#CreditorRFC").attr('style', 'border: 1px solid red !Important;');
            errorAlert('RFC incorrecto.');
            validForm = true;
        }
        else {
            row.find("#CreditorRFC").css({ "border": "" });
        }
    }

    if (rowNewValues.organismID == null) {
        row.find("#OrganismID").attr('style', 'border: 1px solid red !Important;');
        validForm = true;
    }
    else {
        row.find("#OrganismID").css({ "border": "" });
    }

    if (validForm == false) {
        $("#external-organims-table tbody tr").eq(rowIndex).before(tdFinal(rowNewValues));
        row.remove();
    }
});
$(document).on("click", "#cancel-add-org", function (e) {
    e.preventDefault();
    let row = $(this).closest('tr');
    let rowIndex = $('#external-organims-table tbody tr').index($(this).closest('tr'));
    if (row.attr('Class') == 'new') {
        row.remove();
        esNuevo = false;
    }
    else {
        let rowNewValues = {
            emailAlternate: row.find("#EmailAlternate").val(),
            company: row.find("#Company").val(),
            creditorNumber: row.find("#CreditorNumber").val(),
            creditorRFC: row.find("#CreditorRFC").val(),
            organismID: row.find("#OrganismID").val(),
            organismText: row.find("#OrganismID option:selected").text()
        }

        $("#external-organims-table tbody tr").eq(rowIndex).before(tdFinal(rowNewValues));
        row.remove();
    }
});


function onlyNumberKey(evt) {
    // Only ASCII character in that range allowed
    let ASCIICode = (evt.which) ? evt.which : evt.keyCode
    if (ASCIICode > 31 && (ASCIICode < 48 || ASCIICode > 57))
        return false;
    return true;
}
